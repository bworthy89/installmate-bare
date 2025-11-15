<#
.SYNOPSIS
    Publishes a new InstallVibe version to SharePoint for auto-updates.

.DESCRIPTION
    This script automates the process of publishing a new InstallVibe version:
    1. Creates version folder in SharePoint
    2. Uploads MSIX package
    3. Generates SHA256 hash
    4. Updates version.json
    5. Optionally updates InstallVibe.appinstaller

.PARAMETER Version
    The version number to publish (e.g., "1.2.0")

.PARAMETER UpdateType
    Type of update: "critical", "recommended", or "optional"

.PARAMETER SharePointSiteUrl
    URL to the SharePoint site (e.g., "https://yourtenant.sharepoint.com/sites/InstallVibe")

.PARAMETER MsixPath
    Path to the MSIX package file

.PARAMETER ReleaseNotesPath
    Path to the release notes markdown file (optional)

.PARAMETER IsMandatory
    Whether this update is mandatory (default: false)

.PARAMETER MinimumVersion
    Minimum version required before this update can be applied (optional)

.PARAMETER UpdateAppInstaller
    Whether to update the InstallVibe.appinstaller file (default: false)

.EXAMPLE
    .\Publish-Update.ps1 -Version "1.2.0" -UpdateType "recommended" -SharePointSiteUrl "https://contoso.sharepoint.com/sites/InstallVibe" -MsixPath ".\InstallVibe_1.2.0.msix"

.EXAMPLE
    .\Publish-Update.ps1 -Version "2.0.0" -UpdateType "critical" -SharePointSiteUrl "https://contoso.sharepoint.com/sites/InstallVibe" -MsixPath ".\InstallVibe_2.0.0.msix" -IsMandatory -UpdateAppInstaller

.NOTES
    Requires PnP.PowerShell module: Install-Module -Name PnP.PowerShell
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^\d+\.\d+\.\d+$')]
    [string]$Version,

    [Parameter(Mandatory = $true)]
    [ValidateSet('critical', 'recommended', 'optional')]
    [string]$UpdateType,

    [Parameter(Mandatory = $true)]
    [string]$SharePointSiteUrl,

    [Parameter(Mandatory = $true)]
    [ValidateScript({Test-Path $_})]
    [string]$MsixPath,

    [Parameter(Mandatory = $false)]
    [string]$ReleaseNotesPath,

    [Parameter(Mandatory = $false)]
    [switch]$IsMandatory,

    [Parameter(Mandatory = $false)]
    [string]$MinimumVersion,

    [Parameter(Mandatory = $false)]
    [switch]$UpdateAppInstaller
)

# Configuration
$LibraryPath = "AppUpdates/InstallVibe"
$ErrorActionPreference = "Stop"

# Import PnP PowerShell
try {
    Import-Module PnP.PowerShell -ErrorAction Stop
    Write-Host "✓ PnP.PowerShell module loaded" -ForegroundColor Green
}
catch {
    Write-Error "PnP.PowerShell module not found. Install it with: Install-Module -Name PnP.PowerShell"
    exit 1
}

# Validate MSIX file
if (-not (Test-Path $MsixPath)) {
    Write-Error "MSIX file not found: $MsixPath"
    exit 1
}

