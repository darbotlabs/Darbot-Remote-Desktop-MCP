#!/bin/bash

# RetroRDP Setup Wizard
# Interactive setup and configuration wizard for new users

echo "🚀 Welcome to RetroRDP Setup Wizard"
echo "==================================="
echo
echo "This wizard will help you get RetroRDP up and running quickly."
echo "Estimated time: 2-5 minutes"
echo

# Configuration
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
CLIENT_DIR="$PROJECT_ROOT/src/ClientApp/RetroRDPClient"
MCP_DIR="$PROJECT_ROOT/src/MCPServer/RetroRDP.MCPServer"
HEALTH_SCRIPT="$SCRIPT_DIR/health-check.sh"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

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

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

log_header() {
    echo
    echo -e "${CYAN}🔧 $1${NC}"
    echo "----------------------------------------"
}

# Ask yes/no question
ask_yes_no() {
    local question=$1
    local default=${2:-"y"}
    
    if [[ $default == "y" ]]; then
        prompt="[Y/n]"
    else
        prompt="[y/N]"
    fi
    
    read -p "$question $prompt: " answer
    answer=${answer:-$default}
    
    [[ $answer =~ ^[Yy]$ ]]
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Wait for user input
wait_for_enter() {
    read -p "Press Enter to continue..."
}

# Step 1: Welcome and System Check
step1_welcome() {
    log_header "Step 1: System Check and Welcome"
    
    echo "Let's start by checking your system compatibility..."
    echo
    
    # Check OS
    if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" || "$OSTYPE" == "cygwin" ]]; then
        log_success "Running on Windows - Full RDP functionality available"
        OS_COMPATIBLE=true
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        log_warning "Running on Linux - Simulation mode (limited RDP functionality)"
        echo "  💡 For full functionality, consider running on Windows"
        OS_COMPATIBLE=true
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        log_warning "Running on macOS - Simulation mode (limited RDP functionality)" 
        echo "  💡 For full functionality, consider running on Windows"
        OS_COMPATIBLE=true
    else
        log_error "Unsupported operating system: $OSTYPE"
        OS_COMPATIBLE=false
    fi
    
    # Check .NET
    if command_exists dotnet; then
        dotnet_version=$(dotnet --version 2>/dev/null)
        if [[ $? -eq 0 ]]; then
            if [[ $(echo "$dotnet_version" | cut -d'.' -f1) -ge 8 ]]; then
                log_success ".NET $dotnet_version is compatible"
                DOTNET_OK=true
            else
                log_error ".NET version too old (need 8.0+, have $dotnet_version)"
                DOTNET_OK=false
            fi
        else
            log_error ".NET runtime check failed"
            DOTNET_OK=false
        fi
    else
        log_warning ".NET not in PATH (assuming self-contained build)"
        DOTNET_OK=true
    fi
    
    echo
    if [[ $OS_COMPATIBLE == true && $DOTNET_OK == true ]]; then
        log_success "System compatibility: ✅ Good to go!"
    else
        log_error "System compatibility issues detected"
        echo "Please resolve the issues above before continuing."
        return 1
    fi
    
    wait_for_enter
}

# Step 2: Installation Choice  
step2_installation() {
    log_header "Step 2: Installation Method"
    
    echo "Choose your installation method:"
    echo
    echo "1. 📦 Build from Source (Developer)"
    echo "   • Latest features and customizations"
    echo "   • Requires .NET 8 SDK"
    echo "   • ~30 seconds build time"
    echo
    echo "2. 💾 Pre-built Release (End User)"
    echo "   • Stable, tested version"
    echo "   • No build required"
    echo "   • Download from GitHub Releases"
    echo
    read -p "Choose installation method [1/2]: " install_choice
    
    case $install_choice in
        1)
            INSTALL_METHOD="source"
            log_info "Selected: Build from source"
            ;;
        2)
            INSTALL_METHOD="release"
            log_info "Selected: Pre-built release"
            ;;
        *)
            log_info "Invalid choice, defaulting to build from source"
            INSTALL_METHOD="source"
            ;;
    esac
    
    wait_for_enter
}

