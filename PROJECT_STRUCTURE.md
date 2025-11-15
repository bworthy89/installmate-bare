# InstallVibe - Complete Project Structure

## Overview

This document outlines the complete folder and file structure for the InstallVibe WinUI 3 desktop application based on the architecture defined in ARCHITECTURE.md.

---

## Complete Directory Tree

```
InstallVibe/                                    # Solution root
│
├── .github/                                    # GitHub workflows and automation
│   └── workflows/
│       ├── build.yml                          # CI build workflow
│       ├── release.yml                        # Release automation workflow
│       └── tests.yml                          # Automated testing workflow
│
├── docs/                                       # Project documentation
│   ├── ARCHITECTURE.md                        # Architecture document (from Step 1)
│   ├── PROJECT_STRUCTURE.md                   # This document
│   ├── API.md                                 # API documentation
│   ├── DEPLOYMENT.md                          # Deployment guide
│   └── USER_GUIDE.md                          # End-user documentation
│
├── src/                                        # Source code root
│   │
│   ├── InstallVibe/                           # Main WinUI 3 application project
│   │   ├── InstallVibe.csproj                 # Project file (.NET 8, WinUI 3)
│   │   ├── app.manifest                       # Application manifest for elevation/capabilities
│   │   ├── Package.appxmanifest               # MSIX package manifest
│   │   │
│   │   ├── App.xaml                           # Application-level XAML resources
│   │   ├── App.xaml.cs                        # Application entry point, DI setup
│   │   │
│   │   ├── Assets/                            # Static application assets
│   │   │   ├── Fonts/                         # Custom fonts
│   │   │   │   └── SegoeFluentIcons.ttf       # Fluent icon font
│   │   │   ├── Icons/                         # Application icons
│   │   │   │   ├── AppIcon.ico                # Main app icon
│   │   │   │   ├── AppIcon.png                # PNG version for MSIX
│   │   │   │   ├── LockScreenLogo.png         # Lock screen logo
│   │   │   │   ├── SplashScreen.png           # Splash screen image
│   │   │   │   ├── Square44x44Logo.png        # Small tile icon
│   │   │   │   ├── Square150x150Logo.png      # Medium tile icon
│   │   │   │   ├── StoreLogo.png              # Store logo
│   │   │   │   └── Wide310x150Logo.png        # Wide tile icon
│   │   │   ├── Images/                        # UI images and graphics
│   │   │   │   ├── WelcomeBackground.png      # Welcome page background
│   │   │   │   ├── EmptyState.png             # Empty state illustrations
│   │   │   │   └── Logo.png                   # InstallVibe logo
│   │   │   └── Animations/                    # Lottie animations (optional)
│   │   │       └── Loading.json               # Loading animation
│   │   │
│   │   ├── Views/                             # XAML views/pages
│   │   │   ├── Shell/                         # Application shell views
│   │   │   │   ├── MainWindow.xaml            # Main application window
│   │   │   │   ├── MainWindow.xaml.cs
│   │   │   │   ├── ShellPage.xaml             # Main shell with NavigationView
│   │   │   │   └── ShellPage.xaml.cs
│   │   │   │
│   │   │   ├── Activation/                    # Product key activation views
│   │   │   │   ├── WelcomePage.xaml           # First-run welcome page
│   │   │   │   ├── WelcomePage.xaml.cs
│   │   │   │   ├── ActivationPage.xaml        # Product key entry page
│   │   │   │   ├── ActivationPage.xaml.cs
│   │   │   │   ├── LicenseInfoPage.xaml       # License status display
│   │   │   │   └── LicenseInfoPage.xaml.cs
│   │   │   │
│   │   │   ├── Guides/                        # Guide management views
│   │   │   │   ├── GuideListPage.xaml         # Browse all guides (grid/list)
│   │   │   │   ├── GuideListPage.xaml.cs
│   │   │   │   ├── GuideDetailPage.xaml       # View guide details and steps
│   │   │   │   ├── GuideDetailPage.xaml.cs
│   │   │   │   ├── GuideCategoriesPage.xaml   # Browse by category
│   │   │   │   ├── GuideCategoriesPage.xaml.cs
│   │   │   │   ├── GuideEditorPage.xaml       # Create/edit guides (Admin only)
│   │   │   │   ├── GuideEditorPage.xaml.cs
│   │   │   │   ├── StepEditorPage.xaml        # Edit individual steps (Admin)
│   │   │   │   └── StepEditorPage.xaml.cs
│   │   │   │
│   │   │   ├── Progress/                      # Progress tracking views
│   │   │   │   ├── ProgressPage.xaml          # View all progress/history
│   │   │   │   ├── ProgressPage.xaml.cs
│   │   │   │   ├── ActiveGuidePage.xaml       # Current active guide session
│   │   │   │   └── ActiveGuidePage.xaml.cs
│   │   │   │
│   │   │   ├── Sync/                          # Synchronization views
│   │   │   │   ├── SyncPage.xaml              # Manual sync controls
│   │   │   │   ├── SyncPage.xaml.cs
│   │   │   │   ├── SyncStatusDialog.xaml      # Sync progress dialog
│   │   │   │   └── SyncStatusDialog.xaml.cs
│   │   │   │
│   │   │   ├── Settings/                      # Application settings views
│   │   │   │   ├── SettingsPage.xaml          # Main settings page
│   │   │   │   ├── SettingsPage.xaml.cs
│   │   │   │   ├── AccountSettingsPage.xaml   # SharePoint account settings
│   │   │   │   ├── AccountSettingsPage.xaml.cs
│   │   │   │   ├── CacheSettingsPage.xaml     # Cache management settings
│   │   │   │   ├── CacheSettingsPage.xaml.cs
│   │   │   │   ├── AboutPage.xaml             # About/version info
│   │   │   │   └── AboutPage.xaml.cs
│   │   │   │
│   │   │   └── Dialogs/                       # Reusable dialog views
│   │   │       ├── ConfirmationDialog.xaml    # Generic confirmation dialog
│   │   │       ├── ConfirmationDialog.xaml.cs
│   │   │       ├── ErrorDialog.xaml           # Error message dialog
│   │   │       ├── ErrorDialog.xaml.cs
│   │   │       ├── ProductKeyDialog.xaml      # Quick product key entry
│   │   │       └── ProductKeyDialog.xaml.cs
│   │   │
│   │   ├── ViewModels/                        # MVVM ViewModels
│   │   │   ├── Shell/
│   │   │   │   ├── MainWindowViewModel.cs     # Main window VM
│   │   │   │   └── ShellViewModel.cs          # Shell navigation VM
│   │   │   │
│   │   │   ├── Activation/
│   │   │   │   ├── WelcomeViewModel.cs        # Welcome page VM
│   │   │   │   ├── ActivationViewModel.cs     # Activation logic VM
│   │   │   │   └── LicenseInfoViewModel.cs    # License display VM
│   │   │   │
│   │   │   ├── Guides/
│   │   │   │   ├── GuideListViewModel.cs      # Guide list VM
│   │   │   │   ├── GuideDetailViewModel.cs    # Guide detail VM
│   │   │   │   ├── GuideCategoriesViewModel.cs # Categories VM
│   │   │   │   ├── GuideEditorViewModel.cs    # Editor VM (Admin)
│   │   │   │   └── StepEditorViewModel.cs     # Step editor VM (Admin)
│   │   │   │
│   │   │   ├── Progress/
│   │   │   │   ├── ProgressViewModel.cs       # Progress tracking VM
│   │   │   │   └── ActiveGuideViewModel.cs    # Active session VM
│   │   │   │
│   │   │   ├── Sync/
│   │   │   │   └── SyncViewModel.cs           # Sync control VM
│   │   │   │
│   │   │   └── Settings/
│   │   │       ├── SettingsViewModel.cs       # Main settings VM
│   │   │       ├── AccountSettingsViewModel.cs # Account VM
│   │   │       ├── CacheSettingsViewModel.cs  # Cache VM
│   │   │       └── AboutViewModel.cs          # About VM
│   │   │
│   │   ├── Controls/                          # Custom reusable controls
│   │   │   ├── StepControl.xaml               # Individual step display control
│   │   │   ├── StepControl.xaml.cs
│   │   │   ├── MediaViewer.xaml               # Image/video viewer control
│   │   │   ├── MediaViewer.xaml.cs
│   │   │   ├── ProgressIndicator.xaml         # Guide progress indicator
│   │   │   ├── ProgressIndicator.xaml.cs
│   │   │   ├── ChecklistControl.xaml          # Step checklist control
│   │   │   ├── ChecklistControl.xaml.cs
│   │   │   ├── GuideCard.xaml                 # Guide preview card
│   │   │   ├── GuideCard.xaml.cs
│   │   │   ├── StepNavigator.xaml             # Step navigation control
│   │   │   ├── StepNavigator.xaml.cs
│   │   │   ├── LicenseBadge.xaml              # License type badge
│   │   │   └── LicenseBadge.xaml.cs
│   │   │
│   │   ├── Converters/                        # XAML value converters
│   │   │   ├── BoolToVisibilityConverter.cs   # Boolean to Visibility
│   │   │   ├── InverseBoolConverter.cs        # Inverse boolean
│   │   │   ├── MediaTypeToIconConverter.cs    # Media type to icon glyph
│   │   │   ├── LicenseTypeToColorConverter.cs # License type to accent color
│   │   │   ├── DateTimeToStringConverter.cs   # DateTime formatting
│   │   │   ├── FileSizeConverter.cs           # Bytes to KB/MB/GB
│   │   │   └── NullToVisibilityConverter.cs   # Null checking for visibility
│   │   │
│   │   ├── Helpers/                           # UI helper classes
│   │   │   ├── NavigationHelper.cs            # Navigation service implementation
│   │   │   ├── ResourceHelper.cs              # Resource loading helper
│   │   │   ├── ThemeHelper.cs                 # Theme switching helper
│   │   │   ├── WindowHelper.cs                # Window management helper
│   │   │   └── DispatcherHelper.cs            # UI thread dispatcher helper
│   │   │
│   │   ├── Behaviors/                         # XAML behaviors
│   │   │   ├── AutoScrollBehavior.cs          # Auto-scroll for lists
│   │   │   └── FocusOnLoadBehavior.cs         # Focus control on load
│   │   │
│   │   ├── Styles/                            # Custom styles and themes
│   │   │   ├── _Theme.xaml                    # Main theme resource dictionary
│   │   │   ├── Colors.xaml                    # Color definitions
│   │   │   ├── Brushes.xaml                   # Brush resources
│   │   │   ├── Typography.xaml                # Text styles
│   │   │   ├── Controls.xaml                  # Control style overrides
│   │   │   └── Animations.xaml                # Animation resources
│   │   │
│   │   └── Properties/                        # Assembly properties
│   │       └── launchSettings.json            # Debug launch settings
│   │
│   ├── InstallVibe.Core/                      # Core business logic library
│   │   ├── InstallVibe.Core.csproj            # Class library project (.NET 8)
│   │   │
│   │   ├── Models/                            # Domain models
│   │   │   ├── Domain/                        # Core domain entities
│   │   │   │   ├── Guide.cs                   # Guide model
│   │   │   │   ├── Step.cs                    # Step model
│   │   │   │   ├── MediaReference.cs          # Media reference model
│   │   │   │   ├── ChecklistItem.cs           # Checklist item model
│   │   │   │   ├── GuideMetadata.cs           # Guide metadata model
│   │   │   │   └── GuideCategory.cs           # Category model
│   │   │   │
│   │   │   ├── Activation/                    # Activation models
│   │   │   │   ├── ActivationToken.cs         # Activation token model
│   │   │   │   ├── ProductKey.cs              # Product key model
│   │   │   │   ├── LicenseInfo.cs             # License information model
│   │   │   │   └── LicenseType.cs             # License type enum
│   │   │   │
│   │   │   ├── Progress/                      # Progress tracking models
│   │   │   │   ├── GuideProgress.cs           # Guide progress model
│   │   │   │   ├── StepProgress.cs            # Step progress model
│   │   │   │   └── StepStatus.cs              # Step status enum
│   │   │   │
│   │   │   ├── Sync/                          # Synchronization models
│   │   │   │   ├── SyncMetadata.cs            # Sync metadata model
│   │   │   │   ├── SyncStatus.cs              # Sync status enum
│   │   │   │   ├── SyncResult.cs              # Sync operation result
│   │   │   │   └── SyncConflict.cs            # Sync conflict model
│   │   │   │
│   │   │   ├── Cache/                         # Cache models
│   │   │   │   ├── CacheEntry.cs              # Generic cache entry
│   │   │   │   ├── MediaCacheInfo.cs          # Media cache metadata
│   │   │   │   └── CacheStatistics.cs         # Cache usage statistics
│   │   │   │
│   │   │   └── Settings/                      # Settings models
│   │   │       ├── AppSettings.cs             # Application settings
│   │   │       ├── UserPreferences.cs         # User preferences
│   │   │       └── SharePointSettings.cs      # SharePoint configuration
│   │   │
│   │   ├── Services/                          # Business logic services
│   │   │   ├── Activation/                    # Activation services
│   │   │   │   ├── IActivationService.cs      # Activation service interface
│   │   │   │   ├── ActivationService.cs       # Activation service implementation
│   │   │   │   ├── IProductKeyValidator.cs    # Product key validator interface
│   │   │   │   ├── ProductKeyValidator.cs     # Product key validation logic
│   │   │   │   ├── ILicenseManager.cs         # License manager interface
│   │   │   │   ├── LicenseManager.cs          # License management logic
│   │   │   │   └── ITokenManager.cs           # Token manager interface
│   │   │   │   └── TokenManager.cs            # Token storage/retrieval
│   │   │   │
│   │   │   ├── Data/                          # Data services
│   │   │   │   ├── IGuideService.cs           # Guide service interface
│   │   │   │   ├── GuideService.cs            # Guide service implementation
│   │   │   │   ├── IProgressService.cs        # Progress service interface
│   │   │   │   ├── ProgressService.cs         # Progress tracking implementation
│   │   │   │   ├── IMediaService.cs           # Media service interface
│   │   │   │   └── MediaService.cs            # Media management implementation
│   │   │   │
│   │   │   ├── SharePoint/                    # SharePoint integration services
│   │   │   │   ├── ISharePointService.cs      # SharePoint service interface
│   │   │   │   ├── SharePointService.cs       # Main SharePoint service
│   │   │   │   ├── ISharePointAuthService.cs  # Auth service interface
│   │   │   │   ├── SharePointAuthService.cs   # Authentication/token management
│   │   │   │   ├── IGraphApiClient.cs         # Graph API client interface
│   │   │   │   ├── GraphApiClient.cs          # Graph API implementation
│   │   │   │   ├── ISharePointGuideProvider.cs # Guide provider interface
│   │   │   │   ├── SharePointGuideProvider.cs # Guide download/upload
│   │   │   │   └── SharePointConfig.cs        # SharePoint configuration helper
│   │   │   │
│   │   │   ├── Sync/                          # Synchronization services
│   │   │   │   ├── ISyncService.cs            # Sync service interface
│   │   │   │   ├── SyncService.cs             # Main sync orchestration
│   │   │   │   ├── ISyncEngine.cs             # Sync engine interface
│   │   │   │   ├── SyncEngine.cs              # Sync algorithm implementation
│   │   │   │   ├── IConflictResolver.cs       # Conflict resolver interface
│   │   │   │   ├── ConflictResolver.cs        # Conflict resolution logic
│   │   │   │   └── SyncQueue.cs               # Sync operation queue
│   │   │   │
│   │   │   ├── Cache/                         # Caching services
│   │   │   │   ├── ICacheService.cs           # Cache service interface
│   │   │   │   ├── CacheService.cs            # Main cache service
│   │   │   │   ├── IMediaCacheManager.cs      # Media cache interface
│   │   │   │   ├── MediaCacheManager.cs       # Media caching logic
│   │   │   │   ├── ICacheCleanupService.cs    # Cleanup service interface
│   │   │   │   ├── CacheCleanupService.cs     # LRU cleanup implementation
│   │   │   │   └── MemoryCache.cs             # In-memory cache layer
│   │   │   │
│   │   │   ├── Update/                        # Update services
│   │   │   │   ├── IUpdateService.cs          # Update service interface
│   │   │   │   ├── UpdateService.cs           # Update check/install logic
│   │   │   │   └── VersionManager.cs          # Version comparison logic
│   │   │   │
│   │   │   └── Logging/                       # Logging service
│   │   │       ├── ILoggingService.cs         # Logging service interface
│   │   │       └── LoggingService.cs          # Logging implementation wrapper
│   │   │
│   │   ├── Contracts/                         # Service contracts/interfaces
│   │   │   ├── INavigationService.cs          # Navigation service contract
│   │   │   ├── IDialogService.cs              # Dialog service contract
│   │   │   ├── ISettingsService.cs            # Settings service contract
│   │   │   ├── INotificationService.cs        # Notification service contract
│   │   │   └── IFileService.cs                # File operations contract
│   │   │
│   │   ├── Extensions/                        # Extension methods
│   │   │   ├── ServiceCollectionExtensions.cs # DI registration extensions
│   │   │   ├── StringExtensions.cs            # String utility extensions
│   │   │   ├── DateTimeExtensions.cs          # DateTime extensions
│   │   │   └── EnumExtensions.cs              # Enum utility extensions
│   │   │
│   │   └── Exceptions/                        # Custom exceptions
│   │       ├── ActivationException.cs         # Activation-related exceptions
│   │       ├── SyncException.cs               # Sync-related exceptions
│   │       ├── SharePointException.cs         # SharePoint-related exceptions
│   │       └── CacheException.cs              # Cache-related exceptions
│   │
│   ├── InstallVibe.Data/                      # Data access layer
│   │   ├── InstallVibe.Data.csproj            # Data access project (.NET 8)
│   │   │
│   │   ├── Context/                           # Database context
│   │   │   ├── InstallVibeContext.cs          # Main EF Core DbContext
│   │   │   └── DesignTimeDbContextFactory.cs  # Design-time context for migrations
│   │   │
│   │   ├── Entities/                          # Database entities
│   │   │   ├── GuideEntity.cs                 # Guide table entity
│   │   │   ├── StepEntity.cs                  # Step table entity
│   │   │   ├── MediaCacheEntity.cs            # Media cache table entity
│   │   │   ├── ProgressEntity.cs              # Progress table entity
│   │   │   ├── SyncMetadataEntity.cs          # Sync metadata table entity
│   │   │   └── SettingEntity.cs               # Settings table entity
│   │   │
│   │   ├── Repositories/                      # Repository pattern implementations
│   │   │   ├── IRepository.cs                 # Generic repository interface
│   │   │   ├── Repository.cs                  # Generic repository base class
│   │   │   ├── IGuideRepository.cs            # Guide repository interface
│   │   │   ├── GuideRepository.cs             # Guide repository implementation
│   │   │   ├── IProgressRepository.cs         # Progress repository interface
│   │   │   ├── ProgressRepository.cs          # Progress repository implementation
│   │   │   ├── IMediaCacheRepository.cs       # Media cache repository interface
│   │   │   ├── MediaCacheRepository.cs        # Media cache repository implementation
│   │   │   ├── ISyncMetadataRepository.cs     # Sync metadata repository interface
│   │   │   ├── SyncMetadataRepository.cs      # Sync metadata repository implementation
│   │   │   ├── ISettingsRepository.cs         # Settings repository interface
│   │   │   └── SettingsRepository.cs          # Settings repository implementation
│   │   │
│   │   ├── Configurations/                    # EF Core entity configurations
│   │   │   ├── GuideConfiguration.cs          # Guide entity configuration
│   │   │   ├── StepConfiguration.cs           # Step entity configuration
│   │   │   ├── MediaCacheConfiguration.cs     # Media cache configuration
│   │   │   ├── ProgressConfiguration.cs       # Progress entity configuration
│   │   │   └── SyncMetadataConfiguration.cs   # Sync metadata configuration
│   │   │
│   │   ├── Migrations/                        # EF Core migrations
│   │   │   └── (generated migration files)    # Auto-generated by EF Core
│   │   │
│   │   └── Seed/                              # Database seeding
│   │       └── DataSeeder.cs                  # Initial data seeding logic
│   │
│   ├── InstallVibe.Infrastructure/            # Infrastructure/cross-cutting concerns
│   │   ├── InstallVibe.Infrastructure.csproj  # Infrastructure project (.NET 8)
│   │   │
│   │   ├── Security/                          # Security implementations
│   │   │   ├── Cryptography/                  # Cryptographic services
│   │   │   │   ├── ICryptoService.cs          # Crypto service interface
│   │   │   │   ├── CryptoService.cs           # General crypto operations
│   │   │   │   ├── IRsaValidator.cs           # RSA validator interface
│   │   │   │   ├── RsaValidator.cs            # RSA signature verification
│   │   │   │   ├── IDpapiEncryption.cs        # DPAPI interface
│   │   │   │   ├── DpapiEncryption.cs         # Windows DPAPI implementation
│   │   │   │   ├── IHashService.cs            # Hashing service interface
│   │   │   │   └── HashService.cs             # SHA256/CRC hashing
│   │   │   │
│   │   │   └── Keys/                          # Key management
│   │   │       ├── PublicKeys.cs              # Embedded RSA public keys
│   │   │       └── KeyLoader.cs               # Key loading utilities
│   │   │
│   │   ├── Logging/                           # Logging configuration
│   │   │   ├── LoggingConfiguration.cs        # Serilog setup and config
│   │   │   ├── LoggerExtensions.cs            # Logger extension methods
│   │   │   └── SensitiveDataSanitizer.cs      # Sanitize logs for secrets
│   │   │
│   │   ├── Configuration/                     # Configuration management
│   │   │   ├── AppSettings.cs                 # Application settings model
│   │   │   ├── ConfigurationManager.cs        # Configuration loader/saver
│   │   │   └── appsettings.json               # Default app configuration
│   │   │
│   │   ├── Constants/                         # Application constants
│   │   │   ├── AppConstants.cs                # General app constants
│   │   │   ├── CacheConstants.cs              # Cache-related constants
│   │   │   ├── SharePointConstants.cs         # SharePoint API constants
│   │   │   └── ValidationConstants.cs         # Validation constants
│   │   │
│   │   ├── IO/                                # File I/O utilities
│   │   │   ├── FileSystemService.cs           # File system operations
│   │   │   ├── PathHelper.cs                  # Path manipulation helpers
│   │   │   └── FileIntegrityChecker.cs        # Checksum verification
│   │   │
│   │   └── Networking/                        # Network utilities
│   │       ├── NetworkMonitor.cs              # Network connectivity monitor
│   │       ├── RetryPolicy.cs                 # Retry logic with backoff
│   │       └── HttpClientFactory.cs           # Configured HttpClient factory
│   │
│   └── InstallVibe.Tests/                     # Test projects
│       ├── InstallVibe.Tests.csproj           # Test project (xUnit, .NET 8)
│       │
│       ├── Unit/                              # Unit tests
│       │   ├── Services/                      # Service unit tests
│       │   │   ├── Activation/
│       │   │   │   ├── ActivationServiceTests.cs
│       │   │   │   ├── ProductKeyValidatorTests.cs
│       │   │   │   ├── LicenseManagerTests.cs
│       │   │   │   └── TokenManagerTests.cs
│       │   │   │
│       │   │   ├── Data/
│       │   │   │   ├── GuideServiceTests.cs
│       │   │   │   ├── ProgressServiceTests.cs
│       │   │   │   └── MediaServiceTests.cs
│       │   │   │
│       │   │   ├── SharePoint/
│       │   │   │   ├── SharePointServiceTests.cs
│       │   │   │   ├── SharePointAuthServiceTests.cs
│       │   │   │   └── GraphApiClientTests.cs
│       │   │   │
│       │   │   ├── Sync/
│       │   │   │   ├── SyncServiceTests.cs
│       │   │   │   ├── SyncEngineTests.cs
│       │   │   │   └── ConflictResolverTests.cs
│       │   │   │
│       │   │   └── Cache/
│       │   │       ├── CacheServiceTests.cs
│       │   │       ├── MediaCacheManagerTests.cs
│       │   │       └── CacheCleanupServiceTests.cs
│       │   │
│       │   ├── Infrastructure/                # Infrastructure unit tests
│       │   │   ├── Security/
│       │   │   │   ├── RsaValidatorTests.cs
│       │   │   │   ├── DpapiEncryptionTests.cs
│       │   │   │   └── HashServiceTests.cs
│       │   │   │
│       │   │   └── IO/
│       │   │       └── FileIntegrityCheckerTests.cs
│       │   │
│       │   └── ViewModels/                    # ViewModel unit tests
│       │       ├── ActivationViewModelTests.cs
│       │       ├── GuideListViewModelTests.cs
│       │       ├── GuideDetailViewModelTests.cs
│       │       └── ProgressViewModelTests.cs
│       │
│       ├── Integration/                       # Integration tests
│       │   ├── Database/
│       │   │   ├── GuideRepositoryTests.cs
│       │   │   ├── ProgressRepositoryTests.cs
│       │   │   └── DatabaseMigrationTests.cs
│       │   │
│       │   ├── SharePoint/
│       │   │   ├── SharePointIntegrationTests.cs
│       │   │   └── SyncIntegrationTests.cs
│       │   │
│       │   └── EndToEnd/
│       │       ├── ActivationFlowTests.cs
│       │       └── GuideSyncFlowTests.cs
│       │
│       ├── UI/                                # UI automation tests
│       │   ├── ActivationPageTests.cs
│       │   ├── GuideListPageTests.cs
│       │   └── NavigationTests.cs
│       │
│       └── TestHelpers/                       # Test utilities
│           ├── MockServices/                  # Mock service implementations
│           │   ├── MockActivationService.cs
│           │   ├── MockSharePointService.cs
│           │   ├── MockGuideService.cs
│           │   └── MockCacheService.cs
│           │
│           ├── Fixtures/                      # Test data fixtures
│           │   ├── GuideFixtures.cs
│           │   ├── ProductKeyFixtures.cs
│           │   └── ProgressFixtures.cs
│           │
│           └── Builders/                      # Test data builders
│               ├── GuideBuilder.cs
│               ├── StepBuilder.cs
│               └── ActivationTokenBuilder.cs
│
├── tools/                                      # Build and deployment tools
│   ├── scripts/                               # Build automation scripts
│   │   ├── build.ps1                          # PowerShell build script
│   │   ├── clean.ps1                          # Clean build artifacts
│   │   ├── test.ps1                           # Run all tests
│   │   ├── package.ps1                        # Create MSIX package
│   │   ├── sign.ps1                           # Sign MSIX with certificate
│   │   └── deploy-sharepoint.ps1              # Deploy to SharePoint
│   │
│   ├── keygen/                                # Product key generation tool
│   │   ├── KeyGenerator.csproj                # Standalone keygen tool project
│   │   ├── Program.cs                         # Keygen CLI entry point
│   │   ├── KeyGenerator.cs                    # Key generation logic
│   │   ├── private_key.pem                    # RSA private key (SECURE - NOT IN REPO)
│   │   └── README.md                          # Keygen usage instructions
│   │
│   └── certificates/                          # Code signing certificates
│       ├── README.md                          # Certificate management guide
│       └── .gitignore                         # Ignore actual certificate files
│
├── packaging/                                  # MSIX packaging resources
│   ├── AppInstaller/                          # AppInstaller configuration
│   │   ├── InstallVibe.appinstaller          # AppInstaller manifest template
│   │   └── versions.json                      # Version metadata template
│   │
│   ├── MSIX/                                  # MSIX package configuration
│   │   ├── Package.appxmanifest.template     # MSIX manifest template
│   │   └── priconfig.xml                      # Resource indexing config
│   │
│   └── SharePoint/                            # SharePoint deployment structure
│       ├── Guides/                            # Sample guide structure
│       │   └── SampleGuide/
│       │       ├── guide.json                 # Sample guide definition
│       │       └── steps/
│       │           └── step1.json             # Sample step
│       │
│       ├── AppUpdates/                        # Update package location
│       │   └── README.md                      # Deployment instructions
│       │
│       └── SharePointSetup.md                 # SharePoint configuration guide
│
├── build/                                      # Build output (gitignored)
│   ├── Debug/                                 # Debug build artifacts
│   ├── Release/                               # Release build artifacts
│   └── Packages/                              # Generated MSIX packages
│
├── cache/                                      # Local runtime cache (gitignored)
│   └── README.md                              # Cache structure documentation
│
├── .editorconfig                              # Code style configuration
├── .gitignore                                 # Git ignore rules
├── .gitattributes                             # Git attributes
├── Directory.Build.props                      # Shared MSBuild properties
├── Directory.Packages.props                   # Central package version management
├── nuget.config                               # NuGet package sources
├── InstallVibe.sln                            # Visual Studio solution file
├── README.md                                  # Project README
├── LICENSE                                    # License file
└── CHANGELOG.md                               # Version changelog

```

