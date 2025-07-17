using Microsoft.Extensions.Logging;
using RetroRDP.Shared.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// AI Assistant service that integrates with OpenAI or Azure OpenAI
    /// for natural language command parsing and conversation
    /// </summary>
    public class AssistantAI : IAssistantAI, IDisposable
    {
        private readonly ILogger<AssistantAI>? _logger;
        private readonly HttpClient _httpClient;
        private bool _disposed = false;

        // Configuration
        private string? _apiKey;
        private string? _apiEndpoint;
        private string? _modelName;
        private bool _isAzureOpenAI;

        // System prompt for command parsing
        private const string CommandParsingPrompt = @"You are AssistBot, an AI assistant for a retro-cyber Remote Desktop client application. 

Your job is to parse user commands and return structured JSON responses. You can help with:

**Available Commands:**
- Connect to remote desktop: ""connect to [host] as [user]"" or ""rdp to [server]""
- Disconnect session: ""disconnect session [number/name]"" or ""close connection""
- Disconnect all: ""disconnect all"" or ""close all sessions""
- List sessions: ""show sessions"" or ""list connections""
- Take screenshot: ""screenshot session [number]"" or ""capture screen""
- General help: Any other questions about RDP or the application

**Response Format:**
Always respond with a JSON object containing:
- action: One of [""Connect"", ""Disconnect"", ""DisconnectAll"", ""ListSessions"", ""Screenshot"", ""GeneralHelp"", ""Unknown""]
- host: Server address (for Connect action)
- username: Username (for Connect action)  
- sessionId: Session identifier (for Disconnect/Screenshot actions)
- port: Port number (default 3389 for RDP)
- explanation: Brief explanation of what you understood
- needsMoreInfo: true if you need more details
- followUpQuestions: Array of questions if more info needed

**Examples:**
User: ""connect to server.example.com as admin""
Response: {""action"": ""Connect"", ""host"": ""server.example.com"", ""username"": ""admin"", ""port"": 3389, ""explanation"": ""Connecting to server.example.com as admin"", ""needsMoreInfo"": false}

User: ""disconnect session 1""
Response: {""action"": ""Disconnect"", ""sessionId"": ""1"", ""explanation"": ""Disconnecting session 1"", ""needsMoreInfo"": false}

User: ""what is RDP?""
Response: {""action"": ""GeneralHelp"", ""explanation"": ""General question about RDP"", ""needsMoreInfo"": false}

Be helpful and maintain the retro-cyber assistant personality while being precise about command parsing.";

        public bool IsInitialized { get; private set; }
        public string ServiceName { get; private set; } = "AssistantAI (Not Configured)";

        public AssistantAI(ILogger<AssistantAI>? logger = null)
        {
            _logger = logger;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Initializing AssistantAI service...");

                // Check for Azure OpenAI configuration first
                _apiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
                _apiEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
                _modelName = Environment.GetEnvironmentVariable("AZURE_OPENAI_MODEL") ?? "gpt-4";

                if (!string.IsNullOrEmpty(_apiKey) && !string.IsNullOrEmpty(_apiEndpoint))
                {
                    _isAzureOpenAI = true;
                    ServiceName = $"Azure OpenAI ({_modelName})";
                    _logger?.LogInformation("Using Azure OpenAI service");
                }
                else
                {
                    // Fallback to OpenAI
                    _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
                    _modelName = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";
                    
                    if (!string.IsNullOrEmpty(_apiKey))
                    {
                        _isAzureOpenAI = false;
                        _apiEndpoint = "https://api.openai.com/v1/chat/completions";
                        ServiceName = $"OpenAI ({_modelName})";
                        _logger?.LogInformation("Using OpenAI service");
                    }
                    else
                    {
                        _logger?.LogWarning("No AI API key found. Assistant will use fallback mode.");
                        ServiceName = "Fallback Mode (No API Key)";
                        IsInitialized = true; // Still functional in fallback mode
                        return true;
                    }
                }

                // Set up HTTP client headers
                SetupHttpClient();

                // Test connection with a simple request
                var testResult = await TestConnectionAsync(cancellationToken);
                if (testResult)
                {
                    IsInitialized = true;
                    _logger?.LogInformation("AssistantAI service initialized successfully: {ServiceName}", ServiceName);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("AI service test failed, falling back to local mode");
                    ServiceName = "Fallback Mode (Connection Failed)";
                    IsInitialized = true; // Still functional in fallback mode
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize AssistantAI service");
                ServiceName = "Fallback Mode (Error)";
                IsInitialized = true; // Still functional in fallback mode
                return true;
            }
        }

        private void SetupHttpClient()
        {
            _httpClient.DefaultRequestHeaders.Clear();

            if (_isAzureOpenAI)
            {
                _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            }

            _httpClient.DefaultRequestHeaders.Add("User-Agent", "RetroRDP-Client/1.0");
        }

        private async Task<bool> TestConnectionAsync(CancellationToken cancellationToken)
        {
            try
            {
                var testResponse = await GenerateResponseAsync("test", cancellationToken);
                return !string.IsNullOrEmpty(testResponse);
            }
            catch
            {
                return false;
            }
        }

        public async Task<AssistantResponse> ParseCommandAsync(string userInput, CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("AssistantAI service not initialized");

            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    // Fallback parsing when no API key is available
                    return ParseCommandFallback(userInput);
                }

                var prompt = $"{CommandParsingPrompt}\n\nUser: {userInput}\n\nRespond with JSON only:";
                var aiResponse = await CallAIServiceAsync(prompt, cancellationToken);

                // Parse JSON response
                var response = JsonSerializer.Deserialize<AssistantResponse>(aiResponse);
                if (response?.Command != null)
                {
                    response.Success = true;
                    return response;
                }

                // If JSON parsing failed, return fallback
                _logger?.LogWarning("Failed to parse AI response as JSON, using fallback");
                return ParseCommandFallback(userInput);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error parsing command with AI");
                return ParseCommandFallback(userInput);
            }
        }

        public async Task<string> GenerateResponseAsync(string userInput, CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("AssistantAI service not initialized");

            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    // Use fallback response generation
                    return GenerateResponseFallback(userInput);
                }

                var conversationPrompt = @"You are AssistBot, a helpful AI assistant for a retro-cyber themed Remote Desktop client. 
