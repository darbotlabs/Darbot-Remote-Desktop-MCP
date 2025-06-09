using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Interface for local AI inference service using Phi-4 models
    /// Provides offline AI capabilities without requiring internet access
    /// </summary>
    public interface ILocalAIService
    {
        /// <summary>
        /// Indicates whether the AI service is initialized and ready to use
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Indicates whether a Phi-4 model is currently loaded
        /// </summary>
        bool IsModelLoaded { get; }

        /// <summary>
        /// Gets the name of the currently loaded model
        /// </summary>
        string? CurrentModelName { get; }

        /// <summary>
        /// Initialize the AI service and attempt to load a Phi-4 model
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate a response to the given prompt using the loaded Phi-4 model
        /// </summary>
        /// <param name="prompt">The user's input prompt</param>
        /// <param name="systemPrompt">Optional system prompt to guide the AI's behavior</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The AI's response text</returns>
        Task<string> GenerateResponseAsync(string prompt, string? systemPrompt = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generate a streaming response to the given prompt
        /// </summary>
        /// <param name="prompt">The user's input prompt</param>
        /// <param name="systemPrompt">Optional system prompt to guide the AI's behavior</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of response chunks</returns>
        IAsyncEnumerable<string> GenerateStreamingResponseAsync(string prompt, string? systemPrompt = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Dispose of resources and unload the model
        /// </summary>
        void Dispose();
    }
}