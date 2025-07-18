using System;

namespace RetroRDP.Shared.Models
{
    /// <summary>
    /// Represents the status of an RDP session
    /// </summary>
    public enum RdpSessionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Disconnecting,
        Failed
    }

    /// <summary>
    /// Represents an RDP session configuration and state
    /// </summary>
    public class RdpSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string SessionName { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public int Port { get; set; } = 3389;
        public RdpSessionStatus Status { get; set; } = RdpSessionStatus.Disconnected;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ConnectedAt { get; set; }
        public DateTime? LastActivity { get; set; }
        public string? ErrorMessage { get; set; }
        public int ScreenWidth { get; set; } = 1920;
        public int ScreenHeight { get; set; } = 1080;
        public bool FullScreen { get; set; } = false;
        public int ColorDepth { get; set; } = 32;
        
        /// <summary>
        /// Performance optimization options for this session
        /// </summary>
        public RdpPerformanceOptions PerformanceOptions { get; set; } = RdpPerformanceOptions.GetDefault();
        
        /// <summary>
        /// Gets a display name for the session tab
        /// </summary>
        public string DisplayName => !string.IsNullOrEmpty(SessionName) 
            ? SessionName 
            : !string.IsNullOrEmpty(Host) 
                ? Host 
                : "New Session";

        /// <summary>
        /// Gets the status display text with appropriate emoji
        /// </summary>
        public string StatusDisplay => Status switch
        {
            RdpSessionStatus.Connected => "🟢 Connected",
            RdpSessionStatus.Connecting => "🟡 Connecting...",
            RdpSessionStatus.Reconnecting => "🟡 Reconnecting...",
            RdpSessionStatus.Disconnecting => "🟡 Disconnecting...",
            RdpSessionStatus.Failed => "🔴 Failed",
            _ => "⚪ Disconnected"
        };
    }

    /// <summary>
    /// Represents connection parameters for creating a new RDP session
    /// </summary>
    public class RdpConnectionRequest
    {
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public int Port { get; set; } = 3389;
        public int ScreenWidth { get; set; } = 1920;
        public int ScreenHeight { get; set; } = 1080;
        public bool FullScreen { get; set; } = false;
        public int ColorDepth { get; set; } = 32;
    }

    /// <summary>
    /// Event arguments for session status changes
    /// </summary>
    public class SessionStatusChangedEventArgs : EventArgs
    {
        public string SessionId { get; set; } = string.Empty;
        public RdpSessionStatus OldStatus { get; set; }
        public RdpSessionStatus NewStatus { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
