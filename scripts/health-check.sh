#!/bin/bash

# RetroRDP Installation Health Check Script
# Validates that RetroRDP client is properly configured and operational

echo "🔍 RetroRDP Installation Health Check"
echo "===================================="
echo

# Configuration
CLIENT_PORT=8080
MCP_SERVER_PORT=5000
LOG_PATH="$HOME/.local/share/RetroRDPClient/logs"
CONFIG_PATH="$HOME/.config/RetroRDPClient"

# Windows paths (if running on Windows/WSL)
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" || "$OSTYPE" == "cygwin" ]]; then
    LOG_PATH="$APPDATA/RetroRDPClient/logs"
    CONFIG_PATH="$APPDATA/RetroRDPClient"
fi

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test counters
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0

# Utility functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
    ((PASSED_CHECKS++))
    ((TOTAL_CHECKS++))
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
    ((TOTAL_CHECKS++))
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
    ((FAILED_CHECKS++))
    ((TOTAL_CHECKS++))
}

log_header() {
    echo
    echo -e "${BLUE}🔍 $1${NC}"
    echo "----------------------------------------"
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Check if port is listening
port_listening() {
    if command_exists netstat; then
        netstat -tuln 2>/dev/null | grep ":$1 " >/dev/null
    elif command_exists ss; then
        ss -tuln 2>/dev/null | grep ":$1 " >/dev/null  
    else
        return 1
    fi
}

# Check HTTP endpoint
check_http_endpoint() {
    local url=$1
    local name=$2
    
    if command_exists curl; then
        if response=$(curl -s -w "%{http_code}" -o /dev/null "$url" 2>/dev/null); then
            if [[ $response -eq 200 ]]; then
                log_success "$name endpoint is responding (HTTP $response)"
                return 0
            else
                log_error "$name endpoint returned HTTP $response"
                return 1
            fi
        else
            log_error "$name endpoint is not accessible"
            return 1
        fi
    elif command_exists wget; then
        if wget -q --spider "$url" 2>/dev/null; then
            log_success "$name endpoint is responding"
            return 0
        else
            log_error "$name endpoint is not accessible"
            return 1
        fi
    else
        log_warning "$name endpoint check skipped (no curl/wget available)"
        return 0
    fi
}

# Main health checks
main() {
    log_info "Starting RetroRDP health check..."
    echo

    # Check 1: System Requirements
    log_header "System Requirements"
    
    # Check OS
    if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" || "$OSTYPE" == "cygwin" ]]; then
        log_success "Running on Windows (optimal platform)"
    elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
        log_warning "Running on Linux (simulation mode, limited RDP functionality)"
    elif [[ "$OSTYPE" == "darwin"* ]]; then
        log_warning "Running on macOS (simulation mode, limited RDP functionality)"
    else
        log_warning "Unknown operating system: $OSTYPE"
    fi

    # Check .NET availability
    if command_exists dotnet; then
        dotnet_version=$(dotnet --version 2>/dev/null)
        if [[ $? -eq 0 ]]; then
            log_success ".NET runtime available (version $dotnet_version)"
            
            # Check if it's .NET 8+
            if [[ $(echo "$dotnet_version" | cut -d'.' -f1) -ge 8 ]]; then
                log_success ".NET version is compatible (.NET 8+)"
            else
                log_error ".NET version is too old (need .NET 8+, have $dotnet_version)"
            fi
        else
            log_error ".NET runtime check failed"
        fi
    else
        log_warning ".NET runtime not in PATH (may be using self-contained version)"
    fi

    # Check 2: RetroRDP Client Application
    log_header "RetroRDP Client Application"
    
    # Check if client is running (by checking port or process)
    if port_listening $CLIENT_PORT; then
        log_success "RetroRDP Client appears to be running (port $CLIENT_PORT listening)"
        
        # Check client health endpoint
        check_http_endpoint "http://localhost:$CLIENT_PORT/health" "RetroRDP Client"
        
    else
        log_warning "RetroRDP Client not detected on port $CLIENT_PORT (may not be running)"
    fi

    # Check 3: MCP Server
    log_header "MCP Server (AI Integration)"
    
    if port_listening $MCP_SERVER_PORT; then
        log_success "MCP Server appears to be running (port $MCP_SERVER_PORT listening)"
        
        # Check MCP server health
        check_http_endpoint "http://localhost:$MCP_SERVER_PORT/mcp/health" "MCP Server Health"
        
        # Check MCP capabilities  
        if command_exists curl; then
            if curl -s "http://localhost:$MCP_SERVER_PORT/mcp/capabilities" | grep -q "RetroRDP MCP Server" 2>/dev/null; then
                log_success "MCP Server capabilities are working"
            else
                log_error "MCP Server capabilities endpoint failed"
            fi
        fi
        
    else
        log_warning "MCP Server not detected on port $MCP_SERVER_PORT (may not be needed for basic usage)"
    fi

    # Check 4: Configuration and Data Directories
    log_header "Configuration and Data"
    
    if [[ -d "$CONFIG_PATH" ]]; then
        log_success "Configuration directory exists: $CONFIG_PATH"
    else
        log_warning "Configuration directory not found: $CONFIG_PATH (will be created on first run)"
    fi
    
    if [[ -d "$LOG_PATH" ]]; then
        log_success "Log directory exists: $LOG_PATH"
        
        # Check for recent log files
        if find "$LOG_PATH" -name "*.log" -mtime -1 2>/dev/null | grep -q .; then
            log_success "Recent log files found (application has been active)"
        else
            log_warning "No recent log files found (application may not have been used recently)"
        fi
    else
        log_warning "Log directory not found: $LOG_PATH (will be created on first run)"
    fi

    # Check 5: Dependencies and Libraries  
    log_header "Dependencies and Libraries"
    
    # Check for RDP-related Windows components (Windows only)
    if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "win32" || "$OSTYPE" == "cygwin" ]]; then
        if [[ -f "/c/Windows/System32/mstscax.dll" ]] || [[ -f "C:\\Windows\\System32\\mstscax.dll" ]]; then
            log_success "RDP ActiveX control found (mstscax.dll)"
        else
            log_error "RDP ActiveX control not found - RDP functionality may not work"
        fi
    else
        log_info "Skipping Windows-specific dependency checks on non-Windows platform"
    fi

    # Check 6: Network Connectivity
    log_header "Network Connectivity"
    
    # Test basic network connectivity
    if command_exists ping; then
        if ping -c 1 -W 3 127.0.0.1 >/dev/null 2>&1; then
            log_success "Localhost connectivity working"
        else
            log_error "Localhost connectivity failed"
        fi
        
        # Test external connectivity (for updates/AI models if needed)
        if ping -c 1 -W 3 8.8.8.8 >/dev/null 2>&1; then
            log_success "External network connectivity available"
        else
            log_warning "External network connectivity unavailable (may affect AI model downloads)"
        fi
    else
        log_warning "Network connectivity tests skipped (no ping available)"
    fi

    # Check 7: Performance and Resources
    log_header "Performance and Resources"
    
    # Check available memory
    if command_exists free; then
        total_mem=$(free -m | awk 'NR==2{print $2}')
        if [[ $total_mem -gt 4000 ]]; then
            log_success "Sufficient RAM available (${total_mem}MB total)"
        elif [[ $total_mem -gt 2000 ]]; then
            log_warning "Limited RAM available (${total_mem}MB) - may affect multi-session performance"
        else
            log_error "Insufficient RAM (${total_mem}MB) - need at least 2GB"
        fi
    elif [[ -f /proc/meminfo ]]; then
        total_mem=$(grep MemTotal /proc/meminfo | awk '{print int($2/1024)}')
        if [[ $total_mem -gt 4000 ]]; then
            log_success "Sufficient RAM available (${total_mem}MB total)"
        elif [[ $total_mem -gt 2000 ]]; then
            log_warning "Limited RAM available (${total_mem}MB) - may affect multi-session performance"  
        else
            log_error "Insufficient RAM (${total_mem}MB) - need at least 2GB"
        fi
    else
        log_warning "Memory check skipped (unable to determine available RAM)"
    fi
    
    # Check disk space
    if command_exists df; then
        available_space=$(df -BM . | awk 'NR==2 {print $4}' | sed 's/M//')
        if [[ $available_space -gt 1000 ]]; then
            log_success "Sufficient disk space available (${available_space}MB)"
        elif [[ $available_space -gt 500 ]]; then
            log_warning "Limited disk space (${available_space}MB) - recommend 1GB+ free space"
        else
            log_error "Insufficient disk space (${available_space}MB) - need at least 500MB"
        fi
    else
        log_warning "Disk space check skipped"
    fi

    # Summary
    echo
    echo "========================================"
    echo -e "${BLUE}🏁 Health Check Summary${NC}"
    echo "========================================"
    echo -e "Total Checks: ${TOTAL_CHECKS}"
    echo -e "${GREEN}Passed: ${PASSED_CHECKS}${NC}"
    echo -e "${RED}Failed: ${FAILED_CHECKS}${NC}"
    echo -e "${YELLOW}Warnings: $((TOTAL_CHECKS - PASSED_CHECKS - FAILED_CHECKS))${NC}"
    echo

    if [[ $FAILED_CHECKS -eq 0 ]]; then
        if [[ $((TOTAL_CHECKS - PASSED_CHECKS - FAILED_CHECKS)) -eq 0 ]]; then
            echo -e "${GREEN}🎉 Perfect! RetroRDP is fully operational.${NC}"
            exit_code=0
        else
            echo -e "${YELLOW}⚡ Good! RetroRDP should work with minor limitations.${NC}"
            exit_code=0
        fi
    else
        echo -e "${RED}⚠️  Issues detected. RetroRDP may not work properly.${NC}"
        exit_code=1
    fi

    echo
    echo "📋 Recommendations:"
    
    if [[ $FAILED_CHECKS -gt 0 ]]; then
        echo "• Fix critical issues marked with ❌"
        echo "• Consider running the application as administrator if needed"
        echo "• Check firewall/antivirus settings"
    fi
    
    if port_listening $CLIENT_PORT; then
        echo "• ✅ RetroRDP Client is ready to use!"
    else
        echo "• Start the RetroRDP Client application"
    fi
    
    if ! port_listening $MCP_SERVER_PORT; then
        echo "• Consider starting the MCP Server for AI features"
    fi
    
    echo "• Review the Setup Guide for detailed configuration: docs/Setup-Guide.md"
    echo "• Check application logs if you encounter issues: $LOG_PATH"
    
    echo
    echo -e "${BLUE}For detailed setup instructions, see: docs/Setup-Guide.md${NC}"
    echo -e "${BLUE}For troubleshooting help, see: UserGuide.md${NC}"
    
    exit $exit_code
}

# Run the health check
main "$@"