using RetroRDP.Shared.Models;
using RetroRDPClient.Services;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace RetroRDPClient.Tests
{
    /// <summary>
    /// Tests for the AssistantAI service
    /// </summary>
    public class AssistantAITests
    {
        private readonly ILogger<AssistantAI> _logger;

        public AssistantAITests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<AssistantAI>();
        }

        [Fact]
        public async Task InitializeAsync_ShouldSucceed_EvenWithoutApiKey()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);

            // Act
            var result = await assistant.InitializeAsync();

            // Assert
            Assert.True(result);
            Assert.True(assistant.IsInitialized);
            Assert.Contains("Fallback", assistant.ServiceName);
        }

        [Fact]
        public async Task ParseCommandAsync_ShouldHandleConnectCommand_InFallbackMode()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);
            await assistant.InitializeAsync();

            // Act
            var response = await assistant.ParseCommandAsync("connect to server.example.com as admin");

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(AssistantActionType.Connect, response.Command.Action);
            Assert.Contains("connection", response.Message.ToLowerInvariant());
        }

        [Fact]
        public async Task ParseCommandAsync_ShouldHandleDisconnectAllCommand()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);
            await assistant.InitializeAsync();

            // Act
            var response = await assistant.ParseCommandAsync("disconnect all sessions");

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(AssistantActionType.DisconnectAll, response.Command.Action);
        }

        [Fact]
        public async Task ParseCommandAsync_ShouldHandleListSessionsCommand()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);
            await assistant.InitializeAsync();

            // Act
            var response = await assistant.ParseCommandAsync("list all sessions");

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(AssistantActionType.ListSessions, response.Command.Action);
        }

        [Fact]
        public async Task ParseCommandAsync_ShouldHandleScreenshotCommand()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);
            await assistant.InitializeAsync();

            // Act
            var response = await assistant.ParseCommandAsync("take screenshot of session");

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(AssistantActionType.Screenshot, response.Command.Action);
        }

        [Fact]
        public async Task ParseCommandAsync_ShouldHandleGeneralHelp()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);
            await assistant.InitializeAsync();

            // Act - using a question that won't trigger other patterns
            var response = await assistant.ParseCommandAsync("what is remote desktop?");

            // Assert
            Assert.True(response.Success);
            Assert.NotNull(response.Command);
            Assert.Equal(AssistantActionType.GeneralHelp, response.Command.Action);
        }

        [Fact]
        public async Task GenerateResponseAsync_ShouldReturnFallbackResponse()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);
            await assistant.InitializeAsync();

            // Act
            var response = await assistant.GenerateResponseAsync("hello");

            // Assert
            Assert.NotNull(response);
            Assert.Contains("AssistBot", response);
            Assert.Contains("hello", response.ToLowerInvariant());
        }

        [Fact]
        public void Dispose_ShouldNotThrow()
        {
            // Arrange
            var assistant = new AssistantAI(_logger);

            // Act & Assert
            assistant.Dispose(); // Should not throw
        }
    }

    /// <summary>
    /// Tests for the AssistantCommand model
    /// </summary>
    public class AssistantCommandTests
    {
        [Fact]
        public void IsValid_ShouldReturnTrue_ForValidConnectCommand()
        {
            // Arrange
            var command = new AssistantCommand
            {
                Action = AssistantActionType.Connect,
                Host = "server.example.com"
            };

            // Act
            var isValid = command.IsValid();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_ShouldReturnFalse_ForConnectCommandWithoutHost()
        {
            // Arrange
            var command = new AssistantCommand
            {
                Action = AssistantActionType.Connect
                // Host is null/empty
            };

            // Act
            var isValid = command.IsValid();

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void GetMissingParameters_ShouldReturnHostAndUsername_ForEmptyConnectCommand()
        {
            // Arrange
            var command = new AssistantCommand
            {
                Action = AssistantActionType.Connect
            };

            // Act
            var missing = command.GetMissingParameters();

            // Assert
            Assert.Contains("host", missing);
            Assert.Contains("username", missing);
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_ForDisconnectAllCommand()
        {
            // Arrange
            var command = new AssistantCommand
            {
                Action = AssistantActionType.DisconnectAll
            };

            // Act
            var isValid = command.IsValid();

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValid_ShouldReturnTrue_ForListSessionsCommand()
        {
            // Arrange
            var command = new AssistantCommand
            {
                Action = AssistantActionType.ListSessions
            };

            // Act
            var isValid = command.IsValid();

            // Assert
            Assert.True(isValid);
        }
    }
}