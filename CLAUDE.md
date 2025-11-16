# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

InstallVibe is a WinUI 3 (.NET 8) desktop application that provides technicians with guided installation workflows. It operates offline-first with SharePoint Online as the central repository for content, media, and updates. Product key-based activation (RSA signature validation) controls feature access with two tiers: Admin (full access with guide editor) and Tech (read-only guide access).

## Build & Development Commands

### Building
```powershell
# Build the solution
dotnet build

# Or use the build script
.\tools\scripts\build.ps1

# Build specific configuration
.\tools\scripts\build.ps1 -Configuration Release
```

### Running the Application
```powershell
dotnet run --project src/InstallVibe/InstallVibe.csproj
```

### Testing
```powershell
# Run all tests
dotnet test

# Or use the test script
.\tools\scripts\test.ps1

# Run tests for specific project
dotnet test src/InstallVibe.Tests/InstallVibe.Tests.csproj
```

### Database Migrations
```powershell
# Add a new migration
dotnet ef migrations add MigrationName --project src/InstallVibe.Data --startup-project src/InstallVibe

# Update database
dotnet ef database update --project src/InstallVibe.Data --startup-project src/InstallVibe

# Remove last migration
dotnet ef migrations remove --project src/InstallVibe.Data --startup-project src/InstallVibe
```

### Packaging
```powershell
# Create MSIX package
.\tools\scripts\package.ps1

# Clean build artifacts
.\tools\scripts\clean.ps1
```

## Architecture Overview

### Layered Architecture
The solution follows a clean architecture pattern with clear separation of concerns:

1. **InstallVibe** (Presentation): WinUI 3 application with MVVM pattern using CommunityToolkit.Mvvm
2. **InstallVibe.Core** (Business Logic): Platform-agnostic services and domain models
3. **InstallVibe.Data** (Data Access): EF Core with SQLite for local caching and progress tracking
4. **InstallVibe.Infrastructure** (Cross-cutting): Security, logging, configuration, and utilities

### Project Dependencies
```
InstallVibe (WinUI 3 App)
    ├── InstallVibe.Core
    │   └── InstallVibe.Data
    └── InstallVibe.Infrastructure
        └── InstallVibe.Core

InstallVibe.Tests
    └── All projects
```

### Key Architectural Patterns

**Offline-First Design**: The application works fully offline using local SQLite cache. SharePoint sync happens in the background when online.

**MVVM Pattern**: All views use ViewModels (in `src/InstallVibe/ViewModels/`) with data binding. ViewModels use `CommunityToolkit.Mvvm` source generators for `ObservableObject`, `RelayCommand`, etc.

**Repository Pattern**: Data access is abstracted through repositories in `src/InstallVibe.Data/Repositories/`. All repositories implement `IRepository<T>` base interface.

**Service Layer**: Business logic is encapsulated in services (in `src/InstallVibe.Core/Services/`) registered via dependency injection in `App.xaml.cs`.

**Dependency Injection**: All services are registered in the DI container during app startup. Use constructor injection for all dependencies.

## Critical Service Layers

### Activation & Licensing
- **ProductKeyValidator** (`src/InstallVibe.Core/Services/Activation/`): Validates product keys using offline RSA signature verification
- **LicenseManager**: Manages license type (Admin/Tech) and feature toggles
- **TokenManager**: Stores encrypted activation tokens using Windows DPAPI

### SharePoint Integration
- **SharePointService** (`src/InstallVibe.Core/Services/SharePoint/`): Main orchestration service for SharePoint operations
- **SharePointAuthService**: Handles OAuth 2.0 authentication with Azure AD using MSAL
- **GraphApiClient**: Wraps Microsoft Graph API calls for SharePoint document libraries
- **SharePointGuideProvider**: Downloads/uploads guides and media from SharePoint

