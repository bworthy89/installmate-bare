# Guide Engine

## Overview

The Guide Engine is the main orchestrator for InstallVibe's core functionality. It manages:
- Loading guides from local cache or SharePoint
- Parsing guide.json schema
- Step navigation and rendering
- Progress tracking and persistence
- Guide refresh logic (checking for updates)
- Media caching (images, videos, documents)
- Error handling for missing or corrupted media

## Architecture

### Core Components

```
GuideEngine (Orchestrator)
├── GuideService (Local storage)
├── SharePointService (Remote content)
├── ProgressService (Progress tracking)
└── MediaService (Media caching)
```

### Data Flow

1. **Loading a Guide:**
   ```
   User requests guide
   → GuideEngine.LoadGuideAsync()
   → Check local cache (GuideService)
   → If not found, download from SharePoint
   → Save to local cache
   → Return guide object
   ```

2. **Starting a Guide:**
   ```
   User starts guide
   → GuideEngine.StartGuideAsync()
   → Load guide
   → Create progress object
   → Initialize all steps as NotStarted
   → Mark first step as InProgress
   → Save progress to database
   ```

3. **Completing a Step:**
   ```
   User completes step
   → GuideEngine.CompleteStepAsync()
   → Mark current step as Completed
   → Find next step
   → Mark next step as InProgress
   → Update progress in database
   → Calculate percent complete
   ```

4. **Caching Media:**
   ```
   Guide loaded
   → GuideEngine.EnsureMediaCachedAsync()
   → Extract all media references from steps
   → For each media ID:
       → Check if cached locally
       → If not, download from SharePoint
       → Verify checksum
       → Save to local cache
   → Return list of failed media IDs
   ```

5. **Refreshing a Guide:**
   ```
   User checks for updates
   → GuideEngine.CheckForUpdatesAsync()
   → Get local guide metadata
   → Get SharePoint metadata
   → Compare versions and last modified dates
   → Return update status

   If update available:
   → GuideEngine.RefreshGuideAsync()
   → Download new version from SharePoint
   → Save to local cache
   → Optionally sync existing progress
   → Return refresh result
   ```

## Domain Models

### Guide

```csharp
public class Guide
{
    public string GuideId { get; set; }           // Unique identifier (GUID)
    public string Title { get; set; }             // Display title
    public string Version { get; set; }           // Semantic version (1.2.3)
    public string Category { get; set; }          // Software, Hardware, etc.
    public string? Description { get; set; }      // Brief description
    public LicenseType RequiredLicense { get; set; } // Tech or Admin
    public bool IsPublished { get; set; }         // Visibility flag
    public DateTime LastModified { get; set; }    // Last update timestamp
    public string? Author { get; set; }           // Author name
    public int? EstimatedMinutes { get; set; }    // Estimated completion time
    public List<string> Tags { get; set; }        // Search tags
    public List<Step> Steps { get; set; }         // Ordered steps
    public GuideMetadata Metadata { get; set; }   // Additional metadata
}
```

### Step

```csharp
public class Step
{
    public string StepId { get; set; }            // Unique step ID
    public string Title { get; set; }             // Step title
    public int OrderIndex { get; set; }           // Display order (1-based)
    public string Content { get; set; }           // Markdown content
    public List<MediaReference> MediaReferences { get; set; } // Media files
    public string? Notes { get; set; }            // Additional notes
    public WarningLevel WarningLevel { get; set; } // Info, Warning, Critical
    public bool IsOptional { get; set; }          // Whether step is optional
    public int? ExpectedDurationMinutes { get; set; } // Expected duration
    public List<string> Prerequisites { get; set; } // Required prior steps
}
```

### MediaReference

```csharp
public class MediaReference
{
    public string MediaId { get; set; }           // Unique media ID
    public string MediaType { get; set; }         // image, video, document
    public string? Caption { get; set; }          // Caption text
    public int OrderIndex { get; set; }           // Display order in step
    public string? AltText { get; set; }          // Accessibility text
    public int? WidthHint { get; set; }           // Rendering width hint
    public int? HeightHint { get; set; }          // Rendering height hint
}
```

### GuideMetadata

```csharp
public class GuideMetadata
{
    public List<string> Prerequisites { get; set; }  // Required before starting
    public List<string> RelatedGuides { get; set; }  // Related guide IDs
    public List<ChangeLogEntry> ChangeLog { get; set; } // Version history
    public Dictionary<string, string> CustomMetadata { get; set; } // Custom fields
}
```

### GuideProgress

```csharp
public class GuideProgress
{
    public string ProgressId { get; set; }        // Unique progress ID
    public string GuideId { get; set; }           // Guide being tracked
    public string UserId { get; set; }            // User identifier
    public string? CurrentStepId { get; set; }    // Current step
    public Dictionary<string, StepStatus> StepProgress { get; set; } // Step statuses
    public DateTime StartedDate { get; set; }     // When started
    public DateTime LastUpdated { get; set; }     // Last update
    public DateTime? CompletedDate { get; set; }  // When completed (if finished)
    public string? Notes { get; set; }            // User notes
}
```

