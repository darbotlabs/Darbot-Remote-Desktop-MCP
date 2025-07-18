using Microsoft.Extensions.Logging;
using RetroRDP.Shared.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        // Conversation management
        private readonly Dictionary<string, ConversationContext> _conversations = new();
        private readonly Dictionary<string, SessionProfile> _sessionProfiles = new();

        // Enhanced system prompt for advanced command parsing with chaining and context
        private const string CommandParsingPrompt = @"You are AssistBot, an advanced AI assistant for a retro-cyber Remote Desktop client application. 

**Advanced Capabilities:**
- Command chaining: Handle multiple commands in sequence (""connect to server1 and take screenshot"")
- Context awareness: Remember previous commands and session state
- Smart profiles: Create and manage connection profiles
- Enhanced screenshots: Support multiple modes (session, application, fullscreen)
- Bulk operations: Handle multiple sessions efficiently

**Available Commands:**
- Connect: ""connect to [host] as [user]"" or ""rdp to [server]""
- Disconnect: ""disconnect session [number/name]"" or ""close connection""
- Disconnect all: ""disconnect all"" or ""close all sessions""
- List sessions: ""show sessions"" or ""list connections""
- Screenshot: ""screenshot session [number]"" with modes (session/application/fullscreen)
- Chained commands: ""connect to server1 and server2, then screenshot both""
- Create profile: ""save connection to server1 as [profile name]""
- Load profile: ""connect using [profile name]""
- General help: Any other questions about RDP or the application

**Enhanced Response Format:**
Always respond with JSON containing:
- action: One of [""Connect"", ""Disconnect"", ""DisconnectAll"", ""ListSessions"", ""Screenshot"", ""ChainedCommands"", ""CreateProfile"", ""LoadProfile"", ""GeneralHelp"", ""Unknown""]
- host: Server address (for Connect action)
- username: Username (for Connect action)  
- sessionId: Session identifier (for Disconnect/Screenshot actions)
- port: Port number (default 3389 for RDP)
- chainedCommands: Array of commands for sequential execution
- screenshotMode: ""session"", ""application"", or ""fullscreen""
- profileName: Profile name for save/load operations
- explanation: Brief explanation with context awareness
- needsMoreInfo: true if you need more details
- followUpQuestions: Array of questions if more info needed
- conversationId: Current conversation identifier

