using System.Text.Json.Serialization;

namespace RetroRDP.MCPServer.Models
{
    /// <summary>
    /// MCP (Model Context Protocol) message types
    /// </summary>
    public enum MCPMessageType
    {
        Initialize,
        Tools,
        Resources,
        Call,
        Result,
        Error,
        Progress,
        Notification
    }

    /// <summary>
    /// Base MCP message structure
    /// </summary>
    public abstract class MCPMessage
    {
        [JsonPropertyName("jsonrpc")]
        public string JsonRpc { get; set; } = "2.0";

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("method")]
        public abstract string Method { get; }
    }

    /// <summary>
    /// MCP request message
    /// </summary>
    public abstract class MCPRequest : MCPMessage
    {
        [JsonPropertyName("params")]
        public object? Params { get; set; }
    }

    /// <summary>
    /// MCP response message
    /// </summary>
    public abstract class MCPResponse : MCPMessage
    {
        [JsonPropertyName("result")]
        public object? Result { get; set; }

        [JsonPropertyName("error")]
        public MCPError? Error { get; set; }

        public override string Method => "response";
    }

    /// <summary>
    /// MCP error details
    /// </summary>
    public class MCPError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }
    }

    /// <summary>
    /// MCP tool definition
    /// </summary>
    public class MCPTool
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("inputSchema")]
        public MCPSchema InputSchema { get; set; } = new();
    }

    /// <summary>
    /// JSON schema definition for tool parameters
    /// </summary>
    public class MCPSchema
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "object";

        [JsonPropertyName("properties")]
        public Dictionary<string, MCPSchemaProperty> Properties { get; set; } = new();

        [JsonPropertyName("required")]
        public List<string> Required { get; set; } = new();
    }

    /// <summary>
    /// Schema property definition
    /// </summary>
    public class MCPSchemaProperty
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "string";

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("enum")]
        public List<string>? Enum { get; set; }

        [JsonPropertyName("minimum")]
        public int? Minimum { get; set; }

        [JsonPropertyName("maximum")]
        public int? Maximum { get; set; }
    }

    /// <summary>
    /// MCP resource definition
    /// </summary>
    public class MCPResource
    {
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
    }

    /// <summary>
    /// MCP initialization request
    /// </summary>
    public class MCPInitializeRequest : MCPRequest
    {
        public override string Method => "initialize";

        public class InitializeParams
        {
            [JsonPropertyName("protocolVersion")]
            public string ProtocolVersion { get; set; } = "2024-11-05";

            [JsonPropertyName("clientInfo")]
            public MCPClientInfo ClientInfo { get; set; } = new();

            [JsonPropertyName("capabilities")]
            public MCPCapabilities Capabilities { get; set; } = new();
        }
    }

    /// <summary>
    /// Client information
    /// </summary>
    public class MCPClientInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }

    /// <summary>
    /// MCP capabilities
    /// </summary>
    public class MCPCapabilities
    {
        [JsonPropertyName("tools")]
        public bool Tools { get; set; } = true;

        [JsonPropertyName("resources")]
        public bool Resources { get; set; } = true;

        [JsonPropertyName("prompts")]
        public bool Prompts { get; set; } = true;

        [JsonPropertyName("logging")]
        public bool Logging { get; set; } = true;
    }

    /// <summary>
    /// Tool call request
    /// </summary>
    public class MCPToolCallRequest : MCPRequest
    {
        public override string Method => "tools/call";

        public class ToolCallParams
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("arguments")]
            public Dictionary<string, object> Arguments { get; set; } = new();
        }
    }

    /// <summary>
    /// Tool call result
    /// </summary>
    public class MCPToolCallResult
    {
        [JsonPropertyName("content")]
        public List<MCPContent> Content { get; set; } = new();

        [JsonPropertyName("isError")]
        public bool IsError { get; set; } = false;
    }

    /// <summary>
    /// MCP content block
    /// </summary>
    public class MCPContent
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }

        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }
    }
}