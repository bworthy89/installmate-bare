# Models

This folder contains all domain models used throughout the application.

## Folder Structure

### Domain/
Core business entities:
- **Guide.cs** - Guide model with metadata and steps
- **Step.cs** - Individual step in a guide
- **MediaReference.cs** - Reference to media files
- **ChecklistItem.cs** - Checklist item within a step
- **GuideMetadata.cs** - Guide metadata
- **GuideCategory.cs** - Category organization

### Activation/
Product key and licensing models:
- **ActivationToken.cs** - Validated activation token
- **ProductKey.cs** - Product key representation
- **LicenseInfo.cs** - License information
- **LicenseType.cs** - License type enum (Tech/Admin)

### Progress/
Progress tracking models:
- **GuideProgress.cs** - Overall guide progress
- **StepProgress.cs** - Individual step progress
- **StepStatus.cs** - Step status enum

### Sync/
Synchronization models:
- **SyncMetadata.cs** - Sync metadata tracking
- **SyncStatus.cs** - Sync status enum
- **SyncResult.cs** - Result of sync operation
- **SyncConflict.cs** - Conflict information

### Cache/
Caching models:
- **CacheEntry.cs** - Generic cache entry
- **MediaCacheInfo.cs** - Media cache metadata
- **CacheStatistics.cs** - Cache usage stats

### Settings/
Configuration models:
- **AppSettings.cs** - Application settings
- **UserPreferences.cs** - User preferences
- **SharePointSettings.cs** - SharePoint configuration

## Guidelines

- Models should be POCOs (Plain Old CLR Objects)
- Use nullable reference types appropriately
- Add data annotations for validation where needed
- Keep models in Core layer (platform-agnostic)
