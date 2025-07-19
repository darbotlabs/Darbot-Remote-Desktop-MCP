using RetroRDP.MCPServer.Models;

namespace RetroRDP.MCPServer.Services
{
    /// <summary>
    /// Interface for MCP tool implementations
    /// </summary>
    public interface IMCPTool
    {
        string Name { get; }
        string Description { get; }
        MCPSchema InputSchema { get; }
        Task<MCPToolCallResult> ExecuteAsync(Dictionary<string, object> arguments, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for MCP server operations
    /// </summary>
    public interface IMCPServer
    {
        Task<MCPResponse> HandleRequestAsync(MCPRequest request, CancellationToken cancellationToken = default);
        List<MCPTool> GetAvailableTools();
        List<MCPResource> GetAvailableResources();
        Task StartAsync(CancellationToken cancellationToken = default);
        Task StopAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for MCP resource providers
    /// </summary>
    public interface IMCPResourceProvider
    {
        List<MCPResource> GetResources();
        Task<MCPContent> GetResourceAsync(string uri, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Interface for local connector to RDP client
    /// </summary>
    public interface ILocalConnector
    {
        Task<bool> IsClientAvailableAsync();
        Task<List<object>> GetActiveSessionsAsync();
        Task<object> CreateSessionAsync(string host, string username, string? password = null, Dictionary<string, object>? options = null);
        Task<bool> DisconnectSessionAsync(string sessionId);
        Task<string> CaptureScreenshotAsync(string sessionId, string mode = "session");
        Task<Dictionary<string, object>> GetPerformanceMetricsAsync(string sessionId);
        Task<bool> ApplyPerformanceSettingsAsync(string sessionId, Dictionary<string, object> settings);
    }
}