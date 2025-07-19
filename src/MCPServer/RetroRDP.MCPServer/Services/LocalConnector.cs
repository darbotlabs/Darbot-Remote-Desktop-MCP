using System.Text.Json;
using RetroRDP.Shared.Models;
using RetroRDP.MCPServer.Services;

namespace RetroRDP.MCPServer.Services
{
    /// <summary>
    /// Local connector that communicates with the RetroRDP client via HTTP, named pipes, or shared memory
    /// </summary>
    public class LocalConnector : ILocalConnector
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LocalConnector> _logger;
        private readonly string _clientBaseUrl;

        public LocalConnector(HttpClient httpClient, ILogger<LocalConnector> logger, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _clientBaseUrl = configuration.GetValue<string>("RetroRDPClient:BaseUrl") ?? "http://localhost:8080";
            
            // Configure timeout for local communication
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<bool> IsClientAvailableAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_clientBaseUrl}/health");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "RetroRDP client not available");
                return false;
            }
        }

        public async Task<List<object>> GetActiveSessionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_clientBaseUrl}/api/sessions");
                response.EnsureSuccessStatusCode();
                
                var json = await response.Content.ReadAsStringAsync();
                var sessions = JsonSerializer.Deserialize<List<RdpSession>>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return sessions?.Cast<object>().ToList() ?? new List<object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get active sessions from RetroRDP client");
                return new List<object>();
            }
        }

        public async Task<object> CreateSessionAsync(string host, string username, string? password = null, Dictionary<string, object>? options = null)
        {
            try
            {
                var request = new RdpConnectionRequest
                {
                    Host = host,
                    Username = username,
                    Password = password ?? string.Empty,
                    SessionName = options?.GetValueOrDefault("sessionName")?.ToString() ?? $"{username}@{host}",
                    Port = int.TryParse(options?.GetValueOrDefault("port")?.ToString(), out int port) ? port : 3389,
                    ScreenWidth = int.TryParse(options?.GetValueOrDefault("width")?.ToString(), out int width) ? width : 1920,
                    ScreenHeight = int.TryParse(options?.GetValueOrDefault("height")?.ToString(), out int height) ? height : 1080,
                    ColorDepth = int.TryParse(options?.GetValueOrDefault("colorDepth")?.ToString(), out int colorDepth) ? colorDepth : 32,
                    FullScreen = bool.TryParse(options?.GetValueOrDefault("fullScreen")?.ToString(), out bool fullScreen) && fullScreen
                };

                var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_clientBaseUrl}/api/sessions", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                var session = JsonSerializer.Deserialize<RdpSession>(responseJson, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return session ?? (object)new { error = "Failed to create session" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RDP session");
                return new { error = ex.Message };
            }
        }

        public async Task<bool> DisconnectSessionAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_clientBaseUrl}/api/sessions/{sessionId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disconnect session {SessionId}", sessionId);
                return false;
            }
        }

        public async Task<string> CaptureScreenshotAsync(string sessionId, string mode = "session")
        {
            try
            {
                var response = await _httpClient.PostAsync($"{_clientBaseUrl}/api/sessions/{sessionId}/screenshot?mode={mode}", null);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                return result?.GetValueOrDefault("filePath")?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture screenshot for session {SessionId}", sessionId);
                return string.Empty;
            }
        }

        public async Task<Dictionary<string, object>> GetPerformanceMetricsAsync(string sessionId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_clientBaseUrl}/api/sessions/{sessionId}/performance");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var metrics = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                
                return metrics ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get performance metrics for session {SessionId}", sessionId);
                return new Dictionary<string, object> { { "error", ex.Message } };
            }
        }

        public async Task<bool> ApplyPerformanceSettingsAsync(string sessionId, Dictionary<string, object> settings)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{_clientBaseUrl}/api/sessions/{sessionId}/performance", content);
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to apply performance settings for session {SessionId}", sessionId);
                return false;
            }
        }
    }
}