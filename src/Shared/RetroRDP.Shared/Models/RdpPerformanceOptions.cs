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
        /// Maximum bitrate for video streaming (kbps)
        /// Higher values provide better quality but require more bandwidth
        /// Range: 256 - 50000 kbps
        /// </summary>
        public int MaxBitrate { get; set; } = 4000; // 4 Mbps

        /// <summary>
        /// Minimum bitrate for video streaming (kbps)
        /// Ensures minimum quality level
        /// Range: 128 - 2000 kbps
        /// </summary>
        public int MinBitrate { get; set; } = 512; // 512 kbps

        /// <summary>
        /// Video codec for streaming
        /// </summary>
        public VideoCodec VideoCodec { get; set; } = VideoCodec.H264;

        /// <summary>
        /// Enable adaptive bitrate based on network conditions
        /// </summary>
        public bool EnableAdaptiveBitrate { get; set; } = true;

        /// <summary>
        /// Network latency optimization mode
        /// </summary>
        public NetworkOptimization NetworkMode { get; set; } = NetworkOptimization.Adaptive;

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
                    MaxBitrate = 2000; // Lower bitrate for performance
                    MinBitrate = 256;
                    VideoCodec = VideoCodec.H264; // H.264 for compatibility
                    EnableAdaptiveBitrate = true;
                    NetworkMode = NetworkOptimization.LowLatency;
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
                    MaxBitrate = 15000; // Higher bitrate for quality
                    MinBitrate = 1000;
                    VideoCodec = VideoCodec.H265; // H.265 for better quality
                    EnableAdaptiveBitrate = false; // Fixed high quality
                    NetworkMode = NetworkOptimization.HighBandwidth;
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
                    MaxBitrate = 4000; // Balanced bitrate
                    MinBitrate = 512;
                    VideoCodec = VideoCodec.H264; // H.264 for good compatibility
                    EnableAdaptiveBitrate = true;
                    NetworkMode = NetworkOptimization.Adaptive;
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
                Preset = Preset,
                MaxBitrate = MaxBitrate,
                MinBitrate = MinBitrate,
                VideoCodec = VideoCodec,
                EnableAdaptiveBitrate = EnableAdaptiveBitrate,
                NetworkMode = NetworkMode
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

    /// <summary>
    /// Video codec options for RDP streaming
    /// </summary>
    public enum VideoCodec
    {
        /// <summary>
        /// H.264/AVC codec - Good balance of quality and compatibility
        /// </summary>
        H264,

        /// <summary>
        /// H.265/HEVC codec - Better compression but requires more CPU
        /// </summary>
        H265,

        /// <summary>
        /// Legacy uncompressed mode - Maximum compatibility, highest bandwidth
        /// </summary>
        Uncompressed,

        /// <summary>
        /// Automatic selection based on client capabilities
        /// </summary>
        Auto
    }

    /// <summary>
    /// Network optimization strategies
    /// </summary>
    public enum NetworkOptimization
    {
        /// <summary>
        /// Optimize for low latency connections (gaming, real-time)
        /// </summary>
        LowLatency,

        /// <summary>
        /// Adaptive optimization based on network conditions
        /// </summary>
        Adaptive,

        /// <summary>
        /// Optimize for high bandwidth, stable connections
        /// </summary>
        HighBandwidth,

        /// <summary>
        /// Optimize for low bandwidth, unstable connections
        /// </summary>
        LowBandwidth
    }
}