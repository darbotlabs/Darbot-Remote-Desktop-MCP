using Microsoft.AspNetCore.Mvc;
using RetroRDP.MCPServer.Models;
using RetroRDP.MCPServer.Services;
using System.Text.Json;

namespace RetroRDP.MCPServer.Controllers
{
    /// <summary>
    /// MCP (Model Context Protocol) endpoint controller
    /// Handles JSON-RPC 2.0 requests from AI assistants
    /// </summary>
    [ApiController]
    [Route("mcp")]
    public class MCPController : ControllerBase
    {
        private readonly IMCPServer _mcpServer;
        private readonly ILogger<MCPController> _logger;

        public MCPController(IMCPServer mcpServer, ILogger<MCPController> logger)
        {
            _mcpServer = mcpServer;
            _logger = logger;
        }

        /// <summary>
        /// Handle MCP JSON-RPC 2.0 requests
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> HandleRequest([FromBody] JsonElement requestElement)
        {
            try
            {
                _logger.LogDebug("Received MCP request: {Request}", requestElement.GetRawText());

                // Parse the JSON-RPC request
                var request = ParseMCPRequest(requestElement);
                if (request == null)
                {
                    return BadRequest(new MCPErrorResponse
                    {
                        Error = new MCPError
                        {
                            Code = -32700,
                            Message = "Parse error - Invalid JSON-RPC request"
                        }
                    });
                }

                // Handle the request through the MCP server
                var response = await _mcpServer.HandleRequestAsync(request);
                
                _logger.LogDebug("Sending MCP response: {Response}", JsonSerializer.Serialize(response));
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MCP request");
                return StatusCode(500, new MCPErrorResponse
                {
                    Error = new MCPError
                    {
                        Code = -32603,
                        Message = "Internal error",
                        Data = ex.Message
                    }
                });
            }
        }

        /// <summary>
        /// Get server capabilities and available tools
        /// </summary>
        [HttpGet("capabilities")]
        public IActionResult GetCapabilities()
        {
            var tools = _mcpServer.GetAvailableTools();
            var resources = _mcpServer.GetAvailableResources();

            return Ok(new
            {
                protocolVersion = "2024-11-05",
                serverInfo = new
                {
                    name = "RetroRDP MCP Server",
                    version = "1.0.0",
                    description = "MCP server for RetroRDP client with H.264/H.265 streaming and performance optimization"
                },
                capabilities = new
                {
                    tools = true,
                    resources = true,
                    prompts = false,
                    logging = true
                },
                tools = tools,
                resources = resources
            });
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }

        private MCPRequest? ParseMCPRequest(JsonElement element)
        {
            try
            {
                if (!element.TryGetProperty("method", out var methodElement))
                {
                    return null;
                }

                var method = methodElement.GetString();
                if (string.IsNullOrEmpty(method))
                {
                    return null;
                }

                // Get ID if present
                string? id = null;
                if (element.TryGetProperty("id", out var idElement))
                {
                    id = idElement.GetString();
                }

                // Get params if present
                object? parameters = null;
                if (element.TryGetProperty("params", out var paramsElement))
                {
                    parameters = paramsElement;
                }

                // Create appropriate request type based on method
                return method switch
                {
                    "initialize" => new MCPInitializeRequest { Id = id, Params = parameters },
                    "tools/list" => new MCPGenericRequest { Id = id, Params = parameters, MethodName = method },
                    "tools/call" => new MCPToolCallRequest { Id = id, Params = parameters },
                    "resources/list" => new MCPGenericRequest { Id = id, Params = parameters, MethodName = method },
                    "resources/read" => new MCPGenericRequest { Id = id, Params = parameters, MethodName = method },
                    _ => new MCPGenericRequest { Id = id, Params = parameters, MethodName = method }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing MCP request");
                return null;
            }
        }
    }

    /// <summary>
    /// Generic MCP request for methods we don't have specific implementations for
    /// </summary>
    public class MCPGenericRequest : MCPRequest
    {
        public string MethodName { get; set; } = string.Empty;
        public override string Method => MethodName;
    }
}