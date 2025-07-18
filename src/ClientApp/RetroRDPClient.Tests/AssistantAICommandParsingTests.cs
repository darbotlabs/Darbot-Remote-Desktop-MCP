using RetroRDP.Shared.Models;
using RetroRDPClient.Services;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RetroRDPClient.Tests
{
    /// <summary>
    /// Additional tests for AssistantAI command parsing functionality
    /// </summary>
    public class AssistantAICommandParsingTests
    {
        private readonly AssistantAI _assistant;
        private readonly ILogger<AssistantAI> _logger;

        public AssistantAICommandParsingTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<AssistantAI>();
            _assistant = new AssistantAI(_logger);
            
            // Initialize in fallback mode for testing
            Task.Run(async () => await _assistant.InitializeAsync()).Wait();
        }

        [Theory]
        [InlineData("connect to 192.168.1.100", AssistantActionType.Connect)]
        [InlineData("rdp to server.example.com", AssistantActionType.Connect)]
        [InlineData("remote to my-server as admin", AssistantActionType.Connect)]
        public async Task ParseCommandAsync_ConnectVariations_ShouldBeRecognized(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
        }

        [Theory]
        [InlineData("disconnect session 1", AssistantActionType.Disconnect)]
        [InlineData("close connection to server", AssistantActionType.Disconnect)]
        [InlineData("end session", AssistantActionType.Disconnect)]
        public async Task ParseCommandAsync_DisconnectVariations_ShouldBeRecognized(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
        }

        [Theory]
        [InlineData("disconnect all sessions", AssistantActionType.DisconnectAll)]
        [InlineData("close all connections", AssistantActionType.DisconnectAll)]
        public async Task ParseCommandAsync_DisconnectAllVariations_ShouldBeRecognized(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
        }

        [Theory]
        [InlineData("show sessions", AssistantActionType.ListSessions)]
        [InlineData("list connections", AssistantActionType.ListSessions)]
        [InlineData("what sessions are active", AssistantActionType.ListSessions)]
        [InlineData("session status", AssistantActionType.ListSessions)]
        public async Task ParseCommandAsync_ListSessionsVariations_ShouldBeRecognized(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
        }

        [Theory]
        [InlineData("screenshot session 1", AssistantActionType.Screenshot, "session")]
        [InlineData("capture screen", AssistantActionType.Screenshot, "session")]
        [InlineData("take fullscreen screenshot", AssistantActionType.Screenshot, "fullscreen")]
        [InlineData("screenshot application", AssistantActionType.Screenshot, "application")]
        public async Task ParseCommandAsync_ScreenshotVariations_ShouldBeRecognizedWithMode(string input, AssistantActionType expectedAction, string expectedMode)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
            Assert.Equal(expectedMode, response.Command.ScreenshotMode);
        }

        [Theory]
        [InlineData("save connection profile", AssistantActionType.CreateProfile)]
        [InlineData("create profile for this connection", AssistantActionType.CreateProfile)]
        [InlineData("save this as a profile", AssistantActionType.CreateProfile)]
        public async Task ParseCommandAsync_ProfileCreation_ShouldBeRecognized(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
        }

        [Theory]
        [InlineData("load profile work-server", AssistantActionType.LoadProfile)]
        [InlineData("load connection profile", AssistantActionType.LoadProfile)]
        public async Task ParseCommandAsync_ProfileLoading_ShouldBeRecognized(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
        }

        [Theory]
        [InlineData("connect to server and take screenshot", AssistantActionType.ChainedCommands)]
        [InlineData("rdp to host then capture screen", AssistantActionType.ChainedCommands)]
        [InlineData("connect and list sessions", AssistantActionType.ChainedCommands)]
        public async Task ParseCommandAsync_ChainedCommands_ShouldBeRecognized(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
        }

        [Fact]
        public async Task ParseCommandAsync_WithConversationContext_ShouldMaintainHistory()
        {
            // Arrange
            var context = _assistant.GetOrCreateConversation();

            // Act
            var response1 = await _assistant.ParseCommandAsync("connect to server1", context);
            var response2 = await _assistant.ParseCommandAsync("now connect to server2", context);

            // Assert
            Assert.True(response1.Success);
            Assert.True(response2.Success);
            Assert.Equal(context.ConversationId, response1.ConversationId);
            Assert.Equal(context.ConversationId, response2.ConversationId);
            Assert.True(context.Messages.Count >= 2); // At least user and assistant messages
        }

        [Theory]
        [InlineData("hello there", AssistantActionType.GeneralHelp)]
        [InlineData("what can you do", AssistantActionType.GeneralHelp)]
        [InlineData("help me", AssistantActionType.GeneralHelp)]
        public async Task ParseCommandAsync_GeneralQuestions_ShouldProvideHelp(string input, AssistantActionType expectedAction)
        {
            // Act
            var response = await _assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(expectedAction, response.Command.Action);
            Assert.NotEmpty(response.Message);
        }

        [Fact]
        public async Task ParseCommandAsync_EmptyInput_ShouldHandleGracefully()
        {
            // Act
            var response = await _assistant.ParseCommandAsync("");

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.NotEmpty(response.Message);
        }

        [Fact]
        public async Task ParseCommandAsync_NullInput_ShouldHandleGracefully()
        {
            // Act
            var response = await _assistant.ParseCommandAsync(null!);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.NotEmpty(response.Message);
        }

        [Fact]
        public async Task ParseCommandAsync_VeryLongInput_ShouldHandleGracefully()
        {
            // Arrange
            var longInput = new string('a', 10000); // 10KB of text

            // Act
            var response = await _assistant.ParseCommandAsync(longInput);

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.NotEmpty(response.Message);
        }

        [Fact]
        public void CreateConversation_ShouldGenerateUniqueIds()
        {
            // Act
            var context1 = _assistant.GetOrCreateConversation();
            var context2 = _assistant.GetOrCreateConversation();

            // Assert
            Assert.NotEqual(context1.ConversationId, context2.ConversationId);
            Assert.Empty(context1.Messages);
            Assert.Empty(context2.Messages);
        }
    }
}