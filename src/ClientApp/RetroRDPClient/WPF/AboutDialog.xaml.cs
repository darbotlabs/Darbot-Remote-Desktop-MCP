using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace RetroRDPClient.WPF
{
    /// <summary>
    /// About dialog showing application information, version, and credits
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
            LoadApplicationInfo();
        }

        private void LoadApplicationInfo()
        {
            try
            {
                // Get version information
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version;
                var assemblyTitle = assembly.GetCustomAttribute<AssemblyTitleAttribute>();
                var assemblyDescription = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();

                // Set version text
                VersionText.Text = version?.ToString() ?? "1.0.0";

                // Set build date (from assembly)
                var buildDate = GetBuildDate(assembly);
                BuildDateText.Text = buildDate.ToString("yyyy-MM-dd");

                // Set .NET version
                DotNetVersionText.Text = Environment.Version.ToString();

                // Set platform information
                var architecture = RuntimeInformation.ProcessArchitecture.ToString();
                var osDescription = RuntimeInformation.OSDescription;
                PlatformText.Text = $"{osDescription} ({architecture})";

                // Set AI service status
                try
                {
                    // Try to determine AI service status
                    var hasPhiModel = System.IO.Directory.Exists("Models") || 
                                     Environment.GetEnvironmentVariable("WINDOWS_FOUNDRY_MODEL_PATH") != null;
                    var hasOpenAI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OPENAI_API_KEY")) ||
                                   !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT"));

                    if (hasPhiModel)
                        AIServiceText.Text = "Windows Foundry Local (Microsoft Phi-4)";
                    else if (hasOpenAI)
                        AIServiceText.Text = "OpenAI/Azure OpenAI";
                    else
                        AIServiceText.Text = "Fallback Mode (Local Processing)";
                }
                catch
                {
                    AIServiceText.Text = "AI Assistant Available";
                }
            }
            catch (Exception ex)
            {
                // Fallback values if reflection fails
                VersionText.Text = "1.0.0";
                BuildDateText.Text = DateTime.Now.ToString("yyyy-MM-dd");
                DotNetVersionText.Text = "8.0";
                PlatformText.Text = "Windows";
                AIServiceText.Text = "AI Assistant";
                
                Debug.WriteLine($"Error loading application info: {ex.Message}");
            }
        }

        private DateTime GetBuildDate(Assembly assembly)
        {
            try
            {
                // Get build date from assembly metadata
                var attribute = assembly.GetCustomAttribute<AssemblyMetadataAttribute>();
                if (attribute != null && DateTime.TryParse(attribute.Value, out var buildDate))
                {
                    return buildDate;
                }

                // Fallback: use assembly creation time
                var location = assembly.Location;
                if (!string.IsNullOrEmpty(location) && System.IO.File.Exists(location))
                {
                    return System.IO.File.GetCreationTime(location);
                }
            }
            catch
            {
                // Ignore errors
            }

            // Final fallback: current date
            return DateTime.Now;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void GitHubButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var url = "https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open GitHub repository: {ex.Message}", 
                               "Error", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Warning);
            }
        }

        private void UserGuideButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Try to open local UserGuide.md if available
                var userGuidePath = System.IO.Path.Combine(
                    System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
                    "UserGuide.md");

                if (System.IO.File.Exists(userGuidePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = userGuidePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Fallback to online version
                    var url = "https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP/blob/main/UserGuide.md";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open User Guide: {ex.Message}", 
                               "Error", 
                               MessageBoxButton.OK, 
                               MessageBoxImage.Warning);
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Apply cyber theme effects if available
            try
            {
                // Add subtle animation or effects here if desired
                this.Opacity = 0;
                var animation = new System.Windows.Media.Animation.DoubleAnimation(1, TimeSpan.FromMilliseconds(300));
                this.BeginAnimation(OpacityProperty, animation);
            }
            catch
            {
                // If animation fails, ensure dialog is visible
                this.Opacity = 1;
            }
        }
    }
}