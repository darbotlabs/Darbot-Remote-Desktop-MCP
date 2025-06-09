using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RetroRDPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// WPF Main Window - only used when deployed on Windows
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set up initial state
            CommandInput.Text = "Type your command here...";
            CommandInput.Foreground = System.Windows.Media.Brushes.Gray;
            
            // Add some sample chat messages to demonstrate the interface
            AddWelcomeMessages();
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
                CommandInput.Foreground = (System.Windows.Media.Brush)FindResource("CyberNeonCyanBrush");
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

        private void SendCommand()
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
            
            // Simulate AI response (placeholder for future AI integration)
            AddChatMessage($"🤖 AssistBot: Command received: '{command}'. RDP functionality will be implemented in future levels.", isUser: false);
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