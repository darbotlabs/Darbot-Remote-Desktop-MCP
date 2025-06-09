# Retro Cyber Remote Desktop Client

A modern, retro-futuristic multi-session Remote Desktop client for Windows, built with .NET and featuring an extensive agentic AI framework.

## Project Overview

This application serves as a modern successor to Microsoft's retired Remote Desktop Store app, combining:
- **Retro Cyber Aesthetics**: Dark theme with neon color palette and cyberpunk elements
- **Modern Fluent Design**: Windows 11 design principles with acrylic/mica materials
- **Multi-Session Support**: Tabbed interface for managing multiple RDP connections
- **AI Integration**: Agentic AI assistant for natural language remote session management

## Architecture

- **RetroRDPClient**: WPF desktop application (.NET 8)
- **RetroRDPWeb**: ASP.NET Core Web API for remote status and helper services
- **Shared Libraries**: Common models and utilities

## Getting Started

### Prerequisites

- .NET 8.0 SDK or later
- Windows 10/11
- Visual Studio 2022 (recommended) or Visual Studio Code

### Setup Instructions

1. Clone the repository:
   ```bash
   git clone https://github.com/darbotlabs/Darbot-Remote-Desktop-MCP.git
   cd Darbot-Remote-Desktop-MCP
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the solution:
   ```bash
   dotnet build
   ```

4. Run the client application:
   ```bash
   dotnet run --project src/ClientApp/RetroRDPClient
   ```

5. Run the web API (optional):
   ```bash
   dotnet run --project src/WebApp/RetroRDPWeb
   ```

## Development Status

This project has completed **Level 1: Project Setup and Planning** phase and is ready for Level 2 development.

### ✅ Level 1 Complete - Project Setup and Planning
- ✅ Repository setup with proper .gitignore for .NET projects
- ✅ Solution and project structure setup (RetroRDP.sln with ClientApp, WebApp, Shared)
- ✅ Basic retro-cyber theming implementation (console UI + XAML resources)
- ✅ CI/CD pipeline configuration (GitHub Actions with build, test, CodeQL security scanning)
- ✅ Comprehensive project documentation and architecture planning
- ✅ Cross-platform build validation (.NET 8)

### ✅ Level 2 Complete - User Interface – Retro Fluent Experience
- ✅ WPF application with comprehensive retro cyber theme
- ✅ Main window Grid layout (20% sidebar + 80% main area with splitter)
- ✅ Copilot-style left sidebar with AI AssistBot interface
- ✅ Interactive chat functionality with command input and message history
- ✅ Navigation menu with quick actions (New Session, Settings, etc.)
- ✅ Tabbed RDP session interface with sample sessions
- ✅ Complete retro-cyber aesthetics (neon glow effects, dark theme, cyber fonts)
- ✅ Fluent design materials integration (gradient backgrounds, transparency effects)
- ✅ Responsive UI with window resizing support
- ✅ Full interaction handling and error management
- ✅ Cross-platform build compatibility (WPF files in separate directory)

### ⏳ Level 3 Planned - Multi-Session RDP Functionality
- ⏳ Actual RDP protocol integration
- ⏳ Multiple concurrent session management
- ⏳ Session state persistence and recovery
- ⏳ AI assistant command processing
- ⏳ Advanced session features

## Contributing

This project follows a gamified development approach with sequential levels. Please ensure all Level 1 requirements are met before proceeding to subsequent levels.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
