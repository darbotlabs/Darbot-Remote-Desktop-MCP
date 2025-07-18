using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RetroRDP.Shared.Models
{
    /// <summary>
    /// Represents the type of action the AI assistant should perform
    /// </summary>
    public enum AssistantActionType
    {
        None,
        Connect,
        Disconnect,
        DisconnectAll,
        ListSessions,
        Screenshot,
        GeneralHelp,
        Unknown,
        ChainedCommands,
        CreateProfile,
        LoadProfile,
        ScheduleAction
    }

    /// <summary>
    /// Represents a parsed command from the AI assistant
    /// </summary>
    public class AssistantCommand
    {
        [JsonPropertyName("action")]
        public AssistantActionType Action { get; set; } = AssistantActionType.None;

        [JsonPropertyName("host")]
        public string? Host { get; set; }

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }

        [JsonPropertyName("sessionName")]
        public string? SessionName { get; set; }

        [JsonPropertyName("sessionId")]
        public string? SessionId { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; } = 3389;

        [JsonPropertyName("fullScreen")]
        public bool FullScreen { get; set; } = false;

        [JsonPropertyName("colorDepth")]
        public int ColorDepth { get; set; } = 32;

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; } = 0.0;

        [JsonPropertyName("explanation")]
        public string? Explanation { get; set; }

        [JsonPropertyName("requiresConfirmation")]
        public bool RequiresConfirmation { get; set; } = false;

        [JsonPropertyName("chainedCommands")]
        public AssistantCommand[]? ChainedCommands { get; set; }

        [JsonPropertyName("screenshotMode")]
        public string? ScreenshotMode { get; set; } = "session"; // session, application, fullscreen

        [JsonPropertyName("profileName")]
        public string? ProfileName { get; set; }

        [JsonPropertyName("scheduleTime")]
        public DateTime? ScheduleTime { get; set; }

        [JsonPropertyName("priority")]
        public int Priority { get; set; } = 0;

        /// <summary>
        /// Validates if the command has required parameters for the action
        /// </summary>
        public bool IsValid()
        {
            return Action switch
            {
                AssistantActionType.Connect => !string.IsNullOrWhiteSpace(Host),
                AssistantActionType.Disconnect => !string.IsNullOrWhiteSpace(SessionId),
                AssistantActionType.Screenshot => !string.IsNullOrWhiteSpace(SessionId),
                AssistantActionType.DisconnectAll => true,
                AssistantActionType.ListSessions => true,
                AssistantActionType.GeneralHelp => true,
                AssistantActionType.ChainedCommands => ChainedCommands != null && ChainedCommands.Length > 0,
                AssistantActionType.CreateProfile => !string.IsNullOrWhiteSpace(ProfileName),
                AssistantActionType.LoadProfile => !string.IsNullOrWhiteSpace(ProfileName),
                AssistantActionType.ScheduleAction => ScheduleTime.HasValue,
                AssistantActionType.Unknown => false,
                _ => false
            };
        }

        /// <summary>
        /// Gets the missing required parameters for this command
        /// </summary>
        public string[] GetMissingParameters()
        {
            var missing = new List<string>();

            switch (Action)
            {
                case AssistantActionType.Connect:
                    if (string.IsNullOrWhiteSpace(Host))
                        missing.Add("host");
                    if (string.IsNullOrWhiteSpace(Username))
                        missing.Add("username");
                    break;
                case AssistantActionType.Disconnect:
                case AssistantActionType.Screenshot:
                    if (string.IsNullOrWhiteSpace(SessionId))
                        missing.Add("sessionId");
                    break;
            }

            return missing.ToArray();
        }
    }

    /// <summary>
    /// Response from AI assistant containing parsed command and additional context
    /// </summary>
    public class AssistantResponse
    {
        [JsonPropertyName("command")]
        public AssistantCommand? Command { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("success")]
        public bool Success { get; set; } = false;

        [JsonPropertyName("needsMoreInfo")]
        public bool NeedsMoreInfo { get; set; } = false;

        [JsonPropertyName("followUpQuestions")]
        public string[]? FollowUpQuestions { get; set; }

        [JsonPropertyName("conversationId")]
        public string? ConversationId { get; set; }

        [JsonPropertyName("isStreaming")]
        public bool IsStreaming { get; set; } = false;

        [JsonPropertyName("progressPercent")]
        public int ProgressPercent { get; set; } = 0;
    }

    /// <summary>
    /// Represents conversation context for multi-turn interactions
    /// </summary>
    public class ConversationContext
    {
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("messages")]
        public List<ConversationMessage> Messages { get; set; } = new();

        [JsonPropertyName("sessionContext")]
        public Dictionary<string, object> SessionContext { get; set; } = new();

        [JsonPropertyName("lastActivity")]
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("userPreferences")]
        public Dictionary<string, string> UserPreferences { get; set; } = new();
    }

    /// <summary>
    /// Individual message in a conversation
    /// </summary>
    public class ConversationMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user"; // user, assistant, system

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("metadata")]
        public Dictionary<string, object>? Metadata { get; set; }
    }

    /// <summary>
    /// Session profile for quick connections
    /// </summary>
    public class SessionProfile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("host")]
        public string Host { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; } = 3389;

        [JsonPropertyName("fullScreen")]
        public bool FullScreen { get; set; } = false;

        [JsonPropertyName("colorDepth")]
        public int ColorDepth { get; set; } = 32;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("tags")]
        public string[] Tags { get; set; } = Array.Empty<string>();

        [JsonPropertyName("lastUsed")]
        public DateTime? LastUsed { get; set; }

        [JsonPropertyName("useCount")]
        public int UseCount { get; set; } = 0;

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = "user"; // user, ai, imported
    }
}