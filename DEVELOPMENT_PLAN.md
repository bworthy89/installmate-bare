# InstallVibe Development Plan
## 2-Week Guide Creation & SharePoint Publishing Implementation

**Created:** 2025-11-15
**Timeline:** 2 weeks
**Goal:** Build complete guide authoring experience with SharePoint integration

---

## Vision

### Two-Sided Application

**Consumer Side (End Users):**
- Browse guides by category/tags
- View rich guide details with metadata
- Execute guides step-by-step with progress tracking
- Work offline with cached data

**Admin Side (Content Creators):**
- Create/edit guides within the app
- Rich metadata (tags, target audience, prerequisites, difficulty)
- Upload media (images, videos, PDFs)
- Publish to SharePoint as central repository
- Full edit support for existing guides

---

## User Preferences & Requirements

### Editor Interface
- **Style:** Hybrid forms + markdown
- **Metadata Forms:** Title, Description, Category (pre-defined dropdown), Difficulty, Tags, Target Audience, Prerequisites
- **Content Editor:** Markdown for step instructions with syntax support

### Media Handling
- **Method:** Upload from local files
- **Flow:** Select files → Upload to SharePoint Media Library → Reference in guide
- **Types:** Images, Videos, PDFs

### Edit Support
- **Full edit capability** - Load from SharePoint, edit, save back
- **Version control** - Detect conflicts if modified elsewhere

### Metadata Fields
- Title (required)
- Description (required)
- Category (pre-defined: Installation, Configuration, Troubleshooting, Maintenance, Best Practices)
- Difficulty (Easy, Medium, Hard)
- Estimated Minutes (number)
- Tags (string array for flexible categorization)
- Target Audience (who this guide is for)
- Prerequisites (list of other guide IDs required first)
- Version (for conflict detection)

---

## Week 1: Consumer Experience + Foundation

### Day 1: Foundation (4-5 hours)

**Tasks:**
1. Set up dev branch workflow
   ```bash
   git checkout -b dev
   git push -u origin dev
   ```

2. Extend Guide data model (`src/InstallVibe.Core/Models/Domain/Guide.cs`)
   ```csharp
   // Add new properties:
   public List<string> Tags { get; set; } = new();
   public string? TargetAudience { get; set; }
   public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard
   public List<string> PrerequisiteGuideIds { get; set; } = new();
   public string? Version { get; set; }
   ```

3. Create GuideCategories constants (`src/InstallVibe.Core/Constants/GuideCategories.cs`)
   ```csharp
   public static class GuideCategories
   {
       public const string Installation = "Installation";
       public const string Configuration = "Configuration";
       public const string Troubleshooting = "Troubleshooting";
       public const string Maintenance = "Maintenance";
       public const string BestPractices = "Best Practices";

       public static List<string> All => new()
       {
           Installation, Configuration, Troubleshooting,
           Maintenance, BestPractices
       };
   }
   ```

4. Create initial database migration
   ```bash
   dotnet ef migrations add InitialCreate --project src/InstallVibe.Data --startup-project src/InstallVibe
   ```

5. Seed sample guides with rich metadata
   - Create `SampleDataSeeder.cs` in InstallVibe.Data
   - Generate 3-5 complete guides with:
     - All metadata fields populated
     - Multiple steps with markdown content
     - Media references (placeholder URLs)
     - Different categories and difficulty levels

**Deliverable:** Database schema created, sample data available for testing

---

### Day 2-3: Core User Experience (12-16 hours)

#### Shell Navigation (2-3 hours)
**File:** `src/InstallVibe/Views/Shell/ShellPage.xaml`

Add NavigationView menu structure:
```xml
<NavigationView.MenuItems>
    <NavigationViewItem Icon="Home" Content="Dashboard" Tag="Dashboard"/>
    <NavigationViewItem Icon="Library" Content="Browse Guides" Tag="GuideList"/>
    <NavigationViewItem Icon="Favorite" Content="Favorites" Tag="Favorites"/>
    <NavigationViewItemSeparator/>
    <NavigationViewItem Icon="Admin" Content="Admin Editor" Tag="AdminEditor">
        <NavigationViewItem.InfoBadge>
            <InfoBadge Value="Admin"/>
        </NavigationViewItem.InfoBadge>
    </NavigationViewItem>
</NavigationView.MenuItems>

<NavigationView.FooterMenuItems>
    <NavigationViewItem Icon="Setting" Content="Settings" Tag="Settings"/>
</NavigationView.FooterMenuItems>
```

