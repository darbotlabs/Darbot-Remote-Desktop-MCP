# RetroRDP Complete Setup Guide

## 🚀 Quick Start (5 Minutes)

### For Impatient Users
1. **Download** the latest release from [GitHub Releases](https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP/releases)
2. **Extract** the ZIP file to your desired location (e.g., `C:\Programs\RetroRDP`)
3. **Run** `RetroRDPClient.exe` - no installation required!
4. **Test** connection: Click "New Session" → Enter your RDP server details → Connect

**That's it!** 🎉 You now have a working retro-cyber RDP client with AI assistance.

---

## 📋 System Requirements

### Minimum (Basic RDP Functionality)
- **OS**: Windows 10 (1809+) or Windows 11
- **RAM**: 2 GB
- **Storage**: 500 MB free space
- **Network**: Ethernet or Wi-Fi

### Recommended (Multi-Session + AI Features)
- **OS**: Windows 11 (latest)
- **RAM**: 8 GB (for 3-5 concurrent sessions)
- **CPU**: Intel i5 / AMD Ryzen 5 or better
- **Storage**: 2 GB SSD space
- **Network**: Gigabit Ethernet

---

## 🔽 Installation Options

### Option 1: Self-Contained (Recommended) ⭐
**Best for**: Most users, no dependencies required

1. Download `RetroRDPClient-[version]-SelfContained-win-x64.zip`
2. Extract to your preferred location
3. Run `RetroRDPClient.exe` immediately

**Pros**: Zero dependencies, works everywhere
**Cons**: Larger download (~150MB)

### Option 2: Framework-Dependent
**Best for**: Developers or .NET environments

