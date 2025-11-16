# InstallVibe Manual Import/Export Workflow

## Overview

InstallVibe is configured to operate in **local-only mode** without automatic synchronization. Guides are distributed as `.ivguide` files that can be manually imported and exported.

---

## What is a .ivguide File?

A `.ivguide` file is a ZIP archive containing:
- **guide.json** - Guide metadata, steps, and content
- **media/** folder - All images, videos, and documents referenced by the guide

This single file format makes it easy to share complete guides with all their media assets.

---

## For Admin Users

### Creating a New Guide

1. **Launch InstallVibe** and navigate to the **Guide List** page
2. Click **"Create Guide"** button
3. Fill in guide details:
   - Title and description
   - Category and difficulty
   - Estimated time
   - Tags and prerequisites
4. Add steps with content (supports Markdown)
5. Upload images/media for each step
6. Click **"Save Draft"** or **"Publish"** when complete

### Exporting a Guide

1. Navigate to **Guide List**
2. Click on the guide you want to export
3. In the guide detail view, click the **"Export"** button (⬇️ icon or menu)
4. Choose where to save the `.ivguide` file:
   - **Local disk** for backup
   - **Network share** to distribute to technicians
   - **Email attachment** to send to specific users
   - **USB drive** for offline distribution

**File naming:** The export will suggest a filename like `windows-11-setup.ivguide` based on the guide title.

### Distributing Guides to Technicians

Choose one of these methods:

#### Option 1: Network File Share (Recommended)
```
1. Export guide to a shared network folder
   Example: \\SERVER\InstallVibe\Guides\

2. Technicians import from the same network share

3. Update guides by exporting newer versions to the same location
   (techs can overwrite when reimporting)
```

#### Option 2: Email Distribution
```
1. Export guide to local disk
2. Attach .ivguide file to email
3. Send to technicians
4. Technicians save attachment and import
```

#### Option 3: USB Drive / Removable Media
```
1. Export guide to USB drive
2. Physically deliver to technician locations
3. Technicians import from USB drive
4. Good for air-gapped or high-security environments
```

---

## For Tech Users (Technicians)

### Importing a Guide

1. **Obtain the .ivguide file** from your admin:
   - Download from network share
   - Save email attachment
   - Copy from USB drive

2. **Launch InstallVibe** and navigate to **Guide List**

3. Click the **"Import Guide"** button (usually in the toolbar/menu)

4. **Browse** to the `.ivguide` file location and select it

5. **If the guide already exists**, you'll see a conflict dialog:
   - **Overwrite** - Replace the existing guide with the new version
   - **Import as Copy** - Keep both versions (new one gets a different ID)
   - **Cancel** - Don't import

6. Click **OK** to complete the import

7. The guide will now appear in your guide list with all media included

### Updating an Existing Guide

When your admin sends an updated version:

1. Import the new `.ivguide` file as described above
2. When prompted about the existing guide, choose **"Overwrite"**
3. The guide will be updated with the latest content and media

**Note:** Your personal progress through the guide (completed steps, notes) will be preserved when overwriting.

---

## Workflow Example

### Scenario: Rolling Out a New "Office 365 Setup" Guide

**Admin Steps:**
```
Day 1:
1. Create "Office 365 Setup" guide in InstallVibe
2. Add 12 steps with screenshots
3. Export to \\SERVER\Shared\InstallVibe\office-365-setup.ivguide
4. Email technicians: "New guide available on the share drive"

Day 7 (after feedback):
5. Update guide with clarifications
6. Export updated version to same location (overwrite old file)
7. Email technicians: "Office 365 guide updated - please reimport"
```

**Technician Steps:**
```
Day 1:
1. Open InstallVibe → Guide List → Import
2. Browse to \\SERVER\Shared\InstallVibe\office-365-setup.ivguide
3. Click Import
4. Guide appears in list, ready to use

Day 7 (after update notification):
5. Open InstallVibe → Guide List → Import
6. Browse to \\SERVER\Shared\InstallVibe\office-365-setup.ivguide
7. Choose "Overwrite" when prompted
8. Updated guide is now available
```

---

## Tips and Best Practices

### For Admins

✅ **Use consistent naming:** `category-title-version.ivguide`
   Example: `windows-win11-setup-v2.ivguide`

✅ **Version guides:** Include version number in filename or guide metadata

✅ **Centralize distribution:** Use one network share location for all guides

✅ **Notify on updates:** Email or message techs when guides are updated

✅ **Test before distributing:** Import the exported guide yourself to verify it works

✅ **Backup regularly:** Export all guides periodically to a backup location

### For Technicians

✅ **Create a local import folder:** Keep downloaded guides organized
   Example: `C:\InstallVibeImports\`

✅ **Check for updates regularly:** Ask admin about new guide versions

✅ **Reimport to update:** Don't manually edit guides - always reimport from admin

✅ **Report issues:** If a guide has errors, contact your admin immediately

---

## Troubleshooting

### Import Fails with "Invalid Archive"

**Cause:** File is corrupted or not a valid `.ivguide` file

**Solution:**
- Re-download the file from the network share
- Ask admin to re-export the guide
- Verify file extension is `.ivguide` (not `.zip`)

### Import Fails with "Media Files Missing"

**Cause:** Guide references media that weren't included in export

**Solution:**
- Contact admin to re-export with media included
- Admin should verify "Include Media" option is checked during export

### Guide List Shows Old Version After Import

**Cause:** Didn't choose "Overwrite" option during import

**Solution:**
- Delete the old guide manually
- Reimport and choose "Overwrite" this time

### Can't Find Import Button

**Cause:** UI might differ by version or user role

**Solution:**
- Check under: **File → Import Guide** (menu)
- Or: Toolbar button with folder/arrow icon (📁⬇️)
- Contact support if still not visible

---

## Comparison: Manual vs. Automatic Sync

| Feature | Manual Import/Export | Automatic Sync (Future) |
|---------|---------------------|------------------------|
| **Setup Complexity** | None | Medium (server required) |
| **Distribution** | Manual (network/email/USB) | Automatic background |
| **Update Notification** | Email/message techs | Automatic detection |
| **Offline Capable** | ✅ Yes | ✅ Yes (after initial sync) |
| **Air-gap Compatible** | ✅ Yes (USB) | ❌ No |
| **Version Control** | Manual (filename) | Automatic (server tracks) |
| **Best For** | < 20 users, high security | > 20 users, frequent updates |

---

## Future Migration Path

The manual workflow can coexist with automatic sync. When you're ready to add automatic synchronization:

1. **File Share Sync:** Techs auto-import from network share (no manual intervention)
2. **Web API Sync:** Techs sync from internet-accessible API (works remotely)
3. **Hybrid Mode:** Manual import still works as backup method

Current manual workflow will continue to function even if automatic sync is added later.

---

## Technical Details

### .ivguide File Format

```
office-365-setup.ivguide (ZIP archive)
│
├── guide.json                 # Guide metadata and content
└── media/
    ├── {mediaId1}.jpg        # Image files
    ├── {mediaId2}.png
    ├── {mediaId3}.mp4        # Video files
    └── {mediaId4}.pdf        # Document files
```

### Import Process

1. Validates ZIP structure and guide.json format
2. Checks for GUID conflicts with existing guides
3. Imports guide metadata to SQLite database
4. Extracts media files to local cache folder:
   `%LOCALAPPDATA%\InstallVibe\Cache\Media\`
5. Updates guide list in UI

### Storage Locations

- **Database:** `%LOCALAPPDATA%\InstallVibe\Data\installvibe.db`
- **Media Cache:** `%LOCALAPPDATA%\InstallVibe\Cache\Media\`
- **Logs:** `%LOCALAPPDATA%\InstallVibe\Logs\`

---

## Questions?

For technical support or questions about the manual workflow, contact your InstallVibe administrator.
