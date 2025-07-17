using RetroRDP.Shared.Models;
using System;
using System.Windows;

namespace RetroRDPClient.WPF
{
    /// <summary>
    /// Connection dialog for creating new RDP sessions
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public RdpConnectionRequest? ConnectionRequest { get; private set; }

        public ConnectionDialog()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(HostTextBox.Text))
                {
                    MessageBox.Show("Server address is required.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    HostTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
                {
                    MessageBox.Show("Username is required.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    UsernameTextBox.Focus();
                    return;
                }

                if (!int.TryParse(PortTextBox.Text, out int port) || port <= 0 || port > 65535)
                {
                    MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    PortTextBox.Focus();
                    return;
                }

                if (!int.TryParse(ScreenWidthTextBox.Text, out int width) || width < 640)
                {
                    MessageBox.Show("Please enter a valid screen width (minimum 640).", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ScreenWidthTextBox.Focus();
                    return;
                }

                if (!int.TryParse(ScreenHeightTextBox.Text, out int height) || height < 480)
                {
                    MessageBox.Show("Please enter a valid screen height (minimum 480).", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    ScreenHeightTextBox.Focus();
                    return;
                }

                // Get color depth from combo box
                int colorDepth = ColorDepthComboBox.SelectedIndex switch
                {
                    0 => 15,
                    1 => 16,
                    2 => 24,
                    3 => 32,
                    _ => 32
                };

                // Create connection request
                ConnectionRequest = new RdpConnectionRequest
                {
                    Host = HostTextBox.Text.Trim(),
                    Username = UsernameTextBox.Text.Trim(),
                    Password = PasswordBox.Password,
                    SessionName = string.IsNullOrWhiteSpace(SessionNameTextBox.Text) 
                        ? HostTextBox.Text.Trim() 
                        : SessionNameTextBox.Text.Trim(),
                    Port = port,
                    ScreenWidth = width,
                    ScreenHeight = height,
                    ColorDepth = colorDepth,
                    FullScreen = FullScreenCheckBox.IsChecked == true
                };

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating connection: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}