Handle navigation in `ShellPage.xaml.cs`:
```csharp
private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
{
    if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
    {
        ViewModel.NavigationService.NavigateTo(tag);
    }
}
```

#### Enhanced Guide Detail Page (4-6 hours)
**File:** `src/InstallVibe/Views/Guide/GuideDetailPage.xaml`

Add sections for:
- **Header:** Title, Category badge, Difficulty badge
- **Metadata Panel:**
  - Estimated time
  - Target audience
  - Tags (as chips)
  - Prerequisites (linked guides)
- **Step Preview:** ListView showing all steps with numbers
- **Action Button:** Start/Resume/Continue based on progress

**ViewModel Updates:** `src/InstallVibe/ViewModels/Guide/GuideDetailViewModel.cs`
- Load prerequisite guides for display
- Calculate completion status
- Format tags for display

#### Step Rendering (8-12 hours)
**File:** `src/InstallVibe/Views/Guide/StepPage.xaml`

Implement:
1. **Step Header:** Step number, title, progress indicator
2. **Markdown Rendering:**
   - Use MarkdownTextBlock (WinUI Community Toolkit) for instruction display
   - Install: `CommunityToolkit.WinUI.Controls.MarkdownTextBlock`
3. **Media Display:**
   - Image viewer for images
   - MediaPlayerElement for videos
   - PDF viewer placeholder (future enhancement)
4. **Checklist Rendering:**
   - CheckBox for each checklist item
   - Save state to progress
5. **Navigation:**
   - Previous/Next buttons
   - Step counter (Step 3 of 10)
   - Complete step button

**ViewModel Updates:** `src/InstallVibe/ViewModels/Guide/StepViewModel.cs`
- Parse markdown content
- Load media from cache or SharePoint
- Track checklist state
- Save progress on each action

**Deliverable:** Users can view guides with full metadata and execute them step-by-step

---

### Day 4-5: Polish & Test (10-12 hours)

#### Guide List Enhancements (6-8 hours)
**File:** `src/InstallVibe/Views/Guide/GuideListPage.xaml`

Add filtering:
- Category filter (ComboBox with pre-defined categories)
- Difficulty filter (Easy/Medium/Hard)
- Tag search (search in tags array)
- Text search in title/description

Update `GuideListViewModel.cs`:
- Implement filter logic
- ObservableCollection updates based on filters
- Search debouncing for performance

#### Settings Page (6-8 hours)
**File:** `src/InstallVibe/Views/Settings/SettingsPage.xaml`

Implement TODOs from `SettingsViewModel.cs`:
- Theme selection (System/Light/Dark)
- Auto-sync toggle and interval
- Cache size limit
- Clear cache button
- About section (version, license info)
- Save to local settings file

#### End-to-End Testing (4 hours)
Test complete flows:
1. Browse guides → Filter by category → Select guide → View details → Start guide
2. Execute guide step-by-step → Check items → Navigate steps → Complete guide
3. Dashboard shows progress → Resume guide → Continue from last step
4. Pin guide → Shows in Pinned section → Unpin
5. Offline mode → Disconnect → App works with cached data

**Deliverable:** Fully functional consumer app ready for demo

---

## Week 2: Admin Guide Editor + SharePoint Integration

### Day 6-7: Guide Editor UI (14-18 hours)

#### New Files to Create

