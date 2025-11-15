# Build Scripts

PowerShell scripts for building, testing, and packaging InstallVibe.

## Available Scripts

### build.ps1
Builds the solution.

```powershell
.\build.ps1                    # Debug build
.\build.ps1 -Configuration Release  # Release build
```

### test.ps1
Runs all tests.

```powershell
.\test.ps1                     # Run tests
.\test.ps1 -Configuration Release  # Run tests on Release build
```

### clean.ps1
Cleans build artifacts.

```powershell
.\clean.ps1                    # Clean all build outputs
```

### package.ps1
Creates MSIX package.

```powershell
.\package.ps1                  # Create Release package
```

### sign.ps1
Signs the MSIX package with code signing certificate.

```powershell
.\sign.ps1                     # Sign package
```

### deploy-sharepoint.ps1
Deploys package to SharePoint for distribution.

```powershell
.\deploy-sharepoint.ps1        # Upload to SharePoint
```

## Prerequisites

- PowerShell 5.1 or later
- .NET 8 SDK
- Visual Studio 2022 (for MSIX packaging)
- Code signing certificate (for sign.ps1)

## Execution Policy

If scripts won't run, you may need to set execution policy:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```
