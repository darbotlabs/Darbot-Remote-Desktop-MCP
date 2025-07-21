using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Renci.SshNet;
using System.Threading;

namespace RetroRDPClient.WPF
{
    public partial class SshTerminalWindow : Window
    {
        private SshClient? _sshClient;
        private ShellStream? _shellStream;
        private bool _isConnected = false;
        private readonly StringBuilder _terminalBuffer = new();
        private CancellationTokenSource? _readCancellationTokenSource;
        private bool _waitingForPassword = false;
        private TaskCompletionSource<string>? _passwordTaskSource;

        public SshTerminalWindow()
        {
            InitializeComponent();
            Closed += SshTerminalWindow_Closed;
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isConnected)
            {
                await DisconnectAsync();
                return;
            }

            await ConnectAsync();
        }

        private async Task ConnectAsync()
        {
            try
            {
                ConnectButton.IsEnabled = false;
                ConnectionStatusText.Text = "Connecting...";
                AppendToTerminal("Attempting to connect...\n");

                var host = HostTextBox.Text.Trim();
                var username = UsernameTextBox.Text.Trim();
                var port = int.Parse(PortTextBox.Text);

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Please enter host and username.", "Connection Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Show password dialog
                _waitingForPassword = true;
                _passwordTaskSource = new TaskCompletionSource<string>();
                PasswordOverlay.Visibility = Visibility.Visible;
                PasswordBox.Focus();

                var password = await _passwordTaskSource.Task;
                PasswordOverlay.Visibility = Visibility.Collapsed;

                if (string.IsNullOrEmpty(password))
                {
                    AppendToTerminal("Connection cancelled.\n");
                    return;
                }

                var connectionInfo = new ConnectionInfo(host, port, username,
                    new PasswordAuthenticationMethod(username, password));

                _sshClient = new SshClient(connectionInfo);
                
                await Task.Run(() => _sshClient.Connect());

                if (_sshClient.IsConnected)
                {
                    _isConnected = true;
                    ConnectionStatusText.Text = $"Connected to {username}@{host}:{port}";
                    ConnectButton.Content = "🔌 Disconnect";
                    
                    // Create shell stream
                    _shellStream = _sshClient.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
                    
                    // Enable command input
                    CommandInput.IsEnabled = true;
                    SendButton.IsEnabled = true;
                    CommandInput.Focus();

                    // Start reading from shell
                    _readCancellationTokenSource = new CancellationTokenSource();
                    _ = Task.Run(() => ReadShellOutputAsync(_readCancellationTokenSource.Token));

                    AppendToTerminal($"Successfully connected to {host}\n");
                    UpdatePrompt(username, host);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Connection Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ConnectionStatusText.Text = "Connection failed";
                AppendToTerminal($"Connection failed: {ex.Message}\n");
            }
            finally
            {
                ConnectButton.IsEnabled = true;
                _waitingForPassword = false;
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                _readCancellationTokenSource?.Cancel();
                
                if (_shellStream != null)
                {
                    _shellStream.Close();
                    _shellStream.Dispose();
                    _shellStream = null;
                }

                if (_sshClient?.IsConnected == true)
                {
                    await Task.Run(() => _sshClient.Disconnect());
                }
                _sshClient?.Dispose();
                _sshClient = null;
                
                _isConnected = false;
                ConnectionStatusText.Text = "Disconnected";
                ConnectButton.Content = "🔗 Connect";
                
                // Disable command input
                CommandInput.IsEnabled = false;
                SendButton.IsEnabled = false;
                CommandInput.Clear();
                
                PromptText.Text = "$";
                AppendToTerminal("Disconnected from server.\n");
            }
            catch (Exception ex)
            {
                AppendToTerminal($"Disconnect error: {ex.Message}\n");
            }
        }

        private async Task ReadShellOutputAsync(CancellationToken cancellationToken)
        {
            if (_shellStream == null)
                return;

            var buffer = new byte[1024];
            
            try
            {
                while (!cancellationToken.IsCancellationRequested && _shellStream.CanRead)
                {
                    var bytesRead = await _shellStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead > 0)
                    {
                        var output = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Dispatcher.Invoke(() => AppendToTerminal(output));
                    }
                    else
                    {
                        await Task.Delay(50, cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => AppendToTerminal($"\nError reading output: {ex.Message}\n"));
            }
        }

        private void AppendToTerminal(string text)
        {
            _terminalBuffer.Append(text);
            TerminalOutput.Text = _terminalBuffer.ToString();
            
            // Auto-scroll to bottom
            TerminalScrollViewer.ScrollToEnd();
        }

        private void UpdatePrompt(string username, string hostname)
        {
            PromptText.Text = $"{username}@{hostname}:~$";
        }

        private async void CommandInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendCommand();
            }
            else if (e.Key == Key.Up)
            {
                // TODO: Implement command history
                e.Handled = true;
            }
            else if (e.Key == Key.Down)
            {
                // TODO: Implement command history
                e.Handled = true;
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendCommand();
        }

        private async Task SendCommand()
        {
            if (_shellStream == null || !_isConnected)
                return;

            var command = CommandInput.Text;
            if (string.IsNullOrWhiteSpace(command))
                return;

            try
            {
                // Display the command in terminal
                AppendToTerminal($"{PromptText.Text} {command}\n");
                
                // Send command to shell
                await Task.Run(() => _shellStream.WriteLine(command));
                
                CommandInput.Clear();
            }
            catch (Exception ex)
            {
                AppendToTerminal($"Error sending command: {ex.Message}\n");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _terminalBuffer.Clear();
            TerminalOutput.Text = "Terminal cleared.\n";
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PasswordOK_Click(sender, e);
            }
            else if (e.Key == Key.Escape)
            {
                PasswordCancel_Click(sender, e);
            }
        }

        private void PasswordOK_Click(object sender, RoutedEventArgs e)
        {
            if (_passwordTaskSource != null && !_passwordTaskSource.Task.IsCompleted)
            {
                _passwordTaskSource.SetResult(PasswordBox.Password);
                PasswordBox.Clear();
            }
        }

        private void PasswordCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_passwordTaskSource != null && !_passwordTaskSource.Task.IsCompleted)
            {
                _passwordTaskSource.SetResult("");
                PasswordBox.Clear();
            }
        }

        private async void SshTerminalWindow_Closed(object? sender, EventArgs e)
        {
            await DisconnectAsync();
        }

        protected override async void OnClosed(EventArgs e)
        {
            await DisconnectAsync();
            base.OnClosed(e);
        }
    }
}