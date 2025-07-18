@echo off
setlocal EnableDelayedExpansion

REM RetroRDP Client Deployment Script for Windows
REM This script builds and packages the application for distribution

set APP_NAME=RetroRDPClient
set VERSION=%1
if "%VERSION%"=="" set VERSION=1.0.0
set OUTPUT_DIR=.\dist
set PROJECT_PATH=.\src\ClientApp\RetroRDPClient

echo 🚀 Starting deployment process for %APP_NAME% v%VERSION%

REM Clean previous builds
echo 🧹 Cleaning previous builds...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
mkdir "%OUTPUT_DIR%"

REM Build solution first to ensure everything compiles
echo 🔨 Building solution...
dotnet build --configuration Release
if %ERRORLEVEL% neq 0 (
    echo ❌ Build failed
    exit /b 1
)

REM Run tests to ensure quality
echo 🧪 Running tests...
dotnet test --configuration Release --no-build --logger "console;verbosity=minimal"
if %ERRORLEVEL% neq 0 (
    echo ❌ Tests failed
    exit /b 1
)

REM Publish self-contained version
echo 📦 Publishing self-contained version...
dotnet publish "%PROJECT_PATH%" ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output "%OUTPUT_DIR%\self-contained" ^
    -p:PublishSingleFile=true ^
    -p:PublishReadyToRun=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:DebugType=embedded ^
    -p:PublishTrimmed=false

if %ERRORLEVEL% neq 0 (
    echo ❌ Self-contained publish failed
    exit /b 1
)

REM Publish framework-dependent version
echo 📦 Publishing framework-dependent version...
dotnet publish "%PROJECT_PATH%" ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained false ^
    --output "%OUTPUT_DIR%\framework-dependent" ^
    -p:PublishReadyToRun=true

if %ERRORLEVEL% neq 0 (
    echo ❌ Framework-dependent publish failed
    exit /b 1
)

REM Copy additional files
echo 📄 Copying documentation and licenses...
if exist README.md copy README.md "%OUTPUT_DIR%\" >nul 2>&1
if exist LICENSE copy LICENSE "%OUTPUT_DIR%\" >nul 2>&1

REM Create deployment info file
echo 📋 Creating deployment info...
(
echo RetroRDP Client Deployment Package
echo ==================================
echo.
echo Version: %VERSION%
echo Build Date: %DATE% %TIME%
echo .NET Version: 8.0
echo Target Platform: Windows x64
echo.
echo Contents:
echo - self-contained\: Complete application with .NET runtime included
echo - framework-dependent\: Smaller application requiring .NET 8 runtime
echo - README.md: Application documentation
echo - LICENSE: License information
echo.
echo Installation Instructions:
echo - Self-contained: Extract and run RetroRDPClient.exe directly
echo - Framework-dependent: Install .NET 8 runtime first, then run RetroRDPClient.exe
echo.
echo For support and updates, visit: https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP
) > "%OUTPUT_DIR%\deployment-info.txt"

REM Create ZIP packages for distribution (if 7-Zip or PowerShell available)
echo 📦 Creating ZIP packages...
cd "%OUTPUT_DIR%"

REM Try using PowerShell to create ZIP files
powershell -command "if (Get-Command Compress-Archive -ErrorAction SilentlyContinue) { Compress-Archive -Path 'self-contained', 'deployment-info.txt', 'README.md', 'LICENSE' -DestinationPath '%APP_NAME%-%VERSION%-SelfContained-win-x64.zip' -Force; Compress-Archive -Path 'framework-dependent', 'deployment-info.txt', 'README.md', 'LICENSE' -DestinationPath '%APP_NAME%-%VERSION%-FrameworkDependent-win-x64.zip' -Force; Write-Host '✅ ZIP packages created successfully' } else { Write-Host '⚠️ PowerShell Compress-Archive not available, skipping package creation' }"

cd ..

REM Display completion message
echo.
echo ✅ Deployment completed successfully!
echo 📁 Output directory: %OUTPUT_DIR%
echo.
echo Next steps:
echo 1. Test the packages on clean Windows machines
echo 2. Upload packages to GitHub Releases
echo 3. Update documentation with installation instructions

pause