Be helpful, concise, and maintain a friendly but technical tone. Help users with RDP connections, troubleshooting, and application features.
Keep responses under 200 words and use appropriate emojis when relevant.";

                var prompt = $"{conversationPrompt}\n\nUser: {userInput}\nAssistBot:";
                var response = await CallAIServiceAsync(prompt, cancellationToken);
                
                return $"🤖 AssistBot: {response.Trim()}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating AI response");
                return GenerateResponseFallback(userInput);
            }
        }

        private async Task<string> CallAIServiceAsync(string prompt, CancellationToken cancellationToken)
        {
            var requestBody = new
            {
                model = _modelName,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                max_tokens = 500,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            string endpoint = _isAzureOpenAI 
                ? $"{_apiEndpoint}/openai/deployments/{_modelName}/chat/completions?api-version=2024-02-15-preview"
                : _apiEndpoint!;

            var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var aiResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            return aiResponse
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "No response";
        }

        private AssistantResponse ParseCommandFallback(string userInput)
        {
            var lowerInput = userInput.ToLowerInvariant();
            var command = new AssistantCommand();

            // Simple pattern matching for common commands - order matters for specificity
            if (lowerInput.Contains("disconnect all") || lowerInput.Contains("close all"))
            {
                command.Action = AssistantActionType.DisconnectAll;
                command.Explanation = "Disconnecting all active sessions";
                return new AssistantResponse { Command = command, Success = true, Message = "Disconnecting all sessions..." };
            }
            else if (lowerInput.Contains("disconnect") || lowerInput.Contains("close"))
            {
                command.Action = AssistantActionType.Disconnect;
                command.Explanation = "Disconnect request detected";
                return new AssistantResponse 
                { 
                    Command = command, 
                    Success = true, 
                    Message = "Which session would you like to disconnect?",
                    NeedsMoreInfo = true
                };
            }
            else if (lowerInput.Contains("list") || lowerInput.Contains("sessions") || lowerInput.Contains("status"))
            {
                command.Action = AssistantActionType.ListSessions;
                command.Explanation = "Listing active sessions";
                return new AssistantResponse { Command = command, Success = true, Message = "Here are your active sessions:" };
            }
            else if (lowerInput.Contains("screenshot") || lowerInput.Contains("capture"))
            {
                command.Action = AssistantActionType.Screenshot;
                command.Explanation = "Screenshot request detected";
                return new AssistantResponse 
                { 
                    Command = command, 
                    Success = true, 
                    Message = "Which session would you like to capture?",
                    NeedsMoreInfo = true
                };
            }
            else if (lowerInput.Contains("connect") || lowerInput.Contains("rdp"))
            {
                command.Action = AssistantActionType.Connect;
                command.Explanation = "Detected connection request - please use the connection dialog for setup";
                return new AssistantResponse 
                { 
                    Command = command, 
                    Success = true, 
                    Message = "I'll help you create a new RDP connection.",
                    NeedsMoreInfo = true,
                    FollowUpQuestions = new[] { "What server would you like to connect to?", "What username should I use?" }
                };
            }
            else
            {
                command.Action = AssistantActionType.GeneralHelp;
                command.Explanation = "General assistance request";
                return new AssistantResponse { Command = command, Success = true, Message = GenerateResponseFallback(userInput) };
            }
        }

        private string GenerateResponseFallback(string userInput)
        {
            var lowerInput = userInput.ToLowerInvariant();

            if (lowerInput.Contains("hello") || lowerInput.Contains("hi"))
                return "🤖 AssistBot: Hello! I'm here to help you manage RDP connections. What can I do for you?";
            
            if (lowerInput.Contains("help"))
                return "🤖 AssistBot: I can help you connect to servers, manage sessions, take screenshots, and troubleshoot RDP issues. Just tell me what you'd like to do!";
            
            return "🤖 AssistBot: I understand you're asking about RDP connections. I can help with connecting to servers, managing sessions, and troubleshooting. What specifically would you like assistance with?";
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _httpClient?.Dispose();
                _disposed = true;
                _logger?.LogInformation("AssistantAI service disposed");
            }
        }
    }
}