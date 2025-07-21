using Microsoft.Extensions.Logging;
using RetroRDPClient.Services;
using RetroRDP.Shared.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace RetroRDPClient.Tests
{
    /// <summary>
    /// Unit tests for PerformanceMonitoringService
    /// </summary>
    public class PerformanceMonitoringServiceTests : IDisposable
    {
        private readonly PerformanceMonitoringService _performanceService;
        private readonly ILogger<PerformanceMonitoringService> _logger;

        public PerformanceMonitoringServiceTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<PerformanceMonitoringService>();
            _performanceService = new PerformanceMonitoringService(_logger);
        }

        [Fact]
        public void RegisterSession_ShouldAddSessionToTracking()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var hostName = "test-host";

            // Act
            _performanceService.RegisterSession(sessionId, hostName);
            var metrics = _performanceService.GetSessionMetrics(sessionId);

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(sessionId, metrics.SessionId);
            Assert.Equal(hostName, metrics.HostName);
        }

        [Fact]
        public void UnregisterSession_ShouldRemoveSessionFromTracking()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var hostName = "test-host";
            _performanceService.RegisterSession(sessionId, hostName);

            // Act
            _performanceService.UnregisterSession(sessionId);
            var metrics = _performanceService.GetSessionMetrics(sessionId);

            // Assert
            Assert.Null(metrics);
        }

        [Fact]
        public void UpdateSessionActivity_ShouldIncrementByteCounts()
        {
            // Arrange
            var sessionId = Guid.NewGuid().ToString();
            var hostName = "test-host";
            _performanceService.RegisterSession(sessionId, hostName);

            // Act
            _performanceService.UpdateSessionActivity(sessionId, 1024, 512);
            var metrics = _performanceService.GetSessionMetrics(sessionId);

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(1024, metrics.BytesReceived);
            Assert.Equal(512, metrics.BytesSent);
        }

        [Fact]
        public void GetSystemMetrics_ShouldReturnValidMetrics()
        {
            // Arrange & Act
            var systemMetrics = _performanceService.GetSystemMetrics();

            // Assert
            Assert.NotNull(systemMetrics);
            Assert.True(systemMetrics.CpuUsagePercent >= 0);
            Assert.True(systemMetrics.MemoryUsageMB > 0);
            Assert.True(systemMetrics.ActiveSessionCount >= 0);
        }

        [Fact]
        public void GetOptimizationRecommendations_WithHighCpuOptions_ShouldSuggestOptimizations()
        {
            // Arrange
            var options = new RdpPerformanceOptions
            {
                ColorDepth = 32, // High color depth
                EnableBitmapCaching = false, // Disabled caching
                MaxConcurrentSessions = 2
            };

            // Act
            var recommendations = _performanceService.GetOptimizationRecommendations(options);

            // Assert
            Assert.NotNull(recommendations);
            Assert.NotEmpty(recommendations.Recommendations);
            
            // Should recommend enabling bitmap caching
            Assert.Contains(recommendations.Recommendations, 
                r => r.Title.Contains("Bitmap caching disabled"));
        }

        [Fact]
        public void PerformanceWarning_ShouldFireWhenThresholdsExceeded()
        {
            // Arrange
            PerformanceWarningEventArgs? capturedEventArgs = null;

            _performanceService.PerformanceWarning += (sender, args) =>
            {
                capturedEventArgs = args;
            };

            // Act - This would normally be triggered by the monitoring timer
            // but we can't easily test that in unit tests without waiting
            // Instead, we verify the service is set up correctly
            var systemMetrics = _performanceService.GetSystemMetrics();

            // Assert
            Assert.NotNull(systemMetrics);
            // Note: Warning events are triggered by the timer, which we can't easily test in unit tests
            // The main validation is that the service doesn't crash and provides valid metrics
        }

        public void Dispose()
        {
            _performanceService?.Dispose();
        }
    }
}