using System;

namespace RetroRDP.Shared.Models
{
    /// <summary>
    /// Configuration options for RDP session performance optimization
    /// </summary>
    public class RdpPerformanceOptions
    {
        /// <summary>
        /// Enable bitmap caching for improved performance
        /// </summary>
        public bool EnableBitmapCaching { get; set; } = true;

        /// <summary>
        /// Enable persistent bitmap caching
        /// </summary>
        public bool EnablePersistentBitmapCaching { get; set; } = true;

        /// <summary>
        /// Screen update frequency (higher = more responsive, lower = better performance)
        /// Range: 1-30 FPS
        /// </summary>
        public int ScreenUpdateFrequency { get; set; } = 15;

        /// <summary>
        /// Color depth (lower = better performance)
        /// Valid values: 8, 15, 16, 24, 32
        /// </summary>
        public int ColorDepth { get; set; } = 16;

        /// <summary>
        /// Screen resolution width
        /// </summary>
        public int ScreenWidth { get; set; } = 1920;

        /// <summary>
        /// Screen resolution height
        /// </summary>
        public int ScreenHeight { get; set; } = 1080;

        /// <summary>
        /// Enable compression to reduce bandwidth
        /// </summary>
        public bool EnableCompression { get; set; } = true;

        /// <summary>
        /// Audio quality: None=0, Low=1, Medium=2, High=3
        /// </summary>
        public int AudioQuality { get; set; } = 1; // Low quality for better performance

        /// <summary>
        /// Enable desktop wallpaper (disable for better performance)
        /// </summary>
        public bool EnableDesktopBackground { get; set; } = false;

        /// <summary>
        /// Enable visual effects (disable for better performance)
        /// </summary>
        public bool EnableVisualEffects { get; set; } = false;

        /// <summary>
        /// Enable themes (disable for better performance)
        /// </summary>
        public bool EnableThemes { get; set; } = false;

        /// <summary>
        /// Maximum number of concurrent sessions for performance control
        /// </summary>
        public int MaxConcurrentSessions { get; set; } = 5;

        /// <summary>
        /// CPU usage warning threshold (percentage)
        /// </summary>
        public int CpuWarningThreshold { get; set; } = 80;

        /// <summary>
        /// Memory usage warning threshold (MB)
        /// </summary>
        public long MemoryWarningThreshold { get; set; } = 2048; // 2GB

        /// <summary>
        /// Performance preset: Balanced, Performance, Quality
        /// </summary>
        public PerformancePreset Preset { get; set; } = PerformancePreset.Balanced;

        /// <summary>
        /// Apply a performance preset
        /// </summary>
        public void ApplyPreset(PerformancePreset preset)
        {
            Preset = preset;
            
            switch (preset)
            {
                case PerformancePreset.Performance:
                    // Optimize for maximum performance
                    ColorDepth = 16;
                    ScreenUpdateFrequency = 10;
                    AudioQuality = 0; // No audio
                    EnableDesktopBackground = false;
                    EnableVisualEffects = false;
                    EnableThemes = false;
                    EnableCompression = true;
                    EnableBitmapCaching = true;
                    EnablePersistentBitmapCaching = true;
                    break;

                case PerformancePreset.Quality:
                    // Optimize for maximum quality
                    ColorDepth = 32;
                    ScreenUpdateFrequency = 30;
                    AudioQuality = 3; // High quality
                    EnableDesktopBackground = true;
                    EnableVisualEffects = true;
                    EnableThemes = true;
                    EnableCompression = false;
                    EnableBitmapCaching = true;
                    EnablePersistentBitmapCaching = true;
                    break;

                case PerformancePreset.Balanced:
                default:
                    // Balance between performance and quality
                    ColorDepth = 16;
                    ScreenUpdateFrequency = 15;
                    AudioQuality = 1; // Low quality
                    EnableDesktopBackground = false;
                    EnableVisualEffects = false;
                    EnableThemes = false;
                    EnableCompression = true;
                    EnableBitmapCaching = true;
                    EnablePersistentBitmapCaching = true;
                    break;
            }
        }

        /// <summary>
        /// Get a copy of the default performance options
        /// </summary>
        public static RdpPerformanceOptions GetDefault()
        {
            var options = new RdpPerformanceOptions();
            options.ApplyPreset(PerformancePreset.Balanced);
            return options;
        }

        /// <summary>
        /// Clone the current options
        /// </summary>
        public RdpPerformanceOptions Clone()
        {
            return new RdpPerformanceOptions
            {
                EnableBitmapCaching = EnableBitmapCaching,
                EnablePersistentBitmapCaching = EnablePersistentBitmapCaching,
                ScreenUpdateFrequency = ScreenUpdateFrequency,
                ColorDepth = ColorDepth,
                ScreenWidth = ScreenWidth,
                ScreenHeight = ScreenHeight,
                EnableCompression = EnableCompression,
                AudioQuality = AudioQuality,
                EnableDesktopBackground = EnableDesktopBackground,
                EnableVisualEffects = EnableVisualEffects,
                EnableThemes = EnableThemes,
                MaxConcurrentSessions = MaxConcurrentSessions,
                CpuWarningThreshold = CpuWarningThreshold,
                MemoryWarningThreshold = MemoryWarningThreshold,
                Preset = Preset
            };
        }
    }

    /// <summary>
    /// Performance preset options
    /// </summary>
    public enum PerformancePreset
    {
        /// <summary>
        /// Optimize for maximum performance over quality
        /// </summary>
        Performance,

        /// <summary>
        /// Balance between performance and quality
        /// </summary>
        Balanced,

        /// <summary>
        /// Optimize for maximum quality over performance
        /// </summary>
        Quality
    }
}