**1. GuideEditorViewModel.cs** (`src/InstallVibe/ViewModels/Admin/GuideEditorViewModel.cs`)
```csharp
public partial class GuideEditorViewModel : ObservableObject
{
    // Properties for metadata
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private string _selectedCategory = GuideCategories.Installation;
    [ObservableProperty] private string _selectedDifficulty = "Medium";
    [ObservableProperty] private int _estimatedMinutes = 30;
    [ObservableProperty] private string _tagsText = string.Empty; // Comma-separated
    [ObservableProperty] private string _targetAudience = string.Empty;

    // Steps management
    [ObservableProperty] private ObservableCollection<StepEditorViewModel> _steps = new();
    [ObservableProperty] private StepEditorViewModel? _selectedStep;

    // State
    [ObservableProperty] private bool _isSaving = false;
    [ObservableProperty] private bool _isEditMode = false;
    [ObservableProperty] private string? _editingGuideId;

    // Available options
    public List<string> Categories => GuideCategories.All;
    public List<string> Difficulties => new() { "Easy", "Medium", "Hard" };
    public ObservableCollection<Guide> AvailableGuides { get; set; } = new(); // For prerequisites

    // Commands
    [RelayCommand] private Task SaveToSharePointAsync();
    [RelayCommand] private Task SaveDraftLocallyAsync();
    [RelayCommand] private void AddStep();
    [RelayCommand] private void RemoveStep(StepEditorViewModel step);
    [RelayCommand] private void MoveStepUp(StepEditorViewModel step);
    [RelayCommand] private void MoveStepDown(StepEditorViewModel step);
    [RelayCommand] private void Cancel();
}
```

**2. StepEditorViewModel.cs** (`src/InstallVibe/ViewModels/Admin/StepEditorViewModel.cs`)
```csharp
public partial class StepEditorViewModel : ObservableObject
{
    [ObservableProperty] private int _stepNumber;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _content = string.Empty; // Markdown
    [ObservableProperty] private ObservableCollection<MediaItemViewModel> _mediaFiles = new();
    [ObservableProperty] private ObservableCollection<string> _checklist = new();

    [RelayCommand]
    private async Task AddMediaAsync()
    {
        // FileOpenPicker to select files
        // Add to mediaFiles collection
    }

    [RelayCommand]
    private void RemoveMedia(MediaItemViewModel media);

    [RelayCommand]
    private void AddChecklistItem();
}
```

**3. MediaItemViewModel.cs** (`src/InstallVibe/ViewModels/Admin/MediaItemViewModel.cs`)
```csharp
public partial class MediaItemViewModel : ObservableObject
{
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private string _localPath = string.Empty;
    [ObservableProperty] private string? _sharePointUrl; // After upload
    [ObservableProperty] private MediaType _type; // Image, Video, Document
    [ObservableProperty] private bool _isUploaded = false;
}
```

**4. GuideEditorPage.xaml** (`src/InstallVibe/Views/Admin/GuideEditorPage.xaml`)

