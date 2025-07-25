using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

#if NET8_0_WINDOWS
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
#endif

namespace RetroRDPClient.Services
{
    /// <summary>
    /// Interface for capturing screenshots of RDP sessions
    /// </summary>
    public interface IScreenshotService
    {
        /// <summary>
        /// Capture a screenshot of the specified session
        /// </summary>
        /// <param name="sessionId">The session to capture</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The file path where the screenshot was saved, or null if failed</returns>
        Task<string?> CaptureSessionScreenshotAsync(string sessionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Capture a screenshot of the entire application window
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The file path where the screenshot was saved, or null if failed</returns>
        Task<string?> CaptureApplicationScreenshotAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Capture a screenshot with enhanced options
        /// </summary>
        /// <param name="sessionId">Session to capture (optional)</param>
        /// <param name="mode">Screenshot mode: session, application, fullscreen</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The file path where the screenshot was saved, or null if failed</returns>
        Task<string?> CaptureEnhancedScreenshotAsync(string? sessionId = null, string mode = "session", CancellationToken cancellationToken = default);

        /// <summary>
        /// Capture screenshots of all active sessions
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Array of file paths for captured screenshots</returns>
        Task<string[]> CaptureAllSessionsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for capturing screenshots of RDP sessions and the application
    /// </summary>
    public class ScreenshotService : IScreenshotService
    {
        private readonly ILogger<ScreenshotService>? _logger;
        private readonly string _screenshotDirectory;

        public ScreenshotService(ILogger<ScreenshotService>? logger = null)
        {
            _logger = logger;
            
            // Create screenshots directory in user's Pictures folder
            var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            _screenshotDirectory = Path.Combine(picturesPath, "RetroRDP Screenshots");
            
            try
            {
                Directory.CreateDirectory(_screenshotDirectory);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create screenshot directory");
                // Fallback to temp directory
                _screenshotDirectory = Path.GetTempPath();
            }
        }

        public async Task<string?> CaptureSessionScreenshotAsync(string sessionId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Capturing screenshot for session {SessionId}", sessionId);

                // For now, since we don't have actual RDP controls rendered,
                // we'll create a placeholder screenshot that shows session info
                return await CreatePlaceholderScreenshotAsync(sessionId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to capture session screenshot for {SessionId}", sessionId);
                return null;
            }
        }

        public async Task<string?> CaptureApplicationScreenshotAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger?.LogInformation("Capturing application screenshot");

#if NET8_0_WINDOWS
                return await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        var mainWindow = Application.Current.MainWindow;
                        if (mainWindow == null)
                        {
                            _logger?.LogWarning("Main window not found for screenshot");
                            return null;
                        }

                        // Create render target bitmap
                        var renderTargetBitmap = new RenderTargetBitmap(
                            (int)mainWindow.ActualWidth,
                            (int)mainWindow.ActualHeight,
                            96, 96, PixelFormats.Pbgra32);

                        renderTargetBitmap.Render(mainWindow);

                        // Save to file
                        var fileName = $"RetroRDP_App_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var filePath = Path.Combine(_screenshotDirectory, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                            encoder.Save(fileStream);
                        }

                        _logger?.LogInformation("Application screenshot saved to {FilePath}", filePath);
                        return filePath;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error capturing application screenshot");
                        return null;
                    }
                });
#else
                // Cross-platform fallback
                var fileName = $"RetroRDP_App_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(_screenshotDirectory, fileName);

                var screenshotInfo = $@"RetroRDP Application Screenshot
=================================
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Note: This is a placeholder for application screenshot.

On Windows with WPF, this would capture the actual application window.
Cross-platform screenshot functionality is not available in this build.

