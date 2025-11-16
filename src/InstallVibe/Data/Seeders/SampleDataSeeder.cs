using InstallVibe.Core.Constants;
using InstallVibe.Core.Models.Activation;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using Microsoft.EntityFrameworkCore;

namespace InstallVibe.Data.Seeders;

/// <summary>
/// Seeds the database with sample guide data for development and testing.
/// </summary>
public class SampleDataSeeder
{
    private readonly IGuideService _guideService;

    public SampleDataSeeder(IGuideService guideService)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
    }

    /// <summary>
    /// Seeds sample guides if the database is empty.
    /// </summary>
    public async Task SeedAsync()
    {
        // Check if guides already exist
        var existingGuides = await _guideService.GetAllGuidesAsync();
        if (existingGuides.Any())
        {
            return; // Already seeded
        }

        var guides = new List<Guide>
        {
            CreateWindowsServer2022InstallationGuide(),
            CreateActiveDirectoryConfigurationGuide(),
            CreateSQLServerInstallationGuide(),
            CreateNetworkTroubleshootingGuide(),
            CreateOffice365MigrationGuide()
        };

        foreach (var guide in guides)
        {
            await _guideService.SaveGuideAsync(guide);
        }
    }

    private Guide CreateWindowsServer2022InstallationGuide()
    {
        var guideId = Guid.NewGuid().ToString();

        return new Guide
        {
            GuideId = guideId,
            Title = "Windows Server 2022 Installation",
            Version = "1.0.0",
            Category = GuideCategories.Installation,
            Description = "Complete guide for installing Windows Server 2022 Standard Edition on physical or virtual hardware.",
            RequiredLicense = LicenseType.Tech,
            IsPublished = true,
            LastModified = DateTime.UtcNow.AddDays(-25),
            Author = "System Administrator",
            EstimatedMinutes = 45,
            Tags = new List<string> { "windows", "server", "2022", "installation", "datacenter" },
            TargetAudience = "System Administrators, IT Technicians",
            Difficulty = GuideDifficulty.Medium,
            Steps = new List<Step>
            {
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 1,
                    Title = "Prepare Installation Media",
                    Content = @"# Prepare Installation Media

Before starting the installation, ensure you have:

- Windows Server 2022 ISO file
- USB drive (8GB minimum) or DVD
- Valid product key

## Create Bootable USB
1. Download Rufus or use Windows Media Creation Tool
2. Insert USB drive
3. Select ISO file
4. Click **Start** to create bootable media

**Important:** Backup any data on the USB drive as it will be formatted.

## Checklist
- [ ] USB/DVD prepared
- [ ] Product key available
- [ ] Installation media verified",
                    MediaReferences = new List<MediaReference>
                    {
                        new MediaReference
                        {
                            MediaId = Guid.NewGuid().ToString(),
                            MediaType = "image",
                            Caption = "Rufus USB creation tool"
                        }
                    }
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 2,
                    Title = "Boot from Installation Media",
                    Content = @"# Boot from Installation Media

1. Insert the USB drive or DVD into the server
2. Power on or restart the server
3. Press **F2**, **F12**, **DEL**, or **ESC** (varies by manufacturer) to access BIOS
4. Change boot order to prioritize USB/DVD
5. Save and exit BIOS

The Windows Setup screen should appear within 30 seconds.

## Checklist
- [ ] USB/DVD inserted
- [ ] BIOS boot order configured
- [ ] Windows Setup screen visible"
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 3,
                    Title = "Configure Installation Settings",
                    Content = @"# Configure Installation Settings

1. Select language, time, and keyboard preferences
2. Click **Install now**
3. Enter product key or select **I don't have a product key**
4. Select **Windows Server 2022 Standard (Desktop Experience)**
5. Accept license terms
6. Choose **Custom: Install Windows only (advanced)**",
                    MediaReferences = new List<MediaReference>
                    {
                        new MediaReference
                        {
                            MediaId = Guid.NewGuid().ToString(),
                            MediaType = "image",
                            Caption = "Edition selection screen",
                            OrderIndex = 1
                        }
                    }
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 4,
                    Title = "Partition and Install",
                    Content = @"# Partition and Install

## Disk Partitioning
1. Select target drive for installation
2. Delete existing partitions if necessary (data will be lost)
3. Create new partition or use unallocated space
4. Click **Next**

Installation will take 15-30 minutes depending on hardware.

## Post-Installation
After restart:
1. Set Administrator password (minimum 8 characters, complex)
2. Log in with Administrator account

**Password Requirements:**
- At least 8 characters
- Mix of uppercase, lowercase, numbers, symbols

## Checklist
- [ ] Disk partitioned correctly
- [ ] Installation completed successfully
- [ ] Administrator password set
- [ ] Successfully logged in"
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 5,
                    Title = "Initial Configuration",
                    Content = @"# Initial Configuration

Run through Server Manager initial setup:

1. Set server name
2. Configure network settings (static IP recommended)
3. Join domain (if applicable)
4. Enable Windows Update
5. Configure Windows Firewall

Use **sconfig** command for quick configuration in PowerShell.

```powershell
# Check Windows version
Get-ComputerInfo | Select-Object WindowsProductName, WindowsVersion

# Set static IP example
New-NetIPAddress -InterfaceAlias 'Ethernet' -IPAddress 192.168.1.10 -PrefixLength 24 -DefaultGateway 192.168.1.1
```

## Checklist
- [ ] Server name configured
- [ ] Network settings configured
- [ ] Windows Update enabled
- [ ] Initial updates installed"
                }
            },
            Metadata = new GuideMetadata
            {
                Prerequisites = new List<string>(),
                RelatedGuides = new List<string>(),
                ChangeLog = new List<ChangeLogEntry>
                {
                    new ChangeLogEntry
                    {
                        Version = "1.0.0",
                        Date = DateTime.UtcNow.AddDays(-25),
                        Changes = "Initial guide creation",
                        Author = "System Administrator"
                    }
                }
            }
        };
    }

    private Guide CreateActiveDirectoryConfigurationGuide()
    {
        var guideId = Guid.NewGuid().ToString();

        return new Guide
        {
            GuideId = guideId,
            Title = "Active Directory Domain Services Configuration",
            Version = "2.1.0",
            Category = GuideCategories.Configuration,
            Description = "Step-by-step guide to configure Active Directory Domain Services (AD DS) and create a new domain.",
            RequiredLicense = LicenseType.Admin,
            IsPublished = true,
            LastModified = DateTime.UtcNow.AddDays(-15),
            Author = "Domain Administrator",
            EstimatedMinutes = 60,
            Tags = new List<string> { "active-directory", "domain", "ldap", "authentication", "windows" },
            TargetAudience = "Senior System Administrators, Domain Administrators",
            Difficulty = GuideDifficulty.Hard,
            Steps = new List<Step>
            {
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 1,
                    Title = "Prerequisites Check",
                    Content = @"# Prerequisites Check

Verify the following before proceeding:

## System Requirements
- Windows Server 2022 installed and updated
- Static IP address configured
- DNS server address configured (can point to self)
- Server renamed (not default WIN-XXXX format)
- Administrator privileges

## Network Configuration
Verify network settings:
```powershell
Get-NetIPConfiguration
Get-DnsClientServerAddress
```

Expected output: Static IP, not DHCP

## Server Name
```powershell
hostname
```

Should be meaningful name like DC01, ADDS01, etc.

## Checklist
- [ ] Windows Server 2022 installed
- [ ] Static IP configured
- [ ] DNS configured
- [ ] Server renamed
- [ ] Administrator access confirmed",
                    ExpectedDurationMinutes = 10
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 2,
                    Title = "Install AD DS Role",
                    Content = @"# Install AD DS Role

## Using Server Manager
1. Open Server Manager
2. Click **Manage** → **Add Roles and Features**
3. Click **Next** through wizard until **Server Roles**
4. Check **Active Directory Domain Services**
5. Click **Add Features** when prompted
6. Click **Next** through to **Install**

## Using PowerShell (Alternative)
```powershell
Install-WindowsFeature -Name AD-Domain-Services -IncludeManagementTools
```

Installation takes 2-5 minutes.

## Checklist
- [ ] AD DS role installed
- [ ] Management tools installed
- [ ] No installation errors",
                    MediaReferences = new List<MediaReference>
                    {
                        new MediaReference
                        {
                            MediaId = Guid.NewGuid().ToString(),
                            MediaType = "image",
                            Caption = "AD DS role selection in Server Manager",
                            OrderIndex = 1
                        }
                    },
                    ExpectedDurationMinutes = 5
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 3,
                    Title = "Promote to Domain Controller",
                    Content = @"# Promote to Domain Controller

## Start Promotion Wizard
1. In Server Manager, click notification flag
2. Click **Promote this server to a domain controller**

## Configure Deployment
Select: **Add a new forest**
- Root domain name: `contoso.local` (or your domain)
- Click **Next**

## Domain Controller Options
- Forest/Domain functional level: **Windows Server 2016** or higher
- Check **Domain Name System (DNS) server**
- Check **Global Catalog (GC)**
- Enter **Directory Services Restore Mode (DSRM) password**

⚠️ **DSRM Password:** Store securely - needed for disaster recovery!

## Additional Options
- NetBIOS name: Auto-generated (e.g., CONTOSO)
- Review and accept

## Paths
Accept defaults or customize:
- Database: `C:\Windows\NTDS`
- Log files: `C:\Windows\NTDS`
- SYSVOL: `C:\Windows\SYSVOL`

## Review and Install
- Review summary
- Click **Install**

Server will restart automatically after installation (10-15 minutes).

## Checklist
- [ ] New forest created
- [ ] Domain name configured
- [ ] DSRM password set and documented
- [ ] DNS installed
- [ ] Server restarted successfully",
                    WarningLevel = WarningLevel.Critical,
                    ExpectedDurationMinutes = 30
                }
            },
            Metadata = new GuideMetadata
            {
                Prerequisites = new List<string>(),
                RelatedGuides = new List<string>(),
                ChangeLog = new List<ChangeLogEntry>
                {
                    new ChangeLogEntry
                    {
                        Version = "2.1.0",
                        Date = DateTime.UtcNow.AddDays(-15),
                        Changes = "Added PowerShell commands for verification and post-configuration",
                        Author = "Domain Administrator"
                    }
                }
            }
        };
    }

    private Guide CreateSQLServerInstallationGuide()
    {
        return new Guide
        {
            GuideId = Guid.NewGuid().ToString(),
            Title = "SQL Server 2022 Express Installation",
            Version = "1.2.0",
            Category = GuideCategories.Installation,
            Description = "Install SQL Server 2022 Express edition for small-scale applications and development environments.",
            RequiredLicense = LicenseType.Tech,
            IsPublished = true,
            LastModified = DateTime.UtcNow.AddDays(-8),
            Author = "Database Administrator",
            EstimatedMinutes = 30,
            Tags = new List<string> { "sql-server", "database", "2022", "express", "microsoft" },
            TargetAudience = "Database Administrators, Developers, IT Technicians",
            Difficulty = GuideDifficulty.Easy,
            Steps = new List<Step>
            {
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 1,
                    Title = "Download SQL Server 2022 Express",
                    Content = @"# Download SQL Server 2022 Express

1. Visit Microsoft Download Center
2. Search for ""SQL Server 2022 Express""
3. Select **Download** for your architecture (x64)
4. Choose installation type:
   - **Basic**: Recommended for quick setup
   - **Custom**: For advanced configuration
   - **Download Media**: For offline installation

For this guide, select **Basic** installation.

Download size: ~280 MB",
                    ExpectedDurationMinutes = 5
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 2,
                    Title = "Run Installation Wizard",
                    Content = @"# Run Installation Wizard

1. Run downloaded executable as **Administrator**
2. Accept license terms
3. Select installation type: **Basic**
4. Choose installation location (default: `C:\Program Files\Microsoft SQL Server`)
5. Click **Install**

Installation will:
- Download additional components (~700 MB)
- Install SQL Server Database Engine
- Configure default instance

This takes 10-15 minutes depending on internet speed.",
                    MediaReferences = new List<MediaReference>
                    {
                        new MediaReference
                        {
                            MediaId = Guid.NewGuid().ToString(),
                            MediaType = "image",
                            Caption = "SQL Server installation type selection"
                        }
                    },
                    ExpectedDurationMinutes = 15
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 3,
                    Title = "Post-Installation Configuration",
                    Content = @"# Post-Installation Configuration

## Note Installation Details
After installation, note:
- **Instance name:** SQLEXPRESS (default)
- **Connection string:** `localhost\SQLEXPRESS` or `.\SQLEXPRESS`
- **Authentication:** Windows Authentication (default)

## Enable TCP/IP (For Remote Connections)
1. Open **SQL Server Configuration Manager**
2. Expand **SQL Server Network Configuration**
3. Click **Protocols for SQLEXPRESS**
4. Right-click **TCP/IP** → **Enable**
5. Restart SQL Server service

## Configure Windows Firewall
```powershell
New-NetFirewallRule -DisplayName ""SQL Server"" `
    -Direction Inbound -Protocol TCP -LocalPort 1433 -Action Allow
```

## Verify Installation
```powershell
Get-Service -Name MSSQL`$SQLEXPRESS
# Should show 'Running'
```",
                    ExpectedDurationMinutes = 10
                }
            },
            Metadata = new GuideMetadata
            {
                ChangeLog = new List<ChangeLogEntry>
                {
                    new ChangeLogEntry
                    {
                        Version = "1.2.0",
                        Date = DateTime.UtcNow.AddDays(-8),
                        Changes = "Added firewall configuration command",
                        Author = "Database Administrator"
                    }
                }
            }
        };
    }

    private Guide CreateNetworkTroubleshootingGuide()
    {
        return new Guide
        {
            GuideId = Guid.NewGuid().ToString(),
            Title = "Network Connectivity Troubleshooting",
            Version = "3.0.0",
            Category = GuideCategories.Troubleshooting,
            Description = "Systematic approach to diagnosing and resolving network connectivity issues in Windows environments.",
            RequiredLicense = LicenseType.Tech,
            IsPublished = true,
            LastModified = DateTime.UtcNow.AddDays(-3),
            Author = "Network Engineer",
            EstimatedMinutes = 20,
            Tags = new List<string> { "network", "troubleshooting", "connectivity", "dns", "tcp-ip" },
            TargetAudience = "Help Desk Technicians, System Administrators, Network Engineers",
            Difficulty = GuideDifficulty.Easy,
            Steps = new List<Step>
            {
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 1,
                    Title = "Verify Physical Connection",
                    Content = @"# Verify Physical Connection

## Check Network Cable
- Ensure ethernet cable is firmly connected
- Check for damaged cables
- Look for link lights on NIC and switch port
- Try different cable if available

## Check Network Adapter Status
```powershell
Get-NetAdapter | Select-Object Name, Status, LinkSpeed
```

Expected: `Status = Up` and `LinkSpeed = 1 Gbps` or `100 Mbps`

## Checklist
- [ ] Physical cable checked/reconnected
- [ ] Network adapter shows 'Up' status
- [ ] Link lights visible",
                    ExpectedDurationMinutes = 3
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 2,
                    Title = "Check IP Configuration",
                    Content = @"# Check IP Configuration

## View Current Configuration
```powershell
ipconfig /all
```

Look for:
- **IPv4 Address:** Should not be 169.254.x.x (APIPA)
- **Subnet Mask:** Typically 255.255.255.0
- **Default Gateway:** Should be present
- **DNS Servers:** Should be configured

## Common Issues

### APIPA Address (169.254.x.x)
Means DHCP is not working:
```powershell
ipconfig /release
ipconfig /renew
```

### No Default Gateway
Static IP may be misconfigured. Check network settings.

## Reset Network Stack (If Needed)
```powershell
netsh int ip reset
netsh winsock reset
```
**Note:** Requires restart",
                    ExpectedDurationMinutes = 5
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 3,
                    Title = "Test DNS and Connectivity",
                    Content = @"# Test DNS and Connectivity

## Ping Default Gateway
```powershell
ping [gateway-ip]
# Example: ping 192.168.1.1
```

## Test Name Resolution
```powershell
nslookup google.com
```

## Flush DNS Cache
```powershell
ipconfig /flushdns
```

## Test Connectivity
```powershell
Test-NetConnection -ComputerName google.com -Port 443
```

## Checklist
- [ ] Gateway pingable
- [ ] DNS resolution working
- [ ] External connectivity verified",
                    ExpectedDurationMinutes = 7
                }
            },
            Metadata = new GuideMetadata
            {
                ChangeLog = new List<ChangeLogEntry>
                {
                    new ChangeLogEntry
                    {
                        Version = "3.0.0",
                        Date = DateTime.UtcNow.AddDays(-3),
                        Changes = "Major revision with PowerShell commands",
                        Author = "Network Engineer"
                    }
                }
            }
        };
    }

    private Guide CreateOffice365MigrationGuide()
    {
        return new Guide
        {
            GuideId = Guid.NewGuid().ToString(),
            Title = "Office 365 Email Migration",
            Version = "1.5.0",
            Category = GuideCategories.Migration,
            Description = "Migrate user mailboxes from on-premises Exchange Server to Microsoft 365.",
            RequiredLicense = LicenseType.Admin,
            IsPublished = true,
            LastModified = DateTime.UtcNow.AddDays(-12),
            Author = "Exchange Administrator",
            EstimatedMinutes = 120,
            Tags = new List<string> { "office365", "exchange", "migration", "cloud", "email", "m365" },
            TargetAudience = "Exchange Administrators, Cloud Architects",
            Difficulty = GuideDifficulty.Hard,
            Steps = new List<Step>
            {
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 1,
                    Title = "Pre-Migration Assessment",
                    Content = @"# Pre-Migration Assessment

## Environment Inventory
- Number of mailboxes to migrate
- Exchange Server version (2013, 2016, 2019)
- Mailbox sizes and total data volume
- Public folders usage
- Distribution lists

## Office 365 Licensing
Verify licenses:
- E1, E3, or E5 licenses required
- Exchange Online Plan 1 or 2
- Sufficient licenses for all users

## Prerequisites
- Office 365 tenant created and verified
- Exchange certificates valid
- Migration admin account with necessary permissions

## Checklist
- [ ] Mailbox inventory completed
- [ ] Licenses purchased
- [ ] Prerequisites verified
- [ ] Migration plan documented
- [ ] Rollback plan prepared",
                    WarningLevel = WarningLevel.Warning,
                    ExpectedDurationMinutes = 30
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 2,
                    Title = "Create Migration Batch",
                    Content = @"# Create Migration Batch

## Prepare CSV File
Create CSV with columns: EmailAddress

```
EmailAddress
john.doe@contoso.com
jane.smith@contoso.com
```

## Create Migration Batch
```powershell
Connect-ExchangeOnline

New-MigrationBatch -Name ""Batch1"" `
    -SourceEndpoint ""On-Premises"" `
    -CSVData ([System.IO.File]::ReadAllBytes(""C:\migration.csv"")) `
    -AutoStart
```

## Monitor Progress
```powershell
Get-MigrationBatch ""Batch1"" | Format-List Status
```

Migration does NOT delete source mailboxes!",
                    ExpectedDurationMinutes = 45
                },
                new Step
                {
                    StepId = Guid.NewGuid().ToString(),
                    OrderIndex = 3,
                    Title = "Cutover to Office 365",
                    Content = @"# Cutover to Office 365

**Critical Step - Schedule During Maintenance Window**

## Complete Migration Batch
```powershell
Complete-MigrationBatch -Identity ""Batch1""
```

## Update MX Records
1. Go to DNS provider
2. Update MX record to point to Office 365:
   ```
   Priority: 0
   Host: @
   Points to: contoso-com.mail.protection.outlook.com
   ```

## Update Autodiscover
```
autodiscover.contoso.com → autodiscover.outlook.com
```

## Configure SPF Record
```
v=spf1 include:spf.protection.outlook.com -all
```

## Checklist
- [ ] Migration batch completed
- [ ] MX records updated
- [ ] Autodiscover CNAME updated
- [ ] SPF record configured
- [ ] Test email flow",
                    WarningLevel = WarningLevel.Critical,
                    ExpectedDurationMinutes = 45
                }
            },
            Metadata = new GuideMetadata
            {
                ChangeLog = new List<ChangeLogEntry>
                {
                    new ChangeLogEntry
                    {
                        Version = "1.5.0",
                        Date = DateTime.UtcNow.AddDays(-12),
                        Changes = "Added post-migration checklist",
                        Author = "Exchange Administrator"
                    }
                }
            }
        };
    }
}
