# InstallVibe - Development Environment Setup

Complete guide to setting up your development environment for InstallVibe.

---

## 📋 Prerequisites

### Required Software

1. **Windows 10/11**
   - Windows 10 version 1809 (build 17763) or later
   - Windows 11 recommended for best WinUI 3 experience

2. **Visual Studio 2022** (Version 17.8 or later)
   - Workloads required:
     - `.NET Desktop Development`
     - `Universal Windows Platform development`
   - Individual components:
     - `Windows App SDK C# Templates`
     - `WinUI 3 Templates`

3. **.NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0
   - Verify installation: `dotnet --version`

4. **Git**
   - Download from: https://git-scm.com/
   - Optional: GitHub Desktop or Git CLI

### Optional Tools

- **GitHub CLI** (`gh`) for creating PRs from command line
- **Windows Terminal** for better command-line experience
- **OpenSSL** (for generating RSA keys - only needed for production keys)

---

## 🚀 Quick Start (5 Minutes)

### 1. Clone the Repository

```powershell
# Clone the repository
git clone https://github.com/bworthy89/installmate-bare.git
cd installmate-bare
```

### 2. Restore Packages

```powershell
# Restore NuGet packages
dotnet restore
```

### 3. Build the Solution

```powershell
# Build with x64 platform (required for WinUI 3)
dotnet build -p:Platform=x64
```

### 4. Run the Application

**Option A: Using Visual Studio**
1. Open `InstallVibe.sln` in Visual Studio 2022
2. Set platform to **x64** (dropdown at top)
3. Press **F5** to run with debugging

**Option B: Using Command Line**
```powershell
dotnet run --project src\InstallVibe\InstallVibe.csproj -p:Platform=x64
```

### 5. Activate with Test Key

When the app starts, enter one of these test keys on the activation screen:

**Tech License (Read-Only):**
```
TEST1-TEST1-TEST1-TEST1-TEST1
```

**Admin License (Full Access):**
```
ADMIN-ADMIN-ADMIN-ADMIN-ADMIN
```

✅ **You're ready to develop!**

---

## ⚙️ Configuration

### appsettings.json

The configuration file is located at `src/InstallVibe/appsettings.json`.

**Default configuration works out of the box** for local development. SharePoint features will be disabled until configured.

#### SharePoint Configuration (Optional)

To enable SharePoint integration:

1. **Create Azure AD App Registration**
   - Go to Azure Portal → Azure Active Directory → App registrations
   - Create new registration
   - Note the **Application (client) ID** and **Directory (tenant) ID**

2. **Generate/Upload Certificate**
   - Generate: `openssl genrsa -out private_key.pem 2048`
   - Generate public: `openssl rsa -in private_key.pem -pubout -out public_key.pem`
   - Upload public key to Azure AD app registration
   - Install certificate in Windows Certificate Store (LocalMachine\My)
   - Copy the certificate thumbprint

3. **Update appsettings.json**
```json
{
  "SharePoint": {
    "TenantId": "your-tenant-id-from-azure",
    "ClientId": "your-client-id-from-azure",
    "CertificateThumbprint": "your-certificate-thumbprint",
    "SiteUrl": "https://yourtenant.sharepoint.com/sites/InstallVibe"
  }
}
```

---

## 🏗️ Project Structure

```
InstallVibe/
├── src/
│   ├── InstallVibe/              # Main WinUI 3 application
│   │   ├── Views/                # XAML pages
│   │   ├── ViewModels/           # MVVM ViewModels
│   │   ├── Converters/           # Value converters
│   │   ├── Services/             # UI services
│   │   └── appsettings.json      # Configuration
│   ├── InstallVibe.Core/         # Business logic
│   │   ├── Models/               # Domain models
│   │   ├── Services/             # Core services
│   │   └── Exceptions/           # Custom exceptions
│   ├── InstallVibe.Data/         # Data access (EF Core + SQLite)
│   │   ├── Context/              # DbContext
│   │   ├── Entities/             # Database entities
│   │   └── Repositories/         # Repository pattern
│   ├── InstallVibe.Infrastructure/ # Cross-cutting concerns
│   │   ├── Configuration/        # Config models
│   │   ├── Security/             # Cryptography, validation
│   │   └── Device/               # Device information
│   └── InstallVibe.Tests/        # Unit & integration tests
├── tools/
│   ├── keygen/                   # Product key generator
│   └── scripts/                  # Build automation scripts
├── packaging/                    # MSIX packaging resources
└── docs/                         # Documentation
```

---

## 🔧 Development Workflow

### Building

```powershell
# Clean build
dotnet clean
dotnet build -p:Platform=x64

# Build specific project
dotnet build src/InstallVibe.Core/InstallVibe.Core.csproj

# Build in Release mode
dotnet build -c Release -p:Platform=x64
```

### Running Tests

```powershell
# Run all tests
dotnet test

# Run tests with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test src/InstallVibe.Tests/InstallVibe.Tests.csproj
```

### Database Migrations

The app uses **SQLite** with **Entity Framework Core**.

```powershell
# Add a new migration
dotnet ef migrations add MigrationName --project src/InstallVibe.Data --startup-project src/InstallVibe

# Update database
dotnet ef database update --project src/InstallVibe.Data --startup-project src/InstallVibe

# Remove last migration
dotnet ef migrations remove --project src/InstallVibe.Data --startup-project src/InstallVibe
```

### Debugging Tips

1. **Enable verbose logging** - Set environment variable:
   ```powershell
   $env:DOTNET_ENVIRONMENT = "Development"
   ```

2. **View logs** - Logs are written to:
   ```
   %LOCALAPPDATA%\InstallVibe\Logs\installvibe-{date}.log
   ```

