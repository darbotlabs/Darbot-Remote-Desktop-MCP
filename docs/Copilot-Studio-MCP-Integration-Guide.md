# Microsoft Copilot Studio MCP Integration Guide

## Overview

This guide provides step-by-step instructions for integrating the RetroRDP Model Context Protocol (MCP) server with Microsoft Copilot Studio agents. The MCP connector enables AI assistants to control remote desktop sessions with advanced video streaming and performance optimization capabilities.

## 🚀 Features Available to Copilot Studio

### **Remote Desktop Management**
- **Connect to RDP servers** with H.264/H.265 video codecs
- **Configure bitrate and resolution** dynamically (256-50,000 kbps, up to 7680x4320)
- **Performance presets** for optimal streaming (Performance/Balanced/Quality modes)
- **Adaptive bitrate streaming** based on network conditions

### **Session Control & Monitoring**  
- **List active sessions** with real-time performance metrics
- **Configure running sessions** with codec switching and optimization
- **Advanced compression settings** with bitmap caching

### **Enterprise-Grade Capabilities**
- **Secure authentication** with credential management
- **Production logging** with Serilog integration
- **RESTful health monitoring** and capabilities discovery
- **Cross-platform compatibility** (Windows, Linux, macOS)

---

## 📋 Prerequisites

### **System Requirements**
- ✅ **.NET 8.0 Runtime** or later
- ✅ **RetroRDP Client** application running locally
- ✅ **Network access** to target RDP servers
- ✅ **Copilot Studio Premium** subscription (for custom connectors)

### **RetroRDP Client Setup**
1. Download and install the RetroRDP Client application
2. Ensure the client is configured to accept HTTP API calls on `localhost:8080`
3. Verify the client health endpoint: `http://localhost:8080/health`

---

## 🛠️ MCP Server Setup

### **1. Installation & Configuration**

#### **Download and Build**
```bash
# Clone the repository
git clone https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP.git
cd Darbot-Remote-Desktop-MCP

# Navigate to MCP server
cd src/MCPServer/RetroRDP.MCPServer

# Build the project
dotnet build

# Run the server
dotnet run
```

#### **Configuration Settings**
Edit `appsettings.json` to configure the RetroRDP client connection:

```json
{
  "RetroRDPClient": {
    "BaseUrl": "http://localhost:8080"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "RetroRDP.MCPServer": "Debug"
    }
  }
}
```

### **2. Verify Server Operation**

#### **Health Check**
```bash
curl http://localhost:5000/mcp/health
# Expected response:
# {"status":"healthy","timestamp":"2024-12-15T10:30:00Z"}
```

#### **Capabilities Discovery**
```bash
curl http://localhost:5000/mcp/capabilities | jq
# Returns detailed tool schemas and server information
```

#### **MCP Protocol Test**
```bash
curl -X POST http://localhost:5000/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "1",
    "method": "initialize",
    "params": {
      "protocolVersion": "2024-11-05",
      "clientInfo": {"name": "Test Client", "version": "1.0.0"}
    }
  }'
```

---

## 🤖 Copilot Studio Integration

### **Step 1: Create Custom Connector**

#### **1.1 Navigate to Connectors**
1. Open **Microsoft Copilot Studio**
2. Go to **Settings** → **Generative AI** → **Custom Connectors**
3. Click **"New custom connector"**