### StepStatus

```csharp
public enum StepStatus
{
    NotStarted,    // Not yet started
    InProgress,    // Currently working on
    Completed,     // Finished
    Skipped        // Skipped (optional steps)
}
```

## GuideEngine API

### Loading Guides

```csharp
// Load guide (local cache first, then SharePoint)
var guide = await guideEngine.LoadGuideAsync("guide-id");

// Force refresh from SharePoint
var guide = await guideEngine.LoadGuideAsync("guide-id", forceRefresh: true);
```

### Progress Management

```csharp
// Start a new guide
var progress = await guideEngine.StartGuideAsync("guide-id", "user-id");

// Get existing progress
var progress = await guideEngine.GetProgressAsync("guide-id", "user-id");

// Update step status
progress = await guideEngine.UpdateStepStatusAsync(
    progress.ProgressId, 
    "step-001", 
    StepStatus.InProgress);

// Complete a step (advances to next step)
progress = await guideEngine.CompleteStepAsync(progress.ProgressId, "step-001");
```

### Step Navigation

```csharp
// Get specific step
var step = await guideEngine.GetStepAsync("guide-id", "step-001");

// Get next step
var nextStep = await guideEngine.GetNextStepAsync("guide-id", "step-001");

// Get previous step
var prevStep = await guideEngine.GetPreviousStepAsync("guide-id", "step-001");
```

### Refresh Logic

```csharp
// Check for updates
var refreshResult = await guideEngine.CheckForUpdatesAsync("guide-id");

if (refreshResult.UpdateAvailable)
{
    Console.WriteLine($"Update available: v{refreshResult.PreviousVersion} → v{refreshResult.NewVersion}");
    
    // Refresh guide
    var result = await guideEngine.RefreshGuideAsync("guide-id", syncProgress: true);
    
    if (result.Success && result.WasUpdated)
    {
        Console.WriteLine($"Guide updated successfully ({result.StepsChanged} steps changed)");
    }
}
```

### Media Caching

```csharp
// Ensure all media is cached (with progress reporting)
var progress = new Progress<MediaCacheProgress>(p =>
{
    Console.WriteLine($"Caching media: {p.PercentComplete:F1}% ({p.ProcessedCount}/{p.TotalCount})");
});

var failedMedia = await guideEngine.EnsureMediaCachedAsync("guide-id", progress);

if (failedMedia.Count > 0)
{
    Console.WriteLine($"Failed to cache {failedMedia.Count} media files: {string.Join(", ", failedMedia)}");
}
```

## MediaService API

### Getting Media

```csharp
// Get media (downloads if not cached)
var imageBytes = await mediaService.GetMediaAsync("media-id");

// Get media path (if cached)
var path = await mediaService.GetMediaPathAsync("media-id");

// Check if cached
var isCached = await mediaService.IsMediaCachedAsync("media-id");
```

### Caching Media

```csharp
// Download single media file
var success = await mediaService.DownloadMediaAsync("media-id");

// Cache all media for a guide
var failedIds = await mediaService.CacheGuideMediaAsync(guide);
```

### Statistics

```csharp
var stats = await mediaService.GetCacheStatisticsAsync();
Console.WriteLine($"Cached media: {stats.TotalMediaFiles} files, {stats.TotalSizeMB:F2} MB");
Console.WriteLine($"  Images: {stats.ImageCount}");
Console.WriteLine($"  Videos: {stats.VideoCount}");
Console.WriteLine($"  Documents: {stats.DocumentCount}");
```

## Error Handling

### Missing Media

When a guide references media that doesn't exist:

1. **During Loading:**
   - Media is not downloaded yet (normal)
   - UI shows placeholder or "loading" indicator

2. **During Caching:**
   - `CacheGuideMediaAsync()` returns list of failed media IDs
   - Logs warning for each failure
   - Continues with other media files

3. **During Rendering:**
   - `GetMediaAsync()` returns null if not found
   - UI shows "Media unavailable" placeholder
   - Option to retry download

### Corrupted Cache

When cached files fail checksum verification:

1. **Automatic Recovery:**
   - CacheService detects mismatch
   - Invalidates corrupted file
   - Marks guide/media for re-download
   - Logs corruption warning

2. **User Action:**
   - Force refresh guide: `LoadGuideAsync(guideId, forceRefresh: true)`
   - Re-download media: `DownloadMediaAsync(mediaId)`

### Offline Mode

When SharePoint is unreachable:

1. **LoadGuideAsync():**
   - Returns locally cached guide
   - Logs warning if cache is stale

2. **RefreshGuideAsync():**
   - Returns offline error
   - No changes made to local cache