# Step 3: Build/Install Application
step3_build() {
    log_header "Step 3: Building/Installing RetroRDP"
    
    if [[ $INSTALL_METHOD == "source" ]]; then
        log_info "Building RetroRDP from source..."
        
        # Check if we're in the right directory
        if [[ ! -f "$PROJECT_ROOT/RetroRDP.sln" ]]; then
            log_error "RetroRDP solution not found. Are you in the correct directory?"
            echo "Expected: $PROJECT_ROOT/RetroRDP.sln"
            return 1
        fi
        
        # Restore packages
        log_info "Restoring NuGet packages..."
        cd "$PROJECT_ROOT"
        if dotnet restore; then
            log_success "Package restore completed"
        else
            log_error "Package restore failed"
            return 1
        fi
        
        # Build solution
        log_info "Building solution..."
        if dotnet build --configuration Release --no-restore; then
            log_success "Build completed successfully"
        else
            log_error "Build failed"
            return 1
        fi
        
        # Run tests
        if ask_yes_no "Run tests to verify build quality?" "y"; then
            log_info "Running tests..."
            if dotnet test --configuration Release --no-build --verbosity quiet; then
                log_success "All tests passed ✅"
            else
                log_warning "Some tests failed - application may still work"
            fi
        fi
        
    else
        log_info "For pre-built releases:"
        echo "1. Visit: https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP/releases"
        echo "2. Download the latest RetroRDPClient-*-win-x64.zip"
        echo "3. Extract to your preferred location"
        echo "4. Come back here and we'll continue the setup"
        echo
        if ! ask_yes_no "Have you downloaded and extracted the release?"; then
            log_info "Setup paused. Please download the release and run this wizard again."
            return 1
        fi
    fi
    
    wait_for_enter
}

# Step 4: Configuration
step4_configure() {
    log_header "Step 4: Configuration"
    
    log_info "Configuring RetroRDP for optimal performance..."
    
    # Detect system specs for performance tuning
    echo "🔍 Detecting system specifications..."
    
    # Memory detection
    if command_exists free; then
        total_mem=$(free -m | awk 'NR==2{print $2}')
    elif [[ -f /proc/meminfo ]]; then
        total_mem=$(grep MemTotal /proc/meminfo | awk '{print int($2/1024)}')
    else
        total_mem=4096  # Default assumption
    fi
    
    # Recommend performance settings
    echo
    echo "💡 Performance Recommendations:"
    if [[ $total_mem -gt 8000 ]]; then
        echo "   • High-end system detected (${total_mem}MB RAM)"
        echo "   • Recommended: Quality preset, 3-5 concurrent sessions"
        RECOMMENDED_PRESET="Quality"
        RECOMMENDED_SESSIONS=5
    elif [[ $total_mem -gt 4000 ]]; then
        echo "   • Mid-range system detected (${total_mem}MB RAM)"
        echo "   • Recommended: Balanced preset, 2-3 concurrent sessions"
        RECOMMENDED_PRESET="Balanced"
        RECOMMENDED_SESSIONS=3
    else
        echo "   • Basic system detected (${total_mem}MB RAM)"
        echo "   • Recommended: Performance preset, 1-2 concurrent sessions"
        RECOMMENDED_PRESET="Performance"
        RECOMMENDED_SESSIONS=2
    fi
    
    echo
    if ask_yes_no "Use recommended settings?" "y"; then
        log_success "Using recommended performance settings"
        USE_RECOMMENDED=true
    else
        USE_RECOMMENDED=false
        echo "You can customize settings later in the application."
    fi
    
    # AI Features
    echo
    log_info "AI Assistant Configuration:"
    echo "RetroRDP includes an intelligent AI assistant for voice commands."
    echo
    echo "Features:"
    echo "• Natural language RDP commands ('connect to server1')"
    echo "• Session management and optimization suggestions"  
    echo "• Offline processing for privacy"
    echo
    
    if ask_yes_no "Enable AI assistant features?" "y"; then
        ENABLE_AI=true
        log_success "AI assistant will be enabled"
        
        # Check for models
        log_info "Checking for local AI models..."
        models_found=false
        
        # Common model locations (Windows)
        model_paths=(
            "$HOME/.cache/huggingface/"
            "$APPDATA/Microsoft/Phi/"
            "$CLIENT_DIR/Models/"
        )
        
        for path in "${model_paths[@]}"; do
            if [[ -d "$path" ]] && [[ $(find "$path" -name "*.onnx" 2>/dev/null | wc -l) -gt 0 ]]; then
                log_success "AI models found in $path"
                models_found=true
                break
            fi
        done
        
        if [[ $models_found == false ]]; then
            log_info "No local AI models detected - using enhanced fallback mode"
            echo "   💡 For full AI capabilities, you can download Phi-4 models later"
        fi
    else
        ENABLE_AI=false
        log_info "AI assistant will be disabled"
    fi
    
    wait_for_enter
}

