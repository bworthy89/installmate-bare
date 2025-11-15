# SharePoint Integration System

## Overview

InstallVibe uses SharePoint Online as the central content repository for installation guides, media assets, and optional product key validation. The integration uses Microsoft Graph API with **app-only authentication** using client certificates for secure, unattended access.

## Authentication Method

### App-Only Authentication with Client Certificate

**Why Client Certificate Authentication?**
- More secure than client secrets (no password stored in configuration)
- Supports certificate rotation without code changes
- Required for production enterprise applications
- FIPS 140-2 compliant when using appropriate certificates

**Certificate Requirements:**
- X.509 certificate with private key (.pfx format)
- 2048-bit RSA or 256-bit ECC recommended
- Valid for at least 1 year
- Stored securely in Windows Certificate Store (LocalMachine/My)

**Azure AD App Registration:**
```
Application (client) ID: {your-client-id}
Directory (tenant) ID: {your-tenant-id}
Certificate Thumbprint: {certificate-thumbprint}

Required API Permissions (Application):
- Sites.Read.All - Read items in all site collections
- Sites.ReadWrite.All - Read and write items in all site collections (Admin only)
- Files.Read.All - Read files in all site collections
- Files.ReadWrite.All - Read and write files (Admin only)
```

**Configuration:**
```json
{
  "SharePoint": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CertificateThumbprint": "certificate-thumbprint",
    "SiteUrl": "https://yourtenant.sharepoint.com/sites/InstallVibe",
    "GuideLibrary": "Guides",
    "MediaLibrary": "Media",
    "GuideIndexList": "GuideIndex"
  }
}
```

## SharePoint Site Structure

### Site Hierarchy

```
InstallVibe (Team Site)
│
├── Lists
│   ├── GuideIndex (Custom List)
│   └── ProductKeys (Optional - Custom List)
│
└── Document Libraries
    ├── Guides
    │   ├── {guideId-1}
    │   │   ├── guide.json
    │   │   └── metadata.json
    │   ├── {guideId-2}
    │   │   ├── guide.json
    │   │   └── metadata.json
    │   └── ...
    │
    └── Media
        ├── Images
        │   ├── {mediaId-1}.png
        │   ├── {mediaId-2}.jpg
        │   └── ...
        ├── Videos
        │   ├── {mediaId-1}.mp4
        │   └── ...
        └── Documents
            ├── {mediaId-1}.pdf
            └── ...
```

## GuideIndex List

### Purpose
Central index of all available guides with metadata for filtering, search, and sync status tracking.

### Column Definitions

| Column Name | Type | Required | Indexed | Description |
|------------|------|----------|---------|-------------|
| Title | Single line of text | Yes | Yes | Guide display title |
| GuideId | Single line of text | Yes | Yes | Unique identifier (GUID) |
| Version | Single line of text | Yes | Yes | Semantic version (e.g., "1.2.3") |
| Category | Choice | Yes | Yes | Software, Hardware, Network, Cloud, Other |
| Description | Multiple lines of text | No | No | Brief description (max 500 chars) |
| RequiredLicense | Choice | Yes | Yes | Tech, Admin |
| Published | Yes/No | Yes | Yes | Visibility flag |
| LastModified | Date and Time | Yes | Yes | Last update timestamp |
| Author | Person or Group | Yes | No | Guide author |
| ApprovedBy | Person or Group | No | No | Approval authority (Admin guides) |
| StepCount | Number | Yes | No | Total number of steps |
| EstimatedMinutes | Number | No | No | Estimated completion time |
| Tags | Multiple lines of text | No | No | Comma-separated tags for search |
| Checksum | Single line of text | Yes | No | SHA256 hash of guide.json |
| FileSize | Number | Yes | No | Size of guide.json in bytes |
| FolderPath | Single line of text | Yes | No | Relative path in Guides library |
| MediaCount | Number | Yes | No | Number of media references |
| SyncPriority | Choice | No | Yes | Critical, High, Normal, Low |
| MinClientVersion | Single line of text | No | No | Minimum InstallVibe version required |

### List Settings
- Enable versioning (major versions only)
- Enable content approval (for Admin-level guides)
- Enable search indexing
- Default view: Filter by Published=Yes, Sort by Category then Title

## Document Libraries

### Guides Library

**Purpose:** Stores guide.json files organized by GuideId folders.

**Library Settings:**
- Versioning: Enabled (major and minor versions, keep last 10 versions)
- Content approval: Enabled for Admin guides
- Check-out required: Yes (prevents simultaneous edits)
- Search: Enabled

**Folder Structure:**
```
/Guides
  /{guideId}/
    guide.json       - Main guide content
    metadata.json    - Additional metadata (optional)
```

