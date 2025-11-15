# InstallVibe Architecture Document

## Executive Summary

InstallVibe is a Windows-first desktop application built on WinUI 3 (.NET 8) that provides technicians with guided installation workflows. The application operates in an offline-first model with SharePoint Online serving as the central repository for content, media, and application updates. Product key-based activation controls feature access, validated through offline RSA signature verification with optional online validation.

---

## 1. Application Layer Overview

### 1.1 Presentation Layer (WinUI 3)
- **UI Framework**: WinUI 3 with XAML
- **Design System**: Windows 11 Fluent Design principles
- **Navigation**: Frame-based navigation with NavigationView
- **State Management**: MVVM pattern with CommunityToolkit.Mvvm
- **Theming**: Light/Dark mode support, accent color customization

### 1.2 Business Logic Layer
- **Services**: Dependency-injected service layer for core functionality
- **Domain Models**: Strongly-typed models for guides, media, progress
- **Validation**: Product key validation, input validation
- **Orchestration**: Workflow coordination between services

### 1.3 Data Access Layer
- **Local Storage**: SQLite for structured data (progress, cache metadata)
- **File Storage**: Local file system for cached media and guide content
- **Remote Access**: SharePoint REST API client
- **Sync Engine**: Intelligent sync between local and remote data

### 1.4 Infrastructure Layer
- **Logging**: Structured logging with Serilog
- **Configuration**: Encrypted app settings and user preferences
- **Security**: Credential management, encryption, signature verification
- **Updates**: MSIX AppInstaller update check and installation

### Layer Diagram (Text)
```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Views      │  │  ViewModels  │  │   Controls   │      │
│  │   (XAML)     │  │   (MVVM)     │  │   (Custom)   │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                   BUSINESS LOGIC LAYER                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  Activation  │  │    Guide     │  │   Progress   │      │
│  │   Service    │  │   Service    │  │   Service    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │  SharePoint  │  │    Cache     │  │    Update    │      │
│  │   Service    │  │   Service    │  │   Service    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                    DATA ACCESS LAYER                         │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   SQLite     │  │  File System │  │  SharePoint  │      │
│  │  Repository  │  │     Cache    │  │     API      │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            │
┌─────────────────────────────────────────────────────────────┐
│                   INFRASTRUCTURE LAYER                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Logging    │  │  Encryption  │  │    Config    │      │
│  │  (Serilog)   │  │   (DPAPI)    │  │   Manager    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
```

---

## 2. Product Key Validation Architecture

### 2.1 Overview
The product key system supports two activation tiers:
- **Admin Keys**: Full access including content editor tools
- **Tech Keys**: Read-only access to guides and workflows

### 2.2 Key Format
```
Format: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX (25 characters, 5 groups)

Structure:
- Group 1-3: Base58-encoded payload (15 chars)
- Group 4-5: Base58-encoded RSA signature (10 chars)

Payload contains:
- License type (1 byte): 0x01=Tech, 0x02=Admin
- Expiration timestamp (4 bytes): Unix epoch or 0xFFFFFFFF for perpetual
- Customer ID (4 bytes): Unique identifier
- Version flags (1 byte): Feature toggles
- Checksum (2 bytes): CRC16 of above
```

### 2.3 Validation Flow

#### Offline Validation (Primary)
1. Parse product key into payload + signature
2. Verify signature using embedded RSA public key (2048-bit)
3. Validate checksum
4. Check expiration date
5. Extract license type and features
6. Generate activation token (signed, timestamped)
7. Store activation token locally (encrypted)

#### Online Validation (Optional Fallback)
1. If offline validation fails OR user chooses to validate online
2. Send hashed key to SharePoint validation endpoint
3. SharePoint checks against key database list
4. Returns: valid/invalid, license type, expiration, customer info
5. Store result and sync with local token

### 2.4 Security Measures
- **Private key isolation**: Never embedded in app, used offline to generate keys
- **Public key embedding**: RSA public key compiled into application
- **Token encryption**: Activation tokens encrypted with DPAPI
- **Signature verification**: All keys must have valid RSA signature
- **Anti-tampering**: Token includes machine ID binding
- **Revocation support**: Online check can invalidate previously valid keys

### 2.5 Activation Token Storage
```
Location: %LOCALAPPDATA%\InstallVibe\activation.dat

Structure (encrypted):
{
  "ProductKey": "XXXXX-XXXXX-...",  // Hash only, not plaintext
  "LicenseType": "Admin|Tech",
  "ExpirationDate": "2025-12-31T23:59:59Z",
  "CustomerId": "12345678",
  "Features": ["Editor", "AdvancedReporting"],
  "MachineId": "HASH-OF-HARDWARE-ID",
  "ValidatedDate": "2025-01-15T10:30:00Z",
  "OnlineValidation": true,
  "Signature": "RSA_SIGNATURE_OF_ABOVE"
}
```