# Step 5: MCP Server Setup (Optional)
step5_mcp() {
    log_header "Step 5: MCP Server Setup (Optional)"
    
    echo "The MCP (Model Context Protocol) server enables integration with"
    echo "AI assistants like Microsoft Copilot Studio."
    echo
    echo "Features:"
    echo "• Voice commands through external AI assistants"
    echo "• Remote session management via API"
    echo "• Integration with enterprise AI workflows"
    echo
    
    if ask_yes_no "Set up MCP Server for AI integration?" "n"; then
        SETUP_MCP=true
        log_info "Setting up MCP Server..."
        
        # Test build MCP server
        cd "$MCP_DIR"
        if dotnet build --configuration Release; then
            log_success "MCP Server built successfully"
            
            echo
            echo "MCP Server will be available at: http://localhost:5000"
            echo "Health check: http://localhost:5000/mcp/health"
            echo "Capabilities: http://localhost:5000/mcp/capabilities"
            echo
            
            if ask_yes_no "Start MCP Server now for testing?" "n"; then
                log_info "Starting MCP Server in background..."
                nohup dotnet run --no-build > /dev/null 2>&1 &
                MCP_PID=$!
                sleep 3
                
                if kill -0 $MCP_PID 2>/dev/null; then
                    log_success "MCP Server started (PID: $MCP_PID)"
                    MCP_STARTED=true
                else
                    log_warning "MCP Server failed to start"
                    MCP_STARTED=false
                fi
            else
                MCP_STARTED=false
            fi
        else
            log_error "MCP Server build failed"
            SETUP_MCP=false
            MCP_STARTED=false
        fi
    else
        SETUP_MCP=false
        MCP_STARTED=false
        log_info "Skipping MCP Server setup"
    fi
    
    wait_for_enter
}

# Step 6: Test Installation  
step6_test() {
    log_header "Step 6: Installation Testing"
    
    log_info "Testing your RetroRDP installation..."
    echo
    
    # Run health check if available
    if [[ -x "$HEALTH_SCRIPT" ]]; then
        log_info "Running comprehensive health check..."
        echo
        "$HEALTH_SCRIPT"
        health_result=$?
        echo
        
        if [[ $health_result -eq 0 ]]; then
            log_success "Health check passed! ✅"
        else
            log_warning "Health check found some issues - see details above"
        fi
    else
        log_warning "Health check script not found, performing basic tests..."
        
        # Basic tests
        if [[ -f "$CLIENT_DIR/bin/Release/net8.0/RetroRDPClient.dll" ]] || [[ -f "$CLIENT_DIR/bin/Release/net8.0-windows/RetroRDPClient.exe" ]]; then
            log_success "RetroRDP Client build found"
        else
            log_error "RetroRDP Client build not found"
        fi
    fi
    
    echo
    if ask_yes_no "Launch RetroRDP Client now?" "y"; then
        log_info "Launching RetroRDP Client..."
        
        cd "$PROJECT_ROOT"
        if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" || "$OSTYPE" == "cygwin" ]]; then
            # Windows - launch GUI
            if dotnet run --project src/ClientApp/RetroRDPClient --no-build & then
                CLIENT_PID=$!
                log_success "RetroRDP Client started (PID: $CLIENT_PID)"
                sleep 2
            else
                log_error "Failed to launch RetroRDP Client"
            fi
        else
            # Non-Windows - console mode
            log_info "Starting in console mode (GUI not available on this platform)"
            timeout 5 dotnet run --project src/ClientApp/RetroRDPClient --no-build || true
        fi
    fi
    
    wait_for_enter
}