**guide.json Schema:**
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
      "content": "Download the Windows Server 2022 ISO...",
      "mediaReferences": [
        {
          "mediaId": "media-12345",
          "mediaType": "image",
          "caption": "Download page screenshot",
          "orderIndex": 1
        }
      ],
      "notes": "Ensure you have a valid license key",
      "warningLevel": "info"
    }
  ],
  "metadata": {
    "prerequisites": ["8GB RAM", "64-bit processor"],
    "relatedGuides": ["guide-002", "guide-003"],
    "changeLog": [
      {
        "version": "1.2.3",
        "date": "2025-01-15",
        "changes": "Updated screenshots for latest installer"
      }
    ]
  }
}
```

### Media Library

**Purpose:** Stores all media assets (images, videos, PDFs) referenced by guides.

**Library Settings:**
- Versioning: Enabled (major versions only, keep last 5 versions)
- Content approval: Disabled (approved with parent guide)
- Max file size: 100 MB per file
- Allowed types: .png, .jpg, .jpeg, .gif, .mp4, .webm, .pdf

**Folder Structure:**
```
/Media
  /Images/{mediaId}.{ext}
  /Videos/{mediaId}.{ext}
  /Documents/{mediaId}.{ext}
```

**Custom Columns:**
| Column Name | Type | Description |
|------------|------|-------------|
| MediaId | Single line of text | Unique identifier (GUID) |
| MediaType | Choice | Image, Video, Document |
| FileFormat | Single line of text | png, jpg, mp4, pdf, etc. |
| FileSizeBytes | Number | File size in bytes |
| Checksum | Single line of text | SHA256 hash |
| ReferencedByGuides | Multiple lines of text | Comma-separated list of GuideIds |
| UploadedBy | Person or Group | Uploader |
| UploadDate | Date and Time | Upload timestamp |

## Product Keys List (Optional)

**Purpose:** Optional server-side product key validation and customer tracking.

**Column Definitions:**
| Column Name | Type | Required | Indexed | Description |
|------------|------|----------|---------|-------------|
| Title | Single line of text | Yes | Yes | Product key (masked: XXXXX-...-XXXXX) |
| ProductKeyHash | Single line of text | Yes | Yes | SHA256 hash of full key |
| LicenseType | Choice | Yes | Yes | Tech, Admin |
| CustomerId | Single line of text | Yes | Yes | Customer identifier |
| ExpirationDate | Date and Time | No | Yes | Null = perpetual |
| IssuedDate | Date and Time | Yes | No | Issue timestamp |
| IsRevoked | Yes/No | Yes | Yes | Revocation flag |
| RevokedDate | Date and Time | No | No | Revocation timestamp |
| ActivationCount | Number | Yes | No | Number of activations |
| MaxActivations | Number | Yes | No | Maximum allowed activations |
| LastActivatedDate | Date and Time | No | No | Last activation timestamp |
| Notes | Multiple lines of text | No | No | Admin notes |

**Security:**
- Read permissions: App-only (no user read access)
- Write permissions: Designated administrators only
- Full product keys are NEVER stored (only hashes)
- Audit log enabled for all changes

## Microsoft Graph API Endpoints

### 1. Fetch Guide Index

**Endpoint:**
```
GET https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items
    ?$expand=fields
    &$filter=fields/Published eq true
    &$orderby=fields/Category,fields/Title
    &$select=fields
```

**Response Model:**
```json
{
  "value": [
    {
      "fields": {
        "Title": "Install Windows Server 2022",
        "GuideId": "550e8400-e29b-41d4-a716-446655440000",
        "Version": "1.2.3",
        "Category": "Software",
        "RequiredLicense": "Tech",
        "Published": true,
        "LastModified": "2025-01-15T10:30:00Z",
        "Checksum": "abc123...",
        "FileSize": 45678
      }
    }
  ]
}
```

### 2. Download guide.json

**Endpoint:**
```
GET https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:
    /Guides/{guideId}/guide.json:/content
```

**Response:** Binary stream (JSON content)

**Alternative (get metadata first):**
```
GET https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:
    /Guides/{guideId}/guide.json

Response includes:
- @microsoft.graph.downloadUrl (direct download URL, valid 1 hour)
- size (file size)
- file.hashes.sha256Hash (checksum)
```

### 3. Sync Updated Guides (Admin Only)

**Step 1: Get updated guides since last sync**
```
GET https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items
    ?$expand=fields
    &$filter=fields/Published eq true and fields/LastModified gt {lastSyncDate}
    &$select=fields
```

**Step 2: Download each updated guide.json**
```
GET https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:
    /Guides/{guideId}/guide.json:/content
```

**Step 3: Update local cache and database**
- Verify checksum matches GuideIndex
- Cache guide.json locally
- Update GuideEntity in database
- Mark sync status as "synced"

### 4. Upload Media (Admin Only)

**Step 1: Create folder if needed**
```
PUT https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:
    /Media/{subfolder}:/children
