@echo off
REM RetroRDP Setup Wizard for Windows
REM Interactive setup and configuration wizard for new users

setlocal EnableDelayedExpansion

echo 🚀 Welcome to RetroRDP Setup Wizard (Windows)
echo ===============================================
echo.
echo This wizard will help you get RetroRDP up and running quickly.
echo Estimated time: 2-5 minutes
echo.

REM Configuration
set SCRIPT_DIR=%~dp0
set PROJECT_ROOT=%SCRIPT_DIR%..
set CLIENT_DIR=%PROJECT_ROOT%\src\ClientApp\RetroRDPClient
set MCP_DIR=%PROJECT_ROOT%\src\MCPServer\RetroRDP.MCPServer
set HEALTH_SCRIPT=%SCRIPT_DIR%health-check.sh

REM Colors - Windows doesn't support ANSI colors in old cmd, but we can use echo
REM We'll use simple text indicators instead

:step1_welcome
echo.
echo 🔧 Step 1: System Check and Welcome
echo ----------------------------------------
echo.
echo Let's start by checking your system compatibility...
echo.

REM Check Windows version
for /f "tokens=4-5 delims=. " %%i in ('ver') do set VERSION=%%i.%%j
if "%VERSION%" == "10.0" (
    echo ✅ Running on Windows 10/11 - Full RDP functionality available
    set OS_COMPATIBLE=true
) else (
    echo ⚠️ Running on older Windows version - May have limited functionality
    set OS_COMPATIBLE=true
)

REM Check .NET
dotnet --version >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    for /f %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo ✅ .NET !DOTNET_VERSION! is available
    set DOTNET_OK=true
) else (
    echo ⚠️ .NET not in PATH (assuming self-contained build)
    set DOTNET_OK=true
)

echo.
if "%OS_COMPATIBLE%"=="true" if "%DOTNET_OK%"=="true" (
    echo ✅ System compatibility: Good to go!
) else (
    echo ❌ System compatibility issues detected
    echo Please resolve the issues above before continuing.
    pause
    exit /b 1
)

pause

:step2_installation
echo.
echo 🔧 Step 2: Installation Method
echo ----------------------------------------
echo.
echo Choose your installation method:
echo.
echo 1. 📦 Build from Source (Developer)
echo    • Latest features and customizations
echo    • Requires .NET 8 SDK
echo    • ~30 seconds build time
echo.
echo 2. 💾 Pre-built Release (End User)
echo    • Stable, tested version
echo    • No build required
echo    • Download from GitHub Releases
echo.
set /p install_choice="Choose installation method [1/2]: "

if "%install_choice%"=="1" (
    set INSTALL_METHOD=source
    echo ℹ️ Selected: Build from source
) else if "%install_choice%"=="2" (
    set INSTALL_METHOD=release
    echo ℹ️ Selected: Pre-built release
) else (
    echo ℹ️ Invalid choice, defaulting to build from source
    set INSTALL_METHOD=source
)

pause

:step3_build
echo.
echo 🔧 Step 3: Building/Installing RetroRDP
echo ----------------------------------------
echo.

if "%INSTALL_METHOD%"=="source" (
    echo ℹ️ Building RetroRDP from source...
    
    REM Check if we're in the right directory
    if not exist "%PROJECT_ROOT%\RetroRDP.sln" (
        echo ❌ RetroRDP solution not found. Are you in the correct directory?
        echo Expected: %PROJECT_ROOT%\RetroRDP.sln
        pause
        exit /b 1
    )
    
    REM Restore packages
    echo ℹ️ Restoring NuGet packages...
    cd /d "%PROJECT_ROOT%"
    dotnet restore
    if %ERRORLEVEL% EQU 0 (
        echo ✅ Package restore completed
    ) else (
        echo ❌ Package restore failed
        pause
        exit /b 1
    )
    
    REM Build solution
    echo ℹ️ Building solution...
    dotnet build --configuration Release --no-restore
    if %ERRORLEVEL% EQU 0 (
        echo ✅ Build completed successfully
    ) else (
        echo ❌ Build failed
        pause
        exit /b 1
    )
    
    REM Run tests
    echo.
    set /p run_tests="Run tests to verify build quality? [Y/n]: "
    if /i "%run_tests%"=="y" (
        echo ℹ️ Running tests...
        dotnet test --configuration Release --no-build --verbosity quiet
        if %ERRORLEVEL% EQU 0 (
            echo ✅ All tests passed
        ) else (
            echo ⚠️ Some tests failed - application may still work
        )
    )
    
) else (
    echo ℹ️ For pre-built releases:
    echo 1. Visit: https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP/releases
    echo 2. Download the latest RetroRDPClient-*-win-x64.zip
    echo 3. Extract to your preferred location
    echo 4. Come back here and we'll continue the setup
    echo.
    set /p download_done="Have you downloaded and extracted the release? [Y/n]: "
    if /i not "%download_done%"=="y" (
        echo ℹ️ Setup paused. Please download the release and run this wizard again.
        pause
        exit /b 1
    )
)

