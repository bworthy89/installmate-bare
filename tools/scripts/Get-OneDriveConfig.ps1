# Get OneDrive/SharePoint Site and Drive IDs for InstallVibe Configuration
# This script uses your existing SharePoint credentials from appsettings.json

param(
    [Parameter(Mandatory=$false)]
    [string]$SiteUrl = "https://yourtenant.sharepoint.com/sites/InstallVibe",

    [Parameter(Mandatory=$false)]
    [string]$LibraryName = "Guides"
)

Write-Host "=== InstallVibe OneDrive Configuration Helper ===" -ForegroundColor Cyan
Write-Host ""

# Read credentials from appsettings.json
$appSettingsPath = Join-Path $PSScriptRoot "..\..\src\InstallVibe\appsettings.json"
if (-not (Test-Path $appSettingsPath)) {
    Write-Host "ERROR: Could not find appsettings.json at: $appSettingsPath" -ForegroundColor Red
    exit 1
}

$appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
$tenantId = $appSettings.SharePoint.TenantId
$clientId = $appSettings.SharePoint.ClientId
$clientSecret = $appSettings.SharePoint.ClientSecret

if (-not $tenantId -or -not $clientId -or -not $clientSecret) {
    Write-Host "ERROR: SharePoint credentials not found in appsettings.json" -ForegroundColor Red
    Write-Host "Please configure SharePoint settings first." -ForegroundColor Yellow
    exit 1
}

Write-Host "Using credentials from appsettings.json" -ForegroundColor Green
Write-Host "  Tenant ID: $tenantId" -ForegroundColor Gray
Write-Host "  Client ID: $clientId" -ForegroundColor Gray
Write-Host ""

# Get access token
Write-Host "Step 1: Getting access token..." -ForegroundColor Yellow
$tokenUrl = "https://login.microsoftonline.com/$tenantId/oauth2/v2.0/token"
$tokenBody = @{
    client_id     = $clientId
    client_secret = $clientSecret
    scope         = "https://graph.microsoft.com/.default"
    grant_type    = "client_credentials"
}

try {
    $tokenResponse = Invoke-RestMethod -Uri $tokenUrl -Method Post -Body $tokenBody -ContentType "application/x-www-form-urlencoded"
    $accessToken = $tokenResponse.access_token
    Write-Host "  ✓ Access token obtained" -ForegroundColor Green
}
catch {
    Write-Host "  ✗ Failed to get access token" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Extract site path from URL
$siteUrlParts = $SiteUrl -replace "https://", "" -split "/"
$hostname = $siteUrlParts[0]
$sitePath = "/" + ($siteUrlParts[1..($siteUrlParts.Length-1)] -join "/")

# Get Site ID
Write-Host ""
Write-Host "Step 2: Getting Site ID..." -ForegroundColor Yellow
Write-Host "  Site URL: $SiteUrl" -ForegroundColor Gray

$siteApiUrl = "https://graph.microsoft.com/v1.0/sites/$hostname`:$sitePath"
$headers = @{
    Authorization = "Bearer $accessToken"
    Accept = "application/json"
}

try {
    $siteResponse = Invoke-RestMethod -Uri $siteApiUrl -Headers $headers -Method Get
    $siteId = $siteResponse.id
    Write-Host "  ✓ Site ID: $siteId" -ForegroundColor Green
}
catch {
    Write-Host "  ✗ Failed to get Site ID" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Make sure the Site URL is correct: $SiteUrl" -ForegroundColor Yellow
    exit 1
}

# Get Drive ID
Write-Host ""
Write-Host "Step 3: Getting Drive ID for library '$LibraryName'..." -ForegroundColor Yellow

$drivesApiUrl = "https://graph.microsoft.com/v1.0/sites/$siteId/drives"

try {
    $drivesResponse = Invoke-RestMethod -Uri $drivesApiUrl -Headers $headers -Method Get

    # Find the drive matching the library name
    $targetDrive = $drivesResponse.value | Where-Object { $_.name -eq $LibraryName }

    if (-not $targetDrive) {
        Write-Host "  Available libraries:" -ForegroundColor Yellow
        $drivesResponse.value | ForEach-Object {
            Write-Host "    - $($_.name) (ID: $($_.id))" -ForegroundColor Gray
        }
        Write-Host ""
        Write-Host "  Library '$LibraryName' not found. Please specify a library from the list above." -ForegroundColor Red
        Write-Host "  Usage: .\Get-OneDriveConfig.ps1 -LibraryName 'YourLibraryName'" -ForegroundColor Yellow
        exit 1
    }

    $driveId = $targetDrive.id
    Write-Host "  ✓ Drive ID: $driveId" -ForegroundColor Green
}
catch {
    Write-Host "  ✗ Failed to get Drive ID" -ForegroundColor Red
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Display configuration
Write-Host ""
Write-Host "=== Configuration Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Add these values to your appsettings.json under the 'OneDrive' section:" -ForegroundColor Green
Write-Host ""
Write-Host '  "OneDrive": {' -ForegroundColor White
Write-Host '    "Enabled": true,' -ForegroundColor White
Write-Host "    `"SiteId`": `"$siteId`"," -ForegroundColor Yellow
Write-Host "    `"DriveId`": `"$driveId`"," -ForegroundColor Yellow
Write-Host '    "FolderPath": "/InstallVibe/Guides",' -ForegroundColor White
Write-Host '    "SyncIntervalMinutes": 15,' -ForegroundColor White
Write-Host '    "SyncOnStartup": true' -ForegroundColor White
Write-Host '  }' -ForegroundColor White
Write-Host ""

# Optionally update appsettings.json automatically
$updateConfig = Read-Host "Would you like to automatically update appsettings.json? (y/n)"
if ($updateConfig -eq 'y' -or $updateConfig -eq 'Y') {
    try {
        # Read current config
        $config = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

        # Update OneDrive settings
        $config.OneDrive.Enabled = $true
        $config.OneDrive.SiteId = $siteId
        $config.OneDrive.DriveId = $driveId

        # Save back to file
        $config | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath

        Write-Host "✓ appsettings.json updated successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "You can now run InstallVibe and use OneDrive sync!" -ForegroundColor Cyan
    }
    catch {
        Write-Host "✗ Failed to update appsettings.json automatically" -ForegroundColor Red
        Write-Host "Please update manually using the values above." -ForegroundColor Yellow
    }
}
else {
    Write-Host "Please manually update appsettings.json with the values above." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
