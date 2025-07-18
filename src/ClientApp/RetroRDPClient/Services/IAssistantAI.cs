using System;
using System.Threading;
using System.Threading.Tasks;
using RetroRDP.Shared.Models;
using System.Collections.Generic;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Interface for AI-powered assistant that can parse natural language commands
    /// and invoke appropriate application actions with conversation context
    /// </summary>
    public interface IAssistantAI
    {
        /// <summary>
        /// Indicates whether the AI service is initialized and ready to use
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the name of the AI service being used
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Initialize the AI service
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Parse a natural language command and return structured action with context
        /// </summary>
        /// <param name="userInput">The user's natural language input</param>
        /// <param name="conversationContext">Current conversation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed assistant response with command and explanation</returns>
        Task<AssistantResponse> ParseCommandAsync(string userInput, ConversationContext? conversationContext = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate a conversational response for general questions
        /// </summary>
        /// <param name="userInput">The user's input</param>
        /// <param name="conversationContext">Current conversation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>AI-generated response</returns>
        Task<string> GenerateResponseAsync(string userInput, ConversationContext? conversationContext = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get or create conversation context
        /// </summary>
        /// <param name="conversationId">Optional conversation ID</param>
        /// <returns>Conversation context</returns>
        ConversationContext GetOrCreateConversation(string? conversationId = null);

        /// <summary>
        /// Execute chained commands in sequence
        /// </summary>
        /// <param name="commands">Commands to execute</param>
        /// <param name="conversationContext">Conversation context</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Results of each command execution</returns>
        Task<List<AssistantResponse>> ExecuteChainedCommandsAsync(AssistantCommand[] commands, ConversationContext conversationContext, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate streaming response for long operations
        /// </summary>
        /// <param name="userInput">User input</param>
        /// <param name="conversationContext">Conversation context</param>
        /// <param name="progressCallback">Progress callback</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Streaming response</returns>
        IAsyncEnumerable<string> GenerateStreamingResponseAsync(string userInput, ConversationContext? conversationContext = null, Action<int>? progressCallback = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create session profile from user input
        /// </summary>
        /// <param name="userInput">User description of session</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Generated session profile</returns>
        Task<SessionProfile?> CreateSessionProfileAsync(string userInput, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispose of resources
        /// </summary>
        void Dispose();
    }
}