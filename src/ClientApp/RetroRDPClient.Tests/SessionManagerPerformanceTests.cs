using Microsoft.Extensions.Logging;
using RetroRDPClient.Services;
using RetroRDP.Shared.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RetroRDPClient.Tests
{
    /// <summary>
    /// Additional tests for SessionManager performance and optimization features
    /// </summary>
    public class SessionManagerPerformanceTests : IDisposable
    {
        private readonly SessionManager _sessionManager;
        private readonly ILogger<SessionManager> _logger;

        public SessionManagerPerformanceTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<SessionManager>();
            _sessionManager = new SessionManager(_logger);
        }

        [Fact]
        public async Task StartSessionAsync_WithPerformanceOptions_ShouldApplyConfiguration()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "127.0.0.1",
                Username = "testuser",
                Password = "testpass",
                SessionName = "Performance Test Session",
                Port = 3389
            };

            // Act
            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);
            var session = _sessionManager.GetSession(sessionId!);

            // Assert
            Assert.NotNull(sessionId);
            Assert.NotNull(session);
            Assert.NotNull(session.PerformanceOptions);
            Assert.Equal(PerformancePreset.Balanced, session.PerformanceOptions.Preset);
        }

        [Fact]
        public async Task StartSessionAsync_WithCustomPerformancePreset_ShouldMaintainSettings()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "192.168.1.100",
                Username = "testuser", 
                Password = "testpass",
                SessionName = "Quality Test Session",
                Port = 3389
            };

            // Act
            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);
            var session = _sessionManager.GetSession(sessionId!);
            
            // Modify performance options after creation
            session!.PerformanceOptions.ApplyPreset(PerformancePreset.Quality);

            // Assert
            Assert.Equal(PerformancePreset.Quality, session.PerformanceOptions.Preset);
            Assert.Equal(32, session.PerformanceOptions.ColorDepth);
            Assert.Equal(30, session.PerformanceOptions.ScreenUpdateFrequency);
            Assert.True(session.PerformanceOptions.EnableDesktopBackground);
        }

        [Fact]
        public void GetPerformanceRecommendations_ShouldReturnValidRecommendations()
        {
            // Act
            var recommendations = _sessionManager.GetPerformanceRecommendations();

            // Assert
            Assert.NotNull(recommendations);
            Assert.NotNull(recommendations.Recommendations);
            // Recommendations may be empty if system performance is good
        }

        [Fact]
        public void GetSystemPerformanceMetrics_ShouldReturnValidMetrics()
        {
            // Act
            var metrics = _sessionManager.GetSystemPerformanceMetrics();

            // Assert
            Assert.NotNull(metrics);
            Assert.True(metrics.CpuUsagePercent >= 0);
            Assert.True(metrics.MemoryUsageMB > 0);
            Assert.True(metrics.ActiveSessionCount >= 0);
            Assert.True(metrics.TotalBytesReceived >= 0);
            Assert.True(metrics.TotalBytesSent >= 0);
        }

        [Fact]
        public async Task MultipleSessions_ShouldTrackPerformanceIndependently()
        {
            // Arrange
            var request1 = new RdpConnectionRequest { Host = "127.0.0.1", Username = "user1", Password = "pass1" };
            var request2 = new RdpConnectionRequest { Host = "192.168.1.1", Username = "user2", Password = "pass2" };

            // Act
            var sessionId1 = await _sessionManager.StartSessionAsync(request1);
            var sessionId2 = await _sessionManager.StartSessionAsync(request2);
            
            var session1 = _sessionManager.GetSession(sessionId1!);
            var session2 = _sessionManager.GetSession(sessionId2!);

            // Configure different performance settings
            session1!.PerformanceOptions.ApplyPreset(PerformancePreset.Performance);
            session2!.PerformanceOptions.ApplyPreset(PerformancePreset.Quality);

            // Assert
            Assert.NotEqual(session1.PerformanceOptions.ColorDepth, session2.PerformanceOptions.ColorDepth);
            Assert.NotEqual(session1.PerformanceOptions.AudioQuality, session2.PerformanceOptions.AudioQuality);
            Assert.NotEqual(session1.PerformanceOptions.EnableDesktopBackground, session2.PerformanceOptions.EnableDesktopBackground);
        }

        [Fact]
        public async Task EndSessionAsync_ShouldCleanupPerformanceMonitoring()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "127.0.0.1",
                Username = "testuser",
                Password = "testpass"
            };

            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);
            
            // Act
            var result = await _sessionManager.EndSessionAsync(sessionId!);
            var session = _sessionManager.GetSession(sessionId!);

            // Assert
            Assert.True(result);
            Assert.Null(session);
        }

        [Theory]
        [InlineData(PerformancePreset.Performance)]
        [InlineData(PerformancePreset.Balanced)]
        [InlineData(PerformancePreset.Quality)]
        public async Task SessionPerformanceOptions_AllPresets_ShouldBeConfigurable(PerformancePreset preset)
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "test-server",
                Username = "testuser",
                Password = "testpass"
            };

            // Act
            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);
            var session = _sessionManager.GetSession(sessionId!);
            session!.PerformanceOptions.ApplyPreset(preset);

            // Assert
            Assert.Equal(preset, session.PerformanceOptions.Preset);
            
            // Verify preset-specific settings
            switch (preset)
            {
                case PerformancePreset.Performance:
                    Assert.Equal(16, session.PerformanceOptions.ColorDepth);
                    Assert.Equal(0, session.PerformanceOptions.AudioQuality);
                    Assert.False(session.PerformanceOptions.EnableDesktopBackground);
                    break;
                    
                case PerformancePreset.Quality:
                    Assert.Equal(32, session.PerformanceOptions.ColorDepth);
                    Assert.Equal(3, session.PerformanceOptions.AudioQuality);
                    Assert.True(session.PerformanceOptions.EnableDesktopBackground);
                    break;
                    
                case PerformancePreset.Balanced:
                    Assert.Equal(16, session.PerformanceOptions.ColorDepth);
                    Assert.Equal(1, session.PerformanceOptions.AudioQuality);
                    Assert.False(session.PerformanceOptions.EnableDesktopBackground);
                    break;
            }
        }

        [Fact]
        public async Task SessionWithLowPerformanceSettings_ShouldGenerateOptimizationRecommendations()
        {
            // Arrange
            var connectionRequest = new RdpConnectionRequest
            {
                Host = "test-server",
                Username = "testuser",
                Password = "testpass"
            };

            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);
            var session = _sessionManager.GetSession(sessionId!);
            
            // Configure poor performance settings
            session!.PerformanceOptions.EnableBitmapCaching = false;
            session.PerformanceOptions.ColorDepth = 32;
            session.PerformanceOptions.MaxConcurrentSessions = 1;

            // Act
            var recommendations = _sessionManager.GetPerformanceRecommendations();

            // Assert
            Assert.NotNull(recommendations);
            Assert.Contains(recommendations.Recommendations, 
                r => r.Title.Contains("Bitmap caching disabled"));
        }

        public void Dispose()
        {
            _sessionManager?.Dispose();
        }
    }
}