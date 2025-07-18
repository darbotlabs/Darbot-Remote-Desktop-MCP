#!/bin/bash

# RetroRDP Client Deployment Script
# This script builds and packages the application for distribution

set -e  # Exit on any error

# Configuration
APP_NAME="RetroRDPClient"
VERSION=${1:-"1.0.0"}
OUTPUT_DIR="./dist"
PROJECT_PATH="./src/ClientApp/RetroRDPClient"

echo "🚀 Starting deployment process for $APP_NAME v$VERSION"

# Clean previous builds
echo "🧹 Cleaning previous builds..."
if [ -d "$OUTPUT_DIR" ]; then
    rm -rf "$OUTPUT_DIR"
fi
mkdir -p "$OUTPUT_DIR"

# Build solution first to ensure everything compiles
echo "🔨 Building solution..."
dotnet build --configuration Release

# Run tests to ensure quality
echo "🧪 Running tests..."
dotnet test --configuration Release --no-build --logger "console;verbosity=minimal"

# Publish self-contained version
echo "📦 Publishing self-contained version..."
dotnet publish "$PROJECT_PATH" \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output "$OUTPUT_DIR/self-contained" \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:EnableCompressionInSingleFile=true \
    -p:DebugType=embedded \
    -p:PublishTrimmed=false

# Publish framework-dependent version
echo "📦 Publishing framework-dependent version..."
dotnet publish "$PROJECT_PATH" \
    --configuration Release \
    --runtime win-x64 \
    --self-contained false \
    --output "$OUTPUT_DIR/framework-dependent" \
    -p:PublishReadyToRun=true

# Copy additional files
echo "📄 Copying documentation and licenses..."
cp README.md "$OUTPUT_DIR/" 2>/dev/null || echo "README.md not found in root"
cp LICENSE "$OUTPUT_DIR/" 2>/dev/null || echo "LICENSE not found in root"

# Create deployment info file
echo "📋 Creating deployment info..."
cat > "$OUTPUT_DIR/deployment-info.txt" << EOF
RetroRDP Client Deployment Package
==================================

Version: $VERSION
Build Date: $(date)
.NET Version: 8.0
Target Platform: Windows x64

Contents:
- self-contained/: Complete application with .NET runtime included
- framework-dependent/: Smaller application requiring .NET 8 runtime
- README.md: Application documentation
- LICENSE: License information

Installation Instructions:
- Self-contained: Extract and run RetroRDPClient.exe directly
- Framework-dependent: Install .NET 8 runtime first, then run RetroRDPClient.exe

For support and updates, visit: https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP
EOF

# Create ZIP packages for distribution
if command -v zip >/dev/null 2>&1; then
    echo "📦 Creating ZIP packages..."
    
    # Self-contained package
    cd "$OUTPUT_DIR"
    zip -r "${APP_NAME}-${VERSION}-SelfContained-win-x64.zip" self-contained/ deployment-info.txt README.md LICENSE 2>/dev/null || true
    
    # Framework-dependent package  
    zip -r "${APP_NAME}-${VERSION}-FrameworkDependent-win-x64.zip" framework-dependent/ deployment-info.txt README.md LICENSE 2>/dev/null || true
    
    cd ..
    echo "✅ ZIP packages created successfully"
else
    echo "⚠️  ZIP utility not found, skipping package creation"
fi

# Display file sizes
echo ""
echo "📊 Package Information:"
echo "======================"
if [ -f "$OUTPUT_DIR/${APP_NAME}-${VERSION}-SelfContained-win-x64.zip" ]; then
    echo "Self-contained package: $(du -h "$OUTPUT_DIR/${APP_NAME}-${VERSION}-SelfContained-win-x64.zip" | cut -f1)"
fi
if [ -f "$OUTPUT_DIR/${APP_NAME}-${VERSION}-FrameworkDependent-win-x64.zip" ]; then
    echo "Framework-dependent package: $(du -h "$OUTPUT_DIR/${APP_NAME}-${VERSION}-FrameworkDependent-win-x64.zip" | cut -f1)"
fi

echo ""
echo "✅ Deployment completed successfully!"
echo "📁 Output directory: $OUTPUT_DIR"
echo ""
echo "Next steps:"
echo "1. Test the packages on clean Windows machines"
echo "2. Upload packages to GitHub Releases"
echo "3. Update documentation with installation instructions"