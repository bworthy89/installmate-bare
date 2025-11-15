# Local Data Layer - Design & Implementation

## Overview

InstallVibe uses a hybrid local data storage system combining SQLite (for structured metadata) and file system caching (for guide content and media). This enables full offline functionality with efficient sync capabilities.

---

## Folder Structure

### Base Path

```
%LOCALAPPDATA%\InstallVibe\
```

**Windows Path Examples:**
- Windows 10/11: `C:\Users\{Username}\AppData\Local\InstallVibe\`
- Programmatic: `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)`

### Complete Hierarchy

```
%LOCALAPPDATA%\InstallVibe\
│
├── Config\                              # Configuration and activation
│   ├── activation.dat                   # Encrypted activation token (DPAPI)
│   └── settings.json                    # User preferences (JSON)
│
├── Data\                                # Database files
│   ├── installvibe.db                   # SQLite database
│   ├── installvibe.db-shm               # Shared memory (temp)
│   └── installvibe.db-wal               # Write-ahead log (temp)
│
├── Cache\                               # Cached content
│   ├── Guides\                          # Guide content cache
│   │   └── {GuideId}\                   # Per-guide folder
│   │       ├── guide.json               # Guide metadata and structure
│   │       ├── steps\                   # Step content
│   │       │   ├── {StepId}.json        # Individual step files
│   │       │   └── ...
│   │       └── media\                   # Guide-specific media
│   │           ├── {MediaId}.jpg
│   │           ├── {MediaId}.mp4
│   │           └── ...
│   │
│   ├── Media\                           # Shared media library
│   │   ├── Images\                      # Shared images
│   │   │   └── {MediaId}.{ext}
│   │   ├── Videos\                      # Shared videos
│   │   │   └── {MediaId}.{ext}
│   │   └── Documents\                   # Shared PDFs/docs
│   │       └── {MediaId}.{ext}
│   │
│   └── Temp\                            # Temporary download folder
│       └── {download-id}\               # Temporary files during download
│
├── Logs\                                # Application logs
│   ├── app-{date}.log                   # General application log
│   ├── errors-{date}.log                # Error log
│   └── sync-{date}.log                  # Sync operation log
│
└── Backup\                              # Backup folder (optional)
    ├── installvibe-{timestamp}.db       # Database backups
    └── ...
```

---

## Database Schema (SQLite)

### Tables

#### Guides Table

```sql
CREATE TABLE Guides (
    GuideId TEXT PRIMARY KEY,              -- GUID or unique identifier
    Title TEXT NOT NULL,                   -- Guide title
    Version TEXT NOT NULL,                 -- Version string (e.g., "1.0.0")
    Category TEXT,                         -- Category name
    Description TEXT,                      -- Guide description
    RequiredLicense TEXT CHECK(RequiredLicense IN ('Tech', 'Admin')),
    Published BOOLEAN NOT NULL DEFAULT 0,  -- Is published?
    LastModified DATETIME NOT NULL,        -- Last modification date (UTC)
    LocalPath TEXT NOT NULL,               -- Path to guide.json
    SharePointPath TEXT,                   -- SharePoint URL
    CachedDate DATETIME,                   -- When cached locally
    Checksum TEXT,                         -- SHA256 checksum
    SyncStatus TEXT DEFAULT 'synced' CHECK(SyncStatus IN ('synced', 'pending', 'conflict', 'error')),
    FileSize INTEGER,                      -- Total size in bytes
    StepCount INTEGER DEFAULT 0,           -- Number of steps
    CreatedDate DATETIME NOT NULL,         -- Creation date
    IsDeleted BOOLEAN DEFAULT 0            -- Soft delete flag
);