### 2.6 Component Diagram
```
┌─────────────────────────────────────────────────────────────┐
│                  PRODUCT KEY VALIDATION                      │
│                                                               │
│  User Input                                                   │
│      │                                                        │
│      ▼                                                        │
│  ┌────────────────┐                                          │
│  │  Key Parser    │                                          │
│  └────────┬───────┘                                          │
│           │                                                   │
│           ▼                                                   │
│  ┌────────────────┐        ┌──────────────────┐            │
│  │ Offline RSA    │───────▶│  Public Key      │            │
│  │  Validator     │        │  (Embedded)      │            │
│  └────────┬───────┘        └──────────────────┘            │
│           │                                                   │
│           │ Valid? ──No──▶ ┌──────────────────┐            │
│           │                 │ Online Validator │            │
│           │                 │  (SharePoint)    │            │
│           │                 └────────┬─────────┘            │
│           │                          │                       │
│           └──────────Yes─────────────┘                       │
│                      │                                        │
│                      ▼                                        │
│           ┌──────────────────┐                               │
│           │ Token Generator  │                               │
│           └────────┬─────────┘                               │
│                    │                                          │
│                    ▼                                          │
│           ┌──────────────────┐                               │
│           │  Encrypted Store │                               │
│           │     (DPAPI)      │                               │
│           └──────────────────┘                               │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. SharePoint Integration Architecture

### 3.1 SharePoint Structure

```
SharePoint Site: https://{tenant}.sharepoint.com/sites/InstallVibe

Document Libraries:
├── Guides/
│   ├── Metadata (List columns: Title, Version, Category, LastModified)
│   ├── {GuideId}/
│   │   ├── guide.json          (Guide definition)
│   │   ├── steps/
│   │   │   ├── step1.json
│   │   │   ├── step2.json
│   │   └── media/
│   │       ├── image1.jpg
│   │       ├── video1.mp4
│
├── Media/
│   ├── Shared media library
│   ├── Organized by type (images/, videos/, pdfs/)
│
├── AppUpdates/
│   ├── InstallVibe.msix        (Latest MSIX package)
│   ├── InstallVibe.appinstaller (Update manifest)
│   ├── versions.json            (Version history)
│
└── ProductKeys/               (Optional)
    └── keys.json              (Valid keys database, hashed)
```

### 3.2 SharePoint Lists

**Guides List**
- Title (Text)
- GuideId (Text, Unique)
- Version (Text)
- Category (Choice)
- Description (Multiline)
- RequiredLicense (Choice: Tech, Admin)
- Published (Yes/No)
- LastModified (DateTime)
- ContentPath (Text) - Path to guide.json

**Product Keys List** (Optional)
- KeyHash (Text, Indexed)
- LicenseType (Choice: Tech, Admin)
- CustomerId (Number)
- ExpirationDate (DateTime)
- IsActive (Yes/No)
- ActivationCount (Number)
- MaxActivations (Number)

### 3.3 API Integration Strategy

**Authentication**:
- Azure AD App Registration with delegated permissions
- OAuth 2.0 with refresh tokens
- Offline credential caching (encrypted)

**API Access Pattern**:
1. **Initial Authentication**: User signs in with Microsoft account
2. **Token Caching**: Store access/refresh tokens encrypted locally
3. **Automatic Refresh**: Silent token refresh before expiration
4. **Offline Mode**: Continue with cached data when no connectivity

**REST API Endpoints**:
```
GET /sites/{site}/lists/Guides/items
GET /sites/{site}/drives/{drive}/items/{item}/content
POST /sites/{site}/lists/ProductKeys/items
GET /sites/{site}/drives/{drive}/root:/AppUpdates/versions.json
```

### 3.4 Data Synchronization Strategy

**Download Sync** (SharePoint → Local):
1. Check network connectivity
2. Compare local cache metadata with SharePoint versions
3. Download changed/new guides and media
4. Update local SQLite cache metadata
5. Verify integrity (checksums)

**Upload Sync** (Local → SharePoint) - Admin only:
1. User creates/edits guide in editor
2. Save to local draft storage
3. On publish: Upload guide.json and media to SharePoint
4. Update Guides list metadata
5. Increment version number
6. Mark as published

**Conflict Resolution**:
- Server-wins for Tech users (read-only)
- Admin users: Warn on conflict, allow merge or overwrite
- Version tracking prevents data loss

### 3.5 Connection Resilience
```
┌──────────────────────────────────────────────────────────┐
│              SharePoint Connection Manager               │
│                                                            │
│  ┌─────────────┐     ┌──────────────┐                   │
│  │ Health Check│────▶│ Online Mode  │                   │
│  │   Timer     │     │   (Active)   │                   │
│  └─────────────┘     └──────┬───────┘                   │
│         │                    │                            │
│         │ No Connectivity    │ Connectivity OK           │
│         ▼                    ▼                            │
│  ┌─────────────┐     ┌──────────────┐                   │
│  │ Offline Mode│     │ Sync Queue   │                   │
│  │  (Cached)   │     │   Manager    │                   │
│  └─────────────┘     └──────────────┘                   │
│         │                    │                            │
│         └────────────────────┘                            │
│                  │                                         │
│                  ▼                                         │
│         ┌────────────────┐                                │
│         │  Local Cache   │                                │
│         │    (SQLite)    │                                │
│         └────────────────┘                                │
└──────────────────────────────────────────────────────────┘
```

---

## 4. Offline Caching Architecture

### 4.1 Cache Strategy

**Three-Tier Cache**:
1. **Memory Cache**: Frequently accessed data (current guide, recent steps)
2. **File System Cache**: Downloaded media, guide JSON files
3. **Database Cache**: Metadata, progress, indexes

### 4.2 SQLite Database Schema

```sql
-- Guides metadata
CREATE TABLE Guides (
    GuideId TEXT PRIMARY KEY,
    Title TEXT NOT NULL,
    Version TEXT NOT NULL,
    Category TEXT,
    Description TEXT,
    RequiredLicense TEXT CHECK(RequiredLicense IN ('Tech', 'Admin')),
    Published BOOLEAN,
    LastModified DATETIME,
    LocalPath TEXT,
    SharePointPath TEXT,
    CachedDate DATETIME,
    Checksum TEXT
);

