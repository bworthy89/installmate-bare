# Claude.md - InstallVibe Project Guide

This file provides context and guidelines for AI assistants working with the InstallVibe codebase.

## Project Overview

**InstallVibe** is a Windows desktop application built with WinUI 3 that provides technicians with guided installation workflows. The application operates in an offline-first model with SharePoint Online serving as the central repository for content, media, and application updates. Product key-based activation controls feature access through offline RSA signature verification with optional online validation.

## Technology Stack

- **Language**: C# (.NET 8)
- **UI Framework**: WinUI 3 with XAML
- **Design System**: Windows 11 Fluent Design
- **Architecture**: MVVM pattern with Clean Architecture principles
- **State Management**: CommunityToolkit.Mvvm
- **Database**: Entity Framework Core 8 + SQLite
- **Cloud Integration**: Microsoft Graph API, SharePoint Online (PnP.Core)
- **Authentication**: Microsoft Identity Client (MSAL)
- **Logging**: Serilog
- **Packaging**: MSIX with AppInstaller
- **Testing**: xUnit, Moq, FluentAssertions

## Repository Structure

```
InstallVibe/
├── src/
│   ├── InstallVibe/                    # WinUI 3 presentation layer
│   │   ├── Views/                      # XAML pages and windows
│   │   ├── ViewModels/                 # MVVM view models
│   │   ├── Converters/                 # XAML value converters
│   │   ├── Behaviors/                  # UI behaviors
│   │   └── Helpers/                    # UI utilities
│   ├── InstallVibe.Core/               # Business logic layer
│   │   ├── Models/                     # Domain models
│   │   ├── Services/                   # Business services
│   │   │   ├── Activation/             # Product key validation
│   │   │   ├── Cache/                  # Local caching
│   │   │   ├── Data/                   # Data services
│   │   │   ├── Engine/                 # Guide workflow engine
│   │   │   ├── Media/                  # Media management
│   │   │   ├── SharePoint/             # SharePoint integration
│   │   │   └── Sync/                   # Data synchronization
│   │   ├── Contracts/                  # Service interfaces
│   │   └── Extensions/                 # Extension methods
│   ├── InstallVibe.Data/               # Data access layer
│   │   ├── Context/                    # EF Core DbContext
│   │   ├── Entities/                   # Database entities
│   │   ├── Repositories/               # Repository pattern
│   │   └── Configurations/             # EF configurations
│   ├── InstallVibe.Infrastructure/     # Cross-cutting concerns
│   │   ├── Security/                   # Cryptography, authentication
│   │   ├── Configuration/              # App settings
│   │   ├── Constants/                  # Application constants
│   │   ├── Device/                     # Device identification
│   │   └── Logging/                    # Logging configuration
│   └── InstallVibe.Tests/              # Unit and integration tests
├── tools/
│   ├── scripts/                        # PowerShell build scripts
│   ├── keygen/                         # Product key generator
│   └── certificates/                   # Code signing certs
├── packaging/                          # MSIX packaging resources
├── docs/                               # Detailed documentation
├── ARCHITECTURE.md                     # Architecture documentation
├── PROJECT_STRUCTURE.md                # Detailed structure guide
└── README.md                           # Project documentation
```

## Architecture Layers

### 1. Presentation Layer (InstallVibe)
- WinUI 3 XAML views and view models
- MVVM pattern with CommunityToolkit.Mvvm
- Frame-based navigation with NavigationView
- Custom behaviors and converters
- Light/Dark theme support

### 2. Business Logic Layer (InstallVibe.Core)
- **Activation Services**: Product key validation, license management
- **Guide Engine**: Workflow orchestration, step progression
- **Cache Service**: Intelligent local caching for offline operation
- **SharePoint Service**: Content sync, guide retrieval, media downloads
- **Progress Service**: Local progress tracking for guide sessions
- **Media Service**: Media loading, caching, and playback

### 3. Data Access Layer (InstallVibe.Data)
- Entity Framework Core 8 with SQLite
- Repository pattern for data access
- Entities: Guides, Steps, Progress, MediaCache, Settings, SyncMetadata
- Migrations managed via EF Core CLI

### 4. Infrastructure Layer (InstallVibe.Infrastructure)
- RSA signature validation for product keys
- DPAPI encryption for secure storage
- Microsoft Graph client factory
- Serilog logging configuration
- Device ID generation
- Path constants and app configuration

## Key Features & Systems