---

## Directory Descriptions

### Solution Root (`InstallVibe/`)
Main solution folder containing all projects, configuration, and documentation.

### `.github/workflows/`
GitHub Actions workflows for CI/CD automation. Includes build, test, and release pipelines.

### `docs/`
Comprehensive project documentation including architecture, API docs, deployment guides, and user manuals.

### `src/InstallVibe/` (Main WinUI 3 Application)
Primary user-facing WinUI 3 desktop application. Contains all UI, views, ViewModels, and presentation logic.

#### `Assets/`
Static resources including fonts, icons, images, and animations used throughout the application.

#### `Views/`
XAML pages organized by feature area:
- **Shell**: Main window and navigation shell
- **Activation**: Product key entry and license management
- **Guides**: Browse, view, and edit guides
- **Progress**: Track guide completion
- **Sync**: Synchronization controls
- **Settings**: Application configuration
- **Dialogs**: Reusable modal dialogs

#### `ViewModels/`
MVVM ViewModels corresponding to Views. Handle UI logic, data binding, and command execution.

#### `Controls/`
Custom reusable WinUI 3 controls for specialized UI components (step display, media viewer, progress indicators, etc.).

#### `Converters/`
XAML value converters for data transformation in bindings (bool to visibility, enums to strings, etc.).