-- Guide steps
CREATE TABLE Steps (
    StepId TEXT PRIMARY KEY,
    GuideId TEXT NOT NULL,
    StepNumber INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Content TEXT,
    MediaReferences TEXT, -- JSON array of media IDs
    FOREIGN KEY (GuideId) REFERENCES Guides(GuideId)
);

-- Media cache metadata
CREATE TABLE MediaCache (
    MediaId TEXT PRIMARY KEY,
    FileName TEXT NOT NULL,
    FileType TEXT NOT NULL,
    LocalPath TEXT NOT NULL,
    SharePointPath TEXT,
    FileSize INTEGER,
    Checksum TEXT,
    CachedDate DATETIME,
    LastAccessed DATETIME
);

-- User progress tracking
CREATE TABLE Progress (
    ProgressId TEXT PRIMARY KEY,
    GuideId TEXT NOT NULL,
    UserId TEXT NOT NULL, -- Machine ID or user identifier
    CurrentStepId TEXT,
    StepProgress TEXT, -- JSON: { "step1": "completed", "step2": "in_progress" }
    StartedDate DATETIME,
    LastUpdated DATETIME,
    CompletedDate DATETIME,
    Notes TEXT,
    FOREIGN KEY (GuideId) REFERENCES Guides(GuideId)
);

-- Sync metadata
CREATE TABLE SyncMetadata (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    EntityType TEXT NOT NULL, -- 'Guide', 'Media', etc.
    EntityId TEXT NOT NULL,
    LastSyncDate DATETIME,
    ServerVersion TEXT,
    LocalVersion TEXT,
    SyncStatus TEXT CHECK(SyncStatus IN ('synced', 'pending', 'conflict', 'error'))
);

-- Settings and configuration
CREATE TABLE Settings (
    Key TEXT PRIMARY KEY,
    Value TEXT NOT NULL,
    EncryptedValue BOOLEAN DEFAULT 0,
    LastModified DATETIME
);
```

### 4.3 File System Cache Structure

```
%LOCALAPPDATA%\InstallVibe\
├── Cache\
│   ├── Guides\
│   │   ├── {GuideId}\
│   │   │   ├── guide.json
│   │   │   ├── steps\
│   │   │   │   ├── {StepId}.json
│   │   │   └── media\
│   │   │       ├── {MediaId}.jpg
│   │   │       ├── {MediaId}.mp4
│   │
│   ├── Media\
│   │   ├── Images\
│   │   ├── Videos\
│   │   └── Documents\
│   │
│   └── Temp\
│       └── (temporary downloads)
│
├── Data\
│   └── installvibe.db  (SQLite database)
│
├── Logs\
│   ├── app-{date}.log
│   └── errors-{date}.log
│
└── Config\
    ├── activation.dat  (encrypted activation token)
    └── settings.json   (user preferences)
```

### 4.4 Cache Management Policies

**Storage Limits**:
- Maximum cache size: 10 GB (configurable)
- Warn at 8 GB
- Auto-cleanup of least-recently-used media when limit reached

**Retention Policies**:
- Keep current guide and all its media indefinitely
- Keep recently accessed guides (last 30 days)
- Purge unused media after 90 days
- Always retain guide metadata (small footprint)

**Integrity Checks**:
- SHA256 checksums for all cached files
- Periodic verification on app startup
- Re-download corrupted files automatically

### 4.5 Cache Synchronization Flow

```
App Startup
    │
    ▼
┌─────────────────┐
│ Check Network   │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
 Offline    Online
    │         │
    ▼         ▼
┌─────┐  ┌──────────────┐
│Load │  │ Sync Check   │
│Cache│  │ (Versions)   │
└─────┘  └──────┬───────┘
    │           │
    │      ┌────┴────┐
    │      │         │
    │   No Δ    Updates?
    │      │         │
    │      ▼         ▼
    │   ┌─────┐  ┌──────────┐
    │   │Load │  │ Download │
    │   │Cache│  │  Changes │
    │   └─────┘  └────┬─────┘
    │               │
    │               ▼
    │          ┌──────────┐
    │          │ Update   │
    │          │  Cache   │
    │          └────┬─────┘
    │               │
    └───────────────┘
            │
            ▼
       ┌─────────┐
       │  Ready  │
       └─────────┘
```

---

## 5. Update Distribution Model

### 5.1 MSIX Packaging

**Package Structure**:
```
InstallVibe_1.0.0.0_x64.msix
├── AppxManifest.xml
├── InstallVibe.exe
├── Assets/
│   ├── Logo.png
│   ├── SplashScreen.png
│   └── (other app assets)
├── Dependencies/
│   └── (runtime dependencies)
└── Resources/
    └── (embedded resources)
```

**Manifest Highlights** (AppxManifest.xml):
- Identity: Publisher certificate-signed
- Capabilities: internetClient, localFolder
- Auto-launch: Support for protocol activation
- File associations: .ivguide (InstallVibe guide files)

### 5.2 AppInstaller Configuration

**InstallVibe.appinstaller**:
```xml
<?xml version="1.0" encoding="utf-8"?>
<AppInstaller
    xmlns="http://schemas.microsoft.com/appx/appinstaller/2021"
    Version="1.0.0.0"
    Uri="https://{tenant}.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe.appinstaller">

    <MainPackage
        Name="InstallVibe"
        Publisher="CN=YourCompany"
        Version="1.0.0.0"
        ProcessorArchitecture="x64"
        Uri="https://{tenant}.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe_1.0.0.0_x64.msix" />

    <UpdateSettings>
        <OnLaunch
            HoursBetweenUpdateChecks="24"
            ShowPrompt="true"
            UpdateBlocksActivation="false" />
        <AutomaticBackgroundTask />
        <ForceUpdateFromAnyVersion>false</ForceUpdateFromAnyVersion>
    </UpdateSettings>
