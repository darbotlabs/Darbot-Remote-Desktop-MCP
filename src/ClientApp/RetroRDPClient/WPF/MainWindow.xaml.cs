using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using RetroRDPClient.Services;
using RetroRDP.Shared.Models;
using RetroRDP.Shared.Services;
using RetroRDPClient.WPF;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RetroRDPClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// WPF Main Window - only used when deployed on Windows
    /// Features local AI integration with Phi-4 model support and RDP session management
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILocalAIService _aiService;
        private readonly IAssistantAI _assistantAI;
        private readonly IScreenshotService _screenshotService;
        private readonly ISessionManager _sessionManager;
        private readonly ILogger<MainWindow>? _logger;
        private readonly Dictionary<string, TabItem> _sessionTabs = new();

        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize logging (simple console logger for now)
            var loggerFactory = LoggerFactory.Create(builder => builder.AddDebug());
            _logger = loggerFactory.CreateLogger<MainWindow>();
            
            // Initialize Local AI Service
            _aiService = new LocalAIService(_logger != null ? 
                loggerFactory.CreateLogger<LocalAIService>() : 
                null);

            // Initialize Assistant AI Service
            _assistantAI = new AssistantAI(_logger != null ?
                loggerFactory.CreateLogger<AssistantAI>() :
                null);

            // Initialize Screenshot Service
            _screenshotService = new ScreenshotService(_logger != null ?
                loggerFactory.CreateLogger<ScreenshotService>() :
                null);

            // Initialize Session Manager
            _sessionManager = new SessionManager(_logger != null ?
                loggerFactory.CreateLogger<SessionManager>() :
                null);

            // Subscribe to session events
            _sessionManager.SessionStatusChanged += OnSessionStatusChanged;
            
            // Set up initial state
            CommandInput.Text = "Type your command here...";
            CommandInput.Foreground = System.Windows.Media.Brushes.Gray;

            // Wire up navigation events
            NavigationList.SelectionChanged += NavigationList_SelectionChanged;
            
            // Initialize AI services and add welcome messages
            InitializeServicesAsync();

            // Set up session tabs
            InitializeSessionTabs();
        }

        private void InitializeSessionTabs()
        {
            // Clear existing tabs
            SessionTabs.Items.Clear();

            // Add the "New Session" tab
            AddNewSessionTab();

            _logger?.LogInformation("Session tabs initialized");
        }

        private void AddNewSessionTab()
        {
            var newSessionTab = new TabItem
            {
                Header = "➕ New Session",
                Style = (Style)FindResource("CyberTabItem")
            };

            var border = new Border
            {
                Background = (System.Windows.Media.Brush)FindResource("CyberDarkGrayBrush"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("CyberNeonGreenBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new System.Windows.CornerRadius(5),
                Margin = new Thickness(10)
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                MaxWidth = 400
            };

            var titleText = new TextBlock
            {
                Text = "🚀 Create New RDP Session",
                Style = (Style)FindResource("CyberText"),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 30)
            };

            var createButton = new Button
            {
                Style = (Style)FindResource("CyberButton"),
                Content = "⚡ OPEN CONNECTION DIALOG",
                Padding = new Thickness(25, 10),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            createButton.Click += CreateNewSessionButton_Click;

            stackPanel.Children.Add(titleText);
            stackPanel.Children.Add(createButton);
            border.Child = stackPanel;
            newSessionTab.Content = border;

            SessionTabs.Items.Add(newSessionTab);
        }

        private async void CreateNewSessionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ConnectionDialog
                {
                    Owner = this
                };

                if (dialog.ShowDialog() == true && dialog.ConnectionRequest != null)
                {
                    _logger?.LogInformation("Creating new RDP session to {Host}", dialog.ConnectionRequest.Host);
                    
                    var sessionId = await _sessionManager.StartSessionAsync(dialog.ConnectionRequest);
                    if (sessionId != null)
                    {
                        AddChatMessage($"🤖 AssistBot: Starting new RDP session to {dialog.ConnectionRequest.Host}...", false);
                        _logger?.LogInformation("Session {SessionId} started successfully", sessionId);
                    }
                    else
                    {
                        AddChatMessage("🤖 AssistBot: Failed to start RDP session. Please check your connection parameters.", false);
                        _logger?.LogWarning("Failed to start RDP session");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating new session");
                AddChatMessage($"🤖 AssistBot: Error creating session: {ex.Message}", false);
            }
        }

        private void OnSessionStatusChanged(object? sender, SessionStatusChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var session = _sessionManager.GetSession(e.SessionId);
                    if (session == null) return;

                    if (e.NewStatus == RdpSessionStatus.Connecting && !_sessionTabs.ContainsKey(e.SessionId))
                    {
                        // Create new tab for the session
                        CreateSessionTab(session);
                    }
                    else if (_sessionTabs.TryGetValue(e.SessionId, out var existingTab))
                    {
                        // Update existing tab
                        UpdateSessionTab(existingTab, session);

                        if (e.NewStatus == RdpSessionStatus.Disconnected)
                        {
                            // Remove tab when session is disconnected
                            SessionTabs.Items.Remove(existingTab);
                            _sessionTabs.Remove(e.SessionId);
                        }
                    }

                    // Add status message to chat
                    var statusMessage = e.NewStatus switch
                    {
                        RdpSessionStatus.Connecting => $"🟡 Connecting to {session.Host}...",
                        RdpSessionStatus.Connected => $"🟢 Connected to {session.Host}",
                        RdpSessionStatus.Disconnected => $"⚪ Disconnected from {session.Host}",
                        RdpSessionStatus.Failed => $"🔴 Connection failed to {session.Host}: {e.ErrorMessage}",
                        _ => $"Status changed for {session.Host}: {e.NewStatus}"
                    };

                    AddChatMessage($"🤖 AssistBot: {statusMessage}", false);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error handling session status change");
                }
            });
        }

        private void CreateSessionTab(RdpSession session)
        {
            var sessionTab = new TabItem
            {
                Header = $"🖥️ {session.DisplayName}",
                Style = (Style)FindResource("CyberTabItem"),
                Tag = session.SessionId
            };

            var border = new Border
            {
                Background = (System.Windows.Media.Brush)FindResource("CyberDarkGrayBrush"),
                BorderBrush = (System.Windows.Media.Brush)FindResource("CyberNeonCyanBrush"),
                BorderThickness = new Thickness(1),
                CornerRadius = new System.Windows.CornerRadius(5),
                Margin = new Thickness(10)
            };

            var grid = new Grid();

            // Main content area for RDP view
            var rdpContent = CreateRdpContent(session);
            grid.Children.Add(rdpContent);

            // Session info overlay
            var sessionInfo = CreateSessionInfoOverlay(session);
            grid.Children.Add(sessionInfo);

            border.Child = grid;
            sessionTab.Content = border;

            // Add context menu for tab operations
            AddTabContextMenu(sessionTab, session.SessionId);

            _sessionTabs[session.SessionId] = sessionTab;
            SessionTabs.Items.Insert(SessionTabs.Items.Count - 1, sessionTab); // Insert before "New Session" tab
            SessionTabs.SelectedItem = sessionTab;
        }

        private FrameworkElement CreateRdpContent(RdpSession session)
        {
            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var rdpViewText = new TextBlock
            {
                Text = "🖥️ Remote Desktop Session View",
                Style = (Style)FindResource("CyberText"),
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };

            var statusText = new TextBlock
            {
                Text = session.StatusDisplay,
                Style = (Style)FindResource("CyberText"),
                FontSize = 14,
                Foreground = GetStatusBrush(session.Status),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10),
                Tag = "StatusText" // For easy updates
            };

            var actionButton = new Button
            {
                Style = (Style)FindResource("CyberButton"),
                Content = GetActionButtonText(session.Status),
                Padding = new Thickness(20, 8),
                Margin = new Thickness(0, 10),
                Tag = session.SessionId
            };
            actionButton.Click += SessionActionButton_Click;

            stackPanel.Children.Add(rdpViewText);
            stackPanel.Children.Add(statusText);
            stackPanel.Children.Add(actionButton);

            // In a real implementation, this would contain the actual RDP control
            // For now, we show a placeholder with session information
            if (session.Status == RdpSessionStatus.Connected)
            {
                var rdpSimulator = new Border
                {
                    Background = System.Windows.Media.Brushes.Black,
                    BorderBrush = (System.Windows.Media.Brush)FindResource("CyberNeonGreenBrush"),
                    BorderThickness = new Thickness(2),
                    CornerRadius = new System.Windows.CornerRadius(5),
                    Margin = new Thickness(0, 20, 0, 0),
                    Width = 400,
                    Height = 200
                };

                var simulatorText = new TextBlock
                {
                    Text = $"🖥️ RDP Session Active\n\nConnected to: {session.Host}\nUser: {session.Username}\nResolution: {session.ScreenWidth}x{session.ScreenHeight}\n\n(Real RDP view would appear here on Windows)",
                    Style = (Style)FindResource("CyberText"),
                    FontSize = 11,
                    Foreground = (System.Windows.Media.Brush)FindResource("CyberNeonGreenBrush"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = TextAlignment.Center
                };

                rdpSimulator.Child = simulatorText;
                stackPanel.Children.Add(rdpSimulator);
            }

            return stackPanel;
        }

        private StackPanel CreateSessionInfoOverlay(RdpSession session)
        {
            var sessionInfo = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(20),
                Tag = "SessionInfo" // For easy updates
            };

            var serverText = new TextBlock
            {
                Text = $"Server: {session.Host}:{session.Port}",
                Style = (Style)FindResource("CyberText"),
                FontSize = 10,
                Opacity = 0.7
            };

            var userText = new TextBlock
            {
                Text = $"User: {session.Username}",
                Style = (Style)FindResource("CyberText"),
                FontSize = 10,
                Opacity = 0.7
            };

            var resolutionText = new TextBlock
            {
                Text = $"Resolution: {session.ScreenWidth}x{session.ScreenHeight}",
                Style = (Style)FindResource("CyberText"),
                FontSize = 10,
                Opacity = 0.7
            };

            var createdText = new TextBlock
            {
                Text = $"Created: {session.CreatedAt:HH:mm:ss}",
                Style = (Style)FindResource("CyberText"),
                FontSize = 10,
                Opacity = 0.7
            };

            sessionInfo.Children.Add(serverText);
            sessionInfo.Children.Add(userText);
            sessionInfo.Children.Add(resolutionText);
            sessionInfo.Children.Add(createdText);

            return sessionInfo;
        }

        private void AddTabContextMenu(TabItem tabItem, string sessionId)
        {
            var contextMenu = new ContextMenu();

            var reconnectMenuItem = new MenuItem
            {
                Header = "🔄 Reconnect",
                Tag = sessionId
            };
            reconnectMenuItem.Click += ReconnectMenuItem_Click;

            var disconnectMenuItem = new MenuItem
            {
                Header = "🔌 Disconnect",
                Tag = sessionId
            };
            disconnectMenuItem.Click += DisconnectMenuItem_Click;

            contextMenu.Items.Add(reconnectMenuItem);
            contextMenu.Items.Add(disconnectMenuItem);

            tabItem.ContextMenu = contextMenu;
        }

        private System.Windows.Media.Brush GetStatusBrush(RdpSessionStatus status)
        {
            return status switch
            {
                RdpSessionStatus.Connected => (System.Windows.Media.Brush)FindResource("CyberNeonGreenBrush"),
                RdpSessionStatus.Connecting or RdpSessionStatus.Reconnecting => (System.Windows.Media.Brush)FindResource("CyberNeonYellowBrush"),
                RdpSessionStatus.Failed => (System.Windows.Media.Brush)FindResource("CyberNeonPinkBrush"),
                _ => (System.Windows.Media.Brush)FindResource("CyberNeonCyanBrush")
            };
        }

        private string GetActionButtonText(RdpSessionStatus status)
        {
            return status switch
            {
                RdpSessionStatus.Connected => "🔌 DISCONNECT",
                RdpSessionStatus.Connecting => "⏳ CONNECTING...",
                RdpSessionStatus.Reconnecting => "⏳ RECONNECTING...",
                RdpSessionStatus.Failed => "🔄 RETRY",
                _ => "🔌 CONNECT"
            };
        }

        private async void SessionActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string sessionId)
            {
                var session = _sessionManager.GetSession(sessionId);
                if (session == null) return;

                try
                {
                    switch (session.Status)
                    {
                        case RdpSessionStatus.Connected:
                            await _sessionManager.EndSessionAsync(sessionId);
                            break;
                        case RdpSessionStatus.Failed:
                        case RdpSessionStatus.Disconnected:
                            await _sessionManager.ReconnectSessionAsync(sessionId);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error performing session action for {SessionId}", sessionId);
                    AddChatMessage($"🤖 AssistBot: Error performing action: {ex.Message}", false);
                }
            }
        }

        private async void ReconnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string sessionId)
            {
                await _sessionManager.ReconnectSessionAsync(sessionId);
            }
        }

        private async void DisconnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string sessionId)
            {
                await _sessionManager.EndSessionAsync(sessionId);
            }
        }

        private void UpdateSessionTab(TabItem tabItem, RdpSession session)
        {
            // Update tab header
            tabItem.Header = $"🖥️ {session.DisplayName}";

            // Find and update status text and action button
            if (tabItem.Content is Border border && border.Child is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is StackPanel stackPanel)
                    {
                        foreach (var stackChild in stackPanel.Children)
                        {
                            if (stackChild is TextBlock textBlock && textBlock.Tag?.ToString() == "StatusText")
                            {
                                textBlock.Text = session.StatusDisplay;
                                textBlock.Foreground = GetStatusBrush(session.Status);
                            }
                            else if (stackChild is Button button && button.Tag?.ToString() == session.SessionId)
                            {
                                button.Content = GetActionButtonText(session.Status);
                            }
                        }
                    }
                }
            }
        }

        private void NavigationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationList.SelectedItem is ListBoxItem selectedItem)
            {
                var content = selectedItem.Content.ToString();
                
                if (content?.Contains("New RDP Session") == true)
                {
                    CreateNewSessionButton_Click(this, new RoutedEventArgs());
                }
                else if (content?.Contains("SSH File Transfer") == true)
                {
                    try
                    {
                        var fileTransferWindow = new SshFileTransferWindow();
                        fileTransferWindow.Show();
                        AddChatMessage("🤖 AssistBot: Opening SSH File Transfer window for Linux file management...", false);
                    }
                    catch (Exception ex)
                    {
                        AddChatMessage($"🤖 AssistBot: Error opening SSH File Transfer: {ex.Message}", false);
                    }
                }
                else if (content?.Contains("SSH Terminal") == true)
                {
                    try
                    {
                        var terminalWindow = new SshTerminalWindow();
                        terminalWindow.Show();
                        AddChatMessage("🤖 AssistBot: Opening SSH Terminal for Ubuntu/Linux server management...", false);
                    }
                    catch (Exception ex)
                    {
                        AddChatMessage($"🤖 AssistBot: Error opening SSH Terminal: {ex.Message}", false);
                    }
                }
                else if (content?.Contains("Settings") == true)
                {
                    AddChatMessage("🤖 AssistBot: Settings panel coming soon in future updates!", false);
                }
                else if (content?.Contains("Performance Monitor") == true)
                {
                    var activeCount = _sessionManager.ActiveSessions.Count;
                    var connectedCount = _sessionManager.ActiveSessions.Count(s => s.Status == RdpSessionStatus.Connected);
                    AddChatMessage($"🤖 AssistBot: Performance Status - Active Sessions: {activeCount}, Connected: {connectedCount}", false);
                }
                else if (content?.Contains("Security Status") == true)
                {
                    AddChatMessage("🤖 AssistBot: All connections use encrypted protocols (RDP/SSH). Session credentials are stored securely.", false);
                }
                else if (content?.Contains("Saved Connections") == true)
                {
                    AddChatMessage("🤖 AssistBot: Saved connections feature will be added in future updates!", false);
                }

                // Clear selection for next use
                NavigationList.SelectedItem = null;
            }
        }

        // ... (rest of the existing methods remain the same)

        private async void InitializeServicesAsync()
        {
            try
            {
                // Initialize Local AI Service (fallback)
                var localAiSuccess = await _aiService.InitializeAsync();
                
                // Initialize Assistant AI Service (cloud AI)
                var assistantSuccess = await _assistantAI.InitializeAsync();
                
                AddWelcomeMessages();
                
                if (assistantSuccess)
                {
                    AddChatMessage($"🤖 AssistBot: Advanced AI assistant ready! Using: {_assistantAI.ServiceName}", isUser: false);
                    AddChatMessage("🤖 AssistBot: I can now parse natural language commands like 'connect to server.com as admin' or 'take screenshot of session 1'. Try it out!", isUser: false);
                }
                else if (localAiSuccess)
                {
                    AddChatMessage($"🤖 AssistBot: Local AI service ready! Using: {_aiService.CurrentModelName}", isUser: false);
                    AddChatMessage("🤖 AssistBot: I'm running in enhanced local mode and ready to help with your RDP needs.", isUser: false);
                }
                else
                {
                    AddChatMessage("🤖 AssistBot: Welcome! I'm running in safe mode and ready to help with your RDP needs.", isUser: false);
                }
                
                _logger?.LogInformation("AI services initialized - Assistant: {AssistantReady}, Local: {LocalReady}", 
                    assistantSuccess, localAiSuccess);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing AI services");
                AddWelcomeMessages();
                AddChatMessage("🤖 AssistBot: Welcome! I'm running in safe mode and ready to help with your RDP needs.", isUser: false);
            }
        }

        private async void InitializeAIServiceAsync()
        {
            // This method is replaced by InitializeServicesAsync()
            // Keeping for backward compatibility but not used
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
                // Enhanced command processing for RDP operations
                var response = await ProcessCommandAsync(command);
                AddChatMessage(response, isUser: false);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing command");
                AddChatMessage($"🤖 AssistBot: I encountered an error processing your request: {ex.Message}. Please try again.", isUser: false);
            }
        }

        private async Task<string> ProcessCommandAsync(string command)
        {
            var lowerCommand = command.ToLowerInvariant();

            try
            {
                // Show "typing" indicator
                AddChatMessage("🤖 AssistBot: *thinking...*", isUser: false);

                // Use AssistantAI to parse the command
                var assistantResponse = await _assistantAI.ParseCommandAsync(command);

                // Remove the "thinking" indicator
                RemoveLastChatMessage();

                if (assistantResponse.Success && assistantResponse.Command != null)
                {
                    var cmd = assistantResponse.Command;
                    
                    // Execute the parsed command
                    var result = await ExecuteAssistantCommandAsync(cmd);
                    
                    // Return appropriate response
                    if (!string.IsNullOrEmpty(result))
                    {
                        return result;
                    }
                    else if (!string.IsNullOrEmpty(assistantResponse.Message))
                    {
                        return $"🤖 AssistBot: {assistantResponse.Message}";
                    }
                    else
                    {
                        return $"🤖 AssistBot: {cmd.Explanation ?? "Command processed successfully."}";
                    }
                }
                else if (assistantResponse.NeedsMoreInfo)
                {
                    var response = $"🤖 AssistBot: {assistantResponse.Message}";
                    if (assistantResponse.FollowUpQuestions != null && assistantResponse.FollowUpQuestions.Length > 0)
                    {
                        response += "\n\n" + string.Join("\n", assistantResponse.FollowUpQuestions.Select(q => $"• {q}"));
                    }
                    return response;
                }
                else
                {
                    // Fallback to general conversation
                    return await _assistantAI.GenerateResponseAsync(command);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error processing command with AssistantAI");
                
                // Remove "thinking" indicator if it exists
                RemoveLastChatMessage();
                
                // Fallback to basic command processing
                return await ProcessCommandFallbackAsync(command);
            }
        }

        private async Task<string> ExecuteAssistantCommandAsync(AssistantCommand command)
        {
            try
            {
                switch (command.Action)
                {
                    case AssistantActionType.Connect:
                        return await HandleConnectCommandAsync(command);

                    case AssistantActionType.Disconnect:
                        return await HandleDisconnectCommandAsync(command);

                    case AssistantActionType.DisconnectAll:
                        var result = await _sessionManager.DisconnectAllSessionsAsync();
                        return result 
                            ? "🤖 AssistBot: ✅ All sessions disconnected successfully!"
                            : "🤖 AssistBot: ⚠️ Some sessions could not be disconnected.";

                    case AssistantActionType.ListSessions:
                        return HandleListSessionsCommand();

                    case AssistantActionType.Screenshot:
                        return await HandleScreenshotCommandAsync(command);

                    case AssistantActionType.GeneralHelp:
                        return await _assistantAI.GenerateResponseAsync("help with RDP");

                    default:
                        return "🤖 AssistBot: I'm not sure how to handle that command. Can you try rephrasing it?";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error executing assistant command: {Action}", command.Action);
                return $"🤖 AssistBot: ❌ Error executing command: {ex.Message}";
            }
        }

        private async Task<string> HandleConnectCommandAsync(AssistantCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.Host))
            {
                return "🤖 AssistBot: I need a server address to connect to. Try: 'connect to server.example.com as username'";
            }

            if (string.IsNullOrWhiteSpace(command.Username))
            {
                // Open connection dialog with pre-filled host
                Dispatcher.Invoke(() =>
                {
                    var dialog = new ConnectionDialog { Owner = this };
                    // Pre-fill the host if we can access the dialog's properties
                    dialog.ShowDialog();
                });
                return $"🤖 AssistBot: Opening connection dialog for {command.Host}. You'll need to provide credentials.";
            }

            if (string.IsNullOrWhiteSpace(command.Password))
            {
                return $"🤖 AssistBot: I found the server ({command.Host}) and user ({command.Username}), but I need a password to connect. For security, please use the connection dialog.";
            }

            // Create connection request and start session
            var connectionRequest = new RdpConnectionRequest
            {
                Host = command.Host,
                Username = command.Username,
                Password = command.Password,
                SessionName = command.SessionName ?? $"{command.Host} ({command.Username})",
                Port = command.Port,
                FullScreen = command.FullScreen,
                ColorDepth = command.ColorDepth
            };

            var sessionId = await _sessionManager.StartSessionAsync(connectionRequest);
            if (sessionId != null)
            {
                return $"🤖 AssistBot: ✅ Starting connection to {command.Host} as {command.Username}...";
            }
            else
            {
                return $"🤖 AssistBot: ❌ Failed to start connection to {command.Host}. Please check your parameters.";
            }
        }

        private async Task<string> HandleDisconnectCommandAsync(AssistantCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.SessionId))
            {
                var sessions = _sessionManager.ActiveSessions;
                if (sessions.Count == 0)
                {
                    return "🤖 AssistBot: No active sessions to disconnect.";
                }
                else if (sessions.Count == 1)
                {
                    // Disconnect the only session
                    var success = await _sessionManager.EndSessionAsync(sessions[0].SessionId);
                    return success 
                        ? $"🤖 AssistBot: ✅ Disconnected session: {sessions[0].DisplayName}"
                        : "🤖 AssistBot: ❌ Failed to disconnect the session.";
                }
                else
                {
                    // Multiple sessions, ask which one
                    var sessionList = string.Join("\n", sessions.Select((s, i) => $"{i + 1}. {s.DisplayName} ({s.Host})"));
                    return $"🤖 AssistBot: Which session would you like to disconnect?\n\n{sessionList}\n\nSay 'disconnect session 1' or use the session name.";
                }
            }

            // Try to find session by ID or name/number
            var session = FindSessionByIdOrName(command.SessionId);
            if (session == null)
            {
                return $"🤖 AssistBot: ❌ Could not find session '{command.SessionId}'. Use 'list sessions' to see available sessions.";
            }

            var result = await _sessionManager.EndSessionAsync(session.SessionId);
            return result 
                ? $"🤖 AssistBot: ✅ Disconnected session: {session.DisplayName}"
                : $"🤖 AssistBot: ❌ Failed to disconnect session: {session.DisplayName}";
        }

        private string HandleListSessionsCommand()
        {
            var sessions = _sessionManager.ActiveSessions;
            if (sessions.Count == 0)
            {
                return "🤖 AssistBot: No active RDP sessions currently.";
            }

            var statusText = $"🤖 AssistBot: Active Sessions ({sessions.Count}):\n\n";
            for (int i = 0; i < sessions.Count; i++)
            {
                var session = sessions[i];
                statusText += $"{i + 1}. **{session.DisplayName}**\n";
                statusText += $"   • Host: {session.Host}:{session.Port}\n";
                statusText += $"   • User: {session.Username}\n";
                statusText += $"   • Status: {session.StatusDisplay}\n";
                statusText += $"   • Created: {session.CreatedAt:HH:mm:ss}\n\n";
            }
            return statusText.TrimEnd();
        }

        private async Task<string> HandleScreenshotCommandAsync(AssistantCommand command)
        {
            if (string.IsNullOrWhiteSpace(command.SessionId))
            {
                var sessions = _sessionManager.ActiveSessions;
                if (sessions.Count == 0)
                {
                    return "🤖 AssistBot: No active sessions to capture.";
                }
                else if (sessions.Count == 1)
                {
                    // Screenshot the only session
                    var filePath = await _screenshotService.CaptureSessionScreenshotAsync(sessions[0].SessionId);
                    return filePath != null 
                        ? $"🤖 AssistBot: 📸 Screenshot captured for {sessions[0].DisplayName}\nSaved to: {filePath}"
                        : "🤖 AssistBot: ❌ Failed to capture screenshot.";
                }
                else
                {
                    // Multiple sessions, ask which one
                    var sessionList = string.Join("\n", sessions.Select((s, i) => $"{i + 1}. {s.DisplayName}"));
                    return $"🤖 AssistBot: Which session would you like to capture?\n\n{sessionList}\n\nSay 'screenshot session 1' or use the session name.";
                }
            }

            // Try to find session by ID or name/number
            var session = FindSessionByIdOrName(command.SessionId);
            if (session == null)
            {
                return $"🤖 AssistBot: ❌ Could not find session '{command.SessionId}'. Use 'list sessions' to see available sessions.";
            }

            var filePath = await _screenshotService.CaptureSessionScreenshotAsync(session.SessionId);
            return filePath != null 
                ? $"🤖 AssistBot: 📸 Screenshot captured for {session.DisplayName}\nSaved to: {filePath}"
                : $"🤖 AssistBot: ❌ Failed to capture screenshot for {session.DisplayName}";
        }

        private RdpSession? FindSessionByIdOrName(string identifier)
        {
            var sessions = _sessionManager.ActiveSessions;

            // Try exact session ID match first
            var session = sessions.FirstOrDefault(s => s.SessionId == identifier);
            if (session != null) return session;

            // Try session name match
            session = sessions.FirstOrDefault(s => s.SessionName.Equals(identifier, StringComparison.OrdinalIgnoreCase));
            if (session != null) return session;

            // Try host match
            session = sessions.FirstOrDefault(s => s.Host.Equals(identifier, StringComparison.OrdinalIgnoreCase));
            if (session != null) return session;

            // Try numeric index (1-based)
            if (int.TryParse(identifier, out var index) && index > 0 && index <= sessions.Count)
            {
                return sessions[index - 1];
            }

            return null;
        }

        private void RemoveLastChatMessage()
        {
            try
            {
                if (ChatPanel.Children.Count > 0)
                {
                    var lastChild = ChatPanel.Children[ChatPanel.Children.Count - 1];
                    if (lastChild is Border border && border.Child is TextBlock textBlock)
                    {
                        if (textBlock.Text.Contains("*thinking...*"))
                        {
                            ChatPanel.Children.RemoveAt(ChatPanel.Children.Count - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error removing last chat message");
            }
        }

        private async Task<string> ProcessCommandFallbackAsync(string command)
        {
            // Fallback to original simple command processing
            var lowerCommand = command.ToLowerInvariant();

            // Handle specific RDP commands
            if (lowerCommand.Contains("connect") || lowerCommand.Contains("new session"))
            {
                CreateNewSessionButton_Click(this, new RoutedEventArgs());
                return "🤖 AssistBot: Opening connection dialog for you!";
            }
            else if (lowerCommand.Contains("disconnect all") || lowerCommand.Contains("close all"))
            {
                var result = await _sessionManager.DisconnectAllSessionsAsync();
                return result 
                    ? "🤖 AssistBot: All sessions have been disconnected successfully."
                    : "🤖 AssistBot: There was an issue disconnecting some sessions.";
            }
            else if (lowerCommand.Contains("status") || lowerCommand.Contains("sessions"))
            {
                var sessions = _sessionManager.ActiveSessions;
                if (sessions.Count == 0)
                {
                    return "🤖 AssistBot: No active RDP sessions currently.";
                }
                
                var statusText = $"🤖 AssistBot: Active Sessions ({sessions.Count}):\n";
                foreach (var session in sessions)
                {
                    statusText += $"• {session.DisplayName} ({session.Host}) - {session.StatusDisplay}\n";
                }
                return statusText.TrimEnd();
            }
            else
            {
                // Use AI service for general responses
                return await _aiService.GenerateResponseAsync(command);
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
            _assistantAI?.Dispose();
            // Clean up session manager resources
            _sessionManager?.Dispose();
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