#### `Helpers/`
UI-specific helper classes for navigation, resources, theming, and window management.

#### `Behaviors/`
XAML behaviors for reusable UI interactions (auto-scroll, focus management, etc.).

#### `Styles/`
XAML resource dictionaries for custom themes, colors, brushes, typography, and control styles.

### `src/InstallVibe.Core/` (Business Logic Layer)
Platform-agnostic business logic library. Contains all domain models, services, and business rules.

#### `Models/`
Domain models organized by feature:
- **Domain**: Core entities (Guide, Step, Media)
- **Activation**: Product key and licensing models
- **Progress**: Progress tracking models
- **Sync**: Synchronization metadata
- **Cache**: Cache management models
- **Settings**: Configuration models

#### `Services/`
Business logic services implementing core functionality:
- **Activation**: Product key validation, license management, token storage
- **Data**: Guide, progress, and media management
- **SharePoint**: SharePoint integration, authentication, API access
- **Sync**: Synchronization engine and conflict resolution
- **Cache**: Multi-tier caching with cleanup policies
- **Update**: Application update checking
- **Logging**: Logging service wrapper

#### `Contracts/`
Service interfaces for navigation, dialogs, settings, notifications, and file operations.

#### `Extensions/`
Extension methods for dependency injection, string manipulation, and utility functions.

