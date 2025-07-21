using RetroRDP.MCPServer.Models;
using RetroRDP.MCPServer.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RetroRDP.MCPServer.Services
{
    /// <summary>
    /// Main MCP server implementation
    /// </summary>
    public class MCPServer : IMCPServer
    {
        private readonly ILogger<MCPServer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<IMCPTool> _tools;
        private readonly List<IMCPResourceProvider> _resourceProviders;
        private bool _initialized = false;

        public MCPServer(ILogger<MCPServer> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _tools = new List<IMCPTool>();
            _resourceProviders = new List<IMCPResourceProvider>();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting MCP server...");
            
            // Register tools
            RegisterTools();
            
            // Register resource providers
            RegisterResourceProviders();
            
            _logger.LogInformation("MCP server started successfully with {ToolCount} tools and {ResourceCount} resource providers", 
                _tools.Count, _resourceProviders.Count);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Stopping MCP server...");
            // Cleanup if needed
            _logger.LogInformation("MCP server stopped");
            return Task.CompletedTask;
        }

        private void RegisterTools()
        {
            // Get all IMCPTool implementations from DI container
            var toolServices = _serviceProvider.GetServices<IMCPTool>();
            _tools.AddRange(toolServices);
            
            _logger.LogInformation("Registered {Count} MCP tools: {Tools}", 
                _tools.Count, string.Join(", ", _tools.Select(t => t.Name)));
        }

        private void RegisterResourceProviders()
        {
            var resourceProviders = _serviceProvider.GetServices<IMCPResourceProvider>();
            _resourceProviders.AddRange(resourceProviders);
            
            _logger.LogInformation("Registered {Count} resource providers", _resourceProviders.Count);
        }

        public List<MCPTool> GetAvailableTools()
        {
            return _tools.Select(t => new MCPTool
            {
                Name = t.Name,
                Description = t.Description,
                InputSchema = t.InputSchema
            }).ToList();
        }

        public List<MCPResource> GetAvailableResources()
        {
            var resources = new List<MCPResource>();
            foreach (var provider in _resourceProviders)
            {
                resources.AddRange(provider.GetResources());
            }
            return resources;
        }

        public async Task<MCPResponse> HandleRequestAsync(MCPRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Handling MCP request: {Method}", request.Method);

                return request.Method switch
                {
                    "initialize" => await HandleInitializeAsync(request, cancellationToken),
                    "tools/list" => HandleToolsList(request),
                    "tools/call" => await HandleToolCallAsync(request, cancellationToken),
                    "resources/list" => HandleResourcesList(request),
                    "resources/read" => await HandleResourceReadAsync(request, cancellationToken),
                    _ => new MCPErrorResponse
                    {
                        Id = request.Id,
                        Error = new MCPError
                        {
                            Code = -32601,
                            Message = $"Method not found: {request.Method}"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MCP request: {Method}", request.Method);
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError
                    {
                        Code = -32603,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                };
            }
        }

        private Task<MCPResponse> HandleInitializeAsync(MCPRequest request, CancellationToken cancellationToken)
        {
            _initialized = true;
            
            return Task.FromResult<MCPResponse>(new MCPInitializeResponse
            {
                Id = request.Id,
                Result = new MCPInitializeResponse.InitializeResult
                {
                    ProtocolVersion = "2024-11-05",
                    ServerInfo = new MCPServerInfo
                    {
                        Name = "RetroRDP MCP Server",
                        Version = "1.0.0"
                    },
                    Capabilities = new MCPCapabilities
                    {
                        Tools = true,
                        Resources = true,
                        Prompts = false,
                        Logging = true
                    }
                }
            });
        }

        private MCPResponse HandleToolsList(MCPRequest request)
        {
            if (!_initialized)
            {
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError { Code = -32002, Message = "Server not initialized" }
                };
            }

            return new MCPToolsListResponse
            {
                Id = request.Id,
                Result = new MCPToolsListResponse.ToolsListResult
                {
                    Tools = GetAvailableTools()
                }
            };
        }

        private async Task<MCPResponse> HandleToolCallAsync(MCPRequest request, CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError { Code = -32002, Message = "Server not initialized" }
                };
            }

            if (request.Params is not JsonElement paramsElement)
            {
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError { Code = -32602, Message = "Invalid params" }
                };
            }

            var toolCallParams = JsonSerializer.Deserialize<MCPToolCallRequest.ToolCallParams>(paramsElement.GetRawText());
            if (toolCallParams == null)
            {
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError { Code = -32602, Message = "Invalid tool call params" }
                };
            }

            var tool = _tools.FirstOrDefault(t => t.Name == toolCallParams.Name);
            if (tool == null)
            {
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError { Code = -32601, Message = $"Tool not found: {toolCallParams.Name}" }
                };
            }

            try
            {
                var result = await tool.ExecuteAsync(toolCallParams.Arguments, cancellationToken);
                
                return new MCPToolCallResponse
                {
                    Id = request.Id,
                    Result = result
                };
            }
            catch (Exception ex)
            {
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError
                    {
                        Code = -32603,
                        Message = "Tool execution error",
                        Data = ex.Message
                    }
                };
            }
        }

        private MCPResponse HandleResourcesList(MCPRequest request)
        {
            if (!_initialized)
            {
                return new MCPErrorResponse
                {
                    Id = request.Id,
                    Error = new MCPError { Code = -32002, Message = "Server not initialized" }
                };
            }

            return new MCPResourcesListResponse
            {
                Id = request.Id,
                Result = new MCPResourcesListResponse.ResourcesListResult
                {
                    Resources = GetAvailableResources()
                }
            };
        }

        private Task<MCPResponse> HandleResourceReadAsync(MCPRequest request, CancellationToken cancellationToken)
        {
            // Implementation for reading resources
            return Task.FromResult<MCPResponse>(new MCPErrorResponse
            {
                Id = request.Id,
                Error = new MCPError { Code = -32601, Message = "Resource read not implemented yet" }
            });
        }
    }

    // Response implementations
    public class MCPInitializeResponse : MCPResponse
    {
        public class InitializeResult
        {
            [JsonPropertyName("protocolVersion")]
            public string ProtocolVersion { get; set; } = string.Empty;

            [JsonPropertyName("serverInfo")]
            public MCPServerInfo ServerInfo { get; set; } = new();

            [JsonPropertyName("capabilities")]
            public MCPCapabilities Capabilities { get; set; } = new();
        }
    }

    public class MCPServerInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
    }

    public class MCPToolsListResponse : MCPResponse
    {
        public class ToolsListResult
        {
            [JsonPropertyName("tools")]
            public List<MCPTool> Tools { get; set; } = new();
        }
    }

    public class MCPToolCallResponse : MCPResponse
    {
        // Result is already defined in base class
    }

    public class MCPResourcesListResponse : MCPResponse
    {
        public class ResourcesListResult
        {
            [JsonPropertyName("resources")]
            public List<MCPResource> Resources { get; set; } = new();
        }
    }

    public class MCPErrorResponse : MCPResponse
    {
        // Error is already defined in base class
    }
}