CREATE INDEX idx_guides_category ON Guides(Category);
CREATE INDEX idx_guides_sync_status ON Guides(SyncStatus);
CREATE INDEX idx_guides_modified ON Guides(LastModified DESC);
```

#### Steps Table

```sql
CREATE TABLE Steps (
    StepId TEXT PRIMARY KEY,               -- Unique step identifier
    GuideId TEXT NOT NULL,                 -- Foreign key to Guides
    StepNumber INTEGER NOT NULL,           -- Order in guide (1-based)
    Title TEXT NOT NULL,                   -- Step title
    Content TEXT,                          -- Markdown content
    MediaReferences TEXT,                  -- JSON array of media IDs
    LocalPath TEXT,                        -- Path to {StepId}.json
    Checksum TEXT,                         -- SHA256 checksum
    CachedDate DATETIME,                   -- When cached
    FOREIGN KEY (GuideId) REFERENCES Guides(GuideId) ON DELETE CASCADE
);

CREATE INDEX idx_steps_guide ON Steps(GuideId, StepNumber);
```

#### MediaCache Table

```sql
CREATE TABLE MediaCache (
    MediaId TEXT PRIMARY KEY,              -- Unique media identifier
    FileName TEXT NOT NULL,                -- Original filename
    FileType TEXT NOT NULL,                -- MIME type (image/jpeg, video/mp4, etc.)
    LocalPath TEXT NOT NULL,               -- Local file path
    SharePointPath TEXT,                   -- SharePoint URL
    FileSize INTEGER NOT NULL,             -- Size in bytes
    Checksum TEXT NOT NULL,                -- SHA256 checksum
    CachedDate DATETIME NOT NULL,          -- When cached
    LastAccessed DATETIME NOT NULL,        -- Last access time (for LRU)
    AccessCount INTEGER DEFAULT 0,         -- Access counter
    IsShared BOOLEAN DEFAULT 0,            -- Shared vs guide-specific
    Category TEXT                          -- Images, Videos, Documents
);

CREATE INDEX idx_media_last_accessed ON MediaCache(LastAccessed);
CREATE INDEX idx_media_category ON MediaCache(Category);
CREATE INDEX idx_media_file_size ON MediaCache(FileSize);
```

#### Progress Table

```sql
CREATE TABLE Progress (
    ProgressId TEXT PRIMARY KEY,           -- Unique progress identifier
    GuideId TEXT NOT NULL,                 -- Foreign key to Guides
    UserId TEXT NOT NULL,                  -- User/machine identifier
    CurrentStepId TEXT,                    -- Current step
    StepProgress TEXT NOT NULL,            -- JSON: {"step1": "completed", ...}
    StartedDate DATETIME NOT NULL,         -- When guide was started
    LastUpdated DATETIME NOT NULL,         -- Last progress update
    CompletedDate DATETIME,                -- When completed (null if in progress)
    Notes TEXT,                            -- User notes
    PercentComplete INTEGER DEFAULT 0,     -- 0-100 percentage
    FOREIGN KEY (GuideId) REFERENCES Guides(GuideId) ON DELETE CASCADE
);

CREATE INDEX idx_progress_guide ON Progress(GuideId);
CREATE INDEX idx_progress_user ON Progress(UserId);
CREATE INDEX idx_progress_updated ON Progress(LastUpdated DESC);
```

#### SyncMetadata Table

```sql
CREATE TABLE SyncMetadata (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityType TEXT NOT NULL,              -- 'Guide', 'Media', 'Step'
    EntityId TEXT NOT NULL,                -- ID of the entity
    LastSyncDate DATETIME NOT NULL,        -- Last successful sync
    ServerVersion TEXT,                    -- Server version identifier
    LocalVersion TEXT,                     -- Local version identifier
    SyncStatus TEXT DEFAULT 'synced' CHECK(SyncStatus IN ('synced', 'pending', 'conflict', 'error')),
    ErrorMessage TEXT,                     -- Error details if status=error
    RetryCount INTEGER DEFAULT 0           -- Number of sync retries
);