# Step 7: Final Summary and Next Steps
step7_summary() {
    log_header "Setup Complete! 🎉"
    
    echo "RetroRDP has been successfully set up on your system!"
    echo
    
    echo "📋 Setup Summary:"
    echo "=================================="
    echo "• Installation Method: $INSTALL_METHOD"
    echo "• Performance Settings: ${RECOMMENDED_PRESET:-Custom}"
    echo "• AI Assistant: ${ENABLE_AI:-Disabled}"
    echo "• MCP Server: ${SETUP_MCP:-Disabled}"
    
    if [[ $MCP_STARTED == true ]]; then
        echo "• MCP Server Status: Running (PID: $MCP_PID)"
    fi
    
    echo
    echo "🚀 Next Steps:"
    echo "=================================="
    echo "1. 🖥️  Launch RetroRDP Client if not already running"
    echo "2. 🔗 Create your first RDP connection"
    echo "3. 🤖 Try the AI assistant: 'connect to [your-server]'"
    echo "4. ⚙️  Explore settings and customize the retro theme"
    echo "5. 📚 Read the User Guide for advanced features"
    echo
    
    echo "📁 Important Locations:"
    echo "• Application: $CLIENT_DIR"
    echo "• Logs: ~/.local/share/RetroRDPClient/logs/ (Linux) or %AppData%\\RetroRDPClient\\logs\\ (Windows)"
    echo "• Documentation: $PROJECT_ROOT/docs/"
    echo "• User Guide: $PROJECT_ROOT/UserGuide.md"
    echo
    
    if [[ $SETUP_MCP == true ]]; then
        echo "🔌 MCP Server Endpoints:"
        echo "• Health: http://localhost:5000/mcp/health"
        echo "• Capabilities: http://localhost:5000/mcp/capabilities"
        echo "• Integration Guide: $PROJECT_ROOT/docs/Copilot-Studio-MCP-Integration-Guide.md"
        echo
    fi
    
    echo "💡 Pro Tips:"
    echo "• Use 'Performance' preset for multiple sessions"
    echo "• Try AI commands like 'list sessions' or 'take screenshot'"
    echo "• Check the health check script regularly: scripts/health-check.sh"
    echo "• Join the discussion on GitHub for updates and support"
    echo
    
    log_success "Welcome to the future of Remote Desktop! 🚀✨"
    
    if ask_yes_no "Open User Guide in browser?" "n"; then
        if command_exists xdg-open; then
            xdg-open "$PROJECT_ROOT/UserGuide.md"
        elif command_exists open; then
            open "$PROJECT_ROOT/UserGuide.md"
        else
            echo "Please open: $PROJECT_ROOT/UserGuide.md"
        fi
    fi
}

# Cleanup function
cleanup() {
    if [[ ${MCP_PID:-} ]] && kill -0 $MCP_PID 2>/dev/null; then
        echo
        log_info "Cleaning up background processes..."
        kill $MCP_PID 2>/dev/null || true
    fi
}

# Set up trap for cleanup
trap cleanup EXIT

# Main execution flow
main() {
    # Check if script is being run from the right location
    if [[ ! -f "$PROJECT_ROOT/RetroRDP.sln" ]]; then
        log_error "Please run this script from the RetroRDP project root directory"
        log_info "Expected location: [RetroRDP-Directory]/scripts/setup-wizard.sh"
        exit 1
    fi
    
    # Execute setup steps
    step1_welcome || exit 1
    step2_installation || exit 1  
    step3_build || exit 1
    step4_configure || exit 1
    step5_mcp || exit 1
    step6_test || exit 1
    step7_summary
    
    echo "Setup wizard completed successfully! ✨"
}

# Run the setup wizard
main "$@"