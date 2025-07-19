using Microsoft.Extensions.Logging;
using RetroRDP.Shared.Models;
using RetroRDP.Shared.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Implementation of ISessionManager for managing RDP sessions
    /// Handles creation, connection, and cleanup of RDP sessions
    /// </summary>
    public class SessionManager : ISessionManager, IDisposable
    {
        private readonly ILogger<SessionManager>? _logger;
        private readonly ConcurrentDictionary<string, RdpSession> _sessions = new();
        private readonly ConcurrentDictionary<string, object> _rdpControls = new(); // Store RDP control references
        private readonly ConcurrentDictionary<string, SecureString> _sessionCredentials = new(); // Secure password storage
        private readonly PerformanceMonitoringService _performanceMonitor;
        private bool _disposed = false;

        public event EventHandler<SessionStatusChangedEventArgs>? SessionStatusChanged;

        public IReadOnlyList<RdpSession> ActiveSessions => _sessions.Values.ToList();

        public SessionManager(ILogger<SessionManager>? logger = null)
        {
            _logger = logger;
            _performanceMonitor = new PerformanceMonitoringService(LoggingService.GetLogger<PerformanceMonitoringService>());
            _performanceMonitor.PerformanceWarning += OnPerformanceWarning;
            _logger?.LogInformation("SessionManager initialized with performance monitoring");
        }

        public RdpSession? GetSession(string sessionId)
        {
            _sessions.TryGetValue(sessionId, out var session);
            return session;
        }

        public async Task<string?> StartSessionAsync(RdpConnectionRequest connectionRequest, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Starting RDP session to {Host}:{Port}", connectionRequest.Host, connectionRequest.Port);

                // Validate connection request
                if (string.IsNullOrWhiteSpace(connectionRequest.Host))
                {
                    _logger?.LogWarning("Invalid connection request: Host is required");
                    return null;
                }

                // Create session
                var session = new RdpSession
                {
                    SessionName = connectionRequest.SessionName,
                    Host = connectionRequest.Host,
                    Username = connectionRequest.Username,
                    Port = connectionRequest.Port,
                    ScreenWidth = connectionRequest.ScreenWidth,
                    ScreenHeight = connectionRequest.ScreenHeight,
                    FullScreen = connectionRequest.FullScreen,
                    ColorDepth = connectionRequest.ColorDepth,
                    Status = RdpSessionStatus.Connecting
                };

                // Store session
                _sessions[session.SessionId] = session;

                // Register session with performance monitor
                _performanceMonitor.RegisterSession(session.SessionId, session.Host);

                // Log connection attempt (no sensitive data)
                LoggingService.LogRdpConnectionAttempt(session.SessionId, session.Host, session.Port, session.Username);

                // Store credentials securely
                var securePassword = new SecureString();
                foreach (char c in connectionRequest.Password)
                {
                    securePassword.AppendChar(c);
                }
                securePassword.MakeReadOnly();
                _sessionCredentials[session.SessionId] = securePassword;

                // Notify status change
                SessionStatusChanged?.Invoke(this, new SessionStatusChangedEventArgs
                {
                    SessionId = session.SessionId,
                    OldStatus = RdpSessionStatus.Disconnected,
                    NewStatus = RdpSessionStatus.Connecting
                });

                // Start connection asynchronously
                _ = Task.Run(async () => await ConnectSessionAsync(session, cancellationToken), cancellationToken);

                _logger?.LogInformation("Session {SessionId} created for {Host}", session.SessionId, session.Host);
                return session.SessionId;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to start RDP session to {Host}", connectionRequest.Host);
                return null;
            }
        }

        public async Task<bool> EndSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_sessions.TryGetValue(sessionId, out var session))
                {
                    _logger?.LogWarning("Session {SessionId} not found for disconnect", sessionId);
                    return false;
                }

                _logger?.LogInformation("Ending session {SessionId} to {Host}", sessionId, session.Host);

                // Update status
                UpdateSessionStatus(sessionId, RdpSessionStatus.Disconnecting);

                // Cleanup RDP control
                if (_rdpControls.TryRemove(sessionId, out var rdpControl))
                {
                    await Task.Run(() => DisposeRdpControl(rdpControl), cancellationToken);
                }

                // Clean up credentials
                if (_sessionCredentials.TryRemove(sessionId, out var credentials))
                {
                    credentials.Dispose();
                }

                // Remove session
                _sessions.TryRemove(sessionId, out _);

                // Unregister from performance monitoring
                _performanceMonitor.UnregisterSession(sessionId);

                // Update status
                UpdateSessionStatus(sessionId, RdpSessionStatus.Disconnected);

                _logger?.LogInformation("Session {SessionId} ended successfully", sessionId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to end session {SessionId}", sessionId);
                return false;
            }
        }

        public async Task<bool> ReconnectSessionAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_sessions.TryGetValue(sessionId, out var session))
                {
                    _logger?.LogWarning("Session {SessionId} not found for reconnect", sessionId);
                    return false;
                }

                _logger?.LogInformation("Reconnecting session {SessionId} to {Host}", sessionId, session.Host);

                // Update status
                UpdateSessionStatus(sessionId, RdpSessionStatus.Reconnecting);

                // Attempt reconnection
                _ = Task.Run(async () => await ConnectSessionAsync(session, cancellationToken), cancellationToken);

                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to reconnect session {SessionId}", sessionId);
                UpdateSessionStatus(sessionId, RdpSessionStatus.Failed, ex.Message);
                return false;
            }
        }

        public async Task<bool> DisconnectAllSessionsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Disconnecting all active sessions");

                var sessionIds = _sessions.Keys.ToList();
                var tasks = sessionIds.Select(id => EndSessionAsync(id, cancellationToken));
                var results = await Task.WhenAll(tasks);

                var success = results.All(r => r);
                _logger?.LogInformation("Disconnected all sessions, success: {Success}", success);
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to disconnect all sessions");
                return false;
            }
        }

        public void UpdateSessionStatus(string sessionId, RdpSessionStatus status, string? errorMessage = null)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return;
            }

            var oldStatus = session.Status;
            session.Status = status;
            session.ErrorMessage = errorMessage;

            if (status == RdpSessionStatus.Connected)
            {
                session.ConnectedAt = DateTime.Now;
                LoggingService.LogRdpConnectionSuccess(sessionId, session.Host);
            }
            else if (status == RdpSessionStatus.Disconnected)
            {
                LoggingService.LogRdpDisconnection(sessionId, session.Host, errorMessage ?? "User disconnected");
            }
            else if (status == RdpSessionStatus.Failed)
            {
                LoggingService.LogRdpConnectionFailure(sessionId, session.Host, errorMessage ?? "Unknown error");
            }

            session.LastActivity = DateTime.Now;

            SessionStatusChanged?.Invoke(this, new SessionStatusChangedEventArgs
            {
                SessionId = sessionId,
                OldStatus = oldStatus,
                NewStatus = status,
                ErrorMessage = errorMessage
            });

            _logger?.LogDebug("Session {SessionId} status changed from {OldStatus} to {NewStatus}", 
                sessionId, oldStatus, status);
        }

        private async Task ConnectSessionAsync(RdpSession session, CancellationToken cancellationToken)
        {
            try
            {
                // On non-Windows platforms, simulate connection for testing
                if (!OperatingSystem.IsWindows())
                {
                    _logger?.LogInformation("Simulating RDP connection on non-Windows platform");
                    await Task.Delay(2000, cancellationToken); // Simulate connection time
                    
                    // Simulate successful connection for demo purposes
                    UpdateSessionStatus(session.SessionId, RdpSessionStatus.Connected);
                    return;
                }

                // On Windows, create actual RDP control
                #if WINDOWS
                var rdpControl = CreateRdpControl(session);
                if (rdpControl != null)
                {
                    _rdpControls[session.SessionId] = rdpControl;
                    
                    // Configure and connect
                    ConfigureRdpControl(rdpControl, session);
                    ConnectRdpControl(rdpControl, session);
                }
                else
                {
                    UpdateSessionStatus(session.SessionId, RdpSessionStatus.Failed, "Failed to create RDP control");
                }
                #endif
            }
            catch (OperationCanceledException)
            {
                UpdateSessionStatus(session.SessionId, RdpSessionStatus.Disconnected);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Connection failed for session {SessionId}", session.SessionId);
                UpdateSessionStatus(session.SessionId, RdpSessionStatus.Failed, ex.Message);
            }
        }

        #if WINDOWS
        private object? CreateRdpControl(RdpSession session)
        {
            try
            {
                // This will be implemented when running on Windows
                // For now, return null to indicate control creation failed
                return null;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create RDP control for session {SessionId}", session.SessionId);
                return null;
            }
        }

        private void ConfigureRdpControl(object rdpControl, RdpSession session)
        {
            // RDP control configuration will be implemented here
            // Apply performance options from session.PerformanceOptions
            // For now, this is a placeholder that would configure:
            // - Bitmap caching: session.PerformanceOptions.EnableBitmapCaching
            // - Color depth: session.PerformanceOptions.ColorDepth
            // - Screen resolution: session.PerformanceOptions.ScreenWidth/ScreenHeight
            // - Compression: session.PerformanceOptions.EnableCompression
            // - Audio quality: session.PerformanceOptions.AudioQuality
            // - Visual effects: session.PerformanceOptions.EnableVisualEffects
            
            _logger?.LogDebug("Configuring RDP control for session {SessionId} with preset {Preset}", 
                session.SessionId, session.PerformanceOptions.Preset);
        }

        private void ConnectRdpControl(object rdpControl, RdpSession session)
        {
            // RDP control connection will be implemented here
        }
        #endif

        private void DisposeRdpControl(object rdpControl)
        {
            try
            {
                if (rdpControl is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error disposing RDP control");
            }
        }

        private void OnPerformanceWarning(object? sender, PerformanceWarningEventArgs e)
        {
            _logger?.LogWarning("Performance warning: {WarningType} - {Message}", e.WarningType, e.Message);
            
            // Could trigger UI notifications or automatic optimizations here
            if (e.Severity == PerformanceSeverity.Critical)
            {
                LoggingService.GetSerilogLogger().Warning("Critical performance issue detected: {Message}", e.Message);
            }
        }

        /// <summary>
        /// Get performance recommendations for the current system state
        /// </summary>
        public PerformanceRecommendations GetPerformanceRecommendations()
        {
            // Use the first session's performance options as a reference, or default
            var referenceOptions = _sessions.Values.FirstOrDefault()?.PerformanceOptions ?? RdpPerformanceOptions.GetDefault();
            return _performanceMonitor.GetOptimizationRecommendations(referenceOptions);
        }

        /// <summary>
        /// Get current system performance metrics
        /// </summary>
        public SystemPerformanceMetrics GetSystemPerformanceMetrics()
        {
            return _performanceMonitor.GetSystemMetrics();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _logger?.LogInformation("Disposing SessionManager");

                // Disconnect all sessions
                _ = DisconnectAllSessionsAsync();

                // Dispose all credentials
                foreach (var credential in _sessionCredentials.Values)
                {
                    credential?.Dispose();
                }
                _sessionCredentials.Clear();

                // Dispose all RDP controls
                foreach (var rdpControl in _rdpControls.Values)
                {
                    DisposeRdpControl(rdpControl);
                }
                _rdpControls.Clear();

                _sessions.Clear();
                
                // Dispose performance monitor
                _performanceMonitor?.Dispose();
                
                _disposed = true;
            }
        }
    }
}