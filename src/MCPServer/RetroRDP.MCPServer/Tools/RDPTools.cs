using RetroRDP.MCPServer.Models;
using RetroRDP.MCPServer.Services;
using System.Text.Json;

namespace RetroRDP.MCPServer.Tools
{
    /// <summary>
    /// MCP tool for connecting to remote desktop sessions
    /// </summary>
    public class ConnectRDPTool : IMCPTool
    {
        private readonly ILocalConnector _connector;
        private readonly ILogger<ConnectRDPTool> _logger;

        public string Name => "connect_rdp";
        
        public string Description => "Connect to a remote desktop server using RDP protocol with configurable video codec, bitrate, and performance settings";

        public MCPSchema InputSchema => new()
        {
            Type = "object",
            Properties = new Dictionary<string, MCPSchemaProperty>
            {
                ["host"] = new() { Type = "string", Description = "Remote desktop server hostname or IP address" },
                ["username"] = new() { Type = "string", Description = "Username for authentication" },
                ["password"] = new() { Type = "string", Description = "Password for authentication (optional, can prompt if needed)" },
                ["port"] = new() { Type = "integer", Description = "RDP port (default: 3389)", Minimum = 1, Maximum = 65535 },
                ["width"] = new() { Type = "integer", Description = "Screen resolution width (default: 1920)", Minimum = 800, Maximum = 7680 },
                ["height"] = new() { Type = "integer", Description = "Screen resolution height (default: 1080)", Minimum = 600, Maximum = 4320 },
                ["colorDepth"] = new() { Type = "integer", Description = "Color depth in bits (8, 15, 16, 24, 32)", Enum = new() { "8", "15", "16", "24", "32" } },
                ["fullScreen"] = new() { Type = "boolean", Description = "Use full screen mode (default: false)" },
                ["sessionName"] = new() { Type = "string", Description = "Custom name for the session tab" },
                ["codec"] = new() { Type = "string", Description = "Video codec (H264, H265, Auto)", Enum = new() { "H264", "H265", "Auto" } },
                ["maxBitrate"] = new() { Type = "integer", Description = "Maximum bitrate in kbps (256-50000)", Minimum = 256, Maximum = 50000 },
                ["minBitrate"] = new() { Type = "integer", Description = "Minimum bitrate in kbps (128-2000)", Minimum = 128, Maximum = 2000 },
                ["preset"] = new() { Type = "string", Description = "Performance preset", Enum = new() { "Performance", "Balanced", "Quality" } },
                ["adaptiveBitrate"] = new() { Type = "boolean", Description = "Enable adaptive bitrate based on network conditions" }
            },
            Required = new() { "host", "username" }
        };

        public ConnectRDPTool(ILocalConnector connector, ILogger<ConnectRDPTool> logger)
        {
            _connector = connector;
            _logger = logger;
        }

