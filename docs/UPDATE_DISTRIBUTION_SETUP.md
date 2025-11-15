# InstallVibe Update Distribution Setup

This document describes how to set up the SharePoint distribution structure for InstallVibe's AppInstaller/MSIX auto-update system.

## Table of Contents

1. [SharePoint Folder Structure](#sharepoint-folder-structure)
2. [version.json Specification](#versionjson-specification)
3. [AppInstaller File](#appinstaller-file)
4. [Deployment Workflow](#deployment-workflow)
5. [Version Publishing Process](#version-publishing-process)
6. [Security Considerations](#security-considerations)

---

## SharePoint Folder Structure

Create the following folder structure in your SharePoint document library:

```
📁 AppUpdates/
├── 📁 InstallVibe/
│   ├── 📄 version.json                 # Latest version metadata
│   ├── 📄 InstallVibe.appinstaller     # AppInstaller manifest
│   │
│   ├── 📁 1.0.0/                       # Version 1.0.0
│   │   ├── 📄 InstallVibe_1.0.0.msix
│   │   ├── 📄 InstallVibe_1.0.0.msixbundle (optional)
│   │   └── 📄 release-notes-1.0.0.md
│   │
│   ├── 📁 1.0.1/                       # Version 1.0.1
│   │   ├── 📄 InstallVibe_1.0.1.msix
│   │   └── 📄 release-notes-1.0.1.md
│   │
│   ├── 📁 1.1.0/                       # Version 1.1.0
│   │   ├── 📄 InstallVibe_1.1.0.msix
│   │   └── 📄 release-notes-1.1.0.md
│   │
│   └── 📁 Archive/                     # Older versions (optional)
│       └── ...
```

### Folder Permissions

**Required Permissions:**
- **Read Access (Anonymous or All Users):**
  - `version.json`
  - `InstallVibe.appinstaller`
  - All `.msix` files
  - All `.msixbundle` files

**Restricted Access (Admins only):**
- Write/modify access to all folders
- Delete access to version folders

---

## version.json Specification

The `version.json` file contains metadata about the latest available version and is checked by InstallVibe on startup and periodically.

### Schema

```json
{
  "version": "1.2.0",
  "releaseDate": "2025-01-15T10:30:00Z",
  "updateType": "recommended",
  "appInstallerUrl": "https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/InstallVibe.appinstaller",
  "msixPackageUrl": "https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/1.2.0/InstallVibe_1.2.0.msix",
  "fileHash": "a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456",
  "fileSize": 52428800,
  "minimumVersion": "1.0.0",
  "releaseNotes": "## What's New in 1.2.0\n\n- Added offline guide caching\n- Improved performance\n- Fixed authentication issues\n\n## Bug Fixes\n\n- Resolved sync errors\n- Fixed UI layout issues on 4K displays",
  "requiresRestart": true,
  "isMandatory": false,
  "releaseNotesUrl": "https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/1.2.0/release-notes-1.2.0.md",
  "changes": [
    {
      "type": "feature",
      "description": "Added offline guide caching for field technicians",
      "issueNumber": "IV-123"
    },
    {
      "type": "improvement",
      "description": "Improved application startup performance by 40%",
      "issueNumber": "IV-145"
    },
    {
      "type": "bugfix",
      "description": "Fixed authentication token expiration handling",
      "issueNumber": "IV-167"
    },
    {
      "type": "security",
      "description": "Updated cryptography libraries to latest versions",
      "issueNumber": "IV-189"
    }
  ]
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `version` | string | ✅ | Version number (semantic versioning: `major.minor.patch`) |
| `releaseDate` | string | ✅ | ISO 8601 formatted date/time of release |
| `updateType` | string | ✅ | Update severity: `"critical"`, `"recommended"`, or `"optional"` |
| `appInstallerUrl` | string | ✅ | Full URL to the `.appinstaller` file |
| `msixPackageUrl` | string | ✅ | Full URL to the `.msix` or `.msixbundle` file |
| `fileHash` | string | ✅ | SHA256 hash of the MSIX package (lowercase hex) |
| `fileSize` | number | ✅ | Package file size in bytes |
| `minimumVersion` | string | ❌ | Minimum version required before this update can be applied |
| `releaseNotes` | string | ❌ | Markdown-formatted release notes |
| `requiresRestart` | boolean | ✅ | Whether app restart is required (default: `true`) |
| `isMandatory` | boolean | ✅ | Whether update is mandatory (default: `false`) |
| `releaseNotesUrl` | string | ❌ | URL to full release notes document |
| `changes` | array | ❌ | Array of changelog entries |

### Update Types

- **`critical`**: Security or stability issues requiring immediate update
- **`recommended`**: Important fixes or features, strongly encouraged
- **`optional`**: Minor improvements, user can skip

### Change Types

- **`feature`**: New functionality
- **`improvement`**: Enhancement to existing feature
- **`bugfix`**: Bug or defect fix
- **`security`**: Security-related update

---

## AppInstaller File

The `.appinstaller` file enables automatic updates through Windows AppInstaller.

### InstallVibe.appinstaller

```xml
<?xml version="1.0" encoding="utf-8"?>
<AppInstaller
    xmlns="http://schemas.microsoft.com/appx/appinstaller/2021"
    Version="1.2.0.0"
    Uri="https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/InstallVibe.appinstaller">

    <MainBundle
        Name="InstallVibe"
        Publisher="CN=YourCompany"
        Version="1.2.0.0"
        Uri="https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/1.2.0/InstallVibe_1.2.0.msixbundle" />

    <!-- OR for single MSIX -->
    <!--
    <MainPackage
        Name="InstallVibe"
        Publisher="CN=YourCompany"
        Version="1.2.0.0"
        ProcessorArchitecture="x64"
        Uri="https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/1.2.0/InstallVibe_1.2.0.msix" />
    -->

    <UpdateSettings>
        <OnLaunch
            HoursBetweenUpdateChecks="12"
            ShowPrompt="true"
            UpdateBlocksActivation="false" />
        <AutomaticBackgroundTask />
        <ForceUpdateFromAnyVersion>false</ForceUpdateFromAnyVersion>
    </UpdateSettings>

</AppInstaller>
```

### AppInstaller Configuration

| Element/Attribute | Description |
|-------------------|-------------|
| `Version` | Current version in AppInstaller manifest |
| `Uri` | URL to this `.appinstaller` file (self-referential) |
| `MainBundle.Uri` | URL to the `.msixbundle` file |
| `HoursBetweenUpdateChecks` | Hours between automatic checks (default: 12) |
| `ShowPrompt` | Show update prompt to user (default: true) |
| `UpdateBlocksActivation` | Block app launch until update (default: false) |
| `AutomaticBackgroundTask` | Enable background update checks |

---

## Deployment Workflow

### Step 1: Build MSIX Package

```powershell
# Build the MSIX package using Visual Studio or CLI
msbuild InstallVibe.sln /p:Configuration=Release /p:Platform=x64 /p:AppxPackageDir=".\AppPackages\" /p:AppxBundle=Always /p:UapAppxPackageBuildMode=StoreUpload
```

### Step 2: Generate File Hash

```powershell
# PowerShell script to generate SHA256 hash
$filePath = ".\AppPackages\InstallVibe_1.2.0\InstallVibe_1.2.0.msix"
$hash = Get-FileHash -Path $filePath -Algorithm SHA256
$hash.Hash.ToLower()
# Output: a1b2c3d4e5f6789012345678901234567890abcdef1234567890abcdef123456
```

### Step 3: Upload to SharePoint

1. Create version folder: `1.2.0/`
2. Upload MSIX package
3. Upload release notes (optional)
4. Update `version.json` with new metadata
5. Update `InstallVibe.appinstaller` with new version and URL

### Step 4: Verify URLs

Ensure all URLs are publicly accessible (or accessible to target users):

```bash
# Test version.json
curl https://yourtenant.sharepoint.com/.../version.json

# Test appinstaller
curl https://yourtenant.sharepoint.com/.../InstallVibe.appinstaller

# Test MSIX package
curl -I https://yourtenant.sharepoint.com/.../InstallVibe_1.2.0.msix
```

---

## Version Publishing Process

### Publishing a New Version

#### 1. Prepare Release

- [ ] Update version number in `Package.appxmanifest`
- [ ] Update version in `InstallVibe.appinstaller`
- [ ] Write release notes
- [ ] Build and test MSIX package
- [ ] Generate SHA256 hash

#### 2. Upload to SharePoint

```powershell
# Example PowerShell script for upload
$version = "1.2.0"
$siteUrl = "https://yourtenant.sharepoint.com/sites/InstallVibe"
$libraryPath = "AppUpdates/InstallVibe"

# Connect to SharePoint
Connect-PnPOnline -Url $siteUrl -Interactive

# Create version folder
Add-PnPFolder -Name $version -Folder "$libraryPath"

# Upload MSIX
Add-PnPFile -Path ".\InstallVibe_$version.msix" -Folder "$libraryPath/$version"

# Upload release notes
Add-PnPFile -Path ".\release-notes-$version.md" -Folder "$libraryPath/$version"
```

#### 3. Update version.json

```powershell
# Generate version.json
$versionJson = @{
    version = "1.2.0"
    releaseDate = (Get-Date).ToUniversalTime().ToString("o")
    updateType = "recommended"
    appInstallerUrl = "https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/InstallVibe.appinstaller"
    msixPackageUrl = "https://yourtenant.sharepoint.com/sites/InstallVibe/AppUpdates/InstallVibe/1.2.0/InstallVibe_1.2.0.msix"
    fileHash = "a1b2c3d4..." # Your generated hash
    fileSize = 52428800
    minimumVersion = "1.0.0"
    releaseNotes = Get-Content ".\release-notes-1.2.0.md" -Raw
    requiresRestart = $true
    isMandatory = $false
} | ConvertTo-Json -Depth 10

# Upload version.json
$versionJson | Out-File -FilePath ".\version.json" -Encoding UTF8
Add-PnPFile -Path ".\version.json" -Folder "$libraryPath" -Values @{} -Overwrite
```

#### 4. Update AppInstaller

```xml
<!-- Update version and URI in InstallVibe.appinstaller -->
<AppInstaller Version="1.2.0.0" ...>
    <MainBundle Version="1.2.0.0"
        Uri="https://.../1.2.0/InstallVibe_1.2.0.msixbundle" />
</AppInstaller>
```

#### 5. Test Update

- [ ] Install previous version on test machine
- [ ] Verify update check detects new version
- [ ] Test update download and installation
- [ ] Verify app restarts correctly
- [ ] Check rollback capability (if needed)

---

## Security Considerations

### 1. HTTPS Only

**All URLs must use HTTPS** to prevent man-in-the-middle attacks.

```json
{
  "msixPackageUrl": "https://yourtenant.sharepoint.com/..."  // ✅ HTTPS
}
```

### 2. File Integrity Verification

**Always include SHA256 hashes** for integrity verification:

```powershell
# Generate hash
Get-FileHash -Algorithm SHA256 -Path InstallVibe_1.2.0.msix
```

InstallVibe validates the hash before installation:

```csharp
var isValid = await _updateService.VerifyUpdateIntegrityAsync(filePath, expectedHash);
```

### 3. Code Signing

**Sign all MSIX packages** with a trusted certificate:

```powershell
# Sign MSIX with certificate
signtool sign /fd SHA256 /a /f YourCertificate.pfx /p Password InstallVibe_1.2.0.msix
```

### 4. Access Control

- **Read-only access** for all users to public files
- **No direct write access** to SharePoint folder for users
- **Restricted admin access** for publishing updates

### 5. Rollback Plan

Maintain previous versions for emergency rollback:

```json
{
  "version": "1.1.0",  // Rollback to previous stable version
  "updateType": "critical",
  "isMandatory": true
}
```

---

## Troubleshooting

### Update Check Fails

**Symptoms:** App cannot detect updates

**Solutions:**
- Verify `version.json` URL is accessible
- Check network connectivity
- Review SharePoint permissions
- Check application logs

### Hash Verification Fails

**Symptoms:** Download succeeds but installation blocked

**Solutions:**
- Regenerate SHA256 hash
- Verify file not corrupted during upload
- Check hash format (lowercase, no hyphens)

### AppInstaller Launch Fails

**Symptoms:** Update button does nothing

**Solutions:**
- Verify `.appinstaller` URL is valid
- Ensure AppInstaller is enabled in Windows
- Check Windows Store is not blocked by policy
- Review Windows Event Logs

---

## Example: Complete Publishing Script

```powershell
# complete-publish.ps1
param(
    [Parameter(Mandatory=$true)]
    [string]$Version,

    [Parameter(Mandatory=$true)]
    [ValidateSet('critical','recommended','optional')]
    [string]$UpdateType,

    [Parameter(Mandatory=$true)]
    [string]$SharePointSiteUrl,

    [Parameter(Mandatory=$false)]
    [bool]$IsMandatory = $false
)

# Configuration
$msixPath = ".\AppPackages\InstallVibe_$Version\InstallVibe_$Version.msix"
$releaseNotesPath = ".\release-notes-$Version.md"
$libraryPath = "AppUpdates/InstallVibe"

# Connect to SharePoint
Connect-PnPOnline -Url $SharePointSiteUrl -Interactive

# Create version folder
Write-Host "Creating version folder: $Version"
Add-PnPFolder -Name $Version -Folder $libraryPath -ErrorAction SilentlyContinue

# Upload MSIX
Write-Host "Uploading MSIX package..."
Add-PnPFile -Path $msixPath -Folder "$libraryPath/$Version"

# Generate hash
Write-Host "Generating SHA256 hash..."
$hash = (Get-FileHash -Path $msixPath -Algorithm SHA256).Hash.ToLower()
$fileSize = (Get-Item $msixPath).Length

# Upload release notes
if (Test-Path $releaseNotesPath) {
    Write-Host "Uploading release notes..."
    Add-PnPFile -Path $releaseNotesPath -Folder "$libraryPath/$Version"
}

# Get SharePoint URLs
$msixUrl = "$SharePointSiteUrl/$libraryPath/$Version/InstallVibe_$Version.msix"
$appInstallerUrl = "$SharePointSiteUrl/$libraryPath/InstallVibe.appinstaller"
$releaseNotesUrl = "$SharePointSiteUrl/$libraryPath/$Version/release-notes-$Version.md"

# Create version.json
Write-Host "Creating version.json..."
$versionJson = @{
    version = $Version
    releaseDate = (Get-Date).ToUniversalTime().ToString("o")
    updateType = $UpdateType
    appInstallerUrl = $appInstallerUrl
    msixPackageUrl = $msixUrl
    fileHash = $hash
    fileSize = $fileSize
    minimumVersion = "1.0.0"
    releaseNotes = if (Test-Path $releaseNotesPath) { Get-Content $releaseNotesPath -Raw } else { "" }
    requiresRestart = $true
    isMandatory = $IsMandatory
    releaseNotesUrl = $releaseNotesUrl
} | ConvertTo-Json -Depth 10

# Upload version.json
$tempVersionFile = [System.IO.Path]::GetTempFileName()
$versionJson | Out-File -FilePath $tempVersionFile -Encoding UTF8
Add-PnPFile -Path $tempVersionFile -Folder $libraryPath -Values @{} -Overwrite
Remove-Item $tempVersionFile

Write-Host "✅ Version $Version published successfully!" -ForegroundColor Green
Write-Host "Update type: $UpdateType" -ForegroundColor Cyan
Write-Host "File hash: $hash" -ForegroundColor Cyan
```

### Usage

```powershell
.\complete-publish.ps1 -Version "1.2.0" -UpdateType "recommended" -SharePointSiteUrl "https://yourtenant.sharepoint.com/sites/InstallVibe"
```

---

## Additional Resources

- [App Installer file documentation](https://learn.microsoft.com/en-us/windows/msix/app-installer/app-installer-file-overview)
- [MSIX packaging documentation](https://learn.microsoft.com/en-us/windows/msix/package/packaging-uwp-apps)
- [Windows AppInstaller protocol](https://learn.microsoft.com/en-us/windows/msix/app-installer/installing-windows10-apps-web)
- [SharePoint PnP PowerShell](https://pnp.github.io/powershell/)