### Product Key Activation
- **Format**: `XXXXX-XXXXX-XXXXX-XXXXX-XXXXX` (25 chars, Base58 encoded)
- **Types**: Admin Keys (full access) and Tech Keys (read-only)
- **Validation**: Offline RSA signature verification, optional SharePoint validation
- **Payload**: License type, expiration, customer ID, version flags, checksum
- See: `docs/ACTIVATION_SYSTEM.md`, `src/InstallVibe.Core/Services/Activation/`

### Guide Engine
- JSON-based guide definitions stored in SharePoint
- Step-by-step workflow with progress tracking
- Media support (images, videos, diagrams)
- Checklist items with validation
- Local caching for offline access
- See: `docs/GUIDE_ENGINE.md`, `src/InstallVibe.Core/Services/Engine/`

### SharePoint Integration
- Microsoft Graph API for authentication
- PnP.Core for SharePoint operations
- Guide repository, media library, update distribution
- Offline-first with background sync
- Health monitoring and connection status
- See: `docs/SHAREPOINT_INTEGRATION.md`, `src/InstallVibe.Core/Services/SharePoint/`

### Local Data Layer
- SQLite database for structured data
- File system caching for media
- Progress persistence across sessions
- Sync metadata for conflict resolution
- See: `docs/LOCAL_DATA_LAYER.md`, `src/InstallVibe.Data/`

## Development Guidelines

### Code Style
- Follow .editorconfig settings (4 spaces, LF line endings)
- Use C# naming conventions (PascalCase for types/methods, camelCase for parameters/locals)
- Prefix interface names with `I`
- Use `async/await` for all I/O operations
- Add XML documentation comments for public APIs
- Use nullable reference types (`#nullable enable`)

### MVVM Pattern
- Views are pure XAML with minimal code-behind
- ViewModels inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- Use `[ObservableProperty]` for bindable properties
- Use `[RelayCommand]` for command methods
- ViewModels should not reference UI types
- Services injected via constructor DI

### Dependency Injection
- Services registered in `App.xaml.cs`
- Use interfaces for all services
- Constructor injection for dependencies
- Singleton for stateful services (cache, settings)
- Transient for stateless services

### Error Handling
- Use custom exceptions: `ActivationException`, `SharePointException`, `CacheException`, `SyncException`
- Log all errors with Serilog
- Display user-friendly messages via `INotificationService`
- Never expose technical details to users
- Always dispose of resources properly

### Async Patterns
- All I/O operations must be async
- Use `ConfigureAwait(false)` in library code
- UI updates must happen on UI thread (use `DispatcherQueue`)
- Cancel long-running operations with `CancellationToken`
- Handle `TaskCanceledException` gracefully

### Security Best Practices
- Never hardcode secrets or keys in code
- Use DPAPI for encrypting sensitive local data
- RSA public keys embedded in `PublicKeys.cs`
- Validate all user inputs
- Sanitize logs to remove sensitive data
- Use Windows Credential Manager for SharePoint tokens

## Common Tasks

### Building the Solution

```powershell
# Restore packages
dotnet restore

# Build solution
dotnet build

# Build specific configuration
dotnet build -c Release

# Run the application
dotnet run --project src/InstallVibe/InstallVibe.csproj
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/InstallVibe.Tests/InstallVibe.Tests.csproj
```

### Database Migrations

```powershell
# Add new migration
dotnet ef migrations add MigrationName --project src/InstallVibe.Data --startup-project src/InstallVibe

# Update database
dotnet ef database update --project src/InstallVibe.Data --startup-project src/InstallVibe

# Generate SQL script
dotnet ef migrations script --project src/InstallVibe.Data --startup-project src/InstallVibe
```

### PowerShell Build Scripts

```powershell
# Build solution
.\tools\scripts\build.ps1

# Run tests
.\tools\scripts\test.ps1

# Package MSIX
.\tools\scripts\package.ps1

# Clean build artifacts
.\tools\scripts\clean.ps1
```

### Generating Product Keys

```powershell
# Navigate to keygen tool
cd tools/keygen

# Run key generator
dotnet run
```

## Testing Strategy

- **Unit Tests**: Test individual services and business logic
- **Integration Tests**: Test database operations, SharePoint integration
- **Mocking**: Use Moq for service dependencies
- **Assertions**: Use FluentAssertions for readable assertions
- **Coverage Target**: Aim for >70% code coverage on Core and Data layers
- **Test Naming**: `MethodName_Scenario_ExpectedResult`

## Deployment

- **Packaging**: MSIX with digital signature
- **Distribution**: SharePoint document library or web server
- **Updates**: AppInstaller automatic update checks
- **Versioning**: Semantic versioning (MAJOR.MINOR.PATCH.BUILD)
- See: `docs/DEPLOYMENT.md`, `packaging/`

## Project Conventions