CREATE INDEX idx_sync_entity ON SyncMetadata(EntityType, EntityId);
CREATE INDEX idx_sync_status ON SyncMetadata(SyncStatus);
```

#### Settings Table

```sql
CREATE TABLE Settings (
    Key TEXT PRIMARY KEY,                  -- Setting key
    Value TEXT NOT NULL,                   -- Setting value (JSON or string)
    EncryptedValue BOOLEAN DEFAULT 0,      -- Is value encrypted?
    LastModified DATETIME NOT NULL,        -- Last modification
    Category TEXT                          -- Setting category
);

CREATE INDEX idx_settings_category ON Settings(Category);
```

---

## Caching Rules

### Cache Size Limits

| Resource | Limit | Enforcement |
|----------|-------|-------------|
| **Total Cache** | 10 GB (default) | LRU eviction when exceeded |
| **Per Guide** | 500 MB | Warning, no hard limit |
| **Per Media File** | 100 MB | Warning for large files |
| **Database** | 500 MB | Vacuum when exceeded |

### Expiration Rules

| Content Type | Expiration | Policy |
|-------------|------------|--------|
| **Guide Metadata** | Never | Always retained |
| **Guide Content** | 90 days | LRU if not accessed |
| **Media (in use)** | Never | Retained while guide exists |
| **Media (orphaned)** | 30 days | Purged if no references |
| **Progress Data** | Never | Always retained |
| **Temp Files** | 24 hours | Auto-cleanup on app start |
| **Logs** | 30 days | Rotated and purged |

### LRU Eviction Strategy

When total cache exceeds limit:

1. **Priority Order** (what to evict first):
   - Temp files (delete immediately)
   - Orphaned media (no guide references)
   - Least recently accessed media
   - Least recently accessed guide content (oldest first)
   - NEVER evict: Current guide, progress data, activation tokens

2. **Eviction Algorithm**:
```sql
-- Find least recently accessed media
SELECT MediaId, FileSize, LocalPath
FROM MediaCache
WHERE MediaId NOT IN (
    SELECT json_each.value
    FROM Steps, json_each(Steps.MediaReferences)
)
ORDER BY LastAccessed ASC
LIMIT 100;
```

3. **Safety Checks**:
   - Never evict if guide is currently open
   - Never evict if cached < 24 hours ago
   - Always keep at least 1 GB free space

### Cache Invalidation

**Triggers for Invalidation:**

1. **Version Mismatch**: Server version > local version
2. **Checksum Failure**: File corrupted or tampered
3. **Manual Refresh**: User requests re-download
4. **Sync Conflict**: Server changes detected
5. **Integrity Check Failure**: Periodic validation fails

**Invalidation Actions:**
- Mark as `SyncStatus = 'pending'` in database
- Delete local files
- Re-download on next access or sync

---

## Serialization Formats

### Guide JSON Schema (guide.json)

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["guideId", "title", "version", "steps"],
  "properties": {
    "guideId": {
      "type": "string",
      "description": "Unique guide identifier (GUID)"
    },
    "title": {
      "type": "string",
      "description": "Guide title"
    },
    "version": {
      "type": "string",
      "pattern": "^\\d+\\.\\d+\\.\\d+$",
      "description": "Semantic version (e.g., 1.0.0)"
    },
    "category": {
      "type": "string",
      "enum": ["Hardware", "Software", "Network", "Other"]
    },
    "description": {
      "type": "string",
      "description": "Guide description (plain text or markdown)"
    },
    "requiredLicense": {
      "type": "string",
      "enum": ["Tech", "Admin"]
    },
    "published": {
      "type": "boolean",
      "default": true
    },
    "lastModified": {
      "type": "string",
      "format": "date-time",
      "description": "ISO 8601 datetime (UTC)"
    },
    "metadata": {
      "type": "object",
      "properties": {
        "author": { "type": "string" },
        "estimatedDuration": { "type": "integer", "description": "Minutes" },
        "difficulty": { "type": "string", "enum": ["Beginner", "Intermediate", "Advanced"] },
        "tags": { "type": "array", "items": { "type": "string" } }
      }
    },
    "steps": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["stepId", "stepNumber", "title", "type"],
        "properties": {
          "stepId": { "type": "string" },
          "stepNumber": { "type": "integer", "minimum": 1 },
          "title": { "type": "string" },
          "content": { "type": "string", "description": "Markdown content" },
          "type": { "type": "string", "enum": ["Info", "Action", "Decision"] },
          "media": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["mediaId", "type"],
              "properties": {
                "mediaId": { "type": "string" },
                "type": { "type": "string", "enum": ["Image", "Video", "PDF", "Audio"] },
                "caption": { "type": "string" },
                "localPath": { "type": "string" },
                "sharePointUrl": { "type": "string" }
              }
            }
          },
          "checklist": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["id", "text"],
              "properties": {
                "id": { "type": "string" },
                "text": { "type": "string" },
                "required": { "type": "boolean", "default": false }
              }
            }
          }
        }
      }
    }
  }
}
```

