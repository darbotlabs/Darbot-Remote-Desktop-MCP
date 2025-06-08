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

This project is currently in **Level 1: Project Setup and Planning** phase.

### Completed
- ✅ Repository setup with proper .gitignore
- ✅ Project documentation

### In Progress
- 🔄 Solution and project structure setup
- 🔄 Basic retro-cyber theming implementation
- 🔄 CI/CD pipeline configuration

### Planned
- ⏳ Multi-session RDP functionality
- ⏳ AI assistant integration
- ⏳ Advanced UI/UX features

## Contributing

This project follows a gamified development approach with sequential levels. Please ensure all Level 1 requirements are met before proceeding to subsequent levels.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
