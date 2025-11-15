# Test script for InstallVibe
# Runs all unit and integration tests

param(
    [string]$Configuration = "Debug"
)

Write-Host "Running InstallVibe tests..." -ForegroundColor Cyan

# Navigate to solution root
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
$solutionRoot = Split-Path -Parent (Split-Path -Parent $scriptPath)
Set-Location $solutionRoot

# Run tests
Write-Host "Executing tests..." -ForegroundColor Yellow
dotnet test InstallVibe.sln -c $Configuration --no-build --verbosity normal

if ($LASTEXITCODE -ne 0) {
    Write-Host "Tests failed!" -ForegroundColor Red
    exit 1
}

Write-Host "All tests passed!" -ForegroundColor Green