### Data & Caching
- **GuideService** (`src/InstallVibe.Core/Services/Data/`): CRUD operations for guides
- **ProgressService**: Tracks user progress through guides
- **MediaService**: Manages media files (images, videos, PDFs)
- **CacheService** (`src/InstallVibe.Core/Services/Cache/`): Three-tier caching (memory, file system, database)
- **MediaCacheManager**: Handles media downloads, caching, and LRU cleanup

### Synchronization
- **SyncService** (`src/InstallVibe.Core/Services/Sync/`): Orchestrates sync between local cache and SharePoint
- **SyncEngine**: Implements sync algorithm with version comparison and change detection
- **ConflictResolver**: Resolves conflicts during sync (server-wins for Tech users, merge options for Admin)

## Database Schema (SQLite)

The local SQLite database (`%LOCALAPPDATA%\InstallVibe\Data\installvibe.db`) contains:

- **Guides**: Guide metadata (title, version, category, sync status)
- **Steps**: Individual guide steps with content (markdown) and media references
- **MediaCache**: Cached media files metadata (paths, checksums, last accessed)
- **Progress**: User progress through guides (step status, notes, timestamps)
- **SyncMetadata**: Sync tracking (entity type, version, sync status)
- **Settings**: Application settings (encrypted when sensitive)
- **Favorites**: User-favorited guides

All entities are in `src/InstallVibe.Data/Entities/`. EF Core configurations are in `src/InstallVibe.Data/Configurations/`.

## SharePoint Configuration

Configuration is in `src/InstallVibe/appsettings.json`:

- **TenantId**: Azure AD tenant ID
- **ClientId**: Azure AD app registration client ID
- **ClientSecret**: Client secret for authentication (use certificate in production)
- **SiteUrl**: SharePoint site URL (e.g., `https://tenant.sharepoint.com/sites/InstallVibe`)
- **GuideLibrary**: Document library name for guides (default: "Guides")
- **MediaLibrary**: Document library name for media (default: "Media")
- **GuideIndexList**: SharePoint list name for guide index (default: "GuideIndex")

## MVVM & UI Guidelines

### ViewModel Location
ViewModels are in `src/InstallVibe/ViewModels/` organized by feature:
- `Shell/`: MainWindow and shell navigation
- `Activation/`: Product key entry and license display
- `Guides/`: Guide list, detail, and editor (Admin only)
- `Progress/`: Progress tracking
- `Sync/`: Synchronization controls
- `Settings/`: Application settings

### View-ViewModel Binding
Views are in `src/InstallVibe/Views/` with matching structure. Each view's DataContext is set to its corresponding ViewModel in the view's code-behind or via navigation.

### Navigation
Navigation uses `INavigationService` (implemented in `src/InstallVibe/Helpers/NavigationHelper.cs`). Navigation is frame-based using WinUI 3's `Frame` control.

### Custom Controls
Reusable controls are in `src/InstallVibe/Controls/`:
- **StepControl**: Displays individual guide steps
- **MediaViewer**: Image/video viewer with zoom and controls
- **ProgressIndicator**: Visual guide progress indicator
- **ChecklistControl**: Step checklist with checkboxes
- **GuideCard**: Guide preview card for list views

## Security Considerations

### Product Key Validation
- **RSA Public Key**: Embedded in `src/InstallVibe.Infrastructure/Security/Keys/PublicKeys.cs` (2048-bit RSA)
- **Private Key**: NEVER committed to repo. Stored securely offline in `tools/keygen/private_key.pem` (gitignored)
- **Validation**: Offline RSA signature verification is primary method. Optional online validation via SharePoint lookup

### Sensitive Data Storage
- **Activation Tokens**: Encrypted with Windows DPAPI (CurrentUser scope) in `%LOCALAPPDATA%\InstallVibe\Config\activation.dat`
- **SharePoint Credentials**: Stored in Windows Credential Manager or encrypted DPAPI
- **Client Secrets**: Never hardcode. Use `appsettings.json` (gitignored for production values) or Azure Key Vault

