# Clean script for InstallVibe
# Removes all build artifacts

Write-Host "Cleaning InstallVibe build artifacts..." -ForegroundColor Cyan

# Navigate to solution root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
Set-Location $solutionRoot

# Clean solution
Write-Host "Cleaning solution..." -ForegroundColor Yellow
dotnet clean InstallVibe.sln

# Remove build folders
if (Test-Path "build") {
    Write-Host "Removing build folder..." -ForegroundColor Yellow
    Remove-Item -Path "build" -Recurse -Force
}

# Remove bin/obj folders
Get-ChildItem -Path "src" -Include "bin","obj" -Recurse -Directory | ForEach-Object {
    Write-Host "Removing $_" -ForegroundColor Yellow
    Remove-Item $_.FullName -Recurse -Force
}

Write-Host "Clean completed successfully!" -ForegroundColor Green
