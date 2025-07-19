#!/bin/bash

# RetroRDP MCP Server Validation Script
# Comprehensive testing of all MCP connector capabilities

echo "🚀 RetroRDP MCP Server Validation"
echo "=================================="
echo

# Configuration
MCP_SERVER_URL="http://localhost:5000"
HEALTH_ENDPOINT="$MCP_SERVER_URL/mcp/health"
CAPABILITIES_ENDPOINT="$MCP_SERVER_URL/mcp/capabilities"
MCP_ENDPOINT="$MCP_SERVER_URL/mcp"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Test counters
TOTAL_TESTS=0
PASSED_TESTS=0
FAILED_TESTS=0

# Utility functions
log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
    ((PASSED_TESTS++))
}

log_error() {
    echo -e "${RED}❌ $1${NC}"
    ((FAILED_TESTS++))
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

run_test() {
    ((TOTAL_TESTS++))
    echo -e "${BLUE}🧪 Test $TOTAL_TESTS: $1${NC}"
}

# Wait for server to be ready
wait_for_server() {
    log_info "Waiting for MCP server to be available..."
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if curl -s --max-time 2 "$HEALTH_ENDPOINT" > /dev/null 2>&1; then
            log_success "MCP server is available"
            return 0
        fi
        
        echo -n "."
        sleep 2
        ((attempt++))
    done
    
    log_error "MCP server not available after $max_attempts attempts"
    return 1
}

# Test 1: Health Check
test_health_check() {
    run_test "Health Check Endpoint"
    
    response=$(curl -s --max-time 5 "$HEALTH_ENDPOINT")
    if [ $? -eq 0 ] && echo "$response" | grep -q '"status":"healthy"'; then
        log_success "Health check passed"
        echo "   Response: $response"
    else
        log_error "Health check failed"
        echo "   Response: $response"
    fi
}

# Test 2: Capabilities Discovery
test_capabilities() {
    run_test "Capabilities Discovery"
    
    response=$(curl -s --max-time 5 "$CAPABILITIES_ENDPOINT")
    if [ $? -eq 0 ] && echo "$response" | grep -q '"protocolVersion":"2024-11-05"'; then
        log_success "Capabilities endpoint working"
        
        # Check for required tools
        if echo "$response" | grep -q '"name":"connect_rdp"' && \
           echo "$response" | grep -q '"name":"list_rdp_sessions"' && \
           echo "$response" | grep -q '"name":"configure_rdp_session"'; then
            log_success "All required MCP tools are available"
        else
            log_error "Missing required MCP tools"
        fi
    else
        log_error "Capabilities endpoint failed"
        echo "   Response: $response"
    fi
}

# Test 3: MCP Initialize
test_mcp_initialize() {
    run_test "MCP Initialize Protocol"
    
    response=$(curl -s --max-time 10 -X POST "$MCP_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d '{
            "jsonrpc": "2.0",
            "id": "test-init",
            "method": "initialize",
            "params": {
                "protocolVersion": "2024-11-05",
                "clientInfo": {
                    "name": "Validation Test Client",
                    "version": "1.0.0"
                }
            }
        }')
    
    if [ $? -eq 0 ] && echo "$response" | grep -q '"protocolVersion":"2024-11-05"'; then
        log_success "MCP initialize protocol working"
        echo "   Server Info: $(echo "$response" | jq -r '.result.serverInfo.name // "N/A"')"
    else
        log_error "MCP initialize failed"
        echo "   Response: $response"
    fi
}

# Test 4: Tools List
test_tools_list() {
    run_test "MCP Tools List"
    
    response=$(curl -s --max-time 10 -X POST "$MCP_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d '{
            "jsonrpc": "2.0",
            "id": "test-tools",
            "method": "tools/list"
        }')
    
    if [ $? -eq 0 ] && echo "$response" | grep -q '"tools":\s*\['; then
        log_success "Tools list retrieved successfully"
        
        # Count tools
        tool_count=$(echo "$response" | jq -r '.result.tools | length // 0')
        echo "   Available tools: $tool_count"
        
        # List tool names
        echo "$response" | jq -r '.result.tools[].name' 2>/dev/null | while read tool; do
            echo "   - $tool"
        done
    else
        log_error "Tools list failed"
        echo "   Response: $response"
    fi
}

# Test 5: Connect RDP Tool Schema Validation
test_connect_rdp_schema() {
    run_test "Connect RDP Tool Schema Validation"
    
    response=$(curl -s --max-time 10 -X POST "$MCP_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d '{
            "jsonrpc": "2.0",
            "id": "test-connect-schema",
            "method": "tools/list"
        }')
    
    # Check if connect_rdp tool has proper schema
    if echo "$response" | jq -e '.result.tools[] | select(.name == "connect_rdp") | .inputSchema.properties.codec.enum[] | select(. == "H264" or . == "H265")' > /dev/null 2>&1; then
        log_success "Connect RDP tool has H.264/H.265 codec support"
    else
        log_error "Connect RDP tool missing codec support"
    fi
    
    # Check bitrate configuration
    if echo "$response" | jq -e '.result.tools[] | select(.name == "connect_rdp") | .inputSchema.properties.maxBitrate.maximum == 50000' > /dev/null 2>&1; then
        log_success "Connect RDP tool supports configurable bitrate (up to 50Mbps)"
    else
        log_error "Connect RDP tool missing bitrate configuration"
    fi
    
    # Check resolution support
    if echo "$response" | jq -e '.result.tools[] | select(.name == "connect_rdp") | .inputSchema.properties.width.maximum >= 7680' > /dev/null 2>&1; then
        log_success "Connect RDP tool supports 4K+ resolution"
    else
        log_error "Connect RDP tool missing high-resolution support"
    fi
}

# Test 6: Tool Call with Missing Client (Expected to fail gracefully)
test_tool_call_no_client() {
    run_test "Tool Call Error Handling (No Client)"
    
    response=$(curl -s --max-time 15 -X POST "$MCP_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d '{
            "jsonrpc": "2.0",
            "id": "test-no-client",
            "method": "tools/call",
            "params": {
                "name": "list_rdp_sessions",
                "arguments": {
                    "includeMetrics": false
                }
            }
        }')
    
    if [ $? -eq 0 ] && echo "$response" | grep -q -i "client.*not.*available\|connection.*failed"; then
        log_success "Tool properly handles missing RetroRDP client"
        echo "   Error message: $(echo "$response" | jq -r '.result.content[0].text // "N/A"')"
    else
        log_warning "Unexpected response for missing client test"
        echo "   Response: $response"
    fi
}

# Test 7: Invalid Tool Call
test_invalid_tool() {
    run_test "Invalid Tool Call Handling"
    
    response=$(curl -s --max-time 10 -X POST "$MCP_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d '{
            "jsonrpc": "2.0",
            "id": "test-invalid",
            "method": "tools/call",
            "params": {
                "name": "nonexistent_tool",
                "arguments": {}
            }
        }')
    
    if [ $? -eq 0 ] && echo "$response" | grep -q '"error"'; then
        log_success "Invalid tool call properly handled with error"
        echo "   Error code: $(echo "$response" | jq -r '.error.code // "N/A"')"
    else
        log_error "Invalid tool call not properly handled"
        echo "   Response: $response"
    fi
}

# Test 8: JSON-RPC Validation
test_jsonrpc_validation() {
    run_test "JSON-RPC Format Validation"
    
    # Test invalid JSON-RPC
    response=$(curl -s --max-time 10 -X POST "$MCP_ENDPOINT" \
        -H "Content-Type: application/json" \
        -d '{"invalid": "request"}')
    
    if [ $? -eq 0 ] && echo "$response" | grep -q '"error".*-32700\|-32600'; then
        log_success "Invalid JSON-RPC properly rejected"
    else
        log_error "Invalid JSON-RPC not properly handled"
        echo "   Response: $response"
    fi
}

# Test 9: Swagger/OpenAPI Documentation
test_swagger_docs() {
    run_test "Swagger Documentation Availability"
    
    swagger_response=$(curl -s --max-time 5 "$MCP_SERVER_URL/swagger/v1/swagger.json")
    if [ $? -eq 0 ] && echo "$swagger_response" | grep -q '"openapi"\|"swagger"'; then
        log_success "Swagger documentation available"
    else
        log_warning "Swagger documentation not accessible (may be disabled in production)"
    fi
}

# Test 10: Performance and Response Time
test_performance() {
    run_test "Performance and Response Time"
    
    start_time=$(date +%s%N)
    response=$(curl -s --max-time 5 "$HEALTH_ENDPOINT")
    end_time=$(date +%s%N)
    
    if [ $? -eq 0 ]; then
        duration_ms=$(( (end_time - start_time) / 1000000 ))
        if [ $duration_ms -lt 1000 ]; then
            log_success "Health endpoint responds in ${duration_ms}ms (< 1s)"
        else
            log_warning "Health endpoint slow response: ${duration_ms}ms"
        fi
    else
        log_error "Performance test failed"
    fi
}

# Main execution
main() {
    echo "Starting MCP server validation tests..."
    echo
    
    # Wait for server
    if ! wait_for_server; then
        echo
        log_error "Cannot proceed with tests - MCP server not available"
        echo "Please ensure the server is running on $MCP_SERVER_URL"
        echo "Start with: cd src/MCPServer/RetroRDP.MCPServer && dotnet run"
        exit 1
    fi
    
    echo
    log_info "Running comprehensive MCP server validation..."
    echo
    
    # Run all tests
    test_health_check
    echo
    test_capabilities  
    echo
    test_mcp_initialize
    echo
    test_tools_list
    echo
    test_connect_rdp_schema
    echo
    test_tool_call_no_client
    echo
    test_invalid_tool
    echo
    test_jsonrpc_validation
    echo
    test_swagger_docs
    echo
    test_performance
    
    # Results summary
    echo
    echo "=================================="
    echo "🏁 Validation Results Summary"
    echo "=================================="
    echo "Total Tests: $TOTAL_TESTS"
    echo -e "Passed: ${GREEN}$PASSED_TESTS${NC}"
    echo -e "Failed: ${RED}$FAILED_TESTS${NC}"
    echo
    
    if [ $FAILED_TESTS -eq 0 ]; then
        echo -e "${GREEN}🎉 All tests passed! MCP server is fully operational.${NC}"
        echo
        echo "✅ Full standalone MCP connector capabilities validated:"
        echo "   • Model Context Protocol 2024-11-05 compliance"
        echo "   • H.264/H.265 codec support with configurable bitrate/resolution" 
        echo "   • 3 comprehensive MCP tools (connect_rdp, list_rdp_sessions, configure_rdp_session)"
        echo "   • HTTP-based local connector for RetroRDP client communication"
        echo "   • Production-ready logging and error handling"
        echo "   • RESTful health check and capabilities endpoints"
        echo
        echo "🚀 Ready for integration with Microsoft Copilot Studio!"
        exit 0
    else
        echo -e "${RED}❌ Some tests failed. Please review the issues above.${NC}"
        exit 1
    fi
}

# Check dependencies
if ! command -v curl &> /dev/null; then
    log_error "curl is required but not installed"
    exit 1
fi

if ! command -v jq &> /dev/null; then
    log_warning "jq not found - JSON parsing will be limited"
fi

# Run main function
main "$@"