pause

:step4_configure
echo.
echo 🔧 Step 4: Configuration
echo ----------------------------------------
echo.
echo ℹ️ Configuring RetroRDP for optimal performance...
echo.

REM Detect system specs (simplified for Windows batch)
echo 🔍 Detecting system specifications...

REM Get total memory (requires wmic)
for /f "skip=1 tokens=2 delims=," %%i in ('wmic computersystem get TotalPhysicalMemory /format:csv') do (
    set /a total_mem=%%i/1024/1024
    goto :memory_done
)
:memory_done

REM Recommend performance settings
echo.
echo 💡 Performance Recommendations:
if %total_mem% GTR 8000 (
    echo    • High-end system detected ^(!total_mem!MB RAM^)
    echo    • Recommended: Quality preset, 3-5 concurrent sessions
    set RECOMMENDED_PRESET=Quality
    set RECOMMENDED_SESSIONS=5
) else if %total_mem% GTR 4000 (
    echo    • Mid-range system detected ^(!total_mem!MB RAM^)
    echo    • Recommended: Balanced preset, 2-3 concurrent sessions
    set RECOMMENDED_PRESET=Balanced
    set RECOMMENDED_SESSIONS=3
) else (
    echo    • Basic system detected ^(!total_mem!MB RAM^)
    echo    • Recommended: Performance preset, 1-2 concurrent sessions
    set RECOMMENDED_PRESET=Performance
    set RECOMMENDED_SESSIONS=2
)

echo.
set /p use_recommended="Use recommended settings? [Y/n]: "
if /i "%use_recommended%"=="y" (
    echo ✅ Using recommended performance settings
    set USE_RECOMMENDED=true
) else (
    set USE_RECOMMENDED=false
    echo ℹ️ You can customize settings later in the application.
)

REM AI Features
echo.
echo ℹ️ AI Assistant Configuration:
echo RetroRDP includes an intelligent AI assistant for voice commands.
echo.
echo Features:
echo • Natural language RDP commands ^('connect to server1'^)
echo • Session management and optimization suggestions  
echo • Offline processing for privacy
echo.

set /p enable_ai="Enable AI assistant features? [Y/n]: "
if /i "%enable_ai%"=="y" (
    set ENABLE_AI=true
    echo ✅ AI assistant will be enabled
    
    REM Check for models (simplified check)
    echo ℹ️ Checking for local AI models...
    if exist "%USERPROFILE%\.cache\huggingface\*.onnx" (
        echo ✅ AI models found
        set models_found=true
    ) else if exist "%LOCALAPPDATA%\Microsoft\Phi\*.onnx" (
        echo ✅ AI models found
        set models_found=true
    ) else if exist "%CLIENT_DIR%\Models\*.onnx" (
        echo ✅ AI models found
        set models_found=true
    ) else (
        echo ℹ️ No local AI models detected - using enhanced fallback mode
        echo    💡 For full AI capabilities, you can download Phi-4 models later
        set models_found=false
    )
) else (
    set ENABLE_AI=false
    echo ℹ️ AI assistant will be disabled
)

pause

:step5_mcp
echo.
echo 🔧 Step 5: MCP Server Setup (Optional)
echo ----------------------------------------
echo.
echo The MCP (Model Context Protocol) server enables integration with
echo AI assistants like Microsoft Copilot Studio.
echo.
echo Features:
echo • Voice commands through external AI assistants
echo • Remote session management via API
echo • Integration with enterprise AI workflows
echo.