```

**Step 2: Upload file (small files < 4MB)**
```
PUT https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:
    /Media/{subfolder}/{mediaId}.{ext}:/content

Headers:
  Content-Type: application/octet-stream

Body: Binary file content
```

**Step 3: Upload large file (>= 4MB) - Resumable Upload Session**
```
POST https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/root:
    /Media/{subfolder}/{mediaId}.{ext}:/createUploadSession

Response:
{
  "uploadUrl": "https://...",
  "expirationDateTime": "2025-01-15T12:00:00Z"
}

Then PUT chunks to uploadUrl with Content-Range headers
```

**Step 4: Update Media Library metadata**
```
PATCH https://graph.microsoft.com/v1.0/sites/{siteId}/drives/{driveId}/items/{itemId}

Body:
{
  "fields": {
    "MediaId": "media-12345",
    "MediaType": "Image",
    "Checksum": "sha256hash..."
  }
}
```

### 5. Validate Product Key (Optional)

**Endpoint:**
```
POST https://graph.microsoft.com/v1.0/sites/{siteId}/lists/{listId}/items
    ?$filter=fields/ProductKeyHash eq '{keyHash}' and fields/IsRevoked eq false

Response:
{
  "value": [
    {
      "fields": {
        "LicenseType": "Admin",
        "ExpirationDate": "2026-12-31",
        "MaxActivations": 5,
        "ActivationCount": 2
      }
    }
  ]
}
```

**Validation Logic:**
1. Compute SHA256 hash of product key
2. Query ProductKeys list by ProductKeyHash
3. Check if key exists and IsRevoked = false
4. Check if ActivationCount < MaxActivations
5. Check if ExpirationDate is null or in future
6. Increment ActivationCount if validation passes
7. Update LastActivatedDate

**Fallback:** If SharePoint is unreachable, use offline RSA validation (already implemented).

## C# Implementation Architecture

### Models

**GuideIndexEntry.cs**
```csharp
public class GuideIndexEntry
{
    public string GuideId { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public LicenseType RequiredLicense { get; set; }
    public bool Published { get; set; }
    public DateTime LastModified { get; set; }
    public string? Author { get; set; }
    public int StepCount { get; set; }
    public int? EstimatedMinutes { get; set; }
    public List<string> Tags { get; set; }
    public string Checksum { get; set; }
    public long FileSize { get; set; }
    public string FolderPath { get; set; }
    public int MediaCount { get; set; }
}
```

**SharePointMedia.cs**
```csharp
public class SharePointMedia
{
    public string MediaId { get; set; }
    public MediaType MediaType { get; set; }
    public string FileFormat { get; set; }
    public long FileSizeBytes { get; set; }
    public string Checksum { get; set; }
    public List<string> ReferencedByGuides { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime UploadDate { get; set; }
    public string DownloadUrl { get; set; }
}

public enum MediaType
{
    Image,
    Video,
    Document
}
```

### Services

**ISharePointService.cs**
```csharp
public interface ISharePointService
{
    // Guide Index
    Task<List<GuideIndexEntry>> GetGuideIndexAsync(LicenseType? filterByLicense = null);
    Task<GuideIndexEntry?> GetGuideMetadataAsync(string guideId);

    // Guide Content
    Task<byte[]> DownloadGuideJsonAsync(string guideId);
    Task<Guide?> GetGuideAsync(string guideId);

    // Sync (Admin only)
    Task<SyncResult> SyncUpdatedGuidesAsync(DateTime? since = null, IProgress<SyncProgress>? progress = null);
    Task<bool> UploadGuideAsync(Guide guide);

    // Media
    Task<SharePointMedia?> GetMediaMetadataAsync(string mediaId);
    Task<byte[]> DownloadMediaAsync(string mediaId);
    Task<string> UploadMediaAsync(string mediaId, Stream content, MediaType mediaType, string fileExtension);

    // Product Key Validation (Optional)
    Task<ProductKeyValidationResult> ValidateProductKeyOnlineAsync(string productKey);

