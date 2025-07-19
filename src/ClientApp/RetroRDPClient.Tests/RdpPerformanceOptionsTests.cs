using RetroRDP.Shared.Models;
using Xunit;

namespace RetroRDPClient.Tests
{
    /// <summary>
    /// Unit tests for RdpPerformanceOptions
    /// </summary>
    public class RdpPerformanceOptionsTests
    {
        [Fact]
        public void GetDefault_ShouldReturnBalancedPreset()
        {
            // Act
            var options = RdpPerformanceOptions.GetDefault();

            // Assert
            Assert.Equal(PerformancePreset.Balanced, options.Preset);
            Assert.True(options.EnableBitmapCaching);
            Assert.True(options.EnableCompression);
            Assert.Equal(16, options.ColorDepth);
            Assert.Equal(15, options.ScreenUpdateFrequency);
        }

        [Fact]
        public void ApplyPreset_Performance_ShouldOptimizeForSpeed()
        {
            // Arrange
            var options = new RdpPerformanceOptions();

            // Act
            options.ApplyPreset(PerformancePreset.Performance);

            // Assert
            Assert.Equal(PerformancePreset.Performance, options.Preset);
            Assert.Equal(16, options.ColorDepth);
            Assert.Equal(10, options.ScreenUpdateFrequency);
            Assert.Equal(0, options.AudioQuality); // No audio
            Assert.False(options.EnableDesktopBackground);
            Assert.False(options.EnableVisualEffects);
            Assert.False(options.EnableThemes);
            Assert.True(options.EnableCompression);
            Assert.True(options.EnableBitmapCaching);
        }

        [Fact]
        public void ApplyPreset_Quality_ShouldOptimizeForVisuals()
        {
            // Arrange
            var options = new RdpPerformanceOptions();

            // Act
            options.ApplyPreset(PerformancePreset.Quality);

            // Assert
            Assert.Equal(PerformancePreset.Quality, options.Preset);
            Assert.Equal(32, options.ColorDepth);
            Assert.Equal(30, options.ScreenUpdateFrequency);
            Assert.Equal(3, options.AudioQuality); // High quality
            Assert.True(options.EnableDesktopBackground);
            Assert.True(options.EnableVisualEffects);
            Assert.True(options.EnableThemes);
            Assert.False(options.EnableCompression);
            Assert.True(options.EnableBitmapCaching);
        }

        [Fact]
        public void ApplyPreset_Balanced_ShouldBalancePerformanceAndQuality()
        {
            // Arrange
            var options = new RdpPerformanceOptions();

            // Act
            options.ApplyPreset(PerformancePreset.Balanced);

            // Assert
            Assert.Equal(PerformancePreset.Balanced, options.Preset);
            Assert.Equal(16, options.ColorDepth);
            Assert.Equal(15, options.ScreenUpdateFrequency);
            Assert.Equal(1, options.AudioQuality); // Low quality
            Assert.False(options.EnableDesktopBackground);
            Assert.False(options.EnableVisualEffects);
            Assert.False(options.EnableThemes);
            Assert.True(options.EnableCompression);
            Assert.True(options.EnableBitmapCaching);
        }

        [Fact]
        public void Clone_ShouldCreateExactCopy()
        {
            // Arrange
            var original = new RdpPerformanceOptions();
            original.ApplyPreset(PerformancePreset.Quality);
            original.ScreenWidth = 2560;
            original.ScreenHeight = 1440;
            original.MaxConcurrentSessions = 10;

            // Act
            var clone = original.Clone();

            // Assert
            Assert.Equal(original.Preset, clone.Preset);
            Assert.Equal(original.ColorDepth, clone.ColorDepth);
            Assert.Equal(original.ScreenUpdateFrequency, clone.ScreenUpdateFrequency);
            Assert.Equal(original.AudioQuality, clone.AudioQuality);
            Assert.Equal(original.EnableDesktopBackground, clone.EnableDesktopBackground);
            Assert.Equal(original.EnableVisualEffects, clone.EnableVisualEffects);
            Assert.Equal(original.EnableThemes, clone.EnableThemes);
            Assert.Equal(original.EnableCompression, clone.EnableCompression);
            Assert.Equal(original.EnableBitmapCaching, clone.EnableBitmapCaching);
            Assert.Equal(original.ScreenWidth, clone.ScreenWidth);
            Assert.Equal(original.ScreenHeight, clone.ScreenHeight);
            Assert.Equal(original.MaxConcurrentSessions, clone.MaxConcurrentSessions);
        }

        [Theory]
        [InlineData(8)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(24)]
        [InlineData(32)]
        public void ColorDepth_ValidValues_ShouldBeSupported(int colorDepth)
        {
            // Arrange
            var options = new RdpPerformanceOptions();

            // Act
            options.ColorDepth = colorDepth;

            // Assert
            Assert.Equal(colorDepth, options.ColorDepth);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(15)]
        [InlineData(30)]
        public void ScreenUpdateFrequency_ValidRange_ShouldBeAccepted(int frequency)
        {
            // Arrange
            var options = new RdpPerformanceOptions();

            // Act
            options.ScreenUpdateFrequency = frequency;

            // Assert
            Assert.Equal(frequency, options.ScreenUpdateFrequency);
        }

        [Theory]
        [InlineData(0)] // No audio
        [InlineData(1)] // Low quality
        [InlineData(2)] // Medium quality
        [InlineData(3)] // High quality
        public void AudioQuality_ValidLevels_ShouldBeSupported(int audioQuality)
        {
            // Arrange
            var options = new RdpPerformanceOptions();

            // Act
            options.AudioQuality = audioQuality;

            // Assert
            Assert.Equal(audioQuality, options.AudioQuality);
        }
    }
}