3. **View database** - Database location:
   ```
   %LOCALAPPDATA%\InstallVibe\Data\installvibe.db
   ```
   Use tools like **DB Browser for SQLite** to inspect

4. **Clear app data** (reset activation):
   ```powershell
   Remove-Item -Recurse -Force "$env:LOCALAPPDATA\InstallVibe"
   ```

---

## 🔑 Product Key Testing

### Development Test Keys

Two test keys are built-in for development:

| Key | License Type | Access Level | Expires |
|-----|--------------|--------------|---------|
| `TEST1-TEST1-TEST1-TEST1-TEST1` | Tech | Read-only guides | Never |
| `ADMIN-ADMIN-ADMIN-ADMIN-ADMIN` | Admin | Full access + editor | Never |

### Generating Real Product Keys

For production or advanced testing:

```powershell
cd tools/keygen

# Generate RSA key pair (first time only)
openssl genrsa -out private_key.pem 2048
openssl rsa -in private_key.pem -pubout -out public_key.pem

# Generate Tech license expiring Dec 31, 2025
dotnet run -- --type Tech --customer 12345 --expires 2025-12-31

# Generate perpetual Admin license
dotnet run -- --type Admin --customer 67890 --perpetual
```

**⚠️ Important:** Keep `private_key.pem` secure! Never commit to source control.

---

## 🐛 Common Issues & Solutions

### Issue: "Platform 'AnyCPU' is not supported"

**Solution:** Always build with **x64** platform:
```powershell
dotnet build -p:Platform=x64
```

Or in Visual Studio: Set platform dropdown to **x64**

### Issue: "appsettings.json not found"

**Solution:** The file exists at `src/InstallVibe/appsettings.json`. If missing:
```powershell
git pull origin main
dotnet restore
```

### Issue: "Cannot access file because it is being used"

**Solution:** Close the running app before rebuilding:
- Close InstallVibe.exe
- Or in Visual Studio: Stop debugging (Shift+F5)

### Issue: InvalidCastException on ActivationPage

**Solution:** This was fixed in commit `cc15728`. Update to latest:
```powershell
git pull origin main
dotnet build -p:Platform=x64
```

### Issue: SharePoint features not working

**Solution:** SharePoint is optional. For local development, you don't need it configured. If you want to test SharePoint integration, see the SharePoint Configuration section above.

---

## 📦 Dependencies

### Main Packages

- **WinUI 3** - Modern Windows UI framework
- **.NET 8** - Latest .NET runtime
- **Entity Framework Core 8** - ORM for database access
- **Microsoft Graph SDK 5.40** - SharePoint Online integration
- **Serilog** - Structured logging
- **CommunityToolkit.Mvvm** - MVVM framework

### All packages are managed centrally in `Directory.Packages.props`

---

## 🚢 Packaging & Deployment

### Create MSIX Package

```powershell
# Using build script
.\tools\scripts\package.ps1

# Or manually
dotnet publish src/InstallVibe/InstallVibe.csproj -c Release -p:Platform=x64
```

### Testing MSIX Package

```powershell
# Install locally
Add-AppxPackage -Path "path\to\package.msix"

# Uninstall
Get-AppxPackage InstallVibe | Remove-AppxPackage
```

---

## 🔄 Git Workflow

### Before Making Changes

```powershell
git checkout main
git pull origin main
git checkout -b feature/your-feature-name
```

### After Making Changes

```powershell
# Stage changes
git add .

# Commit
git commit -m "Description of changes"

# Push to GitHub
git push origin feature/your-feature-name
```

### Create Pull Request

**Option A: GitHub Web Interface**
1. Go to repository on GitHub
2. Click "Compare & pull request"
3. Fill in description
4. Create pull request

**Option B: GitHub CLI**
```powershell
gh pr create --title "Feature: Description" --body "Detailed description"
```

---

## 📚 Additional Resources

- **Architecture Documentation**: [ARCHITECTURE.md](ARCHITECTURE.md)
- **Project Structure**: [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)
- **API Documentation**: Generated from XML comments
- **WinUI 3 Docs**: https://learn.microsoft.com/windows/apps/winui/
- **.NET 8 Docs**: https://learn.microsoft.com/dotnet/

---

## 💡 Tips for New Developers

1. **Start with ViewModels** - The MVVM pattern means most logic is in ViewModels
2. **Use test keys** - No need to generate real keys for development
3. **Check logs** - Serilog writes detailed logs to help debug issues
4. **Use Hot Reload** - Visual Studio 2022 supports XAML Hot Reload (Alt+F10)
5. **Database Browser** - Install "DB Browser for SQLite" to inspect the database
6. **Watch for platform** - Always use **x64**, not AnyCPU
7. **SharePoint is optional** - Don't worry about configuring it for local dev

---

## 🆘 Getting Help

- **Build Issues**: Check this guide's "Common Issues" section
- **Code Questions**: Review ARCHITECTURE.md
- **Feature Requests**: Create a GitHub issue
- **Team Questions**: Contact the development team

---

## ✅ Verification Checklist

After setup, verify everything works:

- [ ] Solution builds without errors: `dotnet build -p:Platform=x64`
- [ ] Tests pass: `dotnet test`
- [ ] App runs: `dotnet run --project src\InstallVibe\InstallVibe.csproj -p:Platform=x64`
- [ ] Test key activates: `TEST1-TEST1-TEST1-TEST1-TEST1`
- [ ] Logs appear in: `%LOCALAPPDATA%\InstallVibe\Logs\`
- [ ] Database creates in: `%LOCALAPPDATA%\InstallVibe\Data\`

**If all checkboxes are complete, you're ready to develop!** 🎉

---

*Last Updated: 2025-01-15*
*InstallVibe v1.0.0*
