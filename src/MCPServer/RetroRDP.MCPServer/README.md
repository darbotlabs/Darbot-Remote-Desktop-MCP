# RetroRDP MCP Server

This is a Model Context Protocol (MCP) server implementation for the RetroRDP client, enabling AI assistants to interact with remote desktop sessions through structured tools and resources.

## Features

### 🔧 MCP Tools
- **connect_rdp**: Create RDP connections with advanced codec and performance settings
- **list_rdp_sessions**: List active sessions with optional performance metrics
- **configure_rdp_session**: Configure video codec, bitrate, resolution, and performance settings

### 🎥 Advanced Video Settings
- **H.264 & H.265 Codec Support**: Choose between compatibility (H.264) and efficiency (H.265)
- **Configurable Bitrate**: Set min/max bitrate with adaptive streaming
- **Resolution Control**: Dynamic resolution adjustment from 800x600 to 7680x4320
- **Performance Presets**: Performance, Balanced, and Quality modes

### 🔗 Local Connector
- HTTP-based communication with RetroRDP client
- Real-time session monitoring and control
- Performance metrics collection
- Screenshot capture capabilities

## Quick Start

### Prerequisites
- .NET 8.0 or later
- RetroRDP Client running locally

### Running the Server
```bash
cd src/MCPServer/RetroRDP.MCPServer
dotnet run
```

The server will start on `http://localhost:5000` by default.

### Health Check
```bash
curl http://localhost:5000/mcp/health
```

### View Capabilities
```bash
curl http://localhost:5000/mcp/capabilities
```

## Configuration

Configure the RetroRDP client connection in `appsettings.json`:

```json
{
  "RetroRDPClient": {
    "BaseUrl": "http://localhost:8080"
  }
}
```

## MCP Protocol Support

This server implements the Model Context Protocol 2024-11-05 specification:
- JSON-RPC 2.0 messaging
- Tool execution with structured schemas
- Resource management
- Error handling and logging

## Example Usage with AI Assistants

```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "method": "tools/call",
  "params": {
    "name": "connect_rdp",
    "arguments": {
      "host": "192.168.1.100",
      "username": "admin",
      "codec": "H265",
      "maxBitrate": 8000,
      "preset": "Quality",
      "width": 1920,
      "height": 1080
    }
  }
}
```

## Architecture

The MCP server acts as a bridge between AI assistants and the RetroRDP client:

```
AI Assistant <-- MCP Protocol --> MCP Server <-- HTTP --> RetroRDP Client
```

## Security Considerations

- Local network communication only
- No sensitive credentials stored in logs
- HTTP-only communication for localhost
- Session isolation and management

## 🤖 Microsoft Copilot Studio Integration

### Quick Start for AI Agents

**Complete Integration Guide**: See [Copilot Studio MCP Integration Guide](../../../docs/Copilot-Studio-MCP-Integration-Guide.md)

### Essential Endpoints for Copilot Studio

1. **Health Check**: `GET /mcp/health`
2. **Capabilities**: `GET /mcp/capabilities` 
3. **MCP Protocol**: `POST /mcp` (JSON-RPC 2.0)

### Sample MCP Tool Calls for AI Agents

#### Connect to RDP Server
```json
{
  "jsonrpc": "2.0",
  "id": "1",
  "method": "tools/call",
  "params": {
    "name": "connect_rdp",
    "arguments": {
      "host": "server.company.com",
      "username": "admin",
      "codec": "H265",
      "maxBitrate": 8000,
      "preset": "Quality",
      "width": 1920,
      "height": 1080
    }
  }
}
```

#### List Active Sessions
```json
{
  "jsonrpc": "2.0", 
  "id": "2",
  "method": "tools/call",
  "params": {
    "name": "list_rdp_sessions",
    "arguments": {"includeMetrics": true}
  }
}
```

#### Configure Session Performance
```json
{
  "jsonrpc": "2.0",
  "id": "3", 
  "method": "tools/call",
  "params": {
    "name": "configure_rdp_session",
    "arguments": {
      "sessionId": "session-123",
      "codec": "H264",
      "maxBitrate": 4000,
      "preset": "Performance"
    }
  }
}
```

### Validation

Run the comprehensive validation script:
```bash
./scripts/validate-mcp-server.sh
```

✅ **Full standalone MCP connector capabilities validated**:
- Model Context Protocol 2024-11-05 compliance
- H.264/H.265 codec support with configurable bitrate/resolution
- 3 comprehensive MCP tools with advanced codec configuration
- HTTP-based local connector for RetroRDP client communication
- Production-ready logging and error handling
- RESTful health check and capabilities endpoints

🚀 **Ready for integration with Microsoft Copilot Studio!**