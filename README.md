# InstallVibe

A Windows desktop application built with WinUI 3 that provides technicians with guided installation workflows.

## Features

- **Product Key Activation**: Offline RSA signature validation with optional SharePoint lookup
- **SharePoint Integration**: Cloud-based guide repository and auto-updates
- **Offline-First**: Full functionality with local caching when disconnected
- **Modern UI**: WinUI 3 with Fluent Design
- **Progress Tracking**: Local progress tracking for all guide sessions
- **Auto-Updates**: MSIX packaging with AppInstaller for seamless updates

## Architecture

See [ARCHITECTURE.md](ARCHITECTURE.md) for detailed architecture documentation.

See [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) for complete project structure.

## Prerequisites

- Windows 10 version 1809 (build 17763) or later
- Windows 11 recommended
- Visual Studio 2022 (17.8 or later)
- Windows App SDK 1.5 or later
- .NET 8 SDK

## Getting Started

### Building the Solution

```powershell
# Restore NuGet packages
dotnet restore

# Build the solution
dotnet build

# Run the application
dotnet run --project src/InstallVibe/InstallVibe.csproj
```

### Using Build Scripts

```powershell
# Build the solution
.\tools\scripts\build.ps1

# Run tests
.\tools\scripts\test.ps1

# Package MSIX
.\tools\scripts\package.ps1
```

## Project Structure

- `src/InstallVibe/` - Main WinUI 3 application
- `src/InstallVibe.Core/` - Business logic and services
- `src/InstallVibe.Data/` - Data access layer (EF Core + SQLite)
- `src/InstallVibe.Infrastructure/` - Cross-cutting concerns
- `src/InstallVibe.Tests/` - Test projects
- `tools/` - Build scripts and utilities
- `packaging/` - MSIX packaging resources

## License Types

- **Admin Keys**: Full access including guide editor
- **Tech Keys**: Read-only access to guides

## Development

### Running Tests

```powershell
dotnet test
```

### Database Migrations

```powershell
# Add a new migration
dotnet ef migrations add MigrationName --project src/InstallVibe.Data

# Update database
dotnet ef database update --project src/InstallVibe.Data
```

## Deployment

See [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for deployment instructions.

## Contributing

This is a proprietary application. For internal development guidelines, see the documentation folder.

## Support

For issues and feature requests, contact the development team.

## Version

Current version: 1.0.0.0