#### `Exceptions/`
Custom exception types for domain-specific error handling.

### `src/InstallVibe.Data/` (Data Access Layer)
Entity Framework Core data access layer. Handles all database operations.

#### `Context/`
EF Core DbContext and design-time factory for migrations.

#### `Entities/`
Database entity models mapped to SQLite tables.

#### `Repositories/`
Repository pattern implementations for data access abstraction.

#### `Configurations/`
EF Core Fluent API configurations for entity mapping.

#### `Migrations/`
EF Core migration files for database schema versioning.

#### `Seed/`
Database seeding logic for initial data.

### `src/InstallVibe.Infrastructure/` (Infrastructure Layer)
Cross-cutting concerns and platform-specific implementations.

#### `Security/`
- **Cryptography**: RSA validation, DPAPI encryption, hashing
- **Keys**: Embedded public keys and key loading

#### `Logging/`
Serilog configuration, logger extensions, and sensitive data sanitization.

#### `Configuration/`
Application settings management and configuration loading.

#### `Constants/`
Application-wide constants organized by domain.

#### `IO/`
File system operations, path helpers, and integrity checking.

#### `Networking/`
Network monitoring, retry policies, and HTTP client configuration.

### `src/InstallVibe.Tests/` (Test Project)
Comprehensive test suite using xUnit.