Layout structure:
```xml
<Grid>
    <ScrollViewer>
        <StackPanel Margin="24" Spacing="16">
            <!-- Header -->
            <TextBlock Text="Create Guide" Style="{ThemeResource TitleTextBlockStyle}"/>

            <!-- Metadata Section -->
            <Expander Header="Guide Information" IsExpanded="True">
                <StackPanel Spacing="12">
                    <TextBox Header="Title" Text="{x:Bind ViewModel.Title, Mode=TwoWay}"/>
                    <TextBox Header="Description" TextWrapping="Wrap"
                             AcceptsReturn="True" Height="100"
                             Text="{x:Bind ViewModel.Description, Mode=TwoWay}"/>

                    <Grid ColumnSpacing="12">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="*"/>
                        </Grid.ColumnDefinitions>

                        <ComboBox Header="Category" Grid.Column="0"
                                  ItemsSource="{x:Bind ViewModel.Categories}"
                                  SelectedItem="{x:Bind ViewModel.SelectedCategory, Mode=TwoWay}"/>

                        <ComboBox Header="Difficulty" Grid.Column="1"
                                  ItemsSource="{x:Bind ViewModel.Difficulties}"
                                  SelectedItem="{x:Bind ViewModel.SelectedDifficulty, Mode=TwoWay}"/>
                    </Grid>

                    <NumberBox Header="Estimated Time (minutes)"
                               Value="{x:Bind ViewModel.EstimatedMinutes, Mode=TwoWay}"/>

                    <TextBox Header="Tags (comma-separated)"
                             Text="{x:Bind ViewModel.TagsText, Mode=TwoWay}"/>

                    <TextBox Header="Target Audience"
                             Text="{x:Bind ViewModel.TargetAudience, Mode=TwoWay}"/>
                </StackPanel>
            </Expander>

            <!-- Steps Section -->
            <Expander Header="Steps" IsExpanded="True">
                <StackPanel Spacing="12">
                    <CommandBar>
                        <AppBarButton Icon="Add" Label="Add Step"
                                      Command="{x:Bind ViewModel.AddStepCommand}"/>
                    </CommandBar>

                    <ListView ItemsSource="{x:Bind ViewModel.Steps, Mode=OneWay}"
                              SelectedItem="{x:Bind ViewModel.SelectedStep, Mode=TwoWay}">
                        <ListView.ItemTemplate>
                            <DataTemplate x:DataType="vm:StepEditorViewModel">
                                <Expander Header="{x:Bind Title, Mode=OneWay}">
                                    <StackPanel Spacing="8">
                                        <TextBox Header="Step Title"
                                                 Text="{x:Bind Title, Mode=TwoWay}"/>

                                        <TextBox Header="Instructions (Markdown)"
                                                 TextWrapping="Wrap" AcceptsReturn="True"
                                                 Height="200"
                                                 Text="{x:Bind Content, Mode=TwoWay}"/>

                                        <!-- Media section -->
                                        <StackPanel>
                                            <TextBlock Text="Media Files"
                                                       Style="{ThemeResource SubtitleTextBlockStyle}"/>
                                            <Button Content="Add Media"
                                                    Command="{x:Bind AddMediaCommand}"/>
                                            <ListView ItemsSource="{x:Bind MediaFiles, Mode=OneWay}">
                                                <!-- Media items display -->
                                            </ListView>
                                        </StackPanel>

                                        <StackPanel Orientation="Horizontal" Spacing="8">
                                            <Button Content="Move Up"/>
                                            <Button Content="Move Down"/>
                                            <Button Content="Remove Step"
                                                    Style="{ThemeResource DangerButtonStyle}"/>
                                        </StackPanel>
                                    </StackPanel>
                                </Expander>
                            </DataTemplate>
                        </ListView.ItemTemplate>
                    </ListView>
                </StackPanel>
            </Expander>

            <!-- Action Buttons -->
            <StackPanel Orientation="Horizontal" Spacing="12">
                <Button Content="Save to SharePoint"
                        Command="{x:Bind ViewModel.SaveToSharePointCommand}"
                        Style="{ThemeResource AccentButtonStyle}"/>
                <Button Content="Save Draft Locally"
                        Command="{x:Bind ViewModel.SaveDraftLocallyCommand}"/>
                <Button Content="Cancel"
                        Command="{x:Bind ViewModel.CancelCommand}"/>
            </StackPanel>
        </StackPanel>
    </ScrollViewer>
</Grid>
```

#### Update AdminEditorPage.xaml
Add navigation to guide editor:
```xml
<CommandBar>
    <AppBarButton Icon="Add" Label="Create New Guide"
                  Command="{x:Bind ViewModel.CreateNewGuideCommand}"/>
</CommandBar>
```

**Deliverable:** Complete UI for creating/editing guides with rich metadata and markdown editor

---

### Day 8: SharePoint Write Operations (6-8 hours)

#### Extend SharePointService (`src/InstallVibe.Core/Services/SharePoint/SharePointService.cs`)

**New Methods:**

```csharp
public async Task<Guide> CreateGuideAsync(Guide guide)
{
    // Create item in SharePoint Guides list
    // POST to /sites/{site}/lists/{listId}/items
    // Map Guide properties to SharePoint fields
    // Return guide with SharePoint ID
}

public async Task UpdateGuideAsync(Guide guide)
{
    // Update existing item in SharePoint
    // PATCH to /sites/{site}/lists/{listId}/items/{itemId}
    // Check version/etag for conflict detection
}

public async Task DeleteGuideAsync(string guideId)
{
    // DELETE from SharePoint Guides list
    // Also delete associated media files
}

public async Task<string> UploadMediaAsync(string guideId, Stream fileStream, string fileName, MediaType type)
{
    // Upload to SharePoint Media Library
    // Set metadata (GuideId, MediaType, etc.)
    // Return SharePoint URL for media file
    // Handle large files (>4MB) with chunked upload
}
```

