using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RetroRDPClient.WPF
{
    public partial class SshFileTransferWindow : Window
    {
        private SftpClient? _sftpClient;
        private bool _isConnected = false;
        private readonly ObservableCollection<FileSystemItem> _localFiles = new();
        private readonly ObservableCollection<SftpFileItem> _remoteFiles = new();

        public SshFileTransferWindow()
        {
            InitializeComponent();
            LocalFilesList.ItemsSource = _localFiles;
            RemoteFilesList.ItemsSource = _remoteFiles;
            
            // Load initial local directory
            LoadLocalDirectory(LocalPathTextBox.Text);
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

                var host = HostTextBox.Text.Trim();
                var username = UsernameTextBox.Text.Trim();
                var password = PasswordBox.Password;
                var port = int.Parse(PortTextBox.Text);

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username))
                {
                    MessageBox.Show("Please enter host and username.", "Connection Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var connectionInfo = new ConnectionInfo(host, port, username,
                    new PasswordAuthenticationMethod(username, password));

                _sftpClient = new SftpClient(connectionInfo);
                
                await Task.Run(() => _sftpClient.Connect());

                if (_sftpClient.IsConnected)
                {
                    _isConnected = true;
                    ConnectionStatusText.Text = $"Connected to {username}@{host}:{port}";
                    ConnectButton.Content = "🔌 Disconnect";
                    
                    // Enable controls
                    UploadButton.IsEnabled = true;
                    DownloadButton.IsEnabled = true;
                    RefreshRemoteButton.IsEnabled = true;
                    NewFolderButton.IsEnabled = true;
                    DeleteButton.IsEnabled = true;

                    // Load remote directory
                    await LoadRemoteDirectoryAsync(RemotePathTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed: {ex.Message}", "Connection Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ConnectionStatusText.Text = "Connection failed";
            }
            finally
            {
                ConnectButton.IsEnabled = true;
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                if (_sftpClient?.IsConnected == true)
                {
                    await Task.Run(() => _sftpClient.Disconnect());
                }
                _sftpClient?.Dispose();
                _sftpClient = null;
                
                _isConnected = false;
                ConnectionStatusText.Text = "Disconnected";
                ConnectButton.Content = "🔗 Connect";
                
                // Disable controls
                UploadButton.IsEnabled = false;
                DownloadButton.IsEnabled = false;
                RefreshRemoteButton.IsEnabled = false;
                NewFolderButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                
                // Clear remote files
                _remoteFiles.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Disconnect error: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void LoadLocalDirectory(string path)
        {
            try
            {
                _localFiles.Clear();
                
                if (!Directory.Exists(path))
                {
                    path = "C:\\";
                    LocalPathTextBox.Text = path;
                }

                var directory = new DirectoryInfo(path);
                
                // Add parent directory if not root
                if (directory.Parent != null)
                {
                    _localFiles.Add(new FileSystemItem 
                    { 
                        Name = "..", 
                        IsDirectory = true, 
                        FullPath = directory.Parent.FullName 
                    });
                }

                // Add directories
                foreach (var dir in directory.GetDirectories().OrderBy(d => d.Name))
                {
                    try
                    {
                        _localFiles.Add(new FileSystemItem
                        {
                            Name = dir.Name,
                            IsDirectory = true,
                            FullPath = dir.FullName,
                            LastWriteTime = dir.LastWriteTime
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip directories we can't access
                    }
                }

                // Add files
                foreach (var file in directory.GetFiles().OrderBy(f => f.Name))
                {
                    try
                    {
                        _localFiles.Add(new FileSystemItem
                        {
                            Name = file.Name,
                            IsDirectory = false,
                            FullPath = file.FullName,
                            Size = FormatFileSize(file.Length),
                            LastWriteTime = file.LastWriteTime
                        });
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Skip files we can't access
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading directory: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadRemoteDirectoryAsync(string path)
        {
            if (_sftpClient == null || !_sftpClient.IsConnected)
                return;

            try
            {
                StatusText.Text = "Loading remote directory...";
                _remoteFiles.Clear();

                var files = await Task.Run(() => _sftpClient.ListDirectory(path).ToList());

                // Add parent directory if not root
                if (path != "/" && path != ".")
                {
                    var parentPath = path.TrimEnd('/');
                    var lastSlash = parentPath.LastIndexOf('/');
                    parentPath = lastSlash > 0 ? parentPath.Substring(0, lastSlash) : "/";
                    
                    _remoteFiles.Add(new SftpFileItem
                    {
                        Name = "..",
                        IsDirectory = true,
                        FullPath = parentPath
                    });
                }

                // Add directories first, then files
                var sortedFiles = files.Where(f => f.Name != "." && f.Name != "..")
                                      .OrderBy(f => !f.IsDirectory)
                                      .ThenBy(f => f.Name);

                foreach (var file in sortedFiles)
                {
                    _remoteFiles.Add(new SftpFileItem
                    {
                        Name = file.Name,
                        IsDirectory = file.IsDirectory,
                        FullPath = file.FullName,
                        Size = file.IsDirectory ? "" : FormatFileSize(file.Length),
                        Permissions = file.Attributes.GroupCanRead + "-" + 
                                    file.Attributes.GroupCanWrite + "-" + 
                                    file.Attributes.GroupCanExecute
                    });
                }

                StatusText.Text = $"Loaded {_remoteFiles.Count} items";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading remote directory: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading directory";
            }
        }

        private void LocalFilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LocalFilesList.SelectedItem is FileSystemItem item && item.IsDirectory)
            {
                LocalPathTextBox.Text = item.FullPath;
                LoadLocalDirectory(item.FullPath);
            }
        }

        private async void RemoteFilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RemoteFilesList.SelectedItem is SftpFileItem item && item.IsDirectory)
            {
                RemotePathTextBox.Text = item.FullPath;
                await LoadRemoteDirectoryAsync(item.FullPath);
            }
        }

        private void BrowseLocalButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LocalPathTextBox.Text = dialog.SelectedPath;
                LoadLocalDirectory(dialog.SelectedPath);
            }
        }

        private async void RefreshRemoteButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadRemoteDirectoryAsync(RemotePathTextBox.Text);
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = LocalFilesList.SelectedItems.Cast<FileSystemItem>()
                .Where(f => !f.IsDirectory).ToList();

            if (!selectedFiles.Any())
            {
                MessageBox.Show("Please select files to upload.", "Upload", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await UploadFilesAsync(selectedFiles);
        }

        private async Task UploadFilesAsync(List<FileSystemItem> files)
        {
            if (_sftpClient == null || !_sftpClient.IsConnected)
                return;

            try
            {
                TransferProgress.Visibility = Visibility.Visible;
                TransferProgress.Maximum = files.Count;
                TransferProgress.Value = 0;

                foreach (var file in files)
                {
                    StatusText.Text = $"Uploading {file.Name}...";
                    
                    var remotePath = RemotePathTextBox.Text.TrimEnd('/') + "/" + file.Name;
                    
                    using (var fileStream = File.OpenRead(file.FullPath))
                    {
                        await Task.Run(() => _sftpClient.UploadFile(fileStream, remotePath));
                    }
                    
                    TransferProgress.Value++;
                }

                StatusText.Text = $"Uploaded {files.Count} file(s)";
                await LoadRemoteDirectoryAsync(RemotePathTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Upload failed: {ex.Message}", "Upload Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Upload failed";
            }
            finally
            {
                TransferProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = RemoteFilesList.SelectedItems.Cast<SftpFileItem>()
                .Where(f => !f.IsDirectory).ToList();

            if (!selectedFiles.Any())
            {
                MessageBox.Show("Please select files to download.", "Download", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            await DownloadFilesAsync(selectedFiles);
        }

        private async Task DownloadFilesAsync(List<SftpFileItem> files)
        {
            if (_sftpClient == null || !_sftpClient.IsConnected)
                return;

            try
            {
                TransferProgress.Visibility = Visibility.Visible;
                TransferProgress.Maximum = files.Count;
                TransferProgress.Value = 0;

                foreach (var file in files)
                {
                    StatusText.Text = $"Downloading {file.Name}...";
                    
                    var localPath = Path.Combine(LocalPathTextBox.Text, file.Name);
                    
                    using (var fileStream = File.Create(localPath))
                    {
                        await Task.Run(() => _sftpClient.DownloadFile(file.FullPath, fileStream));
                    }
                    
                    TransferProgress.Value++;
                }

                StatusText.Text = $"Downloaded {files.Count} file(s)";
                LoadLocalDirectory(LocalPathTextBox.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Download failed: {ex.Message}", "Download Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Download failed";
            }
            finally
            {
                TransferProgress.Visibility = Visibility.Collapsed;
            }
        }

        private async void NewFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderName = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter folder name:", "New Folder", "NewFolder");

            if (!string.IsNullOrEmpty(folderName) && _sftpClient?.IsConnected == true)
            {
                try
                {
                    var remotePath = RemotePathTextBox.Text.TrimEnd('/') + "/" + folderName;
                    await Task.Run(() => _sftpClient.CreateDirectory(remotePath));
                    await LoadRemoteDirectoryAsync(RemotePathTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to create folder: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = RemoteFilesList.SelectedItems.Cast<SftpFileItem>()
                .Where(f => f.Name != "..").ToList();

            if (!selectedFiles.Any())
            {
                MessageBox.Show("Please select files or folders to delete.", "Delete", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete {selectedFiles.Count} item(s)?", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes && _sftpClient?.IsConnected == true)
            {
                try
                {
                    foreach (var file in selectedFiles)
                    {
                        if (file.IsDirectory)
                            await Task.Run(() => _sftpClient.DeleteDirectory(file.FullPath));
                        else
                            await Task.Run(() => _sftpClient.DeleteFile(file.FullPath));
                    }
                    
                    await LoadRemoteDirectoryAsync(RemotePathTextBox.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Delete failed: {ex.Message}", "Delete Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        protected override async void OnClosed(EventArgs e)
        {
            await DisconnectAsync();
            base.OnClosed(e);
        }
    }

    public class FileSystemItem
    {
        public string Name { get; set; } = "";
        public bool IsDirectory { get; set; }
        public string FullPath { get; set; } = "";
        public string Size { get; set; } = "";
        public DateTime LastWriteTime { get; set; }
    }

    public class SftpFileItem
    {
        public string Name { get; set; } = "";
        public bool IsDirectory { get; set; }
        public string FullPath { get; set; } = "";
        public string Size { get; set; } = "";
        public string Permissions { get; set; } = "";
    }
}