</AppInstaller>
```

### 5.3 Update Flow

```
App Launch
    │
    ▼
┌──────────────────┐
│ Check for Update │ (Background)
└────────┬─────────┘
         │
    ┌────┴────┐
    │         │
No Update  Update Available
    │         │
    ▼         ▼
  ┌────┐  ┌──────────────────┐
  │Run │  │ Show Notification│
  │App │  │  "Update Ready"  │
  └────┘  └────────┬─────────┘
              │
         ┌────┴────────┐
         │             │
    Install Now   Install Later
         │             │
         ▼             ▼
  ┌────────────┐   ┌─────┐
  │ Download   │   │ Run │
  │ Install    │   │ App │
  │ Restart    │   └─────┘
  └────────────┘
```

**Update Process**:
1. App queries .appinstaller manifest from SharePoint
2. Compares version in manifest vs. installed version
3. If newer version available:
   - Downloads MSIX package in background
   - Verifies package signature
   - Prompts user or auto-installs (based on policy)
4. Windows handles installation and app restart

### 5.4 Version Management

**Versioning Scheme**: Semantic Versioning (Major.Minor.Patch.Build)
- Major: Breaking changes, new activation required
- Minor: New features, backward compatible
- Patch: Bug fixes
- Build: Auto-incremented CI/CD build number

**Version Metadata** (versions.json on SharePoint):
```json
{
  "currentVersion": "1.2.3.456",
  "minimumVersion": "1.0.0.0",
  "updateChannel": "stable",
  "releaseNotes": {
    "1.2.3.456": {
      "releaseDate": "2025-01-15",
      "changes": [
        "Added offline mode improvements",
        "Fixed sync conflict bugs"
      ],
      "critical": false
    }
  },
  "channels": {
    "stable": "https://.../InstallVibe_1.2.3.456_x64.msix",
    "beta": "https://.../InstallVibe_1.3.0.500_x64.msix"
  }
}
```

### 5.5 Update Security
- MSIX packages signed with code-signing certificate
- SharePoint HTTPS delivery only
- Signature verification before installation
- Rollback support via Windows package management

---

## 6. Local Data Model

### 6.1 Core Domain Models

**Guide Model**:
```csharp
public class Guide
{
    public string GuideId { get; set; }
    public string Title { get; set; }
    public string Version { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public LicenseType RequiredLicense { get; set; }
    public bool IsPublished { get; set; }
    public DateTime LastModified { get; set; }
    public List<Step> Steps { get; set; }
    public GuideMetadata Metadata { get; set; }
}

public class Step
{
    public string StepId { get; set; }
    public int StepNumber { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }  // Markdown
    public List<MediaReference> Media { get; set; }
    public List<ChecklistItem> Checklist { get; set; }
    public StepType Type { get; set; }  // Info, Action, Decision
}

public class MediaReference
{
    public string MediaId { get; set; }
    public MediaType Type { get; set; }  // Image, Video, PDF
    public string LocalPath { get; set; }
    public string SharePointUrl { get; set; }
    public bool IsCached { get; set; }
}
```

**Activation Model**:
```csharp
public class ActivationToken
{
    public string ProductKeyHash { get; set; }
    public LicenseType LicenseType { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public string CustomerId { get; set; }
    public List<string> EnabledFeatures { get; set; }
    public string MachineId { get; set; }
    public DateTime ValidatedDate { get; set; }
    public bool OnlineValidation { get; set; }
    public string Signature { get; set; }
}

public enum LicenseType
{
    Tech,
    Admin
}
```

**Progress Model**:
```csharp
public class GuideProgress
{
    public string ProgressId { get; set; }
    public string GuideId { get; set; }
    public string UserId { get; set; }
    public string CurrentStepId { get; set; }
    public Dictionary<string, StepStatus> StepProgress { get; set; }
    public DateTime StartedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string Notes { get; set; }
}

public enum StepStatus
{
    NotStarted,
    InProgress,
    Completed,
    Skipped
}
```

**Sync Model**:
```csharp
public class SyncMetadata
{
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public DateTime LastSyncDate { get; set; }
    public string ServerVersion { get; set; }
    public string LocalVersion { get; set; }
    public SyncStatus Status { get; set; }
}

public enum SyncStatus
{
    Synced,
    Pending,
    Conflict,
    Error
}
```

### 6.2 Data Relationships

```
Guide (1) ─────── (N) Step
  │
  │
  └─────── (N) MediaReference ─────── (1) MediaCache
  │
  └─────── (1) GuideProgress ─────── (N) StepProgress


ActivationToken (1) ─────── (N) EnabledFeatures


SyncMetadata (N) ─────── (1) Guide/Media/etc
```

---

## 7. Project Structure (WinUI 3 / .NET 8)

```
InstallVibe.sln
│
├── InstallVibe (WinUI 3 App)
│   ├── App.xaml / App.xaml.cs           (Application entry point)
│   ├── Package.appxmanifest              (MSIX manifest)
│   │
│   ├── Views/                            (XAML pages)
│   │   ├── Shell/
│   │   │   ├── MainWindow.xaml
│   │   │   └── ShellPage.xaml
│   │   ├── Activation/
│   │   │   ├── ActivationPage.xaml
│   │   │   └── WelcomePage.xaml
│   │   ├── Guides/
│   │   │   ├── GuideListPage.xaml
│   │   │   ├── GuideDetailPage.xaml
│   │   │   └── GuideEditorPage.xaml     (Admin only)
│   │   ├── Progress/
│   │   │   └── ProgressPage.xaml
│   │   └── Settings/
│   │       └── SettingsPage.xaml
│   │
│   ├── ViewModels/                       (MVVM ViewModels)
│   │   ├── ShellViewModel.cs
│   │   ├── ActivationViewModel.cs
│   │   ├── GuideListViewModel.cs
│   │   ├── GuideDetailViewModel.cs
│   │   ├── GuideEditorViewModel.cs
│   │   ├── ProgressViewModel.cs
│   │   └── SettingsViewModel.cs
│   │
│   ├── Controls/                         (Custom controls)
│   │   ├── StepControl.xaml
│   │   ├── MediaViewer.xaml
│   │   ├── ProgressIndicator.xaml
│   │   └── ChecklistControl.xaml
│   │
│   ├── Converters/                       (XAML converters)
│   │   ├── BoolToVisibilityConverter.cs
│   │   └── MediaTypeToIconConverter.cs
│   │
│   ├── Helpers/
│   │   ├── NavigationHelper.cs
│   │   └── ResourceHelper.cs
│   │
│   └── Assets/
│       ├── Fonts/
│       ├── Icons/
│       └── Images/
│
├── InstallVibe.Core (Class Library)
│   ├── Models/                           (Domain models)
│   │   ├── Guide.cs
│   │   ├── Step.cs
│   │   ├── MediaReference.cs
│   │   ├── ActivationToken.cs
│   │   ├── GuideProgress.cs
│   │   └── SyncMetadata.cs
│   │
│   ├── Services/                         (Business logic)
│   │   ├── Activation/
│   │   │   ├── IActivationService.cs
│   │   │   ├── ActivationService.cs
│   │   │   ├── ProductKeyValidator.cs
│   │   │   └── LicenseManager.cs
│   │   ├── Data/
│   │   │   ├── IGuideService.cs
│   │   │   ├── GuideService.cs
│   │   │   ├── IProgressService.cs
│   │   │   └── ProgressService.cs
│   │   ├── SharePoint/
│   │   │   ├── ISharePointService.cs
│   │   │   ├── SharePointService.cs
│   │   │   ├── SharePointAuthService.cs
│   │   │   └── GraphApiClient.cs
│   │   ├── Sync/
│   │   │   ├── ISyncService.cs
│   │   │   ├── SyncService.cs
│   │   │   ├── SyncEngine.cs
│   │   │   └── ConflictResolver.cs
│   │   ├── Cache/
│   │   │   ├── ICacheService.cs
│   │   │   ├── CacheService.cs
│   │   │   ├── MediaCacheManager.cs
│   │   │   └── CacheCleanupService.cs
│   │   └── Update/
│   │       ├── IUpdateService.cs
│   │       └── UpdateService.cs
│   │
│   ├── Contracts/                        (Interfaces)
│   │   ├── INavigationService.cs
│   │   ├── IDialogService.cs
│   │   └── ISettingsService.cs
│   │
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
├── InstallVibe.Data (Data Access Layer)
│   ├── Context/
│   │   └── InstallVibeContext.cs        (SQLite context)
│   │
│   ├── Repositories/
│   │   ├── IRepository.cs
│   │   ├── GuideRepository.cs
│   │   ├── ProgressRepository.cs
│   │   ├── MediaCacheRepository.cs
│   │   └── SyncMetadataRepository.cs
│   │
│   ├── Entities/                         (DB entities)
│   │   ├── GuideEntity.cs
│   │   ├── StepEntity.cs
│   │   ├── ProgressEntity.cs
│   │   └── MediaCacheEntity.cs
│   │
│   └── Migrations/
│       └── (EF Core migrations)
│
├── InstallVibe.Infrastructure (Cross-cutting)
│   ├── Security/
│   │   ├── CryptoService.cs
│   │   ├── RsaValidator.cs
│   │   └── DpapiEncryption.cs
│   │
│   ├── Logging/
│   │   └── LoggingConfiguration.cs
│   │
│   ├── Configuration/
│   │   ├── AppSettings.cs
│   │   └── ConfigurationManager.cs
│   │
│   └── Constants/
│       ├── AppConstants.cs
│       └── PublicKeys.cs                 (Embedded RSA public key)
│
└── InstallVibe.Tests (Unit/Integration Tests)
    ├── Services/
    │   ├── ActivationServiceTests.cs
    │   ├── ProductKeyValidatorTests.cs
    │   ├── SyncServiceTests.cs
    │   └── CacheServiceTests.cs
    ├── ViewModels/
    │   └── (ViewModel tests)
    └── TestHelpers/
        └── MockServices.cs
```

---

## 8. Technology Stack & Reasoning

### 8.1 Presentation Layer

**WinUI 3 (Windows App SDK)**
- **Why**: Modern Windows-first UI framework with Fluent Design
- **Benefits**: Native performance, Windows 11 integration, modern controls
- **Trade-offs**: Windows-only (acceptable for this use case)

**XAML + MVVM Pattern**
- **Why**: Separation of concerns, testability, data binding
- **Framework**: CommunityToolkit.Mvvm (source generators, reduced boilerplate)

### 8.2 Business Logic

**.NET 8**
- **Why**: Latest LTS, performance improvements, modern C# features
- **Benefits**: Cross-library compatibility, extensive ecosystem

**Dependency Injection**
- **Framework**: Microsoft.Extensions.DependencyInjection
- **Why**: Testability, loose coupling, service lifetime management

### 8.3 Data Access

**SQLite with Entity Framework Core**
- **Why**: Embedded database, no server required, good performance
- **Library**: Microsoft.EntityFrameworkCore.Sqlite
- **Benefits**: LINQ support, migrations, change tracking

**File System Storage**
- **Why**: Direct control over media files, simple caching
- **Structure**: Organized folder hierarchy in LocalAppData

### 8.4 SharePoint Integration

**Microsoft Graph API / SharePoint REST API**
- **Why**: Official APIs, comprehensive access to SharePoint
- **Libraries**:
  - Microsoft.Graph (for authentication, some operations)
  - PnP.Core (for advanced SharePoint operations)
  - Microsoft.Identity.Client (MSAL for authentication)

**Authentication: MSAL (Microsoft Authentication Library)**
- **Why**: OAuth 2.0, token caching, refresh handling
- **Flow**: Authorization Code flow with PKCE

### 8.5 Security

**RSA Cryptography**
- **Library**: System.Security.Cryptography (built-in)
- **Key Size**: 2048-bit RSA keys
- **Why**: Asymmetric encryption for product key signatures

**DPAPI (Data Protection API)**
- **Library**: System.Security.Cryptography.ProtectedData
- **Why**: Windows-native encryption for local secrets
- **Scope**: CurrentUser (machine-bound)

**Certificate Pinning**
- **Why**: Prevent MITM attacks on SharePoint connections
- **Implementation**: HttpClient with custom certificate validation

### 8.6 Packaging & Updates

**MSIX Packaging**
- **Why**: Modern Windows deployment, auto-updates, clean install/uninstall
- **Benefits**: Delta updates, automatic dependency handling

**AppInstaller**
- **Why**: Built-in update mechanism, background updates
- **Hosting**: SharePoint document library (HTTPS)

### 8.7 Logging & Diagnostics

**Serilog**
- **Why**: Structured logging, multiple sinks, rich formatting
- **Sinks**: File, Debug, (optional) Application Insights
- **Configuration**: JSON configuration files

### 8.8 Testing

**xUnit**
- **Why**: Modern, extensible, .NET standard
- **Mocking**: Moq for service mocking
- **UI Testing**: WinAppDriver for UI automation tests

### 8.9 Key NuGet Packages

```xml
<ItemGroup>
  <!-- UI Framework -->
  <PackageReference Include="Microsoft.WindowsAppSDK" Version="1.5.*" />
  <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.*" />
  <PackageReference Include="CommunityToolkit.WinUI.UI.Controls" Version="7.1.*" />