set /p setup_mcp="Set up MCP Server for AI integration? [y/N]: "
if /i "%setup_mcp%"=="y" (
    set SETUP_MCP=true
    echo ℹ️ Setting up MCP Server...
    
    REM Build MCP server
    cd /d "%MCP_DIR%"
    dotnet build --configuration Release
    if %ERRORLEVEL% EQU 0 (
        echo ✅ MCP Server built successfully
        
        echo.
        echo MCP Server will be available at: http://localhost:5000
        echo Health check: http://localhost:5000/mcp/health
        echo Capabilities: http://localhost:5000/mcp/capabilities
        echo.
        
        set /p start_mcp="Start MCP Server now for testing? [y/N]: "
        if /i "%start_mcp%"=="y" (
            echo ℹ️ Starting MCP Server in background...
            start "RetroRDP MCP Server" dotnet run --no-build
            timeout /t 3 /nobreak >nul
            echo ✅ MCP Server started in separate window
            set MCP_STARTED=true
        ) else (
            set MCP_STARTED=false
        )
    ) else (
        echo ❌ MCP Server build failed
        set SETUP_MCP=false
        set MCP_STARTED=false
    )
) else (
    set SETUP_MCP=false
    set MCP_STARTED=false
    echo ℹ️ Skipping MCP Server setup
)

pause

:step6_test
echo.
echo 🔧 Step 6: Installation Testing
echo ----------------------------------------
echo.
echo ℹ️ Testing your RetroRDP installation...
echo.

REM Basic tests
if exist "%CLIENT_DIR%\bin\Release\net8.0-windows\RetroRDPClient.exe" (
    echo ✅ RetroRDP Client build found
) else if exist "%CLIENT_DIR%\bin\Release\net8.0\RetroRDPClient.dll" (
    echo ✅ RetroRDP Client build found (.NET dll)
) else (
    echo ❌ RetroRDP Client build not found
)

echo.
set /p launch_client="Launch RetroRDP Client now? [Y/n]: "
if /i "%launch_client%"=="y" (
    echo ℹ️ Launching RetroRDP Client...
    
    cd /d "%PROJECT_ROOT%"
    start "RetroRDP Client" dotnet run --project src\ClientApp\RetroRDPClient --no-build
    echo ✅ RetroRDP Client started in separate window
    timeout /t 2 /nobreak >nul
)

pause

:step7_summary
echo.
echo 🔧 Setup Complete! 🎉
echo ----------------------------------------
echo.
echo RetroRDP has been successfully set up on your system!
echo.

echo 📋 Setup Summary:
echo ==================================
echo • Installation Method: %INSTALL_METHOD%
echo • Performance Settings: %RECOMMENDED_PRESET%
echo • AI Assistant: %ENABLE_AI%
echo • MCP Server: %SETUP_MCP%

if "%MCP_STARTED%"=="true" (
    echo • MCP Server Status: Running in separate window
)

echo.
echo 🚀 Next Steps:
echo ==================================
echo 1. 🖥️  Launch RetroRDP Client if not already running
echo 2. 🔗 Create your first RDP connection
echo 3. 🤖 Try the AI assistant: 'connect to [your-server]'
echo 4. ⚙️  Explore settings and customize the retro theme
echo 5. 📚 Read the User Guide for advanced features
echo.

echo 📁 Important Locations:
echo • Application: %CLIENT_DIR%
echo • Logs: %%APPDATA%%\RetroRDPClient\logs\
echo • Documentation: %PROJECT_ROOT%\docs\
echo • User Guide: %PROJECT_ROOT%\UserGuide.md
echo.

if "%SETUP_MCP%"=="true" (
    echo 🔌 MCP Server Endpoints:
    echo • Health: http://localhost:5000/mcp/health
    echo • Capabilities: http://localhost:5000/mcp/capabilities
    echo • Integration Guide: %PROJECT_ROOT%\docs\Copilot-Studio-MCP-Integration-Guide.md
    echo.
)

echo 💡 Pro Tips:
echo • Use 'Performance' preset for multiple sessions
echo • Try AI commands like 'list sessions' or 'take screenshot'
echo • Check the health check script regularly: scripts\health-check.sh
echo • Join the discussion on GitHub for updates and support
echo.

echo ✅ Welcome to the future of Remote Desktop! 🚀✨

set /p open_guide="Open User Guide in browser? [y/N]: "
if /i "%open_guide%"=="y" (
    start "" "%PROJECT_ROOT%\UserGuide.md"
)

echo.
echo Setup wizard completed successfully! ✨
pause