#### `Unit/`
Unit tests for services, infrastructure, and ViewModels.

#### `Integration/`
Integration tests for database, SharePoint, and end-to-end flows.

#### `UI/`
UI automation tests using WinAppDriver.

#### `TestHelpers/`
Mock services, test fixtures, and builder patterns for test data.

### `tools/`
Build automation and development tools.

#### `scripts/`
PowerShell scripts for building, testing, packaging, signing, and deployment.

#### `keygen/`
Standalone product key generation tool (uses RSA private key to generate signed keys).

#### `certificates/`
Code signing certificate storage (excluded from source control).

### `packaging/`
MSIX packaging and deployment resources.

#### `AppInstaller/`
AppInstaller manifest templates and version metadata.

#### `MSIX/`
MSIX package manifest templates and resource configuration.

#### `SharePoint/`
Sample SharePoint structure, sample guides, and SharePoint setup documentation.

### `build/` (Generated - Gitignored)
Build output directory containing compiled binaries and MSIX packages.

### `cache/` (Runtime - Gitignored)
Local runtime cache directory. Created at runtime in user's LocalAppData.

---

## Runtime Folder Structure

The following folders are created at runtime in the user's `%LOCALAPPDATA%\InstallVibe\` directory:

```
%LOCALAPPDATA%\InstallVibe/
├── Cache/                                     # Cached content
│   ├── Guides/                                # Cached guide JSON files
│   │   └── {GuideId}/
│   │       ├── guide.json
│   │       ├── steps/
│   │       │   └── {StepId}.json
│   │       └── media/
│   │           └── {MediaId}.{ext}
│   │
│   ├── Media/                                 # Shared media cache
│   │   ├── Images/
│   │   ├── Videos/
│   │   └── Documents/
│   │
│   └── Temp/                                  # Temporary downloads
│
├── Data/                                      # SQLite database
│   └── installvibe.db
│
├── Logs/                                      # Application logs
│   ├── app-{date}.log
│   ├── errors-{date}.log
│   └── sync-{date}.log
│
└── Config/                                    # Configuration files
    ├── activation.dat                         # Encrypted activation token
    ├── settings.json                          # User preferences
    └── sharepoint.dat                         # Encrypted SharePoint credentials
```