3. **EnsureMediaCachedAsync():**
   - Uses only cached media
   - Returns list of uncached media IDs

## JSON Schema

### guide.json Format

```json
{
  "guideId": "550e8400-e29b-41d4-a716-446655440000",
  "title": "Install Windows Server 2022",
  "version": "1.2.3",
  "category": "Software",
  "description": "Complete installation guide for Windows Server 2022",
  "requiredLicense": "Tech",
  "isPublished": true,
  "lastModified": "2025-01-15T10:30:00Z",
  "author": "John Doe",
  "estimatedMinutes": 45,
  "tags": ["windows", "server", "2022", "installation"],
  "steps": [
    {
      "stepId": "step-001",
      "title": "Prepare installation media",
      "orderIndex": 1,
      "content": "Download the Windows Server 2022 ISO from...",
      "mediaReferences": [
        {
          "mediaId": "media-12345",
          "mediaType": "image",
          "caption": "Download page screenshot",
          "orderIndex": 1,
          "altText": "Windows Server download page"
        }
      ],
      "notes": "Ensure you have a valid license key",
      "warningLevel": "info",
      "isOptional": false,
      "expectedDurationMinutes": 10
    }
  ],
  "metadata": {
    "prerequisites": ["8GB RAM", "64-bit processor"],
    "relatedGuides": ["guide-002", "guide-003"],
    "changeLog": [
      {
        "version": "1.2.3",
        "date": "2025-01-15",
        "changes": "Updated screenshots for latest installer",
        "author": "John Doe"
      }
    ]
  }
}
```

## Performance Considerations

### Caching Strategy

1. **Guides:**
   - Cached indefinitely until manually refreshed
   - Checksum verified on every read
   - Re-downloaded if corrupted

2. **Media:**
   - Cached indefinitely
   - LRU eviction when cache size exceeds limit
   - Checksum verified on download

3. **Progress:**
   - Stored in local database
   - Updated on every step change
   - Survives app restarts

### Optimization Tips

1. **Preload Media:**
   ```csharp
   // Cache media when guide is opened, not when step is viewed
   var failedMedia = await guideEngine.EnsureMediaCachedAsync(guideId);
   ```

2. **Batch Operations:**
   ```csharp
   // Cache multiple guides at once (e.g., during sync)
   foreach (var guide in guides)
   {
       await mediaService.CacheGuideMediaAsync(guide);
   }
   ```

3. **Background Refresh:**
   ```csharp
   // Check for updates in background without blocking UI
   Task.Run(async () =>
   {
       var result = await guideEngine.CheckForUpdatesAsync(guideId);
       if (result.UpdateAvailable)
       {
           // Notify user
       }
   });
   ```

## Testing Recommendations

### Unit Tests

```csharp
// Mock dependencies for isolated testing
var mockGuideService = new Mock<IGuideService>();
var mockProgressService = new Mock<IProgressService>();
var mockSharePointService = new Mock<ISharePointService>();
var mockMediaService = new Mock<IMediaService>();

var guideEngine = new GuideEngine(
    mockGuideService.Object,
    mockProgressService.Object,
    mockSharePointService.Object,
    mockMediaService.Object,
    logger);

// Test loading guide
var guide = await guideEngine.LoadGuideAsync("test-guide-id");
Assert.NotNull(guide);
```

### Integration Tests

```csharp
// Test with real database and cache
var guideEngine = serviceProvider.GetRequiredService<IGuideEngine>();

// Start guide
var progress = await guideEngine.StartGuideAsync("guide-id", "user-id");
Assert.Equal(StepStatus.InProgress, progress.StepProgress[progress.CurrentStepId]);

// Complete first step
progress = await guideEngine.CompleteStepAsync(progress.ProgressId, progress.CurrentStepId);
Assert.Equal(StepStatus.Completed, progress.StepProgress[/* first step ID */]);
```

### End-to-End Tests

```csharp
// Test complete workflow
1. Load guide from SharePoint
2. Start guide for user
3. Complete all steps
4. Verify completion
5. Check for updates
6. Refresh if available
```

## Future Enhancements

1. **Progress Sync Across Updates:**
   - Intelligent mapping of old step IDs to new step IDs
   - Preserve completion status when guide structure changes
   - Detect renamed/reordered steps

2. **Offline Media Prefetching:**
   - Download media for all guides in advance
   - Configurable prefetch strategy (WiFi only, etc.)

3. **Step Dependencies:**
   - Enforce prerequisite completion
   - Prevent skipping required steps
   - Visual dependency graph

4. **Collaborative Progress:**
   - Multiple users working on same guide
   - Shared progress tracking
   - Comments and annotations

5. **Analytics:**
   - Track time spent per step
   - Identify problematic steps (high skip rate, long duration)
   - Guide completion rates

6. **Branching/Conditional Steps:**
   - Show/hide steps based on conditions
   - Different paths through guide
   - User choices affecting workflow