Screenshot info saved to: {filePath}
";

                await File.WriteAllTextAsync(filePath, screenshotInfo, cancellationToken);
                _logger?.LogInformation("Cross-platform screenshot placeholder saved to {FilePath}", filePath);
                return filePath;
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to capture application screenshot");
                return null;
            }
        }

        public async Task<string?> CaptureEnhancedScreenshotAsync(string? sessionId = null, string mode = "session", CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Capturing enhanced screenshot - Mode: {Mode}, Session: {SessionId}", mode, sessionId ?? "N/A");

            return mode.ToLowerInvariant() switch
            {
                "fullscreen" => await CaptureFullScreenAsync(cancellationToken),
                "application" => await CaptureApplicationScreenshotAsync(cancellationToken),
                "session" => sessionId != null ? await CaptureSessionScreenshotAsync(sessionId, cancellationToken) : null,
                _ => await CaptureApplicationScreenshotAsync(cancellationToken)
            };
        }

        public async Task<string[]> CaptureAllSessionsAsync(CancellationToken cancellationToken = default)
        {
            _logger?.LogInformation("Capturing screenshots of all active sessions");

            var results = new List<string>();

            // Simulate capturing multiple sessions
            // In a real implementation, this would iterate through active RDP sessions
            var simulatedSessionIds = new[] { "session_1", "session_2", "session_3" };

            foreach (var sessionId in simulatedSessionIds)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var screenshot = await CaptureSessionScreenshotAsync(sessionId, cancellationToken);
                if (screenshot != null)
                {
                    results.Add(screenshot);
                }

                // Brief delay between captures
                await Task.Delay(200, cancellationToken);
            }

            _logger?.LogInformation("Captured {Count} session screenshots", results.Count);
            return results.ToArray();
        }

        private async Task<string?> CaptureFullScreenAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger?.LogInformation("Capturing fullscreen screenshot");

#if NET8_0_WINDOWS
                return await Task.Run(() =>
                {
                    try
                    {
                        // Get primary screen dimensions
                        var screenWidth = (int)SystemParameters.PrimaryScreenWidth;
                        var screenHeight = (int)SystemParameters.PrimaryScreenHeight;

                        // Create a visual brush to capture the screen
                        var renderTargetBitmap = new RenderTargetBitmap(
                            screenWidth, screenHeight, 96, 96, PixelFormats.Pbgra32);

                        // For fullscreen capture, we'd typically use WinAPI or other screen capture methods
                        // This is a simplified version that captures what's available to WPF
                        var visual = Application.Current.MainWindow;
                        if (visual != null)
                        {
                            renderTargetBitmap.Render(visual);
                        }

                        var fileName = $"RetroRDP_Fullscreen_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        var filePath = Path.Combine(_screenshotDirectory, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            var encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                            encoder.Save(fileStream);
                        }

                        _logger?.LogInformation("Fullscreen screenshot saved to {FilePath}", filePath);
                        return filePath;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Error capturing fullscreen screenshot");
                        return null;
                    }
                }, cancellationToken);
#else
                // Cross-platform fallback
                var fileName = $"RetroRDP_Fullscreen_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(_screenshotDirectory, fileName);

                var screenshotInfo = $@"RetroRDP Fullscreen Screenshot
===============================
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Note: This is a placeholder for fullscreen screenshot.

On Windows, this would capture the entire screen including all applications.
Cross-platform fullscreen capture is not available in this build.

Screenshot info saved to: {filePath}
";

                await File.WriteAllTextAsync(filePath, screenshotInfo, cancellationToken);
                _logger?.LogInformation("Cross-platform fullscreen placeholder saved to {FilePath}", filePath);
                return filePath;
#endif
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to capture fullscreen screenshot");
                return null;
            }
        }

        private async Task<string?> CreatePlaceholderScreenshotAsync(string sessionId, CancellationToken cancellationToken)
        {
            try
            {
                // Since we don't have actual RDP controls in this cross-platform implementation,
                // create a informational image that shows this would capture the RDP session
                
                await Task.Delay(100, cancellationToken); // Simulate capture delay

                var fileName = $"RetroRDP_Session_{sessionId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(_screenshotDirectory, fileName);

                var screenshotInfo = $@"RetroRDP Screenshot Capture
=========================
Session ID: {sessionId}
Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Note: This is a placeholder for RDP session screenshot.

In a full Windows implementation, this would capture the actual RDP control content.
The screenshot would show the remote desktop session's current screen state.

Screenshot saved to: {filePath}
";

                await File.WriteAllTextAsync(filePath, screenshotInfo, cancellationToken);
                _logger?.LogInformation("Placeholder screenshot info saved to {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to create placeholder screenshot");
                return null;
            }
        }
    }
}