    // Health & Status
    Task<bool> IsOnlineAsync();
    Task<SharePointHealthStatus> GetHealthStatusAsync();
}
```

**SharePointService.cs**
- Implements all Graph API calls
- Handles authentication via GraphClientFactory
- Graceful offline fallback (returns cached data)
- Retry logic with exponential backoff
- Comprehensive error handling and logging

### Configuration

**SharePointConfiguration.cs**
```csharp
public class SharePointConfiguration
{
    public string TenantId { get; set; }
    public string ClientId { get; set; }
    public string CertificateThumbprint { get; set; }
    public string SiteUrl { get; set; }
    public string GuideLibrary { get; set; } = "Guides";
    public string MediaLibrary { get; set; } = "Media";
    public string GuideIndexList { get; set; } = "GuideIndex";
    public string? ProductKeysListId { get; set; }
    public int RetryCount { get; set; } = 3;
    public int RetryDelayMs { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 30;
}
```

### Authentication

**GraphClientFactory.cs**
- Loads certificate from Windows Certificate Store by thumbprint
- Creates GraphServiceClient with ClientCertificateCredential
- Handles certificate validation and errors
- Caches authenticated client for reuse

## Offline Mode Handling

### Graceful Degradation Strategy

1. **Network Detection:**
   - Quick connectivity check (ping Graph API endpoint)
   - Cache last known online status
   - Timeout after 5 seconds

2. **Fallback Behavior:**
   - `GetGuideIndexAsync()` → Return cached guide list from local database
   - `DownloadGuideJsonAsync()` → Return cached guide.json from file system
   - `GetGuideAsync()` → Use local GuideService (already implemented)
   - `SyncUpdatedGuidesAsync()` → Queue sync request for later, return cached status
   - `UploadMediaAsync()` → Queue upload for later, return pending status

3. **Sync Queue:**
   - Pending uploads/downloads tracked in SyncMetadataEntity
   - Automatic retry when connection restored
   - Background sync service polls every 15 minutes

4. **User Experience:**
   - All read operations work offline (from cache)
   - Write operations (Admin only) queued automatically
   - Sync status visible in UI (not implemented yet)

### Error Handling

**Network Errors:**
- `HttpRequestException` → Mark as offline, fallback to cache
- `TaskCanceledException` (timeout) → Retry once, then fallback
- `ServiceException` (Graph API) → Log error, fallback based on status code

**Authentication Errors:**
- Certificate not found → Log critical error, disable SharePoint features
- Certificate expired → Log warning, prompt for renewal
- Insufficient permissions → Log error, disable write operations

**Data Errors:**
- Checksum mismatch → Re-download file, mark as corrupted if retry fails
- JSON deserialization error → Log error, fallback to metadata only
- Missing file → Mark for download, return cached version if available

## Security Considerations

### Certificate Management
- Store certificate in Windows Certificate Store (LocalMachine\My)
- Use private key with "Allow Export" disabled
- Set appropriate ACLs (only SYSTEM and Administrators)
- Monitor expiration (alert 30 days before)

### API Permissions
- Use least privilege (ReadWrite.All only for Admin users)
- Tech users: Read-only API calls
- Admin users: Full CRUD operations
- Audit all write operations

### Data Protection
- All downloads verified with SHA256 checksums
- Product keys never stored in full (hashes only)
- No PII in logs
- Encrypt cached credentials with DPAPI

### Network Security
- All traffic over HTTPS (TLS 1.2+)
- Certificate pinning for Graph API
- Validate SSL/TLS certificates
- No insecure fallbacks

## Performance Optimization

### Caching Strategy
- Cache guide index for 15 minutes (sliding expiration)
- Cache individual guides until LastModified changes
- Cache media metadata for 1 hour
- Invalidate cache on explicit sync

### Batch Operations
- Download multiple guides in parallel (max 5 concurrent)
- Use delta queries for sync (only changed items)
- Compress large payloads (guide.json > 1MB)

### Resumable Uploads
- Use upload sessions for files > 4MB
- 10MB chunks for optimal performance
- Retry failed chunks automatically
- Store session state for app restart

## Testing Recommendations

### Unit Tests
- Mock GraphServiceClient for all tests
- Test offline mode fallback logic
- Test certificate loading errors
- Test checksum verification

### Integration Tests
- Test against SharePoint sandbox environment
- Verify all Graph API endpoints
- Test large file uploads (resumable sessions)
- Test concurrent download limits

### End-to-End Tests
- Full sync workflow (download all guides)
- Upload new guide with media
- Product key validation flow
- Network interruption recovery

## Deployment Checklist

- [ ] Create Azure AD App Registration
- [ ] Generate and upload client certificate
- [ ] Grant API permissions and admin consent
- [ ] Create SharePoint site and lists
- [ ] Configure list columns and views
- [ ] Set list permissions (app-only access)
- [ ] Upload sample guides for testing
- [ ] Install certificate on client machines
- [ ] Configure appsettings.json with tenant/client IDs
- [ ] Test connectivity from client app
- [ ] Monitor audit logs for errors

## Future Enhancements

1. **Delta Sync:** Use Microsoft Graph delta queries to sync only changes
2. **Webhooks:** Subscribe to SharePoint list changes for real-time updates
3. **Search:** Implement full-text search using Graph Search API
4. **Analytics:** Track guide usage and popularity
5. **Multi-language:** Support localized guide content
6. **Approval Workflow:** Implement custom approval process for guide publishing
