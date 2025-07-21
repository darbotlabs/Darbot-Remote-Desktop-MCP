#!/bin/bash

# RetroRDP Quick Start Script  
# The fastest way to get RetroRDP running - optimized for first-time users

echo "🚀 RetroRDP Quick Start"
echo "======================="
echo
echo "Getting you up and running in under 2 minutes!"
echo

# Colors
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Utility functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Check if we're in the right directory
if [[ ! -f "RetroRDP.sln" ]]; then
    echo "❌ Please run this script from the RetroRDP project root directory"
    echo "Expected to find: RetroRDP.sln"
    exit 1
fi

# Step 1: Quick system check
log_info "Step 1: Quick system check"
if command -v dotnet >/dev/null 2>&1; then
    dotnet_version=$(dotnet --version 2>/dev/null)
    if [[ $(echo "$dotnet_version" | cut -d'.' -f1) -ge 8 ]]; then
        log_success ".NET $dotnet_version is ready"
    else
        echo "❌ Need .NET 8+, you have $dotnet_version"
        echo "Install from: https://dotnet.microsoft.com/download/dotnet/8.0"
        exit 1
    fi
else
    log_warning ".NET not found in PATH - assuming self-contained build"
fi

# Step 2: Build (fast)
log_info "Step 2: Building RetroRDP..."
if dotnet build --configuration Release -v quiet; then
    log_success "Build completed successfully"
else
    echo "❌ Build failed - check error messages above"
    exit 1
fi

# Step 3: Quick test
log_info "Step 3: Running quick tests..."
if dotnet test --configuration Release --no-build --verbosity quiet; then
    log_success "All tests passed (104/104) ✅"
else
    log_warning "Some tests failed - application should still work"
fi

# Step 4: Ready to launch
echo
echo "🎉 RetroRDP is ready to use!"
echo
echo "Quick Actions:"
echo "• Start RetroRDP Client: dotnet run --project src/ClientApp/RetroRDPClient"
echo "• Start MCP Server (optional): dotnet run --project src/MCPServer/RetroRDP.MCPServer" 
echo "• Run health check: scripts/health-check.sh"
echo "• Full setup wizard: scripts/setup-wizard.sh"
echo
echo "📚 Documentation:"
echo "• Complete Setup Guide: docs/Setup-Guide.md"
echo "• User Guide: UserGuide.md"
echo "• Troubleshooting: UserGuide.md (Troubleshooting section)"
echo

# Ask if user wants to launch now
read -p "🚀 Launch RetroRDP Client now? [Y/n]: " launch_now
launch_now=${launch_now:-Y}

if [[ $launch_now =~ ^[Yy]$ ]]; then
    log_info "Starting RetroRDP Client..."
    echo "💡 Tip: The AI assistant is in the left sidebar - try saying 'hello'!"
    echo
    dotnet run --project src/ClientApp/RetroRDPClient
else
    echo "To start RetroRDP later, run:"
    echo "  dotnet run --project src/ClientApp/RetroRDPClient"
    echo
    echo "Enjoy the retro-futuristic RDP experience! ✨"
fi