**Example guide.json:**

```json
{
  "guideId": "g-2024-001",
  "title": "Windows 11 Installation",
  "version": "1.0.0",
  "category": "Software",
  "description": "Complete guide for installing Windows 11 on new hardware",
  "requiredLicense": "Tech",
  "published": true,
  "lastModified": "2025-01-15T10:30:00Z",
  "metadata": {
    "author": "John Doe",
    "estimatedDuration": 45,
    "difficulty": "Beginner",
    "tags": ["windows", "installation", "os"]
  },
  "steps": [
    {
      "stepId": "step-001",
      "stepNumber": 1,
      "title": "Prepare Installation Media",
      "content": "# Prepare Installation Media\n\n1. Download Windows 11 ISO\n2. Create bootable USB drive\n3. Verify checksum",
      "type": "Action",
      "media": [
        {
          "mediaId": "media-001",
          "type": "Image",
          "caption": "USB Boot Creation Tool",
          "localPath": "media/media-001.jpg",
          "sharePointUrl": "https://tenant.sharepoint.com/.../media-001.jpg"
        }
      ],
      "checklist": [
        {
          "id": "check-001",
          "text": "USB drive is at least 8GB",
          "required": true
        },
        {
          "id": "check-002",
          "text": "Backup important data",
          "required": true
        }
      ]
    }
  ]
}
```

### Progress JSON Schema (in database as TEXT)

```json
{
  "progressId": "p-12345",
  "guideId": "g-2024-001",
  "userId": "user-001",
  "currentStepId": "step-002",
  "stepProgress": {
    "step-001": {
      "status": "completed",
      "completedDate": "2025-01-15T11:00:00Z",
      "checklist": {
        "check-001": true,
        "check-002": true
      },
      "notes": "Completed without issues"
    },
    "step-002": {
      "status": "in_progress",
      "startedDate": "2025-01-15T11:15:00Z",
      "checklist": {
        "check-003": true,
        "check-004": false
      }
    }
  },
  "startedDate": "2025-01-15T10:45:00Z",
  "lastUpdated": "2025-01-15T11:20:00Z",
  "completedDate": null,
  "percentComplete": 50,
  "notes": "Installation in progress"
}
```

---

## Error Handling

### Corrupted File Detection

**Checksum Verification:**
```csharp
// On read
var content = await File.ReadAllTextAsync(filePath);
var checksum = ComputeSHA256(content);

if (checksum != expectedChecksum)
{
    // File corrupted - attempt recovery
    await HandleCorruptedFile(filePath, entityId);
}
```

**Recovery Strategies:**

| Scenario | Action |
|----------|--------|
| **Corrupted guide.json** | 1. Try to load from backup<br>2. Mark for re-download<br>3. Log error, notify user |
| **Corrupted step file** | 1. Re-download from server<br>2. Fallback to guide-level content |
| **Corrupted media** | 1. Delete file<br>2. Mark for re-download<br>3. Show placeholder in UI |
| **Corrupted database** | 1. Restore from backup<br>2. Rebuild from cache files<br>3. Last resort: fresh install |
| **Corrupted activation.dat** | 1. Prompt re-activation<br>2. No automatic recovery (security) |

### Database Corruption Recovery

