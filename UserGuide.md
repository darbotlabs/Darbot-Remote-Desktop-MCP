# RetroRDP Client User Guide

Welcome to **RetroRDP Client** - the ultimate retro-futuristic Remote Desktop experience! This guide will help you master all the features of this modern RDP client with its unique cyber aesthetic and AI-powered assistance.

## 🎮 Table of Contents

- [Getting Started](#getting-started)
- [Installation](#installation)
- [Quick Start Guide](#quick-start-guide)
- [User Interface Overview](#user-interface-overview)
- [Creating RDP Connections](#creating-rdp-connections)
- [AI Assistant Usage](#ai-assistant-usage)
- [Performance Optimization](#performance-optimization)
- [Session Management](#session-management)
- [Troubleshooting](#troubleshooting)
- [Advanced Features](#advanced-features)
- [System Requirements](#system-requirements)
- [Support & Feedback](#support--feedback)

## 🚀 Getting Started

### Installation

**Option 1: Self-Contained (Recommended)**
1. Download the `RetroRDPClient-[version]-SelfContained-win-x64.zip` package
2. Extract to your desired location (e.g., `C:\Program Files\RetroRDPClient`)
3. Run `RetroRDPClient.exe` - no additional installation required!

**Option 2: Framework-Dependent**
1. Install [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) if not already installed
2. Download the `RetroRDPClient-[version]-FrameworkDependent-win-x64.zip` package
3. Extract and run `RetroRDPClient.exe`

### First Launch

When you first start RetroRDP Client, you'll be greeted by:
- 🌟 The stunning retro-cyber interface with neon accents
- 🤖 The AI Assistant (AssistBot) in the left sidebar
- 📋 An empty session list ready for your first connection

## 📱 Quick Start Guide

### Creating Your First Connection

1. **Using the AI Assistant (Recommended)**:
   - Type in the chat: `"connect to 192.168.1.100 as administrator"`
   - Follow the assistant's prompts for credentials
   - The AI will guide you through the process!

2. **Using the Traditional UI**:
   - Click the "New Session" button in the navigation menu
   - Fill in the connection details in the dialog
   - Click "Connect" to establish the session

3. **Your session will appear as a new tab** with status indicators:
   - 🟢 Connected
   - 🟡 Connecting/Reconnecting
   - 🔴 Failed/Error

## 🖥️ User Interface Overview

### Main Layout

The RetroRDP Client features a sleek **20/80 split layout**:

**Left Sidebar (20%)**
- 🤖 **AI Assistant Chat**: Interactive AssistBot for hands-free control
- 🧭 **Navigation Menu**: Quick actions and settings
- 📊 **Performance Monitor**: Real-time system metrics

**Main Area (80%)**
- 📑 **Tabbed Sessions**: Each RDP connection gets its own tab
- 🎛️ **Session Controls**: Connect, disconnect, screenshot tools
- 📺 **RDP Display**: Full remote desktop viewing area

### Color Scheme & Aesthetics

- **Dark Theme**: Easy on the eyes for long sessions
- **Neon Accents**: Cyan and magenta highlights
- **Glow Effects**: Subtle lighting for that cyber atmosphere
- **Fluent Design**: Modern Windows 11-style materials

## 🔗 Creating RDP Connections

### Connection Dialog

Access via: Navigation Menu → "New Session" or AI command

**Required Fields:**
- **Host**: IP address or hostname (e.g., `192.168.1.100`, `server.company.com`)
- **Username**: Your remote desktop username
- **Password**: Your remote desktop password
- **Port**: RDP port (default: 3389)

**Optional Settings:**
- **Session Name**: Custom name for easy identification
- **Screen Resolution**: Choose your preferred resolution
- **Color Depth**: Balance between quality and performance
- **Full Screen**: Enable for immersive experience

### Performance Presets

Choose from three optimized presets:

**🏃 Performance Mode**
- 16-bit color depth
- Lower update frequency
- Disabled visual effects
- Best for slow networks or multiple sessions

**⚖️ Balanced Mode (Default)**
- 16-bit color depth
- Moderate update frequency
- Essential features enabled
- Good compromise for most users

**🎨 Quality Mode**
- 32-bit color depth
- High update frequency
- All visual effects enabled
- Best for high-speed local networks

## 🤖 AI Assistant Usage

The AI Assistant (AssistBot) is your intelligent companion for managing RDP sessions. It understands natural language and can perform complex operations through simple chat commands.

### Basic Commands

**Connection Management:**
```
"connect to server.example.com as admin"
"rdp to 192.168.1.100"
"disconnect session 1"
"close all connections"
"show me all active sessions"
```

**Screenshots & Monitoring:**
```
"take a screenshot of session 2"
"capture fullscreen of my-server"
"screenshot session 1 application mode"
```

**Performance & Profiles:**
```
"save this connection as a profile"
"load my work-server profile"
"show performance recommendations"
"switch to performance mode"
```

**Chained Commands:**
```
"connect to server1 and take a screenshot"
"disconnect all sessions then show status"
"switch to quality mode and connect to host"
```

### AI Assistant Features

- **Natural Language Processing**: Speak naturally, no special syntax required
- **Context Awareness**: Remembers previous commands and session state
- **Smart Suggestions**: Provides helpful recommendations
- **Error Guidance**: Offers solutions when connections fail
- **Command History**: Recalls your previous interactions

### Example Conversations

**Getting Started:**
```
You: "help me connect to my work computer"
AssistBot: "I'd be happy to help! What's the IP address or hostname of your work computer?"
You: "it's 10.0.1.50"
AssistBot: "Great! What username should I use for the connection?"
```

**Troubleshooting:**
```
You: "my connection to server1 failed"
AssistBot: "I see the connection failed. This could be due to:
• Network connectivity issues
• Incorrect credentials
• RDP service not running
• Firewall blocking port 3389

Would you like me to try reconnecting or check the connection settings?"
```

## ⚡ Performance Optimization

### Automatic Monitoring

RetroRDP Client continuously monitors:
- **CPU Usage**: System processor utilization
- **Memory Consumption**: RAM usage per session
- **Network Activity**: Bytes sent/received
- **Session Response Time**: Connection latency

### Performance Recommendations

The system automatically suggests optimizations:

**High CPU Usage Detected:**
- Reduce color depth to 16-bit
- Lower screen update frequency
- Disable desktop wallpaper and visual effects

**High Memory Usage:**
- Close unused sessions
- Reduce screen resolution
- Enable more aggressive compression

**Slow Network Performance:**
- Switch to Performance preset
- Enable bitmap caching
- Reduce audio quality or disable

### Manual Optimization

**For Slow Networks:**
1. Use Performance preset
2. Set resolution to 1024x768 or lower
3. Disable audio redirection
4. Enable compression

**For Multiple Sessions:**
1. Limit concurrent sessions to 3-5
2. Use 16-bit color depth
3. Monitor CPU usage warnings
4. Close idle sessions regularly

**For High Quality:**
1. Use Quality preset on fast networks
2. Set full resolution (1920x1080+)
3. Enable 32-bit color depth
4. Keep sessions under 3 concurrent

## 📊 Session Management

### Session States

- **🟢 Connected**: Active RDP session
- **🟡 Connecting**: Establishing connection
- **🟡 Reconnecting**: Attempting to restore connection
- **🟡 Disconnecting**: Ending session
- **🔴 Failed**: Connection error occurred
- **⚪ Disconnected**: Session ended

### Session Controls

**Per-Session Actions:**
- **Connect/Reconnect**: Establish or restore connection
- **Disconnect**: End the RDP session
- **Screenshot**: Capture current screen
- **Performance**: View session-specific metrics

**Global Actions:**
- **Disconnect All**: End all active sessions
- **Performance Monitor**: View system-wide metrics
- **Settings**: Configure global preferences

### Keyboard Shortcuts

- **Ctrl+N**: New Session
- **Ctrl+D**: Disconnect Current Session
- **Ctrl+Shift+D**: Disconnect All Sessions
- **Ctrl+S**: Take Screenshot
- **Ctrl+Tab**: Switch Between Sessions
- **F11**: Toggle Full Screen

## 🔧 Troubleshooting

### Common Connection Issues

**"Connection Failed" or "Unable to Connect"**

**Possible Causes & Solutions:**

1. **Network Connectivity**
   - Check if you can ping the target machine
   - Verify you're on the same network or VPN
   - Test with command: `ping [hostname/ip]`

2. **RDP Service Not Running**
   - Ensure Remote Desktop is enabled on target machine
   - Check Windows Services for "Remote Desktop Services"
   - Verify the service is running and set to automatic

3. **Firewall Blocking Connection**
   - Check Windows Firewall on target machine
   - Ensure RDP (port 3389) is allowed through firewall
   - Corporate firewalls might block RDP traffic

4. **Incorrect Credentials**
   - Verify username and password are correct
   - Try logging in locally to the target machine first
   - Check if account is locked or disabled

5. **Port Issues**
   - Default RDP port is 3389
   - Some systems use custom ports for security
   - Ask your system administrator for the correct port

**Performance Issues**

**Slow Response or Lag:**
- Switch to Performance preset
- Reduce screen resolution
- Close other applications
- Check network bandwidth

**High CPU Usage:**
- Limit concurrent sessions
- Use lower color depth
- Disable visual effects
- Monitor other running applications

**Connection Drops Frequently:**
- Check network stability
- Increase connection timeout settings
- Consider using VPN if connecting over internet
- Update RDP client and target machine

### Error Messages

**"The connection was denied because the user account is not authorized for remote login"**
- User needs "Log on through Remote Desktop Services" permission
- Add user to "Remote Desktop Users" group on target machine

**"The remote computer requires Network Level Authentication"**
- Enable NLA in connection settings, or
- Disable NLA on the target machine (less secure)

**"Your credentials did not work"**
- Verify username format (try `domain\username` or `username@domain`)
- Check if password has recently changed
- Try connecting with a different account

### Getting Help

1. **Check the AI Assistant**: Ask AssistBot for specific troubleshooting help
2. **Performance Monitor**: Review system metrics for bottlenecks
3. **Log Files**: Check `%AppData%\RetroRDPClient\logs\rdpclient.log`
4. **System Requirements**: Verify your system meets minimum requirements

## 🚀 Advanced Features

### Logging & Monitoring

RetroRDP Client maintains comprehensive logs for troubleshooting:

**Log Location:** `%AppData%\RetroRDPClient\logs\rdpclient.log`

**Logged Events:**
- Connection attempts and results
- AI command executions
- Performance warnings
- Error conditions and exceptions

**Log Features:**
- Automatic rotation (daily)
- Size limits (10MB max)
- 7-day retention
- No sensitive data (passwords filtered out)

### Performance Monitoring

**Real-time Metrics:**
- CPU usage percentage
- Memory consumption (MB)
- Active session count
- Network traffic (bytes sent/received)

**Alerts & Warnings:**
- High CPU usage (>85%)
- High memory usage (>3GB)
- Too many concurrent sessions
- Network performance issues

### Session Profiles

Save frequently used connections as profiles:

**Creating Profiles:**
```
"save this connection as work-server profile"
"create profile for this session"
```

**Loading Profiles:**
```
"load work-server profile"
"use my development-server profile"
```

**Profile Benefits:**
- Quick connection to favorite servers
- Consistent performance settings
- Organized connection management

### Command Line Options

RetroRDP Client supports command line parameters:

```bash
# Connect to specific host at startup
RetroRDPClient.exe --host 192.168.1.100 --user admin

# Load specific profile
RetroRDPClient.exe --profile work-server

# Performance mode
RetroRDPClient.exe --performance-mode

# Enable verbose logging
RetroRDPClient.exe --debug
```

## 💻 System Requirements

### Minimum Requirements

- **Operating System**: Windows 10 (version 1809) or Windows 11
- **Processor**: Intel Core i3 or AMD equivalent
- **Memory**: 2 GB RAM
- **Storage**: 500 MB available space
- **Graphics**: DirectX 11 compatible
- **Network**: Ethernet or Wi-Fi connection

### Recommended Requirements

- **Operating System**: Windows 11 (latest version)
- **Processor**: Intel Core i5 or AMD Ryzen 5
- **Memory**: 4 GB RAM (8 GB for multiple sessions)
- **Storage**: 1 GB available space (SSD preferred)
- **Graphics**: Dedicated graphics card with DirectX 12 support
- **Network**: Gigabit Ethernet for best performance

### Performance Considerations

**For Multiple Sessions (3-5 concurrent):**
- 8 GB RAM minimum
- Quad-core processor
- Gigabit network connection
- Consider performance monitoring

**For High-Quality Sessions:**
- 16 GB RAM for best experience
- Fast CPU (Intel i7/AMD Ryzen 7+)
- High-speed network (1 Gbps+)
- Quality display (1920x1080+)

## 📞 Support & Feedback

### Getting Help

1. **AI Assistant**: Ask AssistBot any questions - it's designed to help!
2. **User Guide**: This comprehensive guide covers most scenarios
3. **Log Files**: Check application logs for detailed error information
4. **GitHub Issues**: Report bugs or request features

### Reporting Issues

When reporting problems, please include:

- **RetroRDP Client version** (found in About dialog)
- **Windows version** and build number
- **Error message** (exact text if possible)
- **Steps to reproduce** the issue
- **Log file excerpt** (if applicable)

**Privacy Note**: Never include passwords or sensitive information in reports.

### Feature Requests

We welcome suggestions for improvements! Consider:

- **New AI commands** or conversation features
- **Performance optimizations** for specific scenarios
- **UI/UX enhancements** for better usability
- **Integration options** with other tools

### Contributing

RetroRDP Client is an open-source project. Contributions are welcome:

- **Code contributions**: Submit pull requests with improvements
- **Documentation**: Help improve this user guide
- **Testing**: Report bugs and verify fixes
- **Ideas**: Share suggestions for new features

### Community

- **GitHub Repository**: [Darbot-Remote-Desktop-MCP](https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP)
- **Issue Tracker**: Report bugs and track development
- **Discussions**: Share tips and ask questions

---

## 🎉 Conclusion

Congratulations! You're now ready to master RetroRDP Client and enjoy the ultimate retro-futuristic Remote Desktop experience. The combination of modern functionality with cyber aesthetics, AI assistance, and professional-grade features makes this the most advanced RDP client available.

**Happy Remote Desktop Sessions!** 🚀✨

---

*This user guide is for RetroRDP Client v1.0+. For the latest version and updates, visit our [GitHub repository](https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP).*