  <!-- Data Access -->
  <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.*" />

  <!-- SharePoint & Graph -->
  <PackageReference Include="Microsoft.Graph" Version="5.40.*" />
  <PackageReference Include="PnP.Core" Version="1.11.*" />
  <PackageReference Include="Microsoft.Identity.Client" Version="4.60.*" />

  <!-- Logging -->
  <PackageReference Include="Serilog" Version="3.1.*" />
  <PackageReference Include="Serilog.Sinks.File" Version="5.0.*" />
  <PackageReference Include="Serilog.Sinks.Debug" Version="2.0.*" />

  <!-- Utilities -->
  <PackageReference Include="Newtonsoft.Json" Version="13.0.*" />
  <PackageReference Include="System.Security.Cryptography.Algorithms" Version="4.3.*" />

  <!-- Testing -->
  <PackageReference Include="xUnit" Version="2.6.*" />
  <PackageReference Include="Moq" Version="4.20.*" />
  <PackageReference Include="FluentAssertions" Version="6.12.*" />
</ItemGroup>
```

---

## 9. Component Interaction Diagrams

### 9.1 Application Startup Flow

```
User Launches App
        │
        ▼
┌────────────────────────────────────────────────────────────┐
│                    App.xaml.cs                             │
│  1. Configure DI Container                                 │
│  2. Register Services (Singleton/Transient)                │
│  3. Initialize Logging (Serilog)                           │
│  4. Load Configuration                                     │
└────────────────┬───────────────────────────────────────────┘
                 │
                 ▼
        ┌────────────────┐
        │ ActivationCheck│
        └────────┬───────┘
                 │
          ┌──────┴──────┐
          │             │
    Not Activated   Activated
          │             │
          ▼             ▼
   ┌─────────────┐  ┌──────────────┐
   │ Activation  │  │ Initialize   │
   │    Page     │  │   Services   │
   └─────────────┘  └──────┬───────┘
                            │
                            ▼
                    ┌──────────────┐
                    │ Sync Check   │
                    └──────┬───────┘
                           │
                    ┌──────┴──────┐
                    │             │
                 Offline       Online
                    │             │
                    ▼             ▼
                ┌────────┐   ┌──────────┐
                │ Load   │   │ Sync     │
                │ Cache  │   │ Data     │
                └───┬────┘   └────┬─────┘
                    │             │
                    └──────┬──────┘
                           │
                           ▼
                   ┌───────────────┐
                   │  Main Shell   │
                   │  (Home Page)  │
                   └───────────────┘
```

### 9.2 Guide Viewing Flow

```
User Selects Guide
        │
        ▼
┌────────────────────────────────────────────────────────────┐
│              GuideDetailViewModel                          │
│  1. Load Guide from Cache/Service                          │
│  2. Check License (Tech/Admin)                             │
│  3. Load Progress (if exists)                              │
│  4. Resolve Media References                               │
└────────────────┬───────────────────────────────────────────┘
                 │
                 ▼
        ┌────────────────┐
        │  Guide Service │
        └────────┬───────┘
                 │
          ┌──────┴──────────┐
          │                 │
    In Cache?           Not Cached
          │                 │
          ▼                 ▼
   ┌─────────────┐   ┌──────────────┐
   │Load from DB │   │ Network      │
   │& File Cache │   │ Available?   │
   └──────┬──────┘   └──────┬───────┘
          │              ┌───┴────┐
          │              │        │
          │             Yes       No
          │              │        │
          │              ▼        ▼
          │      ┌───────────┐ ┌──────┐
          │      │ Download  │ │ Error│
          │      │ & Cache   │ │ Msg  │
          │      └─────┬─────┘ └──────┘
          │            │
          └────────────┘
                 │
                 ▼
        ┌────────────────┐
        │  Media Cache   │
        │    Service     │
        └────────┬───────┘
                 │
          ┌──────┴──────────┐
          │                 │
    Media Cached?       Not Cached
          │                 │
          ▼                 ▼
   ┌─────────────┐   ┌──────────────┐
   │ Load Local  │   │ Download or  │
   │    File     │   │ Placeholder  │
   └──────┬──────┘   └──────┬───────┘
          │                 │
          └────────┬────────┘
                   │
                   ▼
          ┌────────────────┐
          │ Render Guide   │
          │  in UI (XAML)  │
          └────────────────┘
```

### 9.3 Sync Operation Flow

```
Sync Triggered (Manual/Auto)
        │
        ▼
┌────────────────────────────────────────────────────────────┐
│                   Sync Service                             │
│  1. Check network connectivity                             │
│  2. Authenticate with SharePoint (refresh token if needed) │
│  3. Query sync metadata                                    │
└────────────────┬───────────────────────────────────────────┘
                 │
                 ▼
        ┌────────────────┐
        │ Compare        │
        │ Versions       │
        └────────┬───────┘
                 │
          ┌──────┴──────────────────┐
          │                         │
    Local Current              Updates Available
          │                         │
          ▼                         ▼
      ┌───────┐            ┌─────────────────┐
      │ Done  │            │ Build Sync Queue│
      └───────┘            └────────┬────────┘
                                    │
                             ┌──────┴──────┐
                             │             │
                        Guides          Media
                             │             │
                             ▼             ▼
                    ┌─────────────┐ ┌──────────────┐
                    │ Download    │ │ Download     │
                    │ guide.json  │ │ Media Files  │
                    │ step files  │ │ (batched)    │
                    └──────┬──────┘ └──────┬───────┘
                           │               │
                           └───────┬───────┘
                                   │
                                   ▼
                          ┌─────────────────┐
                          │ Verify Checksums│
                          └────────┬────────┘
                                   │
                            ┌──────┴──────┐
                            │             │
                        Valid?        Invalid
                            │             │
                            ▼             ▼
                    ┌───────────────┐ ┌────────┐
                    │ Update Cache  │ │ Retry/ │
                    │ Update DB     │ │ Error  │
                    │ Update Sync   │ └────────┘
                    │   Metadata    │
                    └───────┬───────┘
                            │
                            ▼
                    ┌───────────────┐
                    │ Sync Complete │
                    │  Notification │
                    └───────────────┘
```

### 9.4 Product Key Activation Flow

```
User Enters Product Key
        │
        ▼
┌────────────────────────────────────────────────────────────┐
│              ActivationViewModel                           │
│  1. Validate format (XXXXX-XXXXX-XXXXX-XXXXX-XXXXX)       │
│  2. Call ActivationService                                 │
└────────────────┬───────────────────────────────────────────┘
                 │
                 ▼
        ┌────────────────────┐
        │ ProductKeyValidator│
        └────────┬───────────┘
                 │
                 ▼
        ┌────────────────────┐
        │ Parse Key          │
        │  - Decode Base58   │
        │  - Extract Payload │
        │  - Extract Signature│
        └────────┬───────────┘
                 │
                 ▼
        ┌────────────────────┐
        │ RSA Verification   │
        │  - Load Public Key │
        │  - Verify Signature│
        └────────┬───────────┘
                 │
          ┌──────┴──────┐
          │             │
    Valid Sig?    Invalid Sig
          │             │
          ▼             ▼
   ┌─────────────┐  ┌────────────────┐
   │ Validate    │  │ Try Online     │
   │ Checksum    │  │ Validation?    │
   │ Expiration  │  └────────┬───────┘
   └──────┬──────┘        │
          │         ┌─────┴─────┐
          │         │           │
          │        Yes          No
          │         │           │
          │         ▼           ▼
          │  ┌──────────────┐ ┌──────┐
          │  │ SharePoint   │ │Reject│
          │  │ Lookup       │ │ Key  │
          │  └──────┬───────┘ └──────┘
          │         │
          │    ┌────┴────┐
          │    │         │
          │  Valid    Invalid
          │    │         │
          └────┘         ▼
               │      ┌──────┐
               │      │Reject│
               │      │ Key  │
               │      └──────┘
               ▼
      ┌─────────────────┐
      │ Generate Token  │
      │  - Hash key     │
      │  - Extract info │
      │  - Sign token   │
      │  - Bind machine │
      └────────┬────────┘
               │
               ▼
      ┌─────────────────┐
      │ Encrypt & Store │
      │   (DPAPI)       │
      └────────┬────────┘
               │
               ▼
      ┌─────────────────┐
      │ Apply License   │
      │  - Set features │
      │  - Enable UI    │
      └────────┬────────┘
               │
               ▼
      ┌─────────────────┐
      │ Navigate to     │
      │   Main Shell    │
      └─────────────────┘
```

---

## 10. Security Considerations

### 10.1 Product Key Protection
- Private keys never embedded in application
- Public key embedded and obfuscated
- Keys generated offline on secure machine
- Signature verification prevents key tampering

### 10.2 Data Protection
- Activation tokens encrypted with DPAPI
- SharePoint credentials stored in Windows Credential Manager
- Database file not encrypted (contains non-sensitive cache data)
- Sensitive logs sanitized (no keys, tokens, passwords)

### 10.3 Network Security
- HTTPS only for SharePoint communication
- Certificate pinning for SharePoint domain
- Token refresh handled securely (never exposed)
- Retry logic with exponential backoff (prevent timing attacks)

### 10.4 Code Signing
- MSIX packages signed with trusted certificate
- Certificate validation on update installation
- Publisher identity verified by Windows

### 10.5 Audit Logging
- All activation attempts logged (success/failure)
- Sync operations logged with timestamps
- Failed authentication attempts tracked
- Optional telemetry (opt-in) for crash reporting

---

## 11. Performance Considerations

### 11.1 Startup Performance
- Lazy loading of services
- Background initialization of non-critical services
- Cached activation token (skip validation on startup if valid)
- Parallel loading of UI and data

### 11.2 Media Handling
- Progressive image loading
- Thumbnail generation for large images
- Video streaming (not full download)
- Lazy loading of media (load on-demand, not on guide load)

### 11.3 Database Performance
- Indexed columns (GuideId, StepId, MediaId)
- Query optimization with EF Core compiled queries
- Connection pooling
- Batch operations for sync

### 11.4 Memory Management
- Weak references for cached images
- Dispose pattern for media streams
- Limit in-memory cache size
- Garbage collection optimization for large media files

---

## 12. Scalability Considerations

### 12.1 Guide Volume
- Support for 1000+ guides
- Pagination in guide list
- Search and filter optimization
- Category-based organization

### 12.2 Media Library
- 10 GB cache limit (configurable)
- Efficient cleanup algorithms
- Media deduplication (shared media across guides)
- CDN potential for SharePoint files

### 12.3 Multi-User Scenarios
- Progress tracked per-user (or per-machine)
- Concurrent access to SharePoint (read-mostly)
- Admin edits: conflict detection and resolution

---

## 13. Deployment Strategy

### 13.1 Initial Deployment
1. Build MSIX package with code signing
2. Upload to SharePoint AppUpdates library
3. Create/update .appinstaller manifest
4. Distribute install URL or direct MSIX to users
5. Users install via double-click or URL

### 13.2 Update Deployment
1. Build new version with incremented version number
2. Sign MSIX package
3. Upload to SharePoint AppUpdates library
4. Update .appinstaller manifest with new version
5. Users receive update notification on next app launch

### 13.3 Rollback Strategy
- Keep previous versions on SharePoint
- Update .appinstaller to point to stable version
- Windows allows uninstall and reinstall of specific version

---

## 14. Future Extensibility

### 14.1 Planned Features (Not in v1)
- Multi-language support (localization)
- Custom branding per customer
- Integration with ticketing systems (e.g., ServiceNow)
- Mobile companion app (read-only guide viewer)
- Advanced analytics dashboard for admins
- Team collaboration features (shared progress)

### 14.2 Architecture Support
- Plugin architecture for custom step types
- Extensible media types (AR, 3D models)
- API for third-party integrations
- Webhook support for SharePoint events

---

## 15. Development Roadmap

### Phase 1: Foundation (Weeks 1-3)
- Project setup and structure
- Core models and database schema
- Product key validation (offline RSA)
- Basic WinUI 3 shell and navigation

### Phase 2: SharePoint Integration (Weeks 4-6)
- SharePoint authentication (MSAL)
- Guide download and caching
- Media download and management
- Sync service implementation

### Phase 3: UI & UX (Weeks 7-9)
- Guide list and detail views
- Step-by-step navigation
- Media viewer components
- Progress tracking UI

### Phase 4: Admin Features (Weeks 10-11)
- Guide editor (Admin license)
- Upload to SharePoint
- Version management

### Phase 5: Updates & Polish (Weeks 12-13)
- MSIX packaging and signing
- AppInstaller configuration
- Update service implementation
- Performance optimization

### Phase 6: Testing & Release (Weeks 14-15)
- Unit and integration testing
- User acceptance testing
- Documentation
- Initial release

---

## 16. Success Metrics

### 16.1 Performance Metrics
- App startup time < 2 seconds (warm start)
- Guide load time < 1 second (cached)
- Media load time < 3 seconds (high-res images)
- Sync operation < 30 seconds (typical guide with 10 steps)

### 16.2 Reliability Metrics
- Crash-free rate > 99%
- Successful activation rate > 95%
- Sync success rate > 98%
- Offline mode availability 100% (cached content)

### 16.3 User Experience Metrics
- User onboarding time < 5 minutes
- Guide completion rate > 80%
- User satisfaction score > 4.5/5

---

## Conclusion

This architecture provides a robust foundation for InstallVibe, balancing offline-first functionality with cloud-based content management. The product key system ensures proper licensing control, while the SharePoint integration enables centralized content distribution and updates. The modular design supports future extensibility and maintains clear separation of concerns across all layers.

**Key Architectural Decisions:**
1. **Offline-first**: Local cache and SQLite database ensure functionality without connectivity
2. **Product key validation**: RSA signature verification provides secure offline activation
3. **SharePoint as backend**: Leverages existing infrastructure without custom server development
4. **MSIX packaging**: Modern Windows deployment with automatic update support
5. **MVVM pattern**: Maintainable, testable UI code with clear separation of concerns
6. **Layered architecture**: Clean separation enables unit testing and future changes

This architecture is ready for implementation in the next phase.
