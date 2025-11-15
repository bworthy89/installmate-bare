# Package script for InstallVibe
# Creates MSIX package for distribution

param(
    [string]$Configuration = "Release"
)

Write-Host "Packaging InstallVibe..." -ForegroundColor Cyan

# Navigate to solution root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
Set-Location $solutionRoot

# Build in Release configuration
Write-Host "Building Release configuration..." -ForegroundColor Yellow
& "$scriptPath\build.ps1" -Configuration $Configuration

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Cannot create package." -ForegroundColor Red
    exit 1
}

# Create MSIX package
Write-Host "Creating MSIX package..." -ForegroundColor Yellow
# TODO: Add MSIX packaging command
# msbuild src/InstallVibe/InstallVibe.csproj /t:Publish /p:Configuration=Release /p:Platform=x64

Write-Host "Package created successfully!" -ForegroundColor Green
Write-Host "Package location: build/Packages/" -ForegroundColor Cyan
