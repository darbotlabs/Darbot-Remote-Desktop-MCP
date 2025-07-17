using Microsoft.Extensions.Logging;
using RetroRDPClient.Services;
using RetroRDP.Shared.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RetroRDPClient.Tests
{
    /// <summary>
    /// Unit tests for SessionManager
    /// </summary>
    public class SessionManagerTests : IDisposable
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<SessionManager> _logger;

        public SessionManagerTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<SessionManager>();
            _sessionManager = new SessionManager(_logger);
        }

        [Fact]
        public void SessionManager_Initialization_ShouldHaveEmptyActiveSessions()
        {
            // Act & Assert
            Assert.Empty(_sessionManager.ActiveSessions);
        }

        [Fact]
        public async Task StartSessionAsync_WithValidRequest_ShouldCreateSession()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "127.0.0.1",
                Username = "testuser",
                Password = "testpass",
                SessionName = "Test Session",
                Port = 3389
            };

            // Act
            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);

            // Assert
            Assert.NotNull(sessionId);
            Assert.Single(_sessionManager.ActiveSessions);
            
            var session = _sessionManager.GetSession(sessionId);
            Assert.NotNull(session);
            Assert.Equal("Test Session", session.SessionName);
            Assert.Equal("127.0.0.1", session.Host);
            Assert.Equal("testuser", session.Username);
            Assert.Equal(3389, session.Port);
        }

        [Fact]
        public async Task StartSessionAsync_WithEmptyHost_ShouldReturnNull()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "",
                Username = "testuser",
                Password = "testpass"
            };

            // Act
            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);

            // Assert
            Assert.Null(sessionId);
            Assert.Empty(_sessionManager.ActiveSessions);
        }

        [Fact]
        public async Task EndSessionAsync_WithValidSession_ShouldRemoveSession()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "127.0.0.1",
                Username = "testuser",
                Password = "testpass",
                SessionName = "Test Session"
            };

            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);
            Assert.NotNull(sessionId);

            // Act
            var result = await _sessionManager.EndSessionAsync(sessionId);

            // Assert
            Assert.True(result);
            Assert.Empty(_sessionManager.ActiveSessions);
            Assert.Null(_sessionManager.GetSession(sessionId));
        }

        [Fact]
        public async Task EndSessionAsync_WithInvalidSessionId_ShouldReturnFalse()
        {
            // Act
            var result = await _sessionManager.EndSessionAsync("invalid-session-id");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DisconnectAllSessionsAsync_WithMultipleSessions_ShouldDisconnectAll()
        {
            // Arrange
            var request1 = new RdpConnectionRequest { Host = "127.0.0.1", Username = "user1", Password = "pass1" };
            var request2 = new RdpConnectionRequest { Host = "192.168.1.1", Username = "user2", Password = "pass2" };

            await _sessionManager.StartSessionAsync(request1);
            await _sessionManager.StartSessionAsync(request2);
            
            Assert.Equal(2, _sessionManager.ActiveSessions.Count);

            // Act
            var result = await _sessionManager.DisconnectAllSessionsAsync();

            // Assert
            Assert.True(result);
            Assert.Empty(_sessionManager.ActiveSessions);
        }

        [Fact]
        public void UpdateSessionStatus_WithValidSession_ShouldUpdateStatus()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "127.0.0.1",
                Username = "testuser",
                Password = "testpass"
            };

            var sessionId = _sessionManager.StartSessionAsync(connectionRequest).Result;
            Assert.NotNull(sessionId);

            var session = _sessionManager.GetSession(sessionId);
            Assert.NotNull(session);
            Assert.Equal(RdpSessionStatus.Connecting, session.Status);

            // Act
            _sessionManager.UpdateSessionStatus(sessionId, RdpSessionStatus.Connected);

            // Assert
            var updatedSession = _sessionManager.GetSession(sessionId);
            Assert.NotNull(updatedSession);
            Assert.Equal(RdpSessionStatus.Connected, updatedSession.Status);
            Assert.NotNull(updatedSession.ConnectedAt);
        }

        [Fact]
        public void UpdateSessionStatus_WithErrorMessage_ShouldSetErrorMessage()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "127.0.0.1",
                Username = "testuser",
                Password = "testpass"
            };

            var sessionId = _sessionManager.StartSessionAsync(connectionRequest).Result;
            Assert.NotNull(sessionId);

            // Act
            _sessionManager.UpdateSessionStatus(sessionId, RdpSessionStatus.Failed, "Connection timeout");

            // Assert
            var session = _sessionManager.GetSession(sessionId);
            Assert.NotNull(session);
            Assert.Equal(RdpSessionStatus.Failed, session.Status);
            Assert.Equal("Connection timeout", session.ErrorMessage);
        }

        [Fact]
        public void SessionStatusChanged_Event_ShouldFireOnStatusUpdate()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "127.0.0.1",
                Username = "testuser",
                Password = "testpass"
            };

            var sessionId = _sessionManager.StartSessionAsync(connectionRequest).Result;
            Assert.NotNull(sessionId);

            SessionStatusChangedEventArgs? eventArgs = null;
            _sessionManager.SessionStatusChanged += (sender, args) => eventArgs = args;

            // Act
            _sessionManager.UpdateSessionStatus(sessionId, RdpSessionStatus.Connected);

            // Assert
            Assert.NotNull(eventArgs);
            Assert.Equal(sessionId, eventArgs.SessionId);
            Assert.Equal(RdpSessionStatus.Connecting, eventArgs.OldStatus);
            Assert.Equal(RdpSessionStatus.Connected, eventArgs.NewStatus);
        }

        [Fact]
        public void GetSession_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var session = _sessionManager.GetSession("invalid-id");

            // Assert
            Assert.Null(session);
        }

        public void Dispose()
        {
            _sessionManager?.Dispose();
        }
    }
}