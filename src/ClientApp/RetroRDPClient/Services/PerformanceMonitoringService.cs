using Microsoft.Extensions.Logging;
using RetroRDP.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Performance monitoring service for RDP sessions
    /// Tracks CPU usage, memory consumption, and provides optimization recommendations
    /// </summary>
    public class PerformanceMonitoringService : IDisposable
    {
        private readonly ILogger<PerformanceMonitoringService>? _logger;
        private readonly ConcurrentDictionary<string, SessionPerformanceMetrics> _sessionMetrics = new();
        private readonly Timer? _monitoringTimer;
        private readonly PerformanceCounter? _cpuCounter;
        private readonly object _lockObject = new();
        private bool _disposed = false;

        public event EventHandler<PerformanceWarningEventArgs>? PerformanceWarning;

        public PerformanceMonitoringService(ILogger<PerformanceMonitoringService>? logger = null)
        {
            _logger = logger;
            
            try
            {
                // Initialize performance counters if available
                if (OperatingSystem.IsWindows())
                {
                    _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                    _cpuCounter.NextValue(); // First call always returns 0
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to initialize CPU performance counter");
            }

            // Start monitoring timer (every 5 seconds)
            _monitoringTimer = new Timer(MonitorPerformance, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
            
            _logger?.LogInformation("Performance monitoring service initialized");
        }

        /// <summary>
        /// Register a session for performance monitoring
        /// </summary>
        public void RegisterSession(string sessionId, string hostName)
        {
            var metrics = new SessionPerformanceMetrics
            {
                SessionId = sessionId,
                HostName = hostName,
                StartTime = DateTime.UtcNow
            };

            _sessionMetrics[sessionId] = metrics;
            _logger?.LogDebug("Registered session {SessionId} for performance monitoring", sessionId);
        }

        /// <summary>
        /// Unregister a session from performance monitoring
        /// </summary>
        public void UnregisterSession(string sessionId)
        {
            if (_sessionMetrics.TryRemove(sessionId, out var metrics))
            {
                var duration = DateTime.UtcNow - metrics.StartTime;
                _logger?.LogInformation("Session {SessionId} monitored for {Duration:hh\\:mm\\:ss}", sessionId, duration);
            }
        }

        /// <summary>
        /// Update session activity metrics
        /// </summary>
        public void UpdateSessionActivity(string sessionId, long bytesReceived = 0, long bytesSent = 0)
        {
            if (_sessionMetrics.TryGetValue(sessionId, out var metrics))
            {
                lock (_lockObject)
                {
                    metrics.BytesReceived += bytesReceived;
                    metrics.BytesSent += bytesSent;
                    metrics.LastActivity = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// Get current performance metrics for a session
        /// </summary>
        public SessionPerformanceMetrics? GetSessionMetrics(string sessionId)
        {
            return _sessionMetrics.TryGetValue(sessionId, out var metrics) ? metrics : null;
        }

        /// <summary>
        /// Get aggregated system performance metrics
        /// </summary>
        public SystemPerformanceMetrics GetSystemMetrics()
        {
            var process = Process.GetCurrentProcess();
            var systemMetrics = new SystemPerformanceMetrics
            {
                CpuUsagePercent = GetCpuUsage(),
                MemoryUsageMB = process.WorkingSet64 / (1024 * 1024),
                ActiveSessionCount = _sessionMetrics.Count,
                TotalBytesReceived = 0,
                TotalBytesSent = 0
            };

            // Aggregate session metrics
            foreach (var metrics in _sessionMetrics.Values)
            {
                systemMetrics.TotalBytesReceived += metrics.BytesReceived;
                systemMetrics.TotalBytesSent += metrics.BytesSent;
            }

            return systemMetrics;
        }

        /// <summary>
        /// Get performance optimization recommendations
        /// </summary>
        public PerformanceRecommendations GetOptimizationRecommendations(RdpPerformanceOptions currentOptions)
        {
            var systemMetrics = GetSystemMetrics();
            var recommendations = new PerformanceRecommendations();

            // CPU-based recommendations
            if (systemMetrics.CpuUsagePercent > 80)
            {
                recommendations.AddRecommendation(
                    "High CPU usage detected",
                    "Consider reducing color depth, screen update frequency, or number of active sessions",
                    PerformanceImpact.High);
            }

            // Memory-based recommendations
            if (systemMetrics.MemoryUsageMB > 2048)
            {
                recommendations.AddRecommendation(
                    "High memory usage detected",
                    "Consider reducing screen resolution or enabling more aggressive compression",
                    PerformanceImpact.Medium);
            }

            // Session count recommendations
            if (systemMetrics.ActiveSessionCount > currentOptions.MaxConcurrentSessions)
            {
                recommendations.AddRecommendation(
                    "Too many concurrent sessions",
                    "Consider closing unused sessions or increasing the session limit",
                    PerformanceImpact.High);
            }

            // Configuration-based recommendations
            if (currentOptions.ColorDepth > 16 && systemMetrics.CpuUsagePercent > 60)
            {
                recommendations.AddRecommendation(
                    "High color depth with elevated CPU usage",
                    "Reduce color depth to 16-bit for better performance",
                    PerformanceImpact.Medium);
            }

            if (!currentOptions.EnableBitmapCaching)
            {
                recommendations.AddRecommendation(
                    "Bitmap caching disabled",
                    "Enable bitmap caching to improve performance",
                    PerformanceImpact.Medium);
            }

            return recommendations;
        }

        private void MonitorPerformance(object? state)
        {
            try
            {
                var systemMetrics = GetSystemMetrics();
                
                // Log performance metrics
                LoggingService.LogPerformanceMetrics(
                    "system",
                    (int)systemMetrics.CpuUsagePercent,
                    systemMetrics.MemoryUsageMB,
                    0); // Response time not tracked at system level

                // Check for performance warnings
                CheckPerformanceWarnings(systemMetrics);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during performance monitoring");
            }
        }

        private void CheckPerformanceWarnings(SystemPerformanceMetrics metrics)
        {
            if (metrics.CpuUsagePercent > 85)
            {
                PerformanceWarning?.Invoke(this, new PerformanceWarningEventArgs
                {
                    WarningType = PerformanceWarningType.HighCpuUsage,
                    Message = $"CPU usage is at {metrics.CpuUsagePercent:F1}%",
                    Severity = PerformanceSeverity.Critical
                });
            }

            if (metrics.MemoryUsageMB > 3072) // 3GB
            {
                PerformanceWarning?.Invoke(this, new PerformanceWarningEventArgs
                {
                    WarningType = PerformanceWarningType.HighMemoryUsage,
                    Message = $"Memory usage is at {metrics.MemoryUsageMB} MB",
                    Severity = PerformanceSeverity.Warning
                });
            }
        }

        private float GetCpuUsage()
        {
            try
            {
                return OperatingSystem.IsWindows() ? (_cpuCounter?.NextValue() ?? 0) : 0;
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _monitoringTimer?.Dispose();
                _cpuCounter?.Dispose();
                _sessionMetrics.Clear();
                _disposed = true;
                
                _logger?.LogInformation("Performance monitoring service disposed");
            }
        }
    }

    /// <summary>
    /// Performance metrics for a specific RDP session
    /// </summary>
    public class SessionPerformanceMetrics
    {
        public string SessionId { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime LastActivity { get; set; }
        public long BytesReceived { get; set; }
        public long BytesSent { get; set; }
        public TimeSpan Duration => DateTime.UtcNow - StartTime;
    }

    /// <summary>
    /// System-wide performance metrics
    /// </summary>
    public class SystemPerformanceMetrics
    {
        public float CpuUsagePercent { get; set; }
        public long MemoryUsageMB { get; set; }
        public int ActiveSessionCount { get; set; }
        public long TotalBytesReceived { get; set; }
        public long TotalBytesSent { get; set; }
    }

    /// <summary>
    /// Performance optimization recommendations
    /// </summary>
    public class PerformanceRecommendations
    {
        public List<PerformanceRecommendation> Recommendations { get; } = new();

        public void AddRecommendation(string title, string description, PerformanceImpact impact)
        {
            Recommendations.Add(new PerformanceRecommendation
            {
                Title = title,
                Description = description,
                Impact = impact
            });
        }
    }

    public class PerformanceRecommendation
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PerformanceImpact Impact { get; set; }
    }

    public enum PerformanceImpact
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    /// Performance warning event arguments
    /// </summary>
    public class PerformanceWarningEventArgs : EventArgs
    {
        public PerformanceWarningType WarningType { get; set; }
        public string Message { get; set; } = string.Empty;
        public PerformanceSeverity Severity { get; set; }
    }

    public enum PerformanceWarningType
    {
        HighCpuUsage,
        HighMemoryUsage,
        TooManySessions,
        SlowResponse
    }

    public enum PerformanceSeverity
    {
        Info,
        Warning,
        Critical
    }
}