```csharp
// SQLite integrity check
PRAGMA integrity_check;

// If failed:
// 1. Create backup of corrupted database
// 2. Export data from corrupted database
// 3. Create new database
// 4. Import data
// 5. Re-download missing guides
```

### Graceful Degradation

```
Application Startup
    |
    v
Check Database Integrity
    |
    +-- Corrupt? --> Attempt Recovery --> Success? --> Continue
    |                                         |
    |                                         No
    |                                         |
    OK                                        v
    |                                    Offline Mode
    |                                    (Limited functionality)
    v
Load Cached Guides
    |
    +-- Missing/Corrupt? --> Mark for re-download
    |
    v
Application Ready
```

---

## Performance Optimizations

### Database Indexing

- Primary keys on all ID fields
- Indexes on foreign keys
- Composite indexes for common queries
- Covered indexes for read-heavy queries

### File System Optimization

- **Avoid Deep Nesting**: Max 3 levels deep
- **Batch Operations**: Group file I/O operations
- **Async I/O**: All file operations use async/await
- **Stream Large Files**: Don't load entire files into memory

### Caching Strategy

```
L1: Memory Cache (Hot data, <10 MB)
    ↓ miss
L2: SQLite Database (Metadata, indexed)
    ↓ miss
L3: File System (Guide JSON, media)
    ↓ miss
L4: SharePoint (Remote, download)
```

---

## Backup Strategy

### Automatic Backups

| What | When | Retention |
|------|------|-----------|
| **Database** | Daily (if changed) | Keep last 7 |
| **Activation Token** | On change | Keep last 1 |
| **Settings** | On change | Keep last 3 |

### Manual Backups

User can trigger:
- Export all guides (ZIP archive)
- Export progress data (JSON)
- Export settings (JSON)

---

## Security Considerations

### File Permissions

- Config folder: User read/write only
- Database: User read/write only
- Cache: User read/write only
- Logs: User read/write only

### Sensitive Data

- **Activation Token**: DPAPI encrypted
- **Settings**: Plain JSON (no secrets)
- **Database**: Not encrypted (non-sensitive data)
- **Guides**: Plain JSON (public content)

### Integrity

- SHA256 checksums for all cached files
- Foreign key constraints in database
- Transaction-based updates
- Periodic integrity checks

---

## API Surface

### CacheService

```csharp
interface ICacheService
{
    Task<bool> IsCachedAsync(string entityId);
    Task<string> GetCachePathAsync(string entityId);
    Task CacheFileAsync(string entityId, byte[] data, string checksum);
    Task<byte[]> ReadCachedFileAsync(string entityId);
    Task InvalidateCacheAsync(string entityId);
    Task<CacheStatistics> GetCacheStatisticsAsync();
    Task CleanupCacheAsync();
}
```

### LocalDataService

```csharp
interface ILocalDataService
{
    Task InitializeAsync();
    Task<Guide?> GetGuideAsync(string guideId);
    Task SaveGuideAsync(Guide guide);
    Task<List<Guide>> GetAllGuidesAsync();
    Task DeleteGuideAsync(string guideId);

    Task<GuideProgress?> GetProgressAsync(string guideId, string userId);
    Task SaveProgressAsync(GuideProgress progress);

    Task<bool> VerifyIntegrityAsync();
    Task<BackupResult> CreateBackupAsync();
    Task<RestoreResult> RestoreBackupAsync(string backupPath);
}
```

---

## Future Enhancements

1. **Compression**: Compress guide JSON files (gzip)
2. **Encryption**: Optional encryption for sensitive guides
3. **Deduplication**: Deduplicate shared media files
4. **Delta Sync**: Only download changes, not full files
5. **P2P Sync**: Peer-to-peer sync between devices
6. **Cloud Backup**: Optional cloud backup to OneDrive/SharePoint
7. **Offline Search**: Full-text search index for guides

---

**Document Version**: 1.0
**Last Updated**: 2025-01-15
**Author**: InstallVibe Development Team