**Context Integration:**
- Reference previous commands and sessions in explanations
- Suggest improvements based on user patterns
- Provide proactive recommendations
- Remember user preferences across conversations

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
                var testResponse = await GenerateResponseAsync("test", null, cancellationToken);
                return !string.IsNullOrEmpty(testResponse);
            }
            catch
            {
                return false;
            }
        }

        public async Task<AssistantResponse> ParseCommandAsync(string userInput, ConversationContext? conversationContext = null, CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("AssistantAI service not initialized");

            // Create or update conversation context
            if (conversationContext == null)
                conversationContext = GetOrCreateConversation();

            // Add user message to conversation
            conversationContext.Messages.Add(new ConversationMessage
            {
                Role = "user",
                Content = userInput,
                Timestamp = DateTime.UtcNow
            });
            conversationContext.LastActivity = DateTime.UtcNow;

            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    // Fallback parsing when no API key is available
                    var fallbackResponse = ParseCommandFallback(userInput);
                    fallbackResponse.ConversationId = conversationContext.ConversationId;
                    return fallbackResponse;
                }

                // Build context-aware prompt
                var contextPrompt = BuildContextualPrompt(userInput, conversationContext);
                var aiResponse = await CallAIServiceAsync(contextPrompt, cancellationToken);

                // Parse JSON response
                var response = JsonSerializer.Deserialize<AssistantResponse>(aiResponse);
                if (response?.Command != null)
                {
                    response.Success = true;
                    response.ConversationId = conversationContext.ConversationId;
                    
                    // Add assistant response to conversation
                    conversationContext.Messages.Add(new ConversationMessage
                    {
                        Role = "assistant",
                        Content = response.Message,
                        Timestamp = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                        {
                            ["action"] = response.Command.Action.ToString(),
                            ["success"] = response.Success
                        }
                    });

                    return response;
                }

                // If JSON parsing failed, return fallback
                _logger?.LogWarning("Failed to parse AI response as JSON, using fallback");
                var fallback = ParseCommandFallback(userInput);
                fallback.ConversationId = conversationContext.ConversationId;
                return fallback;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error parsing command with AI");
                var fallback = ParseCommandFallback(userInput);
                fallback.ConversationId = conversationContext.ConversationId;
                return fallback;
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
                ? $"{_apiEndpoint}/openai/deployments/{_modelName}/chat/completions?api-version={_apiVersion}"
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

            // Enhanced pattern matching for advanced commands
            if (lowerInput.Contains("save") && (lowerInput.Contains("profile") || lowerInput.Contains("connection")))
            {
                command.Action = AssistantActionType.CreateProfile;
                command.Explanation = "Creating connection profile";
                return new AssistantResponse { Command = command, Success = true, Message = "I'll help you save this connection as a profile." };
            }
            else if (lowerInput.Contains("load") && lowerInput.Contains("profile"))
            {
                command.Action = AssistantActionType.LoadProfile;
                command.Explanation = "Loading connection profile";
                return new AssistantResponse { Command = command, Success = true, Message = "Which profile would you like to load?" };
            }
            else if ((lowerInput.Contains("and") || lowerInput.Contains("then")) && 
                    (lowerInput.Contains("connect") || lowerInput.Contains("screenshot") || lowerInput.Contains("disconnect")))
            {
                command.Action = AssistantActionType.ChainedCommands;
                command.Explanation = "Detected multiple commands to execute in sequence";
                return new AssistantResponse { Command = command, Success = true, Message = "I'll execute these commands in sequence for you." };
            }
            else if (lowerInput.Contains("disconnect all") || lowerInput.Contains("close all"))
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
                
                // Enhanced screenshot mode detection
                if (lowerInput.Contains("fullscreen") || lowerInput.Contains("full screen"))
                    command.ScreenshotMode = "fullscreen";
                else if (lowerInput.Contains("application") || lowerInput.Contains("app"))
                    command.ScreenshotMode = "application";
                else
                    command.ScreenshotMode = "session";

                return new AssistantResponse 
                { 
                    Command = command, 
                    Success = true, 
                    Message = $"Taking {command.ScreenshotMode} screenshot. Which session?",
                    NeedsMoreInfo = string.IsNullOrEmpty(ExtractSessionReference(lowerInput))
                };
            }
            else if (lowerInput.Contains("connect") || lowerInput.Contains("rdp"))
            {
                command.Action = AssistantActionType.Connect;
                command.Explanation = "Detected connection request";
                
                // Try to extract connection details
                var host = ExtractHostname(lowerInput);
                var username = ExtractUsername(lowerInput);
                
                if (!string.IsNullOrEmpty(host))
                    command.Host = host;
                if (!string.IsNullOrEmpty(username))
                    command.Username = username;

                return new AssistantResponse 
                { 
                    Command = command, 
                    Success = true, 
                    Message = string.IsNullOrEmpty(host) ? 
                        "I'll help you create a new RDP connection." :
                        $"Connecting to {host}" + (string.IsNullOrEmpty(username) ? "" : $" as {username}"),
                    NeedsMoreInfo = string.IsNullOrEmpty(host),
                    FollowUpQuestions = string.IsNullOrEmpty(host) ? 
                        new[] { "What server would you like to connect to?", "What username should I use?" } : 
                        null
                };
            }
            else
            {
                command.Action = AssistantActionType.GeneralHelp;
                command.Explanation = "General assistance request";
                return new AssistantResponse { Command = command, Success = true, Message = GenerateResponseFallback(userInput) };
            }
        }

        private string? ExtractHostname(string input)
        {
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in words)
            {
                if (word.Contains('.') && !word.Contains('@'))
                {
                    return word.Trim(',', '.', '!', '?', '"', '\'');
                }
            }
            return null;
        }

        private string? ExtractUsername(string input)
        {
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].ToLowerInvariant() == "as" && i + 1 < words.Length)
                {
                    return words[i + 1].Trim(',', '.', '!', '?', '"', '\'');
                }
            }
            return null;
        }

        private string? ExtractSessionReference(string input)
        {
            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].ToLowerInvariant() == "session" && i + 1 < words.Length)
                {
                    return words[i + 1].Trim(',', '.', '!', '?');
                }
            }
            return null;
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

        public ConversationContext GetOrCreateConversation(string? conversationId = null)
        {
            conversationId ??= Guid.NewGuid().ToString();

            if (!_conversations.TryGetValue(conversationId, out var context))
            {
                context = new ConversationContext
                {
                    ConversationId = conversationId,
                    LastActivity = DateTime.UtcNow
                };
                _conversations[conversationId] = context;
            }

            return context;
        }

        public async Task<List<AssistantResponse>> ExecuteChainedCommandsAsync(AssistantCommand[] commands, ConversationContext conversationContext, CancellationToken cancellationToken = default)
        {
            var results = new List<AssistantResponse>();

            foreach (var command in commands.OrderBy(c => c.Priority))
            {
                try
                {
                    // Create a temporary response for each chained command
                    var response = new AssistantResponse
                    {
                        Command = command,
                        Success = command.IsValid(),
                        Message = $"🔗 Executing chained command: {command.Action}",
                        ConversationId = conversationContext.ConversationId
                    };

                    if (!command.IsValid())
                    {
                        response.Message = $"❌ Invalid command in chain: {command.Action} - missing required parameters";
                        response.NeedsMoreInfo = true;
                        response.FollowUpQuestions = command.GetMissingParameters()
                            .Select(p => $"What is the {p}?").ToArray();
                    }

                    results.Add(response);

                    // Add progress to conversation
                    conversationContext.Messages.Add(new ConversationMessage
                    {
                        Role = "assistant",
                        Content = response.Message,
                        Timestamp = DateTime.UtcNow,
                        Metadata = new Dictionary<string, object>
                        {
                            ["chainedCommand"] = true,
                            ["commandIndex"] = results.Count - 1,
                            ["totalCommands"] = commands.Length
                        }
                    });

                    // Brief delay between chained commands
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    await Task.Delay(100, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error executing chained command: {Action}", command.Action);
                    results.Add(new AssistantResponse
                    {
                        Command = command,
                        Success = false,
                        Message = $"❌ Error executing {command.Action}: {ex.Message}",
                        ConversationId = conversationContext.ConversationId
                    });
                }
            }

            return results;
        }

        public async IAsyncEnumerable<string> GenerateStreamingResponseAsync(string userInput, ConversationContext? conversationContext = null, Action<int>? progressCallback = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("AssistantAI service not initialized");

            conversationContext ??= GetOrCreateConversation();

            // Simulate streaming response with progressive content
            var responses = new[]
            {
                "🤖 AssistBot: Analyzing your request...",
                "🔍 Processing natural language input...",
                "⚡ Connecting to AI service...",
                "🧠 Generating intelligent response...",
                "✨ Finalizing recommendations..."
            };

            for (int i = 0; i < responses.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                progressCallback?.Invoke((i + 1) * 100 / responses.Length);
                yield return responses[i];

                // Simulate processing time
                await Task.Delay(500, cancellationToken);
            }

            // Generate final response
            var finalResponse = await GenerateResponseAsync(userInput, conversationContext, cancellationToken);
            yield return finalResponse;
        }

        public async Task<SessionProfile?> CreateSessionProfileAsync(string userInput, CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                return null;

            try
            {
                // Use AI to extract connection details and create profile
                var prompt = $@"Extract RDP connection details from this input and create a session profile: ""{userInput}""

Return JSON with:
- name: A descriptive name for this profile
- host: Server hostname or IP
- username: Username if specified
- port: Port number (default 3389)
- description: Brief description
- tags: Array of relevant tags

Input: {userInput}
JSON:";

                if (!string.IsNullOrEmpty(_apiKey))
                {
                    var aiResponse = await CallAIServiceAsync(prompt, cancellationToken);
                    var profile = JsonSerializer.Deserialize<SessionProfile>(aiResponse);
                    
                    if (profile != null && !string.IsNullOrEmpty(profile.Host))
                    {
                        profile.CreatedBy = "ai";
                        profile.LastUsed = DateTime.UtcNow;
                        _sessionProfiles[profile.Name] = profile;
                        return profile;
                    }
                }

                // Fallback: simple parsing
                return CreateProfileFallback(userInput);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating session profile");
                return CreateProfileFallback(userInput);
            }
        }

        private SessionProfile? CreateProfileFallback(string userInput)
        {
            // Simple keyword extraction
            var words = userInput.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            string? host = null;
            string? username = null;

            // Look for patterns like "server.com" or IP addresses
            foreach (var word in words)
            {
                if (word.Contains('.') && !word.Contains('@'))
                {
                    host = word.Trim(',', '.', '!', '?');
                    break;
                }
            }

            // Look for "as username" pattern
            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i] == "as" && i + 1 < words.Length)
                {
                    username = words[i + 1];
                    break;
                }
            }

            if (!string.IsNullOrEmpty(host))
            {
                var profile = new SessionProfile
                {
                    Name = $"Profile for {host}",
                    Host = host,
                    Username = username,
                    Description = $"Auto-generated profile from: {userInput}",
                    CreatedBy = "fallback",
                    Tags = new[] { "auto-generated" }
                };

                _sessionProfiles[profile.Name] = profile;
                return profile;
            }

            return null;
        }

        private string BuildContextualPrompt(string userInput, ConversationContext conversationContext)
        {
            var contextBuilder = new StringBuilder(CommandParsingPrompt);
            
            // Add conversation history for context
            if (conversationContext.Messages.Count > 1)
            {
                contextBuilder.AppendLine("\n**Conversation History:**");
                var recentMessages = conversationContext.Messages
                    .TakeLast(6) // Last 3 exchanges
                    .Where(m => m.Role != "system");

                foreach (var msg in recentMessages)
                {
                    contextBuilder.AppendLine($"{msg.Role}: {msg.Content}");
                }
            }

            // Add session context if available
            if (conversationContext.SessionContext.Any())
            {
                contextBuilder.AppendLine("\n**Current Session Context:**");
                foreach (var kvp in conversationContext.SessionContext)
                {
                    contextBuilder.AppendLine($"{kvp.Key}: {kvp.Value}");
                }
            }

            // Add user preferences
            if (conversationContext.UserPreferences.Any())
            {
                contextBuilder.AppendLine("\n**User Preferences:**");
                foreach (var pref in conversationContext.UserPreferences)
                {
                    contextBuilder.AppendLine($"{pref.Key}: {pref.Value}");
                }
            }

            contextBuilder.AppendLine($"\n**Current Request:**");
            contextBuilder.AppendLine($"User: {userInput}");
            contextBuilder.AppendLine("\nRespond with JSON only:");

            return contextBuilder.ToString();
        }

        public async Task<string> GenerateResponseAsync(string userInput, ConversationContext? conversationContext = null, CancellationToken cancellationToken = default)
        {
            if (!IsInitialized)
                throw new InvalidOperationException("AssistantAI service not initialized");

            conversationContext ??= GetOrCreateConversation();

            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    // Use fallback response generation
                    return GenerateResponseFallback(userInput);
                }

                var conversationPrompt = @"You are AssistBot, a helpful AI assistant for a retro-cyber themed Remote Desktop client. 
