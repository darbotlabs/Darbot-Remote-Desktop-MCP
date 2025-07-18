using RetroRDP.Shared.Models;
using RetroRDPClient.Services;
using Xunit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace RetroRDPClient.Tests
{
    /// <summary>
    /// Tests for the Gold Bolt enhanced features that exceed Level 4 expectations
    /// </summary>
    public class GoldBoltFeaturesTests
    {
        private readonly ILogger<AssistantAI> _assistantLogger;
        private readonly ILogger<SessionProfileService> _profileLogger;
        private readonly ILogger<ScreenshotService> _screenshotLogger;

        public GoldBoltFeaturesTests()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _assistantLogger = loggerFactory.CreateLogger<AssistantAI>();
            _profileLogger = loggerFactory.CreateLogger<SessionProfileService>();
            _screenshotLogger = loggerFactory.CreateLogger<ScreenshotService>();
        }

        [Fact]
        public async Task ConversationContext_ShouldMaintainState_AcrossMultipleInteractions()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();
            var conversation = assistant.GetOrCreateConversation();

            // Act - Multiple interactions
            var response1 = await assistant.ParseCommandAsync("connect to server1.com as admin", conversation);
            var response2 = await assistant.ParseCommandAsync("now take a screenshot", conversation);

            // Assert
            Assert.True(response1.Success);
            Assert.True(response2.Success);
            Assert.True(conversation.Messages.Count >= 2); // At least 2 user messages should be added
            Assert.Equal(conversation.ConversationId, response1.ConversationId);
            Assert.Equal(conversation.ConversationId, response2.ConversationId);
        }

        [Fact]
        public async Task ChainedCommands_ShouldParseMultipleActions_InSingleInput()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();

            // Act
            var response = await assistant.ParseCommandAsync("connect to server1 and server2, then take screenshots of both");

            // Assert
            Assert.True(response.Success);
            // In fallback mode, should detect chained commands
            Assert.Equal(AssistantActionType.ChainedCommands, response.Command?.Action);
        }

        [Fact]
        public async Task ExecuteChainedCommands_ShouldProcessMultipleCommands_InSequence()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();
            var conversation = assistant.GetOrCreateConversation();

            var commands = new[]
            {
                new AssistantCommand { Action = AssistantActionType.Connect, Host = "server1.com", Username = "admin", Priority = 1 },
                new AssistantCommand { Action = AssistantActionType.Screenshot, SessionId = "1", Priority = 2 },
                new AssistantCommand { Action = AssistantActionType.ListSessions, Priority = 3 }
            };

            // Act
            var results = await assistant.ExecuteChainedCommandsAsync(commands, conversation);

            // Assert
            Assert.Equal(3, results.Count);
            Assert.All(results, result => Assert.True(result.Success));
            Assert.True(conversation.Messages.Count >= 3); // Should have added progress messages
        }

        [Fact]
        public async Task StreamingResponse_ShouldProvideProgressiveUpdates()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();
            var conversation = assistant.GetOrCreateConversation();
            var progressUpdates = new List<int>();

            // Act
            var responses = new List<string>();
            await foreach (var response in assistant.GenerateStreamingResponseAsync(
                "help me troubleshoot connection issues", 
                conversation, 
                progress => progressUpdates.Add(progress)))
            {
                responses.Add(response);
            }

            // Assert
            Assert.NotEmpty(responses);
            Assert.NotEmpty(progressUpdates);
            Assert.Contains(100, progressUpdates); // Should reach 100% completion
            Assert.All(responses, response => Assert.False(string.IsNullOrEmpty(response)));
        }

        [Fact]
        public async Task CreateSessionProfile_ShouldGenerateProfile_FromNaturalLanguage()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();

            // Act
            var profile = await assistant.CreateSessionProfileAsync("save connection to production-server.company.com as serviceaccount for daily monitoring");

            // Assert
            Assert.NotNull(profile);
            Assert.False(string.IsNullOrEmpty(profile.Name));
            Assert.Equal("production-server.company.com", profile.Host);
            Assert.Equal("fallback", profile.CreatedBy); // Since no API key in tests
        }

        [Fact]
        public async Task EnhancedScreenshot_ShouldSupportMultipleModes()
        {
            // Arrange
            var screenshotService = new ScreenshotService(_screenshotLogger);

            // Act & Assert - Test different modes
            var sessionScreenshot = await screenshotService.CaptureEnhancedScreenshotAsync("session1", "session");
            var appScreenshot = await screenshotService.CaptureEnhancedScreenshotAsync(null, "application");
            var fullscreenScreenshot = await screenshotService.CaptureEnhancedScreenshotAsync(null, "fullscreen");

            Assert.NotNull(sessionScreenshot);
            Assert.NotNull(appScreenshot);
            Assert.NotNull(fullscreenScreenshot);
            Assert.Contains("Session", sessionScreenshot);
            Assert.Contains("App", appScreenshot);
            Assert.Contains("Fullscreen", fullscreenScreenshot);
        }

        [Fact]
        public async Task CaptureAllSessions_ShouldCaptureMultipleScreenshots()
        {
            // Arrange
            var screenshotService = new ScreenshotService(_screenshotLogger);

            // Act
            var screenshots = await screenshotService.CaptureAllSessionsAsync();

            // Assert
            Assert.NotEmpty(screenshots);
            Assert.True(screenshots.Length <= 3); // Simulated sessions
            Assert.All(screenshots, screenshot => Assert.False(string.IsNullOrEmpty(screenshot)));
        }

        [Fact]
        public async Task SessionProfileService_ShouldManageProfiles_EndToEnd()
        {
            // Arrange
            var profileService = new SessionProfileService(_profileLogger);
            var testProfile = new SessionProfile
            {
                Name = "Test Profile",
                Host = "test-server.local",
                Username = "testuser",
                Description = "Test profile for unit testing",
                Tags = new[] { "test", "development" }
            };

            try
            {
                // Act & Assert - Save
                var saveResult = await profileService.SaveProfileAsync(testProfile);
                Assert.True(saveResult);

                // Act & Assert - Load
                var loadedProfile = await profileService.LoadProfileAsync(testProfile.Name);
                Assert.NotNull(loadedProfile);
                Assert.Equal(testProfile.Host, loadedProfile!.Host);
                Assert.Equal(testProfile.Username, loadedProfile.Username);

                // Act & Assert - Update usage
                await profileService.UpdateProfileUsageAsync(testProfile.Name);
                var updatedProfile = await profileService.LoadProfileAsync(testProfile.Name);
                Assert.NotNull(updatedProfile);
                Assert.Equal(1, updatedProfile!.UseCount);

                // Act & Assert - Search
                var searchResults = await profileService.SearchProfilesAsync("test");
                Assert.Contains(searchResults, p => p.Name == testProfile.Name);

                // Act & Assert - Get all
                var allProfiles = await profileService.GetAllProfilesAsync();
                Assert.Contains(allProfiles, p => p.Name == testProfile.Name);
            }
            finally
            {
                // Cleanup
                await profileService.DeleteProfileAsync(testProfile.Name);
            }
        }

        [Fact]
        public async Task EnhancedCommandParsing_ShouldExtractDetails_FromComplexInput()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();

            // Act - Test complex parsing scenarios
            var connectResponse = await assistant.ParseCommandAsync("connect to production-db.company.com as dbadmin");
            var screenshotResponse = await assistant.ParseCommandAsync("take fullscreen screenshot of session 2");
            var profileResponse = await assistant.ParseCommandAsync("save this connection as production database profile");

            // Assert
            Assert.True(connectResponse.Success);
            Assert.Equal(AssistantActionType.Connect, connectResponse.Command?.Action);
            Assert.Equal("production-db.company.com", connectResponse.Command?.Host);
            Assert.Equal("dbadmin", connectResponse.Command?.Username);

            Assert.True(screenshotResponse.Success);
            Assert.Equal(AssistantActionType.Screenshot, screenshotResponse.Command?.Action);
            Assert.Equal("fullscreen", screenshotResponse.Command?.ScreenshotMode);

            Assert.True(profileResponse.Success);
            Assert.Equal(AssistantActionType.CreateProfile, profileResponse.Command?.Action);
        }

        [Fact]
        public async Task AdvancedErrorHandling_ShouldProvideHelpfulSuggestions()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();

            // Act - Test with incomplete commands
            var incompleteConnect = await assistant.ParseCommandAsync("connect to");
            var incompleteScreenshot = await assistant.ParseCommandAsync("screenshot");

            // Assert
            Assert.True(incompleteConnect.Success || incompleteConnect.NeedsMoreInfo);
            Assert.True(incompleteScreenshot.Success || incompleteScreenshot.NeedsMoreInfo);
            
            if (incompleteConnect.NeedsMoreInfo)
            {
                Assert.NotNull(incompleteConnect.FollowUpQuestions);
                Assert.NotEmpty(incompleteConnect.FollowUpQuestions);
            }
        }

        [Fact]
        public void ConversationContext_ShouldTrackMetadata_Correctly()
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            var conversation = assistant.GetOrCreateConversation();

            // Act
            conversation.SessionContext["currentHost"] = "test-server";
            conversation.UserPreferences["preferredMode"] = "fullscreen";
            conversation.Messages.Add(new ConversationMessage
            {
                Role = "user",
                Content = "test message",
                Metadata = new Dictionary<string, object> { ["testKey"] = "testValue" }
            });

            // Assert
            Assert.Equal("test-server", conversation.SessionContext["currentHost"]);
            Assert.Equal("fullscreen", conversation.UserPreferences["preferredMode"]);
            Assert.Single(conversation.Messages);
            Assert.Equal("testValue", conversation.Messages[0].Metadata?["testKey"]);
        }

        [Theory]
        [InlineData("connect to server1 and server2", AssistantActionType.ChainedCommands)]
        [InlineData("save connection as myprofile", AssistantActionType.CreateProfile)]
        [InlineData("load profile myprofile", AssistantActionType.LoadProfile)]
        [InlineData("screenshot session 1", AssistantActionType.Screenshot)]
        [InlineData("list all sessions", AssistantActionType.ListSessions)]
        public async Task EnhancedCommandParsing_ShouldRecognizeActionTypes(string input, AssistantActionType expectedAction)
        {
            // Arrange
            var assistant = new AssistantAI(_assistantLogger);
            await assistant.InitializeAsync();

            // Act
            var response = await assistant.ParseCommandAsync(input);

            // Assert
            Assert.True(response.Success);
            Assert.Equal(expectedAction, response.Command?.Action);
        }
    }
}