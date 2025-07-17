using System;
using System.Threading;
using System.Threading.Tasks;
using RetroRDP.Shared.Models;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Interface for AI-powered assistant that can parse natural language commands
    /// and invoke appropriate application actions
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
        /// Parse a natural language command and return structured action
        /// </summary>
        /// <param name="userInput">The user's natural language input</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Parsed assistant response with command and explanation</returns>
        Task<AssistantResponse> ParseCommandAsync(string userInput, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate a conversational response for general questions
        /// </summary>
        /// <param name="userInput">The user's input</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>AI-generated response</returns>
        Task<string> GenerateResponseAsync(string userInput, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispose of resources
        /// </summary>
        void Dispose();
    }
}