#### **1.2 Basic Information**
- **Name**: `RetroRDP MCP Connector`
- **Description**: `Model Context Protocol connector for RetroRDP client with H.264/H.265 streaming`
- **Host**: `localhost:5000` (or your server's host)
- **Base URL**: `/mcp`

#### **1.3 Security Configuration**
- **Authentication Type**: `No authentication` (for localhost)
- **For production**: Configure API Key or OAuth 2.0 as needed

### **Step 2: Define API Operations**

#### **2.1 Health Check Operation**
```yaml
Operation ID: health-check
Summary: Check MCP server health
HTTP Verb: GET
URL: /health
```

**Response Schema:**
```json
{
  "type": "object",
  "properties": {
    "status": {"type": "string"},
    "timestamp": {"type": "string", "format": "date-time"}
  }
}
```

#### **2.2 MCP Initialize Operation**
```yaml
Operation ID: mcp-initialize
Summary: Initialize MCP connection
HTTP Verb: POST
URL: /
Content-Type: application/json
```

**Request Schema:**
```json
{
  "type": "object",
  "required": ["jsonrpc", "id", "method"],
  "properties": {
    "jsonrpc": {"type": "string", "enum": ["2.0"]},
    "id": {"type": "string"},
    "method": {"type": "string", "enum": ["initialize"]},
    "params": {
      "type": "object",
      "properties": {
        "protocolVersion": {"type": "string", "default": "2024-11-05"},
        "clientInfo": {
          "type": "object",
          "properties": {
            "name": {"type": "string"},
            "version": {"type": "string"}
          }
        }
      }
    }
  }
}
```

#### **2.3 RDP Connect Operation**  
```yaml
Operation ID: rdp-connect
Summary: Connect to RDP server with advanced codec settings
HTTP Verb: POST
URL: /
Content-Type: application/json
```

**Request Schema:**
```json
{
  "type": "object",
  "required": ["jsonrpc", "id", "method", "params"],
  "properties": {
    "jsonrpc": {"type": "string", "enum": ["2.0"]},
    "id": {"type": "string"},
    "method": {"type": "string", "enum": ["tools/call"]},
    "params": {
      "type": "object",
      "required": ["name", "arguments"],
      "properties": {
        "name": {"type": "string", "enum": ["connect_rdp"]},
        "arguments": {
          "type": "object",
          "required": ["host", "username"],
          "properties": {
            "host": {
              "type": "string",
              "description": "RDP server hostname or IP"
            },
            "username": {
              "type": "string", 
              "description": "Username for authentication"
            },
            "password": {
              "type": "string",
              "description": "Password (optional)"
            },
            "port": {
              "type": "integer",
              "minimum": 1,
              "maximum": 65535,
              "default": 3389
            },
            "width": {
              "type": "integer",
              "minimum": 800,
              "maximum": 7680,
              "default": 1920
            },
            "height": {
              "type": "integer", 
              "minimum": 600,
              "maximum": 4320,
              "default": 1080
            },
            "codec": {
              "type": "string",
              "enum": ["H264", "H265", "Auto"],
              "description": "Video codec selection"
            },
            "maxBitrate": {
              "type": "integer",
              "minimum": 256,
              "maximum": 50000,
              "description": "Maximum bitrate in kbps"
            },
            "minBitrate": {
              "type": "integer",
              "minimum": 128, 
              "maximum": 2000,
              "description": "Minimum bitrate in kbps"
            },
            "preset": {
              "type": "string",
              "enum": ["Performance", "Balanced", "Quality"],
              "description": "Performance optimization preset"
            },
            "adaptiveBitrate": {
              "type": "boolean",
              "description": "Enable adaptive bitrate streaming"
            }
          }
        }
      }
    }
  }
}
```

#### **2.4 Session Management Operations**

**List Sessions:**
```yaml
Operation ID: rdp-list-sessions
Method: tools/call with name: list_rdp_sessions
Arguments: {"includeMetrics": boolean}
```

**Configure Session:**
```yaml
Operation ID: rdp-configure-session  
Method: tools/call with name: configure_rdp_session
Arguments: {
  "sessionId": "string",
  "codec": "H264|H265|Auto", 
  "maxBitrate": integer,
  "preset": "Performance|Balanced|Quality",
  // ... additional configuration options
}
```

### **Step 3: Configure Copilot Agent**

#### **3.1 Add Custom Connector**
1. In your **Copilot Studio** project
2. Go to **Settings** → **Generative AI** → **AI capabilities** 
3. Enable **"Actions"** and **"Connect to APIs"**
4. Add your **RetroRDP MCP Connector**

#### **3.2 Create Agent Instructions**
```markdown
You are a Remote Desktop Management Assistant with access to advanced RDP capabilities through the RetroRDP MCP connector.

## Available Actions:
1. **Connect to RDP servers** with H.264/H.265 video codecs
2. **List active RDP sessions** with performance metrics  
3. **Configure session settings** for optimization
4. **Monitor server health** and connection status

## Usage Guidelines:
- Always initialize the MCP connection before performing operations
- Use H.265 codec for best compression on high-bandwidth connections
- Apply "Performance" preset for multiple concurrent sessions
- Apply "Quality" preset for single high-fidelity sessions
- Monitor bitrate and adjust based on network conditions

## Security:
- Never expose passwords in responses
- Always prompt users for credentials when needed
- Use secure connection parameters
```

#### **3.3 Configure Action Prompts**
```yaml
RDP Connection Prompt:
"When users want to connect to a remote desktop, use the rdp-connect action. 
Ask for: hostname, username, and any specific performance requirements.
Suggest appropriate codec and bitrate based on their use case."

Session Management Prompt:  
"For session management tasks, first list current sessions, then perform 
requested operations. Provide clear status updates and error handling."
```

### **Step 4: Test Integration**

#### **4.1 Basic Functionality Test**
**User Input:** *"Connect to server.example.com as administrator"*

**Expected Agent Behavior:**
1. Initialize MCP connection
2. Call `rdp-connect` action with provided parameters
3. Prompt for password securely  
4. Provide connection status feedback

#### **4.2 Advanced Features Test**
**User Input:** *"Connect to prod-db.company.com with H.265 codec and quality preset"*

**Expected Response:**
```
🤖 Connecting to prod-db.company.com with optimized settings:
   • Video Codec: H.265 (HEVC) for maximum compression
   • Performance Preset: Quality mode
   • Adaptive Bitrate: Enabled
   • Resolution: 1920x1080 (default)
   
Please provide your username for the connection.
```

---

## 🔧 Advanced Configuration

### **Production Deployment**

#### **Secure Hosting**
```bash
# Run MCP server with HTTPS (production)
dotnet run --urls="https://localhost:5001" --environment=Production
```

#### **Docker Deployment**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY publish/ .
EXPOSE 5000
ENTRYPOINT ["dotnet", "RetroRDP.MCPServer.dll"]
```

#### **Authentication Setup**
```json
{
  "Authentication": {
    "ApiKey": {
      "HeaderName": "X-API-Key",
      "ValidKeys": ["your-secure-api-key"]
    }
  }
}
```

### **Monitoring & Logging**

#### **Structured Logging**
```json
{
  "Serilog": {
    "WriteTo": [
      {"Name": "Console"},
      {
        "Name": "File", 
        "Args": {
          "path": "logs/mcp-server-.log",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

#### **Health Monitoring**
- Health endpoint: `GET /mcp/health`
- Capabilities: `GET /mcp/capabilities`  
- Metrics: Custom telemetry via Application Insights

---

## 🚨 Troubleshooting

### **Common Issues**

#### **MCP Server Not Starting**
```bash
# Check .NET runtime
dotnet --version

# Verify dependencies  
dotnet restore
dotnet build

# Check port availability
netstat -an | grep :5000
```

#### **Copilot Studio Connection Failures**
1. **Verify server accessibility** from Copilot Studio environment
2. **Check CORS configuration** if cross-origin issues
3. **Validate JSON schemas** match exactly
4. **Review authentication settings**

#### **RDP Client Communication Issues**
```bash
# Test client health
curl http://localhost:8080/health

# Check client logs
# Verify API endpoints are enabled in client configuration
```

### **Debug Mode**
```bash
# Run with debug logging
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --verbosity detailed
```

### **API Testing**
```bash
# Test tool execution
curl -X POST http://localhost:5000/mcp \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "id": "test-1",
    "method": "tools/call",
    "params": {
      "name": "list_rdp_sessions",
      "arguments": {"includeMetrics": true}
    }
  }' | jq
```

---

## 📚 Additional Resources

### **Documentation Links**
- [Model Context Protocol Specification](https://spec.modelcontextprotocol.io/specification/)
- [Microsoft Copilot Studio Documentation](https://docs.microsoft.com/en-us/microsoft-copilot-studio/)
- [RetroRDP Client User Guide](../UserGuide.md)
- [API Reference Documentation](../src/MCPServer/RetroRDP.MCPServer/README.md)

### **Sample Conversations**
```markdown
User: "I need to connect to my production database server"
Agent: "I can help you connect to your production database server using our secure RDP connector. I'll need:
1. Server hostname or IP address
2. Your username  
3. Any specific performance requirements

What's the server address you'd like to connect to?"

User: "db-prod.company.com, username: dbadmin, I need high quality video"
Agent: "Perfect! I'll connect to db-prod.company.com with optimized settings for high-quality video:
• H.265 codec for superior compression
• Quality preset for maximum visual fidelity
• 1920x1080 resolution
• Adaptive bitrate enabled

Please enter your password when prompted by the connection dialog."
```

### **Best Practices**
1. **Always initialize MCP connection** before tool calls
2. **Handle authentication securely** - never log passwords
3. **Use appropriate codecs** based on network conditions
4. **Monitor performance metrics** for optimization
5. **Implement proper error handling** with user-friendly messages
6. **Test thoroughly** before production deployment

---

This integration guide enables powerful remote desktop management capabilities within Microsoft Copilot Studio, providing enterprise-grade RDP control with advanced video streaming and performance optimization features.