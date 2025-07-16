using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RetroRDPClient.Services;

namespace RetroRDPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// WPF Main Window - only used when deployed on Windows
    /// Features local AI integration with Phi-4 model support
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILocalAIService _aiService;
        private readonly ILogger<MainWindow>? _logger;

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize logging (simple console logger for now)
            _logger = LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<MainWindow>();
            
            // Initialize Local AI Service
            _aiService = new LocalAIService(_logger != null ? 
                LoggerFactory.Create(builder => builder.AddDebug()).CreateLogger<LocalAIService>() : 
                null);
            
            // Set up initial state
            CommandInput.Text = "Type your command here...";
            CommandInput.Foreground = System.Windows.Media.Brushes.Gray;
            
            // Initialize AI service and add welcome messages
            InitializeAIServiceAsync();
        }

        private async void InitializeAIServiceAsync()
        {
            try
            {
                var success = await _aiService.InitializeAsync();
                if (success)
                {
                    AddWelcomeMessages();
                    AddChatMessage($"🤖 AssistBot: Local AI service initialized successfully! Running: {_aiService.CurrentModelName}", isUser: false);
                    _logger?.LogInformation("AI service initialized successfully");
                }
                else
                {
                    AddWelcomeMessages();
                    AddChatMessage("🤖 AssistBot: AI service initialization failed. Using basic fallback mode.", isUser: false);
                    _logger?.LogWarning("AI service initialization failed");
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing AI service");
                AddWelcomeMessages();
                AddChatMessage("🤖 AssistBot: Welcome! I'm running in safe mode and ready to help with your RDP needs.", isUser: false);
            }
        }

        private void AddWelcomeMessages()
        {
            // The welcome messages are already in XAML, but we can add dynamic ones here
            // This method can be extended to add more interactive chat functionality
        }

        private void CommandInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (CommandInput.Text == "Type your command here...")
            {
                CommandInput.Text = "";
                var brushResource = TryFindResource("CyberNeonCyanBrush");
                if (brushResource is System.Windows.Media.Brush brush)
                {
                    CommandInput.Foreground = brush;
                }
                else
                {
                    CommandInput.Foreground = System.Windows.Media.Brushes.Black; // Fallback brush
                    _logger?.LogWarning("Resource 'CyberNeonCyanBrush' is missing or not a Brush.");
                }
            }
        }

        private void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Ctrl+Enter to send command
                SendCommand();
                e.Handled = true;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendCommand();
        }

        private async void SendCommand()
        {
            string command = CommandInput.Text.Trim();
            
            if (string.IsNullOrEmpty(command) || command == "Type your command here...")
            {
                return;
            }

            // Add user message to chat
            AddChatMessage($"👤 User: {command}", isUser: true);
            
            // Clear input
            CommandInput.Text = "";
            
            try
            {
                // Use local AI service to generate response
                var response = await _aiService.GenerateResponseAsync(command);
                AddChatMessage(response, isUser: false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating AI response");
                AddChatMessage($"🤖 AssistBot: I encountered an error processing your request: {ex.Message}. Please try again.", isUser: false);
            }
        }

        private void AddChatMessage(string message, bool isUser)
        {
            try
            {
                var border = new Border
                {
                    Background = (System.Windows.Media.Brush)FindResource("CyberGrayBrush"),
                    BorderBrush = isUser ? 
                        (System.Windows.Media.Brush)FindResource("CyberNeonPinkBrush") : 
                        (System.Windows.Media.Brush)FindResource("CyberNeonGreenBrush"),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new System.Windows.CornerRadius(5),
                    Margin = new Thickness(0, 0, 0, 10),
                    Padding = new Thickness(10)
                };

                var textBlock = new TextBlock
                {
                    Text = message,
                    Style = (Style)FindResource("CyberText"),
                    FontSize = 11,
                    Foreground = isUser ? 
                        (System.Windows.Media.Brush)FindResource("CyberNeonPinkBrush") : 
                        (System.Windows.Media.Brush)FindResource("CyberNeonGreenBrush"),
                    TextWrapping = TextWrapping.Wrap
                };

                border.Child = textBlock;
                ChatPanel.Children.Add(border);

                // Scroll to bottom
                var scrollViewer = FindScrollViewer(ChatPanel);
                scrollViewer?.ScrollToBottom();
            }
            catch (Exception ex)
            {
                // Handle any errors gracefully
                System.Diagnostics.Debug.WriteLine($"Error adding chat message: {ex.Message}");
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject parent)
        {
            if (parent == null) return null;

            var parentGrid = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            while (parentGrid != null && !(parentGrid is ScrollViewer))
            {
                parentGrid = System.Windows.Media.VisualTreeHelper.GetParent(parentGrid);
            }
            
            return parentGrid as ScrollViewer;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Try to enable Fluent design effects (Windows 11 specific)
            try
            {
                EnableBlurEffect();
            }
            catch
            {
                // Silently fail if not supported (e.g., on Windows 10 or Linux)
            }
        }

        private void EnableBlurEffect()
        {
            // This is a placeholder for Fluent design effects
            // In a real implementation, you would use Windows 11 APIs
            // For now, we'll rely on the gradient background for the effect
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up AI service resources
            _aiService?.Dispose();
            base.OnClosed(e);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Handle responsive behavior
            if (e.NewSize.Width < 1000)
            {
                // Could collapse sidebar or adjust layout for smaller windows
                // For now, maintain minimum window size requirements
            }
        }
    }
}