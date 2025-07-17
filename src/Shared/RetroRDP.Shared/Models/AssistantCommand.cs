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
        Unknown
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
    }
}