$msixFileName = Split-Path $MsixPath -Leaf
if ($msixFileName -notmatch '\.msix(bundle)?$') {
    Write-Error "File must be a .msix or .msixbundle file"
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  InstallVibe Update Publisher" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Version:        $Version" -ForegroundColor Yellow
Write-Host "Update Type:    $UpdateType" -ForegroundColor Yellow
Write-Host "Mandatory:      $IsMandatory" -ForegroundColor Yellow
Write-Host "MSIX Package:   $msixFileName" -ForegroundColor Yellow
Write-Host "SharePoint:     $SharePointSiteUrl" -ForegroundColor Yellow
Write-Host "========================================`n" -ForegroundColor Cyan

# Connect to SharePoint
Write-Host "[1/7] Connecting to SharePoint..." -ForegroundColor Cyan
try {
    Connect-PnPOnline -Url $SharePointSiteUrl -Interactive
    Write-Host "✓ Connected to SharePoint" -ForegroundColor Green
}
catch {
    Write-Error "Failed to connect to SharePoint: $_"
    exit 1
}

# Create version folder
Write-Host "`n[2/7] Creating version folder: $Version" -ForegroundColor Cyan
try {
    $folderExists = Get-PnPFolder -Url "$LibraryPath/$Version" -ErrorAction SilentlyContinue
    if ($folderExists) {
        Write-Warning "Version folder already exists. Files will be overwritten."
        $confirm = Read-Host "Continue? (y/n)"
        if ($confirm -ne 'y') {
            Write-Host "Aborted by user" -ForegroundColor Yellow
            exit 0
        }
    }
    else {
        Add-PnPFolder -Name $Version -Folder $LibraryPath | Out-Null
        Write-Host "✓ Version folder created" -ForegroundColor Green
    }
}
catch {
    Write-Error "Failed to create version folder: $_"
    exit 1
}

# Upload MSIX package
Write-Host "`n[3/7] Uploading MSIX package..." -ForegroundColor Cyan
try {
    Add-PnPFile -Path $MsixPath -Folder "$LibraryPath/$Version" | Out-Null
    Write-Host "✓ MSIX package uploaded: $msixFileName" -ForegroundColor Green
}
catch {
    Write-Error "Failed to upload MSIX package: $_"
    exit 1
}

# Generate SHA256 hash
Write-Host "`n[4/7] Generating SHA256 hash..." -ForegroundColor Cyan
try {
    $hash = (Get-FileHash -Path $MsixPath -Algorithm SHA256).Hash.ToLower()
    $fileSize = (Get-Item $MsixPath).Length
    Write-Host "✓ Hash: $hash" -ForegroundColor Green
    Write-Host "✓ Size: $([math]::Round($fileSize / 1MB, 2)) MB" -ForegroundColor Green
}
catch {
    Write-Error "Failed to generate hash: $_"
    exit 1
}

# Upload release notes (if provided)
if ($ReleaseNotesPath -and (Test-Path $ReleaseNotesPath)) {
    Write-Host "`n[5/7] Uploading release notes..." -ForegroundColor Cyan
    try {
        Add-PnPFile -Path $ReleaseNotesPath -Folder "$LibraryPath/$Version" | Out-Null
        $releaseNotesContent = Get-Content $ReleaseNotesPath -Raw
        Write-Host "✓ Release notes uploaded" -ForegroundColor Green
    }
    catch {
        Write-Warning "Failed to upload release notes: $_"
        $releaseNotesContent = ""
    }
}
else {
    Write-Host "`n[5/7] Skipping release notes (not provided)" -ForegroundColor Yellow
    $releaseNotesContent = ""
}

# Get SharePoint URLs
$baseUrl = $SharePointSiteUrl.TrimEnd('/')
$msixUrl = "$baseUrl/$LibraryPath/$Version/$msixFileName"
$appInstallerUrl = "$baseUrl/$LibraryPath/InstallVibe.appinstaller"
$releaseNotesUrl = if ($ReleaseNotesPath) { "$baseUrl/$LibraryPath/$Version/release-notes-$Version.md" } else { "" }

# Create version.json
Write-Host "`n[6/7] Creating version.json..." -ForegroundColor Cyan
try {
    $versionManifest = @{
        version            = $Version
        releaseDate        = (Get-Date).ToUniversalTime().ToString("o")
        updateType         = $UpdateType
        appInstallerUrl    = $appInstallerUrl
        msixPackageUrl     = $msixUrl
        fileHash           = $hash
        fileSize           = $fileSize
        minimumVersion     = if ($MinimumVersion) { $MinimumVersion } else { $null }
        releaseNotes       = $releaseNotesContent
        requiresRestart    = $true
        isMandatory        = $IsMandatory.IsPresent
        releaseNotesUrl    = if ($releaseNotesUrl) { $releaseNotesUrl } else { $null }
    }

    $versionJson = $versionManifest | ConvertTo-Json -Depth 10
    $tempVersionFile = [System.IO.Path]::GetTempFileName()
    $versionJson | Out-File -FilePath $tempVersionFile -Encoding UTF8 -NoNewline

    Add-PnPFile -Path $tempVersionFile -Folder $LibraryPath -Values @{} | Out-Null
    Remove-Item $tempVersionFile

    Write-Host "✓ version.json updated" -ForegroundColor Green
}
catch {
    Write-Error "Failed to create version.json: $_"
    exit 1
}

# Update AppInstaller file (if requested)
if ($UpdateAppInstaller) {
    Write-Host "`n[7/7] Updating InstallVibe.appinstaller..." -ForegroundColor Cyan
    Write-Warning "AppInstaller update not yet implemented. Please update manually:"
    Write-Host "  1. Update Version attribute to: $Version.0" -ForegroundColor Yellow
    Write-Host "  2. Update MainPackage/MainBundle Uri to: $msixUrl" -ForegroundColor Yellow
    Write-Host "  3. Upload to SharePoint: $appInstallerUrl" -ForegroundColor Yellow
}
else {
    Write-Host "`n[7/7] Skipping AppInstaller update (use -UpdateAppInstaller to update)" -ForegroundColor Yellow
}

# Summary
Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  ✓ UPDATE PUBLISHED SUCCESSFULLY" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`nVersion:            $Version" -ForegroundColor Cyan
Write-Host "Update Type:        $UpdateType" -ForegroundColor Cyan
Write-Host "File Hash:          $hash" -ForegroundColor Cyan
Write-Host "MSIX URL:           $msixUrl" -ForegroundColor Cyan
Write-Host "AppInstaller URL:   $appInstallerUrl" -ForegroundColor Cyan
Write-Host "`nUsers will be notified of this update within 4 hours (or on next app launch)." -ForegroundColor Yellow

# Disconnect
Disconnect-PnPOnline