---

## Build Output Structure

The `build/` folder structure after a successful build:

```
build/
├── Debug/                                     # Debug configuration
│   └── net8.0-windows10.0.19041.0/
│       ├── win-x64/
│       │   ├── InstallVibe.exe
│       │   ├── InstallVibe.dll
│       │   ├── InstallVibe.Core.dll
│       │   ├── InstallVibe.Data.dll
│       │   ├── InstallVibe.Infrastructure.dll
│       │   └── (dependencies)
│       │
│       └── AppPackages/
│           └── InstallVibe_{version}_x64_Debug_Test/
│               ├── InstallVibe_{version}_x64_Debug.msix
│               └── (package contents)
│
├── Release/                                   # Release configuration
│   └── net8.0-windows10.0.19041.0/
│       ├── win-x64/
│       │   ├── InstallVibe.exe
│       │   ├── InstallVibe.dll
│       │   ├── InstallVibe.Core.dll
│       │   ├── InstallVibe.Data.dll
│       │   ├── InstallVibe.Infrastructure.dll
│       │   └── (dependencies)
│       │
│       └── AppPackages/
│           └── InstallVibe_{version}_x64/
│               ├── InstallVibe_{version}_x64.msix
│               ├── InstallVibe_{version}_x64.cer
│               └── Add-AppDevPackage.ps1
│
└── Packages/                                  # Final distributable packages
    ├── InstallVibe_{version}_x64.msix        # Signed MSIX package
    ├── InstallVibe.appinstaller              # AppInstaller manifest
    └── versions.json                          # Version metadata
```