**Implementation Details:**
- Use Graph API: `/sites/{site-id}/lists/{list-id}/items`
- Map domain model to SharePoint fields:
  ```json
  {
    "fields": {
      "Title": guide.Title,
      "Description": guide.Description,
      "Category": guide.Category,
      "Difficulty": guide.Difficulty,
      "EstimatedMinutes": guide.EstimatedMinutes,
      "Tags": guide.Tags.Join(","),
      "TargetAudience": guide.TargetAudience,
      "GuideId": guide.GuideId,
      "StepsJson": JsonSerializer.Serialize(guide.Steps)
    }
  }
  ```

#### SharePoint Setup (`packaging/SharePoint/SharePointSetup.md`)

Follow documentation to create:
1. Guides list with custom columns:
   - GuideId (Text)
   - Category (Choice)
   - Difficulty (Choice: Easy, Medium, Hard)
   - EstimatedMinutes (Number)
   - Tags (Text)
   - TargetAudience (Text)
   - PrerequisiteGuideIds (Text)
   - StepsJson (Multiline Text)
   - Version (Text)

2. Media Library with metadata:
   - GuideId (Text) - links media to guide
   - MediaType (Choice: Image, Video, Document)

3. Configure app permissions in Azure AD
4. Test authentication with certificate

**Deliverable:** App can write guides to SharePoint and upload media files

---

### Day 9: Integration & Sync Flow (6-8 hours)

#### Implement Complete Publishing Flow

**In GuideEditorViewModel.SaveToSharePointAsync():**

```csharp
private async Task SaveToSharePointAsync()
{
    IsSaving = true;
    try
    {
        // 1. Validate guide data
        if (!ValidateGuide()) return;

        // 2. Build Guide object from form data
        var guide = BuildGuideFromForm();

        // 3. Upload all media files first
        foreach (var step in Steps)
        {
            foreach (var media in step.MediaFiles.Where(m => !m.IsUploaded))
            {
                using var stream = File.OpenRead(media.LocalPath);
                var url = await _sharePointService.UploadMediaAsync(
                    guide.GuideId,
                    stream,
                    media.FileName,
                    media.Type
                );
                media.SharePointUrl = url;
                media.IsUploaded = true;

                // Add reference to step
                step.MediaReferences.Add(new MediaReference
                {
                    Url = url,
                    Type = media.Type
                });
            }
        }

        // 4. Create or update guide in SharePoint
        if (IsEditMode && !string.IsNullOrEmpty(EditingGuideId))
        {
            await _sharePointService.UpdateGuideAsync(guide);
        }
        else
        {
            guide = await _sharePointService.CreateGuideAsync(guide);
        }

        // 5. Save to local database
        await _guideService.SaveGuideAsync(guide);

        // 6. Navigate back to admin editor
        _navigationService.NavigateTo("AdminEditor");

        _logger.LogInformation("Guide {GuideId} published successfully", guide.GuideId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to publish guide");
        ErrorMessage = $"Failed to publish: {ex.Message}";
    }
    finally
    {
        IsSaving = false;
    }
}
```

#### Test Scenarios

**1. Create New Guide:**
- Open Guide Editor
- Fill in metadata (title, description, category, etc.)
- Add 3 steps with markdown content
- Upload 2 images to step 1
- Upload 1 PDF to step 2
- Click "Save to SharePoint"
- Verify guide appears in SharePoint Guides list
- Verify media files in SharePoint Media Library
- Verify guide appears in app when synced

**2. Edit Existing Guide:**
- Load guide from SharePoint
- Modify title and add a new step
- Save back to SharePoint
- Verify changes reflected in SharePoint
- Verify other users get updated version on sync

**3. Conflict Detection:**
- User A loads guide for editing
- User B modifies same guide in SharePoint
- User A tries to save
- App detects conflict (version mismatch)
- Show warning dialog

**4. Offline Scenario:**
- Create guide while offline
- Click "Save Draft Locally" (saves to database only)
- Go online
- Click "Save to SharePoint" (uploads draft)

**Deliverable:** End-to-end flow working - create guide in app → publish to SharePoint → sync to other users

---

### Day 10: Testing & Demo Prep (4-6 hours)

#### Complete Workflow Testing

