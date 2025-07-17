using RetroRDP.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RetroRDP.Shared.Services
{
    /// <summary>
    /// Interface for managing RDP sessions
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Event fired when a session's status changes
        /// </summary>
        event EventHandler<SessionStatusChangedEventArgs>? SessionStatusChanged;

        /// <summary>
        /// Gets all active sessions
        /// </summary>
        IReadOnlyList<RdpSession> ActiveSessions { get; }

        /// <summary>
        /// Gets a session by its ID
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <returns>The session if found, null otherwise</returns>
        RdpSession? GetSession(string sessionId);

        /// <summary>
        /// Starts a new RDP session with the specified connection parameters
        /// </summary>
        /// <param name="connectionRequest">The connection parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The session ID if successful, null if failed</returns>
        Task<string?> StartSessionAsync(RdpConnectionRequest connectionRequest, CancellationToken cancellationToken = default);

        /// <summary>
        /// Ends an active RDP session
        /// </summary>
        /// <param name="sessionId">The session ID to end</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> EndSessionAsync(string sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reconnects to an existing session
        /// </summary>
        /// <param name="sessionId">The session ID to reconnect</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ReconnectSessionAsync(string sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnects all active sessions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if all sessions were disconnected successfully</returns>
        Task<bool> DisconnectAllSessionsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates the status of a session
        /// </summary>
        /// <param name="sessionId">The session ID</param>
        /// <param name="status">The new status</param>
        /// <param name="errorMessage">Optional error message</param>
        void UpdateSessionStatus(string sessionId, RdpSessionStatus status, string? errorMessage = null);
    }
}