---

## Key File Purposes

### Configuration Files

| File | Purpose |
|------|---------|
| `Package.appxmanifest` | MSIX package manifest - defines app identity, capabilities, visual assets |
| `app.manifest` | Windows app manifest - defines UAC elevation, DPI awareness |
| `appsettings.json` | Default application configuration - SharePoint URLs, cache settings |
| `Directory.Build.props` | Shared MSBuild properties across all projects |
| `Directory.Packages.props` | Central NuGet package version management (CPM) |
| `.editorconfig` | Code style and formatting rules |
| `nuget.config` | NuGet package source configuration |

### Security Files

| File | Purpose |
|------|---------|
| `PublicKeys.cs` | Embedded RSA public key for offline product key validation |
| `private_key.pem` | RSA private key for key generation (NEVER in repo, secured offline) |
| `activation.dat` | Encrypted activation token (runtime, per-user) |
| `sharepoint.dat` | Encrypted SharePoint credentials (runtime, per-user) |

### Data Files

| File | Purpose |
|------|---------|
| `installvibe.db` | SQLite database (runtime, per-user) |
| `guide.json` | Guide definition file (JSON schema) |
| `step.json` | Individual step file (JSON schema) |
| `versions.json` | Version metadata for updates |

### Build/Deployment Files

| File | Purpose |
|------|---------|
| `build.ps1` | Main build automation script |
| `package.ps1` | MSIX packaging script |
| `sign.ps1` | Code signing script |
| `deploy-sharepoint.ps1` | SharePoint deployment automation |
| `InstallVibe.appinstaller` | AppInstaller update manifest |