### Logging
Serilog is configured to sanitize sensitive data. Never log:
- Product keys (only hashes)
- Access tokens or refresh tokens
- SharePoint credentials
- User passwords

## Common Development Tasks

### Adding a New Service
1. Create interface in `src/InstallVibe.Core/Services/[Category]/I[ServiceName].cs`
2. Implement in `src/InstallVibe.Core/Services/[Category]/[ServiceName].cs`
3. Register in DI container in `src/InstallVibe/App.xaml.cs` (e.g., `services.AddSingleton<IServiceName, ServiceName>()`)
4. Inject via constructor where needed

### Adding a New View/Page
1. Create XAML view in `src/InstallVibe/Views/[Category]/[PageName].xaml`
2. Create ViewModel in `src/InstallVibe/ViewModels/[Category]/[PageName]ViewModel.cs`
3. Bind ViewModel to view's DataContext
4. Add navigation route if needed

### Adding a New Database Entity
1. Create entity in `src/InstallVibe.Data/Entities/[EntityName]Entity.cs`
2. Create EF Core configuration in `src/InstallVibe.Data/Configurations/[EntityName]Configuration.cs`
3. Add DbSet to `InstallVibeContext.cs`
4. Create repository interface and implementation in `src/InstallVibe.Data/Repositories/`
5. Run `dotnet ef migrations add [MigrationName]` to create migration
6. Update database with `dotnet ef database update`

### License Type Checks
Always check license type before granting access to Admin features:
```csharp
var licenseManager = App.GetService<ILicenseManager>();
if (licenseManager.CurrentLicense?.LicenseType == LicenseType.Admin)
{
    // Admin-only functionality
}
```

## File Paths & Locations

### Runtime Data Locations
- **Cache**: `%LOCALAPPDATA%\InstallVibe\Cache\`
- **Database**: `%LOCALAPPDATA%\InstallVibe\Data\installvibe.db`
- **Logs**: `%LOCALAPPDATA%\InstallVibe\Logs\installvibe-{date}.log`
- **Config**: `%LOCALAPPDATA%\InstallVibe\Config\`
- **Activation Token**: `%LOCALAPPDATA%\InstallVibe\Config\activation.dat`

### Build Output
- Debug builds: `build/Debug/`
- Release builds: `build/Release/`
- MSIX packages: `build/Packages/`

## Testing Guidelines

### Unit Tests
Unit tests are in `src/InstallVibe.Tests/Unit/` organized by layer:
- `Services/`: Service layer tests with mocked dependencies
- `Infrastructure/`: Infrastructure component tests
- `ViewModels/`: ViewModel tests with mock services

### Integration Tests
Integration tests are in `src/InstallVibe.Tests/Integration/`:
- `Database/`: EF Core repository tests with in-memory SQLite
- `SharePoint/`: SharePoint integration tests (require credentials)
- `EndToEnd/`: Full flow tests

### Test Helpers
Mock services are in `src/InstallVibe.Tests/TestHelpers/MockServices/`. Test fixtures and builders are in `TestHelpers/Fixtures/` and `TestHelpers/Builders/`.

## Known Patterns & Conventions

### Async/Await
All I/O operations (database, file system, network) must be async. Service methods return `Task` or `Task<T>`.

### Error Handling
- Use custom exceptions in `src/InstallVibe.Core/Exceptions/`
- Log all exceptions with Serilog
- Show user-friendly error messages via `IDialogService`

### JSON Serialization
Use `System.Text.Json` for new code. Legacy code may use `Newtonsoft.Json`.

### String Constants
Application-wide constants are in `src/InstallVibe.Infrastructure/Constants/`. Avoid magic strings.

## Additional Documentation

For comprehensive architecture details, see:
- **ARCHITECTURE.md**: Complete architecture documentation with diagrams
- **PROJECT_STRUCTURE.md**: Full file and folder structure
- **README.md**: Project overview and getting started guide