### File Organization
- One type per file
- File name matches type name
- Group related types in folders
- Keep folder depth reasonable (<5 levels)

### Naming Patterns
- Services: `*Service.cs` (e.g., `GuideService.cs`)
- Interfaces: `I*Service.cs` (e.g., `IGuideService.cs`)
- ViewModels: `*ViewModel.cs` (e.g., `ActivationViewModel.cs`)
- Views: `*Page.xaml` or `*Window.xaml`
- Entities: `*Entity.cs` (e.g., `GuideEntity.cs`)
- Models: Descriptive names (e.g., `Guide.cs`, `Step.cs`)

### Constants
- Define in `InstallVibe.Infrastructure/Constants/`
- Group by domain: `PathConstants`, `CacheConstants`, `SharePointConstants`
- Use `public static readonly` for reference types
- Use `public const` for primitive types

## Important Paths

- **User Data**: `%LOCALAPPDATA%/InstallVibe/`
- **Database**: `%LOCALAPPDATA%/InstallVibe/installvibe.db`
- **Cache**: `%LOCALAPPDATA%/InstallVibe/cache/`
- **Logs**: `%LOCALAPPDATA%/InstallVibe/logs/`
- **Settings**: `%LOCALAPPDATA%/InstallVibe/settings.json`

## Dependencies

All dependencies are centrally managed in `Directory.Packages.props`:
- Microsoft.WindowsAppSDK 1.5.x
- Microsoft.EntityFrameworkCore 8.0.x
- Microsoft.Graph 5.40.x
- CommunityToolkit.Mvvm 8.2.x
- Serilog 3.1.x
- xUnit 2.6.x

## Resources

- **Architecture**: See `ARCHITECTURE.md` for detailed architecture documentation
- **Structure**: See `PROJECT_STRUCTURE.md` for complete project breakdown
- **Activation**: See `docs/ACTIVATION_SYSTEM.md` for key validation details
- **Guide Engine**: See `docs/GUIDE_ENGINE.md` for workflow system
- **SharePoint**: See `docs/SHAREPOINT_INTEGRATION.md` for cloud integration
- **Data Layer**: See `docs/LOCAL_DATA_LAYER.md` for database schema

## Notes for AI Assistants

### When Working on This Codebase

1. **Architecture Awareness**: This is a layered architecture. Respect layer boundaries:
   - Presentation depends on Core (not Data or Infrastructure directly)
   - Core is independent and defines interfaces
   - Data and Infrastructure implement Core interfaces
   - Never reverse dependencies

2. **Offline-First Design**: All features must work offline after initial sync
   - Cache aggressively
   - Handle network failures gracefully
   - Sync in background, never block UI
   - Show appropriate offline indicators

3. **Security Context**: Product key validation is critical business logic
   - Never modify validation algorithms without understanding implications
   - RSA signature validation must remain secure
   - Admin vs Tech permissions must be enforced consistently

4. **Windows-Specific**: This is a Windows-only application
   - Use Windows APIs when needed (DPAPI, Credential Manager)
   - Follow Windows 11 design guidelines
   - WinUI 3 is Windows-only (not cross-platform)

5. **SharePoint Integration**: Complex cloud integration
   - Authentication uses Microsoft Graph
   - Content stored in SharePoint document libraries
   - Handle throttling, retries, and timeouts
   - Test both online and offline scenarios

6. **MVVM Best Practices**:
   - ViewModels should never reference View types
   - Use data binding, not code-behind
   - Commands for user actions
   - Services for business logic

7. **Code Generation**: CommunityToolkit.Mvvm uses source generators
   - `[ObservableProperty]` generates backing fields and `INotifyPropertyChanged`
   - `[RelayCommand]` generates `ICommand` properties
   - Partial classes required for generated code

8. **Testing**: When adding features, add corresponding tests
   - Mock external dependencies (SharePoint, file system)
   - Test both success and failure paths
   - Test offline scenarios

9. **Logging**: Comprehensive logging with Serilog
   - Use appropriate log levels (Debug, Info, Warning, Error)
   - Include context in log messages
   - Never log sensitive data (keys, passwords, tokens)

10. **Reference Existing Patterns**: Before implementing new features, check existing implementations
    - Guide/Step models and services for data patterns
    - Activation system for validation patterns
    - Cache service for offline patterns
    - SharePoint service for cloud integration patterns

### Common Gotchas

- WinUI 3 requires Windows 10 1809+ (build 17763)
- MSIX packaging requires signing certificate
- SQLite database file locked during debugging (close DB Browser)
- SharePoint throttling can occur during heavy sync
- DPAPI encryption is machine/user-specific (can't transfer encrypted data)
- Graph API requires app registration and permissions configuration