Be helpful, concise, and maintain a friendly but technical tone. Help users with RDP connections, troubleshooting, and application features.
Keep responses under 200 words and use appropriate emojis when relevant.

Consider the conversation context and provide contextually aware responses that reference previous interactions when relevant.";

                var contextualPrompt = new StringBuilder(conversationPrompt);
                
                // Add conversation context
                if (conversationContext.Messages.Count > 1)
                {
                    contextualPrompt.AppendLine("\n**Previous Context:**");
                    var recentMessages = conversationContext.Messages.TakeLast(4);
                    foreach (var msg in recentMessages.Where(m => m.Role != "system"))
                    {
                        contextualPrompt.AppendLine($"{msg.Role}: {msg.Content}");
                    }
                }

                contextualPrompt.AppendLine($"\nUser: {userInput}");
                contextualPrompt.AppendLine("AssistBot:");

                var response = await CallAIServiceAsync(contextualPrompt.ToString(), cancellationToken);
                
                // Add to conversation history
                conversationContext.Messages.Add(new ConversationMessage
                {
                    Role = "user",
                    Content = userInput
                });
                conversationContext.Messages.Add(new ConversationMessage
                {
                    Role = "assistant", 
                    Content = response
                });
                conversationContext.LastActivity = DateTime.UtcNow;
                
                return $"🤖 AssistBot: {response.Trim()}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating AI response");
                return GenerateResponseFallback(userInput);
            }
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