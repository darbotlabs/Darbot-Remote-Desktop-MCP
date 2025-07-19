#!/bin/bash

# RetroRDP MCP Server - Quick Start Script
# Builds and runs the MCP server for Copilot Studio integration

set -e

echo "🚀 RetroRDP MCP Server - Quick Start"
echo "====================================="

# Configuration
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../src/MCPServer/RetroRDP.MCPServer" && pwd)"
SERVER_URL="http://localhost:5000"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Check prerequisites
check_prerequisites() {
    log_info "Checking prerequisites..."
    
    if ! command -v dotnet &> /dev/null; then
        echo "❌ .NET 8.0 SDK is required but not installed"
        echo "   Download from: https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    fi
    
    dotnet_version=$(dotnet --version)
    log_success ".NET SDK version: $dotnet_version"
    
    if ! command -v curl &> /dev/null; then
        log_warning "curl not found - health checks will be skipped"
    fi
}

# Build the project
build_project() {
    log_info "Building MCP server project..."
    
    cd "$PROJECT_DIR"
    
    if dotnet build --configuration Release; then
        log_success "Project built successfully"
    else
        echo "❌ Build failed"
        exit 1
    fi
}

# Start the server
start_server() {
    log_info "Starting MCP server on $SERVER_URL..."
    
    cd "$PROJECT_DIR"
    
    echo
    log_success "RetroRDP MCP Server starting..."
    echo -e "${BLUE}📡 Server URL: $SERVER_URL${NC}"
    echo -e "${BLUE}🔍 Health Check: $SERVER_URL/mcp/health${NC}"
    echo -e "${BLUE}🛠️  Capabilities: $SERVER_URL/mcp/capabilities${NC}"
    echo -e "${BLUE}📖 Integration Guide: docs/Copilot-Studio-MCP-Integration-Guide.md${NC}"
    echo
    echo "Press Ctrl+C to stop the server"
    echo
    
    # Run with production settings
    ASPNETCORE_ENVIRONMENT=Production dotnet run --configuration Release
}

# Health check after startup
health_check() {
    if command -v curl &> /dev/null; then
        log_info "Testing server health..."
        sleep 2
        
        if curl -s --max-time 5 "$SERVER_URL/mcp/health" > /dev/null; then
            log_success "Server is healthy and ready for Copilot Studio integration"
        else
            log_warning "Server health check failed"
        fi
    fi
}

# Main execution
main() {
    echo
    check_prerequisites
    echo
    build_project
    echo
    
    # Start server (this will block)
    start_server
}

# Handle script interruption
cleanup() {
    echo
    log_info "Shutting down MCP server..."
    exit 0
}

trap cleanup SIGINT SIGTERM

# Show help
if [ "$1" = "--help" ] || [ "$1" = "-h" ]; then
    echo "RetroRDP MCP Server Quick Start"
    echo
    echo "Usage: $0 [options]"
    echo
    echo "Options:"
    echo "  --help, -h     Show this help message"
    echo "  --validate     Run validation tests after starting"
    echo
    echo "This script will:"
    echo "1. Check .NET 8.0 SDK prerequisites"
    echo "2. Build the RetroRDP MCP Server project"
    echo "3. Start the server on http://localhost:5000"
    echo
    echo "Integration:"
    echo "- See docs/Copilot-Studio-MCP-Integration-Guide.md for complete setup"
    echo "- Use scripts/validate-mcp-server.sh to test all capabilities"
    echo
    echo "MCP Tools Available:"
    echo "- connect_rdp: Connect to RDP servers with H.264/H.265 codecs"
    echo "- list_rdp_sessions: List active sessions with performance metrics"  
    echo "- configure_rdp_session: Configure video codec, bitrate, resolution"
    echo
    exit 0
fi

# Run validation after startup
if [ "$1" = "--validate" ]; then
    # Start server in background for validation
    main &
    SERVER_PID=$!
    
    # Wait for server to start
    log_info "Starting server for validation..."
    sleep 5
    
    # Run validation
    if [ -f "scripts/validate-mcp-server.sh" ]; then
        ./scripts/validate-mcp-server.sh
    else
        log_warning "Validation script not found"
    fi
    
    # Stop server
    kill $SERVER_PID 2>/dev/null || true
    exit 0
fi

# Normal startup
main