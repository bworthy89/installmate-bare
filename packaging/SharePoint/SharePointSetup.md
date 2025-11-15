# SharePoint Setup Guide

Instructions for configuring SharePoint Online for InstallVibe.

## Prerequisites

- SharePoint Online subscription
- Global Administrator or SharePoint Administrator access
- Azure AD app registration permissions

## Step 1: Create SharePoint Site

1. Navigate to SharePoint Admin Center
2. Create a new site: `https://{tenant}.sharepoint.com/sites/InstallVibe`
3. Set appropriate permissions (limit to authorized users)

## Step 2: Create Document Libraries

Create the following document libraries:

### Guides Library
- Name: `Guides`
- Columns:
  - Title (Single line of text)
  - GuideId (Single line of text, Unique)
  - Version (Single line of text)
  - Category (Choice: Hardware, Software, Network, Other)
  - Description (Multiple lines of text)
  - RequiredLicense (Choice: Tech, Admin)
  - Published (Yes/No)
  - LastModified (Date and Time)
  - ContentPath (Single line of text)

### Media Library
- Name: `Media`
- Standard document library for shared media files

### AppUpdates Library
- Name: `AppUpdates`
- Store MSIX packages and AppInstaller manifest

## Step 3: Create Lists

### Product Keys List (Optional)
- Name: `ProductKeys`
- Columns:
  - Title (Single line of text)
  - KeyHash (Single line of text, Indexed)
  - LicenseType (Choice: Tech, Admin)
  - CustomerId (Number)
  - ExpirationDate (Date and Time)
  - IsActive (Yes/No)
  - ActivationCount (Number)
  - MaxActivations (Number)

## Step 4: Azure AD App Registration

1. Navigate to Azure Portal > Azure Active Directory > App Registrations
2. Create new registration:
   - Name: `InstallVibe`
   - Supported account types: Single tenant
   - Redirect URI: `ms-appx-web://microsoft.aad.brokerplugin/{client-id}`

3. Configure API Permissions:
   - Microsoft Graph:
     - Files.Read.All (Delegated)
     - Sites.Read.All (Delegated)
     - User.Read (Delegated)
   - SharePoint:
     - AllSites.Read (Delegated)
     - MyFiles.Read (Delegated)

4. Generate client secret (note it down)

5. Note the following values:
   - Application (client) ID
   - Directory (tenant) ID
   - Client secret value

## Step 5: Configure App Settings

Update `src/InstallVibe.Infrastructure/Configuration/appsettings.json`:

```json
{
  "SharePoint": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "SiteUrl": "https://yourtenant.sharepoint.com/sites/InstallVibe",
    "GuidesLibrary": "Guides",
    "MediaLibrary": "Media",
    "UpdatesLibrary": "AppUpdates"
  }
}
```

## Step 6: Upload Sample Guide

1. Create folder structure in Guides library:
   ```
   Guides/
   └── SampleGuide/
       ├── guide.json
       └── steps/
           └── step1.json
   ```

2. Upload sample files from `packaging/SharePoint/Guides/SampleGuide/`

## Step 7: Deploy App Updates

1. Build MSIX package: `.\tools\scripts\package.ps1`
2. Upload to AppUpdates library
3. Upload AppInstaller manifest
4. Update versions.json

## Testing

1. Run InstallVibe application
2. Authenticate with SharePoint credentials
3. Verify guide sync works
4. Test offline mode

## Security Considerations

- Enable Azure AD Conditional Access
- Require MFA for admin accounts
- Regular audit of permissions
- Monitor SharePoint usage logs
- Implement DLP policies for sensitive guides

## Support

For issues with SharePoint configuration, contact your SharePoint administrator.
