using RetroRDP.Shared.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace RetroRDPClient.WPF
{
    /// <summary>
    /// Connection dialog for creating new RDP/SSH sessions
    /// </summary>
    public partial class ConnectionDialog : Window
    {
        public RdpConnectionRequest? ConnectionRequest { get; private set; }
        public string ConnectionType { get; private set; } = "RDP";

        public ConnectionDialog()
        {
            InitializeComponent();
        }

        private void ConnectionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConnectionTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var connectionType = selectedItem.Tag?.ToString() ?? "RDP";
                ConnectionType = connectionType;

                // Update default values based on connection type
                switch (connectionType)
                {
                    case "RDP":
                        PortTextBox.Text = "3389";
                        UsernameTextBox.Text = "Administrator";
                        SessionNameTextBox.Text = "RDP Session";
                        break;
                    case "SFTP":
                        PortTextBox.Text = "22";
                        UsernameTextBox.Text = "root";
                        SessionNameTextBox.Text = "SFTP Session";
                        break;
                    case "SSH":
                        PortTextBox.Text = "22";
                        UsernameTextBox.Text = "root";
                        SessionNameTextBox.Text = "SSH Terminal";
                        break;
                }
            }
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

                // Handle based on connection type
                if (ConnectionType == "SFTP")
                {
                    // Open SSH File Transfer window
                    var fileTransferWindow = new SshFileTransferWindow();
                    fileTransferWindow.HostTextBox.Text = HostTextBox.Text.Trim();
                    fileTransferWindow.UsernameTextBox.Text = UsernameTextBox.Text.Trim();
                    fileTransferWindow.PortTextBox.Text = PortTextBox.Text;
                    fileTransferWindow.PasswordBox.Password = PasswordBox.Password;
                    fileTransferWindow.Show();
                    
                    DialogResult = true;
                    Close();
                    return;
                }
                else if (ConnectionType == "SSH")
                {
                    // Open SSH Terminal window
                    var terminalWindow = new SshTerminalWindow();
                    terminalWindow.HostTextBox.Text = HostTextBox.Text.Trim();
                    terminalWindow.UsernameTextBox.Text = UsernameTextBox.Text.Trim();
                    terminalWindow.PortTextBox.Text = PortTextBox.Text;
                    terminalWindow.Show();
                    
                    DialogResult = true;
                    Close();
                    return;
                }

                // For RDP connections, we need additional fields
                if (ConnectionType == "RDP")
                {
                    // Find the screen resolution controls (these should exist in the original XAML)
                    var screenWidthTextBox = FindName("ScreenWidthTextBox") as TextBox;
                    var screenHeightTextBox = FindName("ScreenHeightTextBox") as TextBox;
                    var colorDepthComboBox = FindName("ColorDepthComboBox") as ComboBox;
                    var fullScreenCheckBox = FindName("FullScreenCheckBox") as CheckBox;

                    int width = 1024, height = 768, colorDepth = 32;
                    bool fullScreen = false;

                    if (screenWidthTextBox != null && !int.TryParse(screenWidthTextBox.Text, out width) || width < 640)
                    {
                        width = 1024; // Default value
                    }

                    if (screenHeightTextBox != null && !int.TryParse(screenHeightTextBox.Text, out height) || height < 480)
                    {
                        height = 768; // Default value
                    }

                    if (colorDepthComboBox != null)
                    {
                        colorDepth = colorDepthComboBox.SelectedIndex switch
                        {
                            0 => 15,
                            1 => 16,
                            2 => 24,
                            3 => 32,
                            _ => 32
                        };
                    }

                    if (fullScreenCheckBox != null)
                    {
                        fullScreen = fullScreenCheckBox.IsChecked == true;
                    }

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
                        FullScreen = fullScreen
                    };

                    DialogResult = true;
                    Close();
                }
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