# CLAUDE.md - AI Assistant Guide for InstallVibe

**Version:** 1.0.0
**Last Updated:** 2025-11-16
**Project:** InstallVibe - WinUI 3 Guided Installation Workflow Application

---

## Table of Contents

1. [Quick Reference](#quick-reference)
2. [Project Overview](#project-overview)
3. [Codebase Structure](#codebase-structure)
4. [Development Conventions](#development-conventions)
5. [Common Development Tasks](#common-development-tasks)
6. [Architecture Patterns](#architecture-patterns)
7. [Testing Guidelines](#testing-guidelines)
8. [Git Workflow](#git-workflow)
9. [Troubleshooting](#troubleshooting)
10. [Key Files Reference](#key-files-reference)

---

## Quick Reference

### Essential Commands

```bash
# Build the solution
dotnet build InstallVibe.sln

# Run tests
dotnet test

# Run the application
dotnet run --project src/InstallVibe/InstallVibe.csproj

# Database migrations
dotnet ef migrations add MigrationName --project src/InstallVibe.Data --startup-project src/InstallVibe
dotnet ef database update --project src/InstallVibe.Data --startup-project src/InstallVibe
```

### Project Structure at a Glance

```
InstallVibe/
├── src/
│   ├── InstallVibe/              # WinUI 3 presentation layer
│   ├── InstallVibe.Core/         # Business logic & services
│   ├── InstallVibe.Data/         # EF Core data access
│   ├── InstallVibe.Infrastructure/ # Cross-cutting concerns
│   └── InstallVibe.Tests/        # Test project
├── docs/                          # Comprehensive documentation
├── tools/                         # Build scripts & utilities
└── packaging/                     # MSIX packaging resources
```

### Key Technologies

- **Framework:** .NET 8 with WinUI 3 (Windows App SDK 1.5)
- **UI Pattern:** MVVM with CommunityToolkit.Mvvm
- **Database:** SQLite with Entity Framework Core 8
- **Cloud:** SharePoint Online via Microsoft Graph API
- **Authentication:** Azure AD OAuth 2.0 (MSAL)
- **Logging:** Serilog with structured logging
- **Testing:** xUnit, Moq, FluentAssertions

---

## Project Overview

### What is InstallVibe?

InstallVibe is a Windows desktop application for technicians to execute guided installation workflows. It operates on an **offline-first** model with SharePoint as the central content repository.

### Core Features

1. **Product Key Activation:** RSA-2048 signature validation (offline-capable)
2. **Guide Execution:** Step-by-step installation workflows with media support
3. **Progress Tracking:** Local progress persistence with sync
4. **SharePoint Integration:** Cloud-based guide repository with auto-sync
5. **Admin Tools:** Guide authoring and publishing (Admin license only)
6. **Offline Mode:** Full functionality when disconnected

### License Types

- **Tech Keys:** Read-only access to guides
- **Admin Keys:** Full access including guide authoring/editing

### Architectural Philosophy

- **Offline-First:** Always check local cache before network
- **Security-First:** RSA validation, DPAPI encryption, credential protection
- **User-Centric:** Responsive UI, progress tracking, favorites system
- **Resilient:** Graceful degradation, conflict resolution, retry logic

---

## Codebase Structure

### Layer Architecture

```
┌─────────────────────────────────────────────────┐
│   PRESENTATION (InstallVibe)                    │
│   Views (XAML) ← ViewModels (MVVM) ← Services  │
├─────────────────────────────────────────────────┤
│   BUSINESS LOGIC (InstallVibe.Core)            │
│   Services ← Models ← Interfaces                │
├─────────────────────────────────────────────────┤
│   DATA ACCESS (InstallVibe.Data)               │
│   Repositories ← Entities ← DbContext          │
├─────────────────────────────────────────────────┤
│   INFRASTRUCTURE (InstallVibe.Infrastructure)  │
│   Crypto ← Logging ← IO ← Networking           │
└─────────────────────────────────────────────────┘
```

### Project Descriptions

#### **InstallVibe** (Main Application)
**Location:** `src/InstallVibe/`
**Purpose:** WinUI 3 presentation layer
**Contains:**
- `Views/` - XAML pages organized by feature (Shell, Setup, Dashboard, Guide, Admin, Settings)
- `ViewModels/` - MVVM ViewModels using CommunityToolkit.Mvvm
- `Services/Navigation/` - Navigation service implementation
- `Converters/` - XAML value converters
- `Assets/` - Icons, images, fonts
- `App.xaml.cs` - Application entry point with DI configuration

**Key Patterns:**
- MVVM with `[ObservableProperty]` and `[RelayCommand]` attributes
- Navigation via `INavigationService`
- Data binding with converters for complex transformations

#### **InstallVibe.Core** (Business Logic)
**Location:** `src/InstallVibe.Core/`
**Purpose:** Platform-agnostic business logic
**Contains:**
- `Services/` - All business services (Activation, SharePoint, Engine, Data, Sync, Media, Cache, User, Update, Logging)
- `Models/` - Domain models organized by feature
- `Contracts/` - Service interfaces
- `Extensions/` - Helper extension methods
- `Exceptions/` - Custom exception types

**Key Services:**
- `IActivationService` - Product key validation and licensing
- `ISharePointService` - SharePoint integration
- `IGuideEngine` - Central guide orchestration
- `IGuideService` / `IProgressService` - Local data operations
- `ISyncService` - Synchronization engine

#### **InstallVibe.Data** (Data Access)
**Location:** `src/InstallVibe.Data/`
**Purpose:** Entity Framework Core data access
**Contains:**
- `Context/InstallVibeContext.cs` - EF Core DbContext
- `Entities/` - Database entity models (7 entities)
- `Repositories/` - Repository pattern implementations
- `Configurations/` - Fluent API entity configurations
- `Migrations/` - EF Core migrations

**Database:** SQLite at `%LOCALAPPDATA%\InstallVibe\Data\installvibe.db`

**Entities:**
- `GuideEntity` - Cached guides with metadata
- `StepEntity` - Individual guide steps
- `ProgressEntity` - User progress tracking
- `MediaCacheEntity` - Media cache metadata
- `SyncMetadataEntity` - Sync tracking
- `SettingEntity` - Application settings
- `FavoriteEntity` - User favorites

#### **InstallVibe.Infrastructure** (Cross-Cutting)
**Location:** `src/InstallVibe.Infrastructure/`
**Purpose:** Infrastructure concerns
**Contains:**
- `Security/Cryptography/` - RSA validation, DPAPI encryption, hashing
- `Security/Keys/` - Embedded public keys
- `IO/` - File system operations and integrity checking
- `Networking/` - HTTP client factory, retry policies, network monitoring
- `Device/` - Hardware ID generation
- `Logging/` - Serilog configuration and sanitization
- `Configuration/` - Settings management
- `Constants/` - Application constants

---

## Development Conventions

### C# Coding Standards

```csharp
// Namespace organization
namespace InstallVibe.Core.Services.Activation;

// Interface naming
public interface IActivationService { }

// Async method naming
public async Task<ActivationResult> ActivateAsync(string productKey);

// Property naming (PascalCase)
public string ProductKey { get; set; }

// Private field naming (camelCase with underscore)
private readonly IActivationService _activationService;

// Nullable reference types (enabled)
public string? OptionalField { get; set; }
public string RequiredField { get; set; } = string.Empty;
```

### MVVM Patterns

```csharp
// ViewModel using CommunityToolkit.Mvvm
public partial class DashboardViewModel : ObservableObject
{
    // Observable property (generates INotifyPropertyChanged)
    [ObservableProperty]
    private string userName = string.Empty;

    // Observable property with custom notification
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string firstName = string.Empty;

    public string FullName => $"{FirstName} {LastName}";

    // Relay command (generates async command)
    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // Implementation
    }

    // Relay command with parameter
    [RelayCommand]
    private async Task SelectGuideAsync(string guideId)
    {
        // Implementation
    }
}
```

### Dependency Injection

**Service Registration (App.xaml.cs):**
```csharp
private IServiceProvider ConfigureServices(IConfiguration configuration)
{
    var services = new ServiceCollection();

    // Singleton - Expensive resources, stateless services
    services.AddSingleton<IHashService, HashService>();
    services.AddSingleton<IRsaValidator, RsaValidator>();

    // Scoped - DbContext (one per request/scope)
    services.AddDbContext<InstallVibeContext>(options =>
        options.UseSqlite($"Data Source={dbPath}"));
    services.AddScoped<IGuideRepository, GuideRepository>();

    // Transient - Stateless services, ViewModels
    services.AddTransient<IActivationService, ActivationService>();
    services.AddTransient<DashboardViewModel>();

    return services.BuildServiceProvider();
}
```

**Service Access:**
```csharp
// In App.xaml.cs
public static T GetService<T>() where T : class
{
    return ((App)Current)._serviceProvider.GetRequiredService<T>();
}

// Usage in ViewModels or code-behind
var activationService = App.GetService<IActivationService>();
```

### Async/Await Guidelines

```csharp
// ✅ DO: Use async/await for all I/O operations
public async Task<Guide?> LoadGuideAsync(string guideId)
{
    var guide = await _repository.GetByIdAsync(guideId);
    return guide;
}

// ✅ DO: Use ConfigureAwait(false) in library code (Core, Data, Infrastructure)
var data = await _httpClient.GetStringAsync(url).ConfigureAwait(false);

// ✅ DO: Propagate cancellation tokens
public async Task LoadDataAsync(CancellationToken cancellationToken = default)
{
    await _service.GetDataAsync(cancellationToken);
}

// ❌ DON'T: Block on async code
var result = LoadGuideAsync().Result; // AVOID

// ✅ DO: Report progress for long operations
public async Task DownloadMediaAsync(string mediaId, IProgress<int>? progress = null)
{
    // Report progress
    progress?.Report(50);
}
```

### Error Handling

```csharp
// Custom exception hierarchy
public class InstallVibeException : Exception { }
public class ActivationException : InstallVibeException { }
public class SharePointException : InstallVibeException { }

// Service-level error handling
public async Task<ActivationResult> ActivateAsync(string productKey)
{
    try
    {
        var validation = await _validator.ValidateAsync(productKey);
        if (!validation.IsValid)
        {
            return ActivationResult.Failure("Invalid product key");
        }

        // ... activation logic
        return ActivationResult.Success(licenseInfo);
    }
    catch (ActivationException ex)
    {
        _logger.LogError(ex, "Activation failed for product key");
        return ActivationResult.Failure(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error during activation");
        return ActivationResult.Failure("An unexpected error occurred");
    }
}

// ViewModel error handling with UI feedback
[RelayCommand]
private async Task LoadDataAsync()
{
    IsLoading = true;
    ErrorMessage = null;

    try
    {
        var data = await _service.GetDataAsync();
        DataItems = new ObservableCollection<Item>(data);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to load data");
        ErrorMessage = "Failed to load data. Please try again.";
    }
    finally
    {
        IsLoading = false;
    }
}
```

### Logging Conventions

```csharp
// Structured logging with Serilog
_logger.LogInformation("Guide {GuideId} loaded successfully", guideId);
_logger.LogWarning("Guide {GuideId} not found in cache, downloading from SharePoint", guideId);
_logger.LogError(ex, "Failed to sync guide {GuideId}", guideId);

// Performance tracking
using (_logger.BeginScope("SyncOperation"))
{
    var sw = Stopwatch.StartNew();
    await SyncGuidesAsync();
    _logger.LogInformation("Sync completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
}

// Sensitive data sanitization (handled by SensitiveDataSanitizer)
// Never log: Product keys, passwords, tokens, email addresses in full
```

### File Organization

```
Service Organization:
src/InstallVibe.Core/Services/
├── Activation/           # Feature folder
│   ├── IActivationService.cs
│   ├── ActivationService.cs
│   ├── IProductKeyValidator.cs
│   └── ProductKeyValidator.cs
└── SharePoint/
    ├── ISharePointService.cs
    └── SharePointService.cs

Model Organization:
src/InstallVibe.Core/Models/
├── Domain/               # Core business models
│   ├── Guide.cs
│   └── Step.cs
└── Activation/           # Feature-specific models
    ├── ActivationResult.cs
    └── LicenseInfo.cs
```

---

## Common Development Tasks

### Adding a New View/Page

1. **Create XAML Page:**
```bash
# Location: src/InstallVibe/Views/{Feature}/
# Example: src/InstallVibe/Views/Guide/GuideDetailPage.xaml
```

2. **Create ViewModel:**
```csharp
// Location: src/InstallVibe/ViewModels/{Feature}/GuideDetailViewModel.cs
namespace InstallVibe.ViewModels.Guide;

public partial class GuideDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private Guide? currentGuide;

    [RelayCommand]
    private async Task LoadGuideAsync(string guideId)
    {
        // Implementation
    }
}
```

3. **Register in DI (App.xaml.cs):**
```csharp
services.AddTransient<GuideDetailPage>();
services.AddTransient<GuideDetailViewModel>();
```

4. **Add Navigation:**
```csharp
// In NavigationService
public void NavigateToGuideDetail(string guideId)
{
    _frame.Navigate(typeof(GuideDetailPage), guideId);
}
```

### Adding a New Service

1. **Create Interface:**
```csharp
// Location: src/InstallVibe.Core/Services/{Feature}/I{ServiceName}.cs
namespace InstallVibe.Core.Services.Guide;

public interface IGuideSearchService
{
    Task<List<Guide>> SearchAsync(string query);
}
```

2. **Implement Service:**
```csharp
// Location: src/InstallVibe.Core/Services/{Feature}/{ServiceName}.cs
public class GuideSearchService : IGuideSearchService
{
    private readonly IGuideRepository _repository;
    private readonly ILogger<GuideSearchService> _logger;

    public GuideSearchService(
        IGuideRepository repository,
        ILogger<GuideSearchService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Guide>> SearchAsync(string query)
    {
        // Implementation
    }
}
```

3. **Register in DI:**
```csharp
services.AddScoped<IGuideSearchService, GuideSearchService>();
```

### Adding a Database Migration

```bash
# 1. Add migration
dotnet ef migrations add AddGuideTagsSupport \
    --project src/InstallVibe.Data \
    --startup-project src/InstallVibe

# 2. Review generated migration in src/InstallVibe.Data/Migrations/

# 3. Update database
dotnet ef database update \
    --project src/InstallVibe.Data \
    --startup-project src/InstallVibe

# 4. To rollback
dotnet ef database update PreviousMigrationName \
    --project src/InstallVibe.Data \
    --startup-project src/InstallVibe
```

### Adding Entity to Database

1. **Create Entity:**
```csharp
// Location: src/InstallVibe.Data/Entities/TagEntity.cs
public class TagEntity
{
    public string TagId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
```

2. **Add to DbContext:**
```csharp
// Location: src/InstallVibe.Data/Context/InstallVibeContext.cs
public DbSet<TagEntity> Tags { get; set; } = null!;
```

3. **Create Configuration (Optional):**
```csharp
// Location: src/InstallVibe.Data/Configurations/TagConfiguration.cs
public class TagConfiguration : IEntityTypeConfiguration<TagEntity>
{
    public void Configure(EntityTypeBuilder<TagEntity> builder)
    {
        builder.HasKey(e => e.TagId);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.Name).IsUnique();
    }
}
```

4. **Apply Configuration:**
```csharp
// In InstallVibeContext.OnModelCreating
modelBuilder.ApplyConfiguration(new TagConfiguration());
```

5. **Create Repository:**
```csharp
// Interface: src/InstallVibe.Data/Repositories/ITagRepository.cs
// Implementation: src/InstallVibe.Data/Repositories/TagRepository.cs
```

6. **Add Migration and Update Database** (see above)

### Implementing SharePoint Integration

```csharp
// Example: Downloading a guide from SharePoint
public async Task<Guide?> DownloadGuideAsync(string guideId)
{
    // 1. Get authentication token
    var token = await _authService.GetAccessTokenAsync();

    // 2. Build SharePoint URL
    var guideUrl = $"{_config.SiteUrl}/Guides/{guideId}/guide.json";

    // 3. Download with retry
    var json = await _retryPolicy.ExecuteAsync(async () =>
    {
        return await _httpClient.GetStringAsync(guideUrl);
    });

    // 4. Deserialize
    var guide = JsonConvert.DeserializeObject<Guide>(json);

    // 5. Cache locally
    if (guide != null)
    {
        await _guideRepository.SaveAsync(guide);
    }

    return guide;
}
```

### Working with Configuration

```csharp
// Reading configuration
var sharePointConfig = _configuration.GetSection("SharePoint").Get<SharePointConfiguration>();

// Accessing specific values
var siteUrl = _configuration["SharePoint:SiteUrl"];
var retryCount = _configuration.GetValue<int>("SharePoint:RetryCount");

// Updating appsettings.json for development
// Location: src/InstallVibe/appsettings.Development.json
{
  "SharePoint": {
    "EnableVerboseLogging": true
  }
}
```

---

## Architecture Patterns

### Offline-First Implementation

```csharp
// Pattern: Cache-first with SharePoint fallback
public async Task<Guide?> LoadGuideAsync(string guideId, bool forceRefresh = false)
{
    // 1. Check local cache first (unless force refresh)
    if (!forceRefresh)
    {
        var cachedGuide = await _guideRepository.GetByIdAsync(guideId);
        if (cachedGuide != null)
        {
            _logger.LogInformation("Guide {GuideId} loaded from cache", guideId);
            return cachedGuide;
        }
    }

    // 2. Check network connectivity
    if (!await _networkMonitor.IsOnlineAsync())
    {
        _logger.LogWarning("Guide {GuideId} not in cache and offline", guideId);
        return null;
    }

    // 3. Download from SharePoint
    try
    {
        var guide = await _sharePointService.GetGuideAsync(guideId);
        if (guide != null)
        {
            // 4. Cache for offline use
            await _guideRepository.SaveAsync(guide);
            _logger.LogInformation("Guide {GuideId} downloaded and cached", guideId);
        }
        return guide;
    }
    catch (SharePointException ex)
    {
        _logger.LogError(ex, "Failed to download guide {GuideId}", guideId);

        // 5. Fallback to cache even if stale
        return await _guideRepository.GetByIdAsync(guideId);
    }
}
```

### Product Key Validation Flow

```csharp
// RSA signature validation (offline)
public async Task<ProductKeyValidationResult> ValidateAsync(string productKey)
{
    // 1. Parse product key format
    var parts = productKey.Split('-');
    if (parts.Length != 5)
    {
        return ProductKeyValidationResult.Invalid("Invalid format");
    }

    // 2. Decode Base58 payload and signature
    var payload = DecodeBase58(string.Join("", parts.Take(3)));
    var signature = DecodeBase58(string.Join("", parts.Skip(3)));

    // 3. Verify RSA signature with embedded public key
    var isValid = await _rsaValidator.VerifyAsync(payload, signature);
    if (!isValid)
    {
        return ProductKeyValidationResult.Invalid("Invalid signature");
    }

    // 4. Parse payload (license type, expiration, customer ID)
    var licenseInfo = ParsePayload(payload);

    // 5. Check expiration
    if (licenseInfo.ExpirationDate.HasValue &&
        licenseInfo.ExpirationDate < DateTime.UtcNow)
    {
        return ProductKeyValidationResult.Expired(licenseInfo);
    }

    return ProductKeyValidationResult.Valid(licenseInfo);
}
```

### Sync Engine Pattern

```csharp
public async Task<SyncResult> SyncGuidesAsync(
    DateTime? since = null,
    IProgress<SyncProgress>? progress = null)
{
    var result = new SyncResult();

    // 1. Get guide index from SharePoint
    var remoteGuides = await _sharePointService.GetGuideIndexAsync();
    progress?.Report(new SyncProgress { Stage = "Retrieved index", Percent = 10 });

    // 2. Get local guides
    var localGuides = await _guideRepository.GetAllAsync();

    // 3. Compare versions
    var guidesToSync = remoteGuides
        .Where(remote => ShouldSync(remote, localGuides, since))
        .ToList();

    progress?.Report(new SyncProgress {
        Stage = $"Found {guidesToSync.Count} updates",
        Percent = 20
    });

    // 4. Download updated guides
    for (int i = 0; i < guidesToSync.Count; i++)
    {
        var guideId = guidesToSync[i].GuideId;

        try
        {
            var guide = await _sharePointService.GetGuideAsync(guideId);
            await _guideRepository.SaveAsync(guide);
            result.SyncedGuides.Add(guideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync guide {GuideId}", guideId);
            result.FailedGuides.Add(guideId);
        }

        progress?.Report(new SyncProgress {
            Stage = $"Synced {i + 1}/{guidesToSync.Count}",
            Percent = 20 + (int)(60.0 * (i + 1) / guidesToSync.Count)
        });
    }

    // 5. Update sync metadata
    await UpdateSyncMetadataAsync(result);
    progress?.Report(new SyncProgress { Stage = "Complete", Percent = 100 });

    return result;
}
```

### Repository Pattern

```csharp
// Generic repository base
public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
{
    protected readonly InstallVibeContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public Repository(InstallVibeContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(string id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task SaveAsync(TEntity entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }
}

// Specialized repository
public class GuideRepository : Repository<GuideEntity>, IGuideRepository
{
    public GuideRepository(InstallVibeContext context) : base(context) { }

    public async Task<List<GuideEntity>> GetByCategoryAsync(string category)
    {
        return await _dbSet
            .Where(g => g.Category == category && !g.IsDeleted)
            .OrderBy(g => g.Title)
            .ToListAsync();
    }
}
```

---

## Testing Guidelines

### Test Project Setup

**Location:** `src/InstallVibe.Tests/`
**Framework:** xUnit
**Mocking:** Moq
**Assertions:** FluentAssertions

### Test Naming Convention

```csharp
[Fact]
public async Task MethodName_Scenario_ExpectedOutcome()
{
    // Example:
    // ActivateAsync_WithValidKey_ReturnsSuccess
    // LoadGuideAsync_WhenNotCached_DownloadsFromSharePoint
}
```

### Unit Test Pattern

```csharp
public class ActivationServiceTests
{
    [Fact]
    public async Task ActivateAsync_WithValidKey_ReturnsSuccess()
    {
        // Arrange
        var mockValidator = new Mock<IProductKeyValidator>();
        var mockTokenManager = new Mock<ITokenManager>();
        var mockLogger = new Mock<ILogger<ActivationService>>();

        mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(ProductKeyValidationResult.Valid(
                new LicenseInfo { Type = LicenseType.Tech }));

        var service = new ActivationService(
            mockValidator.Object,
            mockTokenManager.Object,
            mockLogger.Object);

        // Act
        var result = await service.ActivateAsync("VALID-XXXXX-XXXXX-XXXXX-XXXXX");

        // Assert
        result.Success.Should().BeTrue();
        result.LicenseInfo.Should().NotBeNull();
        result.LicenseInfo!.Type.Should().Be(LicenseType.Tech);

        mockTokenManager.Verify(
            x => x.SaveTokenAsync(It.IsAny<ActivationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_WithInvalidKey_ReturnsFailure()
    {
        // Arrange
        var mockValidator = new Mock<IProductKeyValidator>();
        mockValidator
            .Setup(x => x.ValidateAsync(It.IsAny<string>()))
            .ReturnsAsync(ProductKeyValidationResult.Invalid("Invalid signature"));

        var service = new ActivationService(
            mockValidator.Object,
            Mock.Of<ITokenManager>(),
            Mock.Of<ILogger<ActivationService>>());

        // Act
        var result = await service.ActivateAsync("INVALID-KEY");

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid");
    }
}
```

### Integration Test Pattern

```csharp
public class GuideRepositoryTests : IDisposable
{
    private readonly InstallVibeContext _context;
    private readonly GuideRepository _repository;

    public GuideRepositoryTests()
    {
        // Use in-memory database for testing
        var options = new DbContextOptionsBuilder<InstallVibeContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new InstallVibeContext(options);
        _repository = new GuideRepository(_context);
    }

    [Fact]
    public async Task SaveAsync_NewGuide_SavesSuccessfully()
    {
        // Arrange
        var guide = new GuideEntity
        {
            GuideId = "test-guide-1",
            Title = "Test Guide",
            Version = "1.0.0"
        };

        // Act
        await _repository.SaveAsync(guide);

        // Assert
        var saved = await _repository.GetByIdAsync("test-guide-1");
        saved.Should().NotBeNull();
        saved!.Title.Should().Be("Test Guide");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
```

### ViewModel Test Pattern

```csharp
public class DashboardViewModelTests
{
    [Fact]
    public async Task LoadDashboardDataCommand_WhenExecuted_LoadsUserData()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        mockUserService
            .Setup(x => x.GetCurrentUserNameAsync())
            .ReturnsAsync("John Doe");

        var viewModel = new DashboardViewModel(
            mockUserService.Object,
            Mock.Of<IGuideService>(),
            Mock.Of<ILogger<DashboardViewModel>>());

        // Act
        await viewModel.LoadDashboardDataCommand.ExecuteAsync(null);

        // Assert
        viewModel.UserName.Should().Be("John Doe");
        viewModel.IsLoading.Should().BeFalse();
    }
}
```

### Test Coverage Goals

- **Services:** Aim for >80% code coverage
- **Repositories:** Test all CRUD operations and queries
- **ViewModels:** Test command execution and property changes
- **Critical Paths:** 100% coverage for activation, sync, security

---

## Git Workflow

### Branch Strategy

```bash
# Main branches
main          # Production-ready code
develop       # Integration branch for features

# Feature branches
feature/guide-authoring
feature/media-upload
feature/advanced-sync

# Bug fix branches
fix/activation-crash
fix/sync-conflict-resolution

# Working on this project (Claude-specific branches)
claude/claude-md-mi14573jjkbdvcdo-01Q6jQpGtocgevApqzAqZ5rD
```

### Commit Message Conventions

```bash
# Format: <type>(<scope>): <subject>

# Types:
feat: New feature
fix: Bug fix
docs: Documentation changes
style: Code style changes (formatting, etc.)
refactor: Code refactoring
test: Adding or updating tests
chore: Maintenance tasks

# Examples:
git commit -m "feat(activation): Add RSA signature validation"
git commit -m "fix(sync): Resolve conflict resolution bug"
git commit -m "docs: Update CLAUDE.md with testing guidelines"
git commit -m "refactor(repository): Extract common query logic"
```

### Commit Workflow

```bash
# 1. Check status
git status

# 2. Stage changes
git add src/InstallVibe.Core/Services/Activation/

# 3. Commit with descriptive message
git commit -m "feat(activation): Implement offline token validation"

# 4. Push to designated branch
git push -u origin claude/claude-md-mi14573jjkbdvcdo-01Q6jQpGtocgevApqzAqZ5rD
```

### Pull Request Guidelines

When creating PRs:
1. Provide clear description of changes
2. Reference related issues
3. Include test results
4. Update documentation if needed
5. Ensure CI/CD passes

---

## Troubleshooting

### Common Issues

#### Build Errors

**Issue:** "Could not find a part of the path"
```bash
# Solution: Clean and rebuild
dotnet clean
dotnet build
```

**Issue:** "Package restore failed"
```bash
# Solution: Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
```

#### Database Migration Errors

**Issue:** "No migrations found"
```bash
# Solution: Ensure correct project paths
dotnet ef migrations list --project src/InstallVibe.Data --startup-project src/InstallVibe
```

**Issue:** "Database is locked"
```bash
# Solution: Close all running instances of InstallVibe
# Delete: %LOCALAPPDATA%\InstallVibe\Data\installvibe.db
# Recreate: dotnet ef database update
```

#### Runtime Errors

**Issue:** "Service not registered in DI container"
```csharp
// Solution: Add to App.xaml.cs ConfigureServices()
services.AddScoped<IMissingService, MissingServiceImplementation>();
```

**Issue:** "NavigationFailed exception"
```csharp
// Solution: Ensure page is registered in DI
services.AddTransient<YourNewPage>();
```

### Debugging Tips

```csharp
// Enable verbose logging
// In appsettings.Development.json:
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    }
  },
  "SharePoint": {
    "EnableVerboseLogging": true
  }
}

// Logs location: %LOCALAPPDATA%\InstallVibe\Logs\
```

### Performance Investigation

```csharp
// Add performance logging
using (_logger.BeginScope("PerformanceTest"))
{
    var sw = Stopwatch.StartNew();

    // Your code here

    _logger.LogInformation("Operation completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
}
```

---

## Key Files Reference

### Essential Documentation

| File | Description |
|------|-------------|
| `ARCHITECTURE.md` | Complete system architecture (58KB) |
| `PROJECT_STRUCTURE.md` | Detailed project organization (47KB) |
| `DEVELOPMENT_PLAN.md` | 2-week implementation roadmap (30KB) |
| `DEVELOPMENT_SETUP.md` | Environment setup guide (11KB) |
| `README.md` | Quick start guide |
| `CLAUDE.md` | This file - AI assistant guide |

### Critical Source Files

| File | Location | Purpose |
|------|----------|---------|
| `App.xaml.cs` | `src/InstallVibe/` | DI configuration, app lifecycle |
| `InstallVibeContext.cs` | `src/InstallVibe.Data/Context/` | Database schema |
| `IGuideEngine.cs` | `src/InstallVibe.Core/Services/Engine/` | Core guide operations |
| `ISharePointService.cs` | `src/InstallVibe.Core/Services/SharePoint/` | SharePoint integration |
| `IActivationService.cs` | `src/InstallVibe.Core/Services/Activation/` | Product key validation |
| `PublicKeys.cs` | `src/InstallVibe.Infrastructure/Security/Keys/` | Embedded RSA public keys |

### Configuration Files

| File | Location | Purpose |
|------|----------|---------|
| `appsettings.json` | `src/InstallVibe/` | Main configuration |
| `appsettings.Development.json` | `src/InstallVibe/` | Dev overrides |
| `Directory.Build.props` | Root | Shared MSBuild properties |
| `Directory.Packages.props` | Root | NuGet package versions |
| `Package.appxmanifest` | `src/InstallVibe/` | MSIX package manifest |

### Build & Deployment

| File | Location | Purpose |
|------|----------|---------|
| `.github/workflows/build.yml` | `.github/workflows/` | CI build pipeline |
| `.github/workflows/release.yml` | `.github/workflows/` | Release automation |
| `InstallVibe.sln` | Root | Visual Studio solution |

---

## Quick Decision Guide

### When to Use Each Layer

**Use InstallVibe (Presentation) when:**
- Creating UI components
- Handling user interactions
- Implementing navigation
- Displaying data

**Use InstallVibe.Core (Business Logic) when:**
- Implementing business rules
- Coordinating between services
- Processing domain logic
- Orchestrating workflows

**Use InstallVibe.Data (Data Access) when:**
- Querying database
- Saving/updating entities
- Managing data persistence
- Implementing repositories

**Use InstallVibe.Infrastructure when:**
- Implementing cross-cutting concerns
- Handling security/encryption
- Managing logging
- File I/O operations

### Service Lifetime Decision

**Singleton:** Use when service is stateless and expensive to create
- Example: `IHashService`, `IRsaValidator`, `IDeviceIdProvider`

**Scoped:** Use for DbContext and services that should live per request/scope
- Example: `InstallVibeContext`, repositories

**Transient:** Use for lightweight, stateless services
- Example: Most business services, ViewModels

---

## Additional Resources

### Documentation Links

- [Microsoft WinUI 3 Docs](https://learn.microsoft.com/en-us/windows/apps/winui/winui3/)
- [CommunityToolkit.Mvvm Docs](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Entity Framework Core Docs](https://learn.microsoft.com/en-us/ef/core/)
- [Microsoft Graph API Docs](https://learn.microsoft.com/en-us/graph/)
- [Serilog Documentation](https://serilog.net/)

### Project-Specific Resources

- Architecture Document: `docs/ARCHITECTURE.md`
- Development Roadmap: `DEVELOPMENT_PLAN.md`
- Guide Engine Specification: `docs/GUIDE_ENGINE.md`
- SharePoint Integration Guide: `docs/SHAREPOINT_INTEGRATION.md`
- Local Data Layer Design: `docs/LOCAL_DATA_LAYER.md`

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2025-11-16 | Initial comprehensive guide for AI assistants |

---

**Last Updated:** 2025-11-16
**Maintained By:** Development Team
**For Questions:** See issue tracker or documentation folder