**Admin Workflow:**
1. Launch app with admin license
2. Navigate to Admin Editor
3. Click "Create New Guide"
4. Fill in guide:
   - Title: "Windows Server 2022 Installation"
   - Category: Installation
   - Difficulty: Medium
   - Tags: "windows, server, 2022"
   - Target Audience: "System Administrators"
   - 5 steps with detailed markdown instructions
   - 3 images, 1 video, 1 PDF uploaded
5. Click "Save to SharePoint"
6. Verify guide in SharePoint
7. Test edit flow - modify guide, save back

**User Workflow:**
1. Launch app with tech license (different machine/user)
2. App syncs from SharePoint
3. New guide appears in Dashboard "New Guides"
4. Click guide → View details with all metadata
5. Start guide → Execute step-by-step
6. Check off items → Mark steps complete
7. Dashboard shows progress
8. Resume guide → Continues from last step
9. Complete guide → Appears in "Recently Completed"

**Offline Workflow:**
1. Complete guide while online
2. Disconnect from network
3. App still shows all guides (cached)
4. Can execute guides offline
5. Progress saved locally
6. Reconnect → Progress syncs to SharePoint

#### Demo Script

**Opening:**
"InstallVibe is a dual-purpose guide management system. Admins create guides, users follow them."

**Demo Part 1: Admin Creates Guide (3 min)**
- Show admin opening Guide Editor
- Fill in metadata with explanation
- Add steps with markdown
- Upload media files
- Publish to SharePoint
- Show SharePoint list with new guide

**Demo Part 2: User Discovers & Executes Guide (4 min)**
- Different user launches app
- Dashboard shows new guide in "New Guides"
- Browse to guide, view rich details
- Start guide, walk through 2-3 steps
- Show markdown rendering, media display
- Mark items complete, track progress
- Show dashboard updated with progress

**Demo Part 3: Offline Capability (2 min)**
- Disconnect network
- App still functional
- Navigate between pages
- Start another guide
- "This is the power of offline-first"

**Closing:**
"SharePoint as central repository, rich authoring in app, seamless offline experience."

#### Documentation

Create:
1. **USER_GUIDE.md** - How to use the app as end user
2. **ADMIN_GUIDE.md** - How to create/manage guides
3. **SHAREPOINT_SETUP.md** - Update with actual setup steps performed
4. **DEMO_SCRIPT.md** - Step-by-step demo walkthrough

**Deliverable:** Polished demo ready to present, documentation complete

---

## Technical Reference

### Pre-defined Categories

```csharp
public static class GuideCategories
{
    public const string Installation = "Installation";
    public const string Configuration = "Configuration";
    public const string Troubleshooting = "Troubleshooting";
    public const string Maintenance = "Maintenance";
    public const string BestPractices = "Best Practices";
    public const string Migration = "Migration";
    public const string Security = "Security";
    public const string Performance = "Performance";

    public static List<string> All => new()
    {
        Installation, Configuration, Troubleshooting,
        Maintenance, BestPractices, Migration,
        Security, Performance
    };
}
```

### Difficulty Levels

- **Easy:** Basic tasks, minimal prerequisites, 15-30 minutes
- **Medium:** Moderate complexity, some prerequisites, 30-60 minutes
- **Hard:** Complex tasks, multiple prerequisites, 60+ minutes

### Markdown Support

Supported syntax in step instructions:
- Headers: `# ## ###`
- Bold/Italic: `**bold** *italic*`
- Lists: `- item` or `1. item`
- Code: `` `inline` `` or ` ```code block``` `
- Links: `[text](url)`
- Images: `![alt](url)` (auto-inserted when media uploaded)

### Media Types

```csharp
public enum MediaType
{
    Image,  // .jpg, .png, .gif, .bmp
    Video,  // .mp4, .avi, .mov
    Document // .pdf, .docx
}
```

### SharePoint Field Mapping

| Guide Property | SharePoint Column | Type |
|---------------|------------------|------|
| GuideId | GuideId | Text |
| Title | Title | Text |
| Description | Description | Multiline Text |
| Category | Category | Choice |
| Difficulty | Difficulty | Choice |
| EstimatedMinutes | EstimatedMinutes | Number |
| Tags | Tags | Text (comma-separated) |
| TargetAudience | TargetAudience | Text |
| PrerequisiteGuideIds | PrerequisiteGuideIds | Text (comma-separated) |
| Steps | StepsJson | Multiline Text (JSON) |
| Version | Version | Text |
| LastModified | Modified | DateTime (auto) |

