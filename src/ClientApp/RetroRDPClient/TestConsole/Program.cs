using Microsoft.Extensions.Logging;
using RetroRDPClient.Services;

namespace RetroRDPClient.TestConsole
{
    /// <summary>
    /// Simple console application to test the Local AI Service
    /// Useful for verifying Phi-4 model integration without the full WPF UI
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point for testing the Local AI Service
        /// Call Program.TestAI() from the main application to run AI tests
        /// </summary>
        public static async Task TestAI()
        {
            await TestMain(Array.Empty<string>());
        }

        static async Task TestMain(string[] args)
        {
            Console.WriteLine("🤖 Retro RDP Client - Local AI Test Console");
            Console.WriteLine("============================================");
            Console.WriteLine();

            // Set up logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<LocalAIService>();

            // Initialize AI service
            using var aiService = new LocalAIService(logger);
            Console.WriteLine("Initializing Local AI Service...");
            
            bool initialized = await aiService.InitializeAsync();
            if (initialized)
            {
                Console.WriteLine($"✅ AI Service initialized successfully!");
                Console.WriteLine($"📊 Status: {aiService.CurrentModelName}");
                Console.WriteLine($"🧠 Model Loaded: {aiService.IsModelLoaded}");
            }
            else
            {
                Console.WriteLine("❌ AI Service initialization failed");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Enter 'quit' to exit. Try asking about RDP connections, troubleshooting, or security!");
            Console.WriteLine();

            // Interactive chat loop
            while (true)
            {
                Console.Write("👤 User: ");
                string? input = Console.ReadLine();
                
                if (string.IsNullOrWhiteSpace(input) || input.ToLower() == "quit")
                {
                    break;
                }

                try
                {
                    Console.Write("🤖 AssistBot: ");
                    
                    // Use streaming for better user experience
                    await foreach (var chunk in aiService.GenerateStreamingResponseAsync(input))
                    {
                        Console.Write(chunk);
                    }
                    Console.WriteLine();
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error: {ex.Message}");
                    Console.WriteLine();
                }
            }

            Console.WriteLine("👋 Thanks for testing the Local AI Service!");
        }
    }
}