1. Install [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Download `RetroRDPClient-[version]-FrameworkDependent-win-x64.zip`
3. Extract and run `RetroRDPClient.exe`

**Pros**: Smaller download (~30MB)
**Cons**: Requires .NET 8 runtime

### Option 3: Build from Source
**Best for**: Developers wanting latest features

```bash
git clone https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP.git
cd Darbot-Remote-Desktop-MCP
dotnet build --configuration Release
dotnet run --project src/ClientApp/RetroRDPClient
```

---

## ⚡ First Launch Setup Wizard

### Automatic Configuration Detection
RetroRDP automatically detects and configures:

1. **Performance Settings**: Based on your hardware capabilities
2. **Network Optimization**: Adapts to your connection speed  
3. **AI Features**: Detects available local AI models
4. **Security Settings**: Applies Windows security best practices

### Manual Configuration (Optional)

If you want to customize settings:

1. **Performance Mode**:
   - Performance: Optimized for speed (multiple sessions)
   - Balanced: Good compromise (recommended)
   - Quality: Best visuals (single session)

2. **AI Assistant**:
   - Enable/disable local AI features
   - Configure model preferences
   - Set command preferences

3. **Security**:
   - Credential storage options
   - Session encryption preferences
   - Network security settings

---

## 🔌 AI Features Setup (Optional)

### Windows Foundry Local AI
For enhanced AI capabilities with local Microsoft Phi-4 models:

1. **Download Phi-4 Models** (optional):
   ```
   Models are automatically detected from:
   - %USERPROFILE%\.cache\huggingface\
   - %LOCALAPPDATA%\Microsoft\Phi\
   - Application Models\ folder
   ```

2. **Enhanced Features with Models**:
   - More intelligent command parsing
   - Better natural language understanding
   - Offline AI processing (privacy-first)

3. **Without Models**:
   - Intelligent fallback mode works great
   - All core AI features still available
   - No internet required

### Model Download (Optional)
```bash
# If you want full local AI capabilities
# Download from Hugging Face or Microsoft AI
# Models will be auto-detected when placed in standard locations
```

---

## 🛠️ Post-Installation Verification

### Health Check
Run the built-in health check to verify everything works:

1. Open RetroRDP Client
2. Click "Settings" → "System Health"
3. Review the automated diagnostic results

### Test Connection
Verify RDP functionality:

1. Enable RDP on a test machine (or use localhost)
2. Create a new session in RetroRDP
3. Confirm successful connection and interaction

### AI Assistant Test
Test the AI assistant:

1. Type in the chat: `"hello"` 
2. Try a command: `"list my sessions"`
3. Verify intelligent responses

---

## 🚨 Troubleshooting Common Issues

### "Application won't start"
**Cause**: Missing dependencies or antivirus blocking
**Solution**: 
- Use self-contained version
- Add RetroRDP folder to antivirus exclusions
- Run as administrator once to establish trust

### "Can't connect to RDP server"
**Cause**: RDP not enabled or firewall blocking
**Solution**:
- Enable Remote Desktop on target machine
- Check Windows Firewall allows RDP (port 3389)
- Test with `ping [hostname]` first

### "Performance is slow"
**Cause**: Too many sessions or poor network
**Solution**:
- Switch to "Performance" preset
- Reduce concurrent sessions to 2-3
- Lower resolution in session settings

### "AI assistant not responding"
**Cause**: AI service initialization issue
**Solution**:
- Check logs in `%AppData%\RetroRDPClient\logs\`
- Restart application
- Try without AI models first

---

## 🎯 Performance Optimization

### Automatic Optimization
RetroRDP includes intelligent performance optimization:

- **Network Adaptive**: Automatically adjusts quality based on connection
- **Hardware Detection**: Uses optimal settings for your CPU/RAM
- **Session Balancing**: Distributes resources across multiple sessions

### Manual Tuning

**For Multiple Sessions (3-5)**:
```
Performance Preset: Performance
Resolution: 1024x768 or 1280x720
Color Depth: 16-bit
Concurrent Sessions: 3-5 max
```

**For High Quality (1-2 Sessions)**:
```
Performance Preset: Quality  
Resolution: 1920x1080 or higher
Color Depth: 32-bit
Concurrent Sessions: 1-2 max
```

**For Slow Networks**:
```
Performance Preset: Performance
Enable Compression: Yes
Bitmap Caching: Yes
Audio Quality: Low or Disabled
```

---

## 🔐 Security Best Practices

### Credential Management
- Passwords stored securely using Windows SecureString
- No plain text credential storage
- Option for Windows integrated authentication

### Network Security  
- RDP uses built-in Windows encryption
- Certificate validation enabled by default
- Network-level authentication supported

### Privacy
- Local AI processing (no cloud dependency)
- No telemetry or data collection
- Session data never leaves your network

---

## 📞 Getting Help

### Built-in Resources
1. **AI Assistant**: Ask it questions! "how do I optimize performance?"
2. **User Guide**: Comprehensive help in the app
3. **System Health Check**: Automated diagnostics

### Online Resources
- **GitHub Issues**: Report bugs or request features
- **User Guide**: [Complete documentation](UserGuide.md)
- **Troubleshooting**: Check application logs

### Log Files
Find detailed logs at:
```
%AppData%\RetroRDPClient\logs\rdpclient.log
```

---

## 🎉 You're All Set!

**Congratulations!** You now have:
- ✅ A modern, retro-futuristic RDP client
- ✅ AI-powered session management  
- ✅ Multi-session capabilities
- ✅ Optimized performance settings
- ✅ Enterprise-grade security

**Next Steps**:
1. Create your first RDP connection
2. Explore the AI assistant features
3. Set up multiple sessions for your workflow
4. Customize the retro-cyber theme to your taste

**Welcome to the future of Remote Desktop! 🚀**

---

## 🛠️ Post-Installation Verification

### Automated Health Check
Run the built-in health check to verify everything works:

**Windows:**
```cmd
scripts\health-check.sh
```

**Linux/macOS:**
```bash
scripts/health-check.sh
```

**Expected Results:**
- ✅ **Green**: Feature working correctly
- ⚠️ **Yellow**: Warning, may have limitations  
- ❌ **Red**: Critical issue, needs fixing

### Manual Verification Steps

#### Test 1: Application Launch
1. Launch RetroRDP Client
2. Verify the retro-cyber UI loads correctly
3. Check that the AI assistant sidebar appears

#### Test 2: Basic RDP Connection  
1. Enable RDP on a test machine (or use localhost)
2. Click "New Session" in RetroRDP
3. Enter connection details and connect
4. Verify successful RDP session display

#### Test 3: AI Assistant Functionality
1. Type in the chat: `"hello"`
2. Try a command: `"list my sessions"`
3. Verify intelligent responses and command recognition

#### Test 4: MCP Server (If Enabled)
1. Visit: http://localhost:5000/mcp/health
2. Should return: `{"status":"healthy","timestamp":"..."}`
3. Check capabilities: http://localhost:5000/mcp/capabilities

### Performance Validation
- **Multi-session test**: Open 2-3 RDP sessions simultaneously
- **Resource monitoring**: Check CPU/RAM usage stays reasonable
- **Network test**: Verify responsive interaction in RDP sessions

---

*This setup guide ensures you get the best experience from RetroRDP with minimal effort. For advanced configuration and features, see the [complete User Guide](UserGuide.md).*