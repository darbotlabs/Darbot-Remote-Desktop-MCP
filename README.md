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

### ⏳ Level 2 Planned - Multi-Session RDP Functionality
- ⏳ WPF application with full retro cyber theme
- ⏳ Multi-session RDP connection management
- ⏳ Tabbed interface for concurrent remote desktop sessions
- ⏳ AI assistant integration framework
- ⏳ Advanced UI/UX features

## Contributing

This project follows a gamified development approach with sequential levels. Please ensure all Level 1 requirements are met before proceeding to subsequent levels.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