        public async Task<MCPToolCallResult> ExecuteAsync(Dictionary<string, object> arguments, CancellationToken cancellationToken = default)
        {
            try
            {
                var host = arguments.GetValueOrDefault("host")?.ToString();
                var username = arguments.GetValueOrDefault("username")?.ToString();

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
                {
                    return new MCPToolCallResult
                    {
                        IsError = true,
                        Content = new() { new MCPContent { Type = "text", Text = "Host and username are required parameters" } }
                    };
                }

                // Check if client is available
                if (!await _connector.IsClientAvailableAsync())
                {
                    return new MCPToolCallResult
                    {
                        IsError = true,
                        Content = new() { new MCPContent { Type = "text", Text = "RetroRDP client is not available. Please ensure the client application is running." } }
                    };
                }

                // Build connection options with codec and bitrate settings
                var options = new Dictionary<string, object>();
                
                // Basic connection options
                if (arguments.ContainsKey("port")) options["port"] = arguments["port"];
                if (arguments.ContainsKey("width")) options["width"] = arguments["width"];
                if (arguments.ContainsKey("height")) options["height"] = arguments["height"];
                if (arguments.ContainsKey("colorDepth")) options["colorDepth"] = arguments["colorDepth"];
                if (arguments.ContainsKey("fullScreen")) options["fullScreen"] = arguments["fullScreen"];
                if (arguments.ContainsKey("sessionName")) options["sessionName"] = arguments["sessionName"];
                
                // Advanced codec and performance options
                if (arguments.ContainsKey("codec")) options["videoCodec"] = arguments["codec"];
                if (arguments.ContainsKey("maxBitrate")) options["maxBitrate"] = arguments["maxBitrate"];
                if (arguments.ContainsKey("minBitrate")) options["minBitrate"] = arguments["minBitrate"];
                if (arguments.ContainsKey("preset")) options["performancePreset"] = arguments["preset"];
                if (arguments.ContainsKey("adaptiveBitrate")) options["adaptiveBitrate"] = arguments["adaptiveBitrate"];

                var password = arguments.GetValueOrDefault("password")?.ToString();
                var result = await _connector.CreateSessionAsync(host, username, password, options);

                var resultJson = JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
                
                return new MCPToolCallResult
                {
                    Content = new()
                    {
                        new MCPContent
                        {
                            Type = "text",
                            Text = $"Successfully initiated RDP connection to {host}. Session details:\n\n```json\n{resultJson}\n```"
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing RDP connect tool");
                return new MCPToolCallResult
                {
                    IsError = true,
                    Content = new() { new MCPContent { Type = "text", Text = $"Error connecting to RDP: {ex.Message}" } }
                };
            }
        }
    }

    /// <summary>
    /// MCP tool for listing active RDP sessions
    /// </summary>
    public class ListSessionsTool : IMCPTool
    {
        private readonly ILocalConnector _connector;
        private readonly ILogger<ListSessionsTool> _logger;

        public string Name => "list_rdp_sessions";
        
        public string Description => "List all active RDP sessions with their status, performance metrics, and codec information";

        public MCPSchema InputSchema => new()
        {
            Type = "object",
            Properties = new Dictionary<string, MCPSchemaProperty>
            {
                ["includeMetrics"] = new() { Type = "boolean", Description = "Include performance metrics for each session" }
            },
            Required = new()
        };

        public ListSessionsTool(ILocalConnector connector, ILogger<ListSessionsTool> logger)
        {
            _connector = connector;
            _logger = logger;
        }

        public async Task<MCPToolCallResult> ExecuteAsync(Dictionary<string, object> arguments, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await _connector.IsClientAvailableAsync())
                {
                    return new MCPToolCallResult
                    {
                        IsError = true,
                        Content = new() { new MCPContent { Type = "text", Text = "RetroRDP client is not available." } }
                    };
                }

                var sessions = await _connector.GetActiveSessionsAsync();
                var includeMetrics = bool.TryParse(arguments.GetValueOrDefault("includeMetrics")?.ToString(), out bool metrics) && metrics;

                var resultText = "## Active RDP Sessions\n\n";
                
                if (!sessions.Any())
                {
                    resultText += "No active RDP sessions found.";
                }
                else
                {
                    for (int i = 0; i < sessions.Count; i++)
                    {
                        var sessionJson = JsonSerializer.Serialize(sessions[i], new JsonSerializerOptions { WriteIndented = true });
                        resultText += $"### Session {i + 1}\n```json\n{sessionJson}\n```\n\n";
                        
                        if (includeMetrics && sessions[i] is IDictionary<string, object> sessionDict)
                        {
                            var sessionId = sessionDict.TryGetValue("sessionId", out var sessionIdObj) ? sessionIdObj?.ToString() : null;
                            if (!string.IsNullOrEmpty(sessionId))
                            {
                                var performanceMetrics = await _connector.GetPerformanceMetricsAsync(sessionId);
                                var metricsJson = JsonSerializer.Serialize(performanceMetrics, new JsonSerializerOptions { WriteIndented = true });
                                resultText += $"**Performance Metrics:**\n```json\n{metricsJson}\n```\n\n";
                            }
                        }
                    }
                }

                return new MCPToolCallResult
                {
                    Content = new() { new MCPContent { Type = "text", Text = resultText } }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing list sessions tool");
                return new MCPToolCallResult
                {
                    IsError = true,
                    Content = new() { new MCPContent { Type = "text", Text = $"Error listing sessions: {ex.Message}" } }
                };
            }
        }
    }

    /// <summary>
    /// MCP tool for configuring session performance and codec settings
    /// </summary>
    public class ConfigureSessionTool : IMCPTool
    {
        private readonly ILocalConnector _connector;
        private readonly ILogger<ConfigureSessionTool> _logger;

        public string Name => "configure_rdp_session";
        
        public string Description => "Configure performance settings, video codec, bitrate, and resolution for an existing RDP session";

        public MCPSchema InputSchema => new()
        {
            Type = "object",
            Properties = new Dictionary<string, MCPSchemaProperty>
            {
                ["sessionId"] = new() { Type = "string", Description = "ID of the RDP session to configure" },
                ["codec"] = new() { Type = "string", Description = "Video codec", Enum = new() { "H264", "H265", "Auto" } },
                ["maxBitrate"] = new() { Type = "integer", Description = "Maximum bitrate in kbps", Minimum = 256, Maximum = 50000 },
                ["minBitrate"] = new() { Type = "integer", Description = "Minimum bitrate in kbps", Minimum = 128, Maximum = 2000 },
                ["width"] = new() { Type = "integer", Description = "Screen width", Minimum = 800, Maximum = 7680 },
                ["height"] = new() { Type = "integer", Description = "Screen height", Minimum = 600, Maximum = 4320 },
                ["colorDepth"] = new() { Type = "integer", Description = "Color depth in bits", Enum = new() { "8", "15", "16", "24", "32" } },
                ["preset"] = new() { Type = "string", Description = "Performance preset", Enum = new() { "Performance", "Balanced", "Quality" } },
                ["adaptiveBitrate"] = new() { Type = "boolean", Description = "Enable adaptive bitrate" },
                ["enableBitmapCaching"] = new() { Type = "boolean", Description = "Enable bitmap caching" },
                ["enableCompression"] = new() { Type = "boolean", Description = "Enable compression" }
            },
            Required = new() { "sessionId" }
        };

        public ConfigureSessionTool(ILocalConnector connector, ILogger<ConfigureSessionTool> logger)
        {
            _connector = connector;
            _logger = logger;
        }

        public async Task<MCPToolCallResult> ExecuteAsync(Dictionary<string, object> arguments, CancellationToken cancellationToken = default)
        {
            try
            {
                var sessionId = arguments.GetValueOrDefault("sessionId")?.ToString();
                
                if (string.IsNullOrEmpty(sessionId))
                {
                    return new MCPToolCallResult
                    {
                        IsError = true,
                        Content = new() { new MCPContent { Type = "text", Text = "Session ID is required" } }
                    };
                }

                if (!await _connector.IsClientAvailableAsync())
                {
                    return new MCPToolCallResult
                    {
                        IsError = true,
                        Content = new() { new MCPContent { Type = "text", Text = "RetroRDP client is not available." } }
                    };
                }

                var settings = new Dictionary<string, object>();
                
                // Copy all relevant settings from arguments
                var settingsKeys = new[] { "codec", "maxBitrate", "minBitrate", "width", "height", 
                                          "colorDepth", "preset", "adaptiveBitrate", "enableBitmapCaching", "enableCompression" };
                
                foreach (var key in settingsKeys)
                {
                    if (arguments.ContainsKey(key))
                    {
                        settings[key] = arguments[key];
                    }
                }

                var success = await _connector.ApplyPerformanceSettingsAsync(sessionId, settings);
                
                if (success)
                {
                    var settingsJson = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                    return new MCPToolCallResult
                    {
                        Content = new()
                        {
                            new MCPContent
                            {
                                Type = "text",
                                Text = $"Successfully applied performance settings to session {sessionId}:\n\n```json\n{settingsJson}\n```"
                            }
                        }
                    };
                }
                else
                {
                    return new MCPToolCallResult
                    {
                        IsError = true,
                        Content = new() { new MCPContent { Type = "text", Text = $"Failed to apply settings to session {sessionId}" } }
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing configure session tool");
                return new MCPToolCallResult
                {
                    IsError = true,
                    Content = new() { new MCPContent { Type = "text", Text = $"Error configuring session: {ex.Message}" } }
                };
            }
        }
    }
}