---

## Git Workflow

### Branch Strategy

```
main (protected, production-ready)
  └── dev (integration branch)
        ├── feature/data-model-extensions
        ├── feature/shell-navigation
        ├── feature/guide-detail-enhancement
        ├── feature/step-rendering
        ├── feature/guide-list-filters
        ├── feature/settings-page
        ├── feature/guide-editor-ui
        ├── feature/sharepoint-write-ops
        └── feature/guide-publishing-flow
```

### Workflow

```bash
# Day 1
git checkout -b dev
git push -u origin dev
git checkout -b feature/data-model-extensions
# ... work ...
git commit -m "Add Tags, TargetAudience, Difficulty to Guide model"
git push -u origin feature/data-model-extensions
# Create PR: feature/data-model-extensions → dev
# Merge PR
git checkout dev
git pull

# Repeat for each feature...

# End of Week 1
git checkout main
git merge dev
git tag v0.1-consumer-experience
git push --tags

# Week 2 continues same pattern...

# End of Week 2
git checkout main
git merge dev
git tag v1.0-full-authoring
git push --tags
```

---

## Success Criteria

### Week 1 Success
- ✅ Database migrations created and tested
- ✅ App launches without errors
- ✅ Shell navigation functional
- ✅ Can browse guides filtered by category/tags
- ✅ Guide detail shows all metadata
- ✅ Can execute guides step-by-step
- ✅ Markdown rendered correctly
- ✅ Progress tracked and persists
- ✅ Dashboard shows progress accurately
- ✅ Settings page functional

### Week 2 Success
- ✅ Can create new guide in app
- ✅ Markdown editor works for step content
- ✅ Can upload images, videos, PDFs
- ✅ Guide publishes to SharePoint successfully
- ✅ Media files appear in SharePoint Media Library
- ✅ Can edit existing guide and save changes
- ✅ Other users can sync and see new guides
- ✅ Conflict detection works
- ✅ Offline mode functional
- ✅ Complete demo script executed successfully

---

## Risk Mitigation

### Potential Risks

1. **SharePoint API Limitations**
   - Risk: API rate limiting or permission issues
   - Mitigation: Test authentication early (Day 8), handle errors gracefully

2. **Markdown Editor Complexity**
   - Risk: Building rich editor takes longer than planned
   - Mitigation: Use simple TextBox + preview pane, not WYSIWYG

3. **Media Upload Performance**
   - Risk: Large files slow down save process
   - Mitigation: Show progress bar, upload asynchronously, implement chunked upload

4. **Database Migration Issues**
   - Risk: Migration fails on existing database
   - Mitigation: Test migrations on fresh database first, backup before applying

5. **Scope Creep**
   - Risk: Adding features beyond plan (e.g., guide templates, bulk import)
   - Mitigation: Stick to plan, document future enhancements separately

---

## Future Enhancements (Post-MVP)

- Guide templates (pre-configured structure for common tasks)
- Bulk import guides from CSV/JSON
- Guide versioning history (view previous versions)
- Comment/feedback system for guides
- Analytics dashboard (most popular guides, completion rates)
- Export guide to PDF
- Guide sharing/permissions (restrict by role)
- Integration with Teams/Outlook for notifications
- Mobile companion app (read-only guide viewer)

---

## Support & Resources

### Documentation References
- `packaging/SharePoint/SharePointSetup.md` - SharePoint configuration
- `tools/certificates/README.md` - Certificate generation
- WinUI 3 Docs: https://learn.microsoft.com/en-us/windows/apps/winui/
- Microsoft Graph API: https://learn.microsoft.com/en-us/graph/

### Key Dependencies
- WinUI 3 / Windows App SDK
- .NET 8.0
- Entity Framework Core 8
- Microsoft.Graph SDK
- CommunityToolkit.Mvvm
- CommunityToolkit.WinUI.Controls.MarkdownTextBlock

### Contact
For questions during implementation, refer to this plan and project architecture documentation.

---

**Last Updated:** 2025-11-15
**Status:** Ready to implement
**Next Action:** Start Day 1 - Create dev branch and extend data model