---

## Project Dependencies

```
InstallVibe (WinUI 3 App)
    ↓ depends on
    ├── InstallVibe.Core
    │       ↓ depends on
    │       └── InstallVibe.Data
    │
    └── InstallVibe.Infrastructure
            ↓ depends on
            └── InstallVibe.Core

InstallVibe.Tests
    ↓ depends on
    ├── InstallVibe
    ├── InstallVibe.Core
    ├── InstallVibe.Data
    └── InstallVibe.Infrastructure
```

---

## Git Ignore Considerations

Files and folders excluded from version control:

### Build Artifacts
- `build/` - All build outputs
- `**/bin/` - Binary outputs
- `**/obj/` - Intermediate build files
- `**/*.user` - User-specific settings

### Runtime Data
- `cache/` - Local cache directory
- `*.db` - SQLite databases
- `*.dat` - Encrypted data files
- `Logs/` - Log files

### Security Sensitive
- `private_key.pem` - RSA private key
- `*.pfx` - Code signing certificates
- `*.cer` - Certificate files (except public certs)
- `sharepoint.dat` - SharePoint credentials

### IDE Files
- `.vs/` - Visual Studio files
- `*.suo` - Solution user options
- `.vscode/` - VS Code settings (optional)

### Dependencies
- `packages/` - NuGet packages (restored via package references)
- `node_modules/` - If any Node.js tools used

---

## Next Steps

This structure is ready for implementation. The next phase would be:

1. **Create solution and projects** using Visual Studio or `dotnet new`
2. **Set up Directory.Build.props** with common MSBuild properties
3. **Configure NuGet packages** via Directory.Packages.props
4. **Create placeholder files** in each project
5. **Set up git repository** with proper `.gitignore`
6. **Initialize EF Core migrations**
7. **Begin implementation** starting with Core models and Infrastructure

All placeholder files are documented above but no code has been generated yet, as requested.
