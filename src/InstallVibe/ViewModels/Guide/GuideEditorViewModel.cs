using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Constants;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Core.Services.User;
using InstallVibe.Services.Navigation;
using InstallVibe.Views.Shell;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Windows.System;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for creating and editing guides with full WYSIWYG editing capabilities.
/// </summary>
public partial class GuideEditorViewModel : ObservableValidator
{
    private readonly IGuideService _guideService;
    private readonly ISharePointService _sharePointService;
    private readonly ICacheService _cacheService;
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<GuideEditorViewModel> _logger;

    private string? _originalGuideId;
    private bool _isNewGuide;
    private System.Threading.Timer? _autoSaveTimer;
    private DateTime _lastUserEdit = DateTime.Now;
    private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;

    // Guide Metadata
    [ObservableProperty]
    private string _guideId = Guid.NewGuid().ToString();

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Guide name is required")]
    [MinLength(3, ErrorMessage = "Guide name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Guide name must be 100 characters or less")]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Description is required")]
    [MinLength(10, ErrorMessage = "Description must be at least 10 characters")]
    [MaxLength(500, ErrorMessage = "Description must be 500 characters or less")]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Category is required")]
    private string _selectedCategory = GuideCategories.All[0];

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Difficulty is required")]
    private string _selectedDifficulty = GuideDifficulty.All[0];

    [ObservableProperty]
    private string _targetAudience = string.Empty;

    [ObservableProperty]
    private int _estimatedMinutes = 30;

    [ObservableProperty]
    private string _version = "1.0";

    [ObservableProperty]
    private string _author = string.Empty;

    // Collections
    [ObservableProperty]
    private ObservableCollection<string> _tags = new();

    [ObservableProperty]
    private ObservableCollection<string> _prerequisites = new();

    [ObservableProperty]
    private ObservableCollection<StepEditorItem> _steps = new();

    // UI State
    [ObservableProperty]
    private bool _isSaving = false;

    [ObservableProperty]
    private bool _isPublishing = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private StepEditorItem? _selectedStep;

    // Tag/Prerequisite input
    [ObservableProperty]
    private string _newTagInput = string.Empty;

    [ObservableProperty]
    private string _newPrerequisiteInput = string.Empty;

    // Available options
    public List<string> AvailableCategories { get; } = GuideCategories.All.ToList();
    public List<string> AvailableDifficulties { get; } = GuideDifficulty.All.ToList();

    public bool IsEditMode => !_isNewGuide;
    public string PageTitle => _isNewGuide ? "Create New Guide" : $"Edit: {Title}";
    public bool HasAnyErrors => HasErrors;
    public bool CanSaveDraft => !HasErrors && !IsSaving;
    public bool CanPublish => !HasErrors && Steps.Count > 0 && !IsPublishing;

    // Validation error properties for binding
    public IEnumerable<string> TitleErrors => GetErrors(nameof(Title))
        .OfType<System.ComponentModel.DataAnnotations.ValidationResult>()
        .Select(vr => vr.ErrorMessage ?? string.Empty);

    public IEnumerable<string> DescriptionErrors => GetErrors(nameof(Description))
        .OfType<System.ComponentModel.DataAnnotations.ValidationResult>()
        .Select(vr => vr.ErrorMessage ?? string.Empty);

    public IEnumerable<string> SelectedCategoryErrors => GetErrors(nameof(SelectedCategory))
        .OfType<System.ComponentModel.DataAnnotations.ValidationResult>()
        .Select(vr => vr.ErrorMessage ?? string.Empty);

    public IEnumerable<string> SelectedDifficultyErrors => GetErrors(nameof(SelectedDifficulty))
        .OfType<System.ComponentModel.DataAnnotations.ValidationResult>()
        .Select(vr => vr.ErrorMessage ?? string.Empty);

    public GuideEditorViewModel(
        IGuideService guideService,
        ISharePointService sharePointService,
        ICacheService cacheService,
        IUserService userService,
        INavigationService navigationService,
        ILogger<GuideEditorViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Get the dispatcher queue for the current thread (UI thread)
        _dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        // Hook up ErrorsChanged to update CanSaveDraft and CanPublish
        ErrorsChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(HasAnyErrors));
            OnPropertyChanged(nameof(CanSaveDraft));
            OnPropertyChanged(nameof(CanPublish));

            // Notify error properties so UI updates
            if (e.PropertyName == nameof(Title))
                OnPropertyChanged(nameof(TitleErrors));
            else if (e.PropertyName == nameof(Description))
                OnPropertyChanged(nameof(DescriptionErrors));
            else if (e.PropertyName == nameof(SelectedCategory))
                OnPropertyChanged(nameof(SelectedCategoryErrors));
            else if (e.PropertyName == nameof(SelectedDifficulty))
                OnPropertyChanged(nameof(SelectedDifficultyErrors));
        };

        // Initialize auto-save timer (30 seconds)
        _autoSaveTimer = new System.Threading.Timer(
            _ => OnAutoSaveTick(),
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(30));

        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            Author = await _userService.GetCurrentUserNameAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing editor");
        }
    }

    /// <summary>
    /// Gets the next version number for the guide. For new guides, returns "1.0".
    /// For existing guides, increments the minor version (e.g., 1.0 -> 1.1 -> 1.2).
    /// </summary>
    private async Task<string> GetNextVersionAsync()
    {
        if (_isNewGuide)
            return "1.0";

        if (_originalGuideId != null)
        {
            try
            {
                var existingGuide = await _guideService.GetGuideAsync(_originalGuideId);
                if (existingGuide?.Version != null)
                {
                    var parts = existingGuide.Version.Split('.');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int minor))
                    {
                        return $"{parts[0]}.{minor + 1}"; // e.g., 1.0 -> 1.1 -> 1.2
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting next version, defaulting to 1.0");
            }
        }
        return "1.0";
    }

    /// <summary>
    /// Auto-save timer tick handler. Saves draft every 30 seconds if conditions are met.
    /// This runs on a background thread, so we marshal back to the UI thread.
    /// </summary>
    private void OnAutoSaveTick()
    {
        // Marshal to UI thread for property updates
        _dispatcherQueue.TryEnqueue(async () =>
        {
            // Only auto-save if:
            // 1. Not a new unsaved guide (must have been saved at least once)
            // 2. Title is not empty
            // 3. Not currently saving manually
            // 4. User hasn't edited in last 5 seconds (debounce)

            if (!_isNewGuide &&
                !string.IsNullOrWhiteSpace(Title) &&
                !IsSaving &&
                (DateTime.Now - _lastUserEdit).TotalSeconds >= 5)
            {
                await AutoSaveAsync();
            }
        });
    }

    /// <summary>
    /// Automatically saves the guide draft in the background.
    /// </summary>
    private async Task AutoSaveAsync()
    {
        try
        {
            IsSaving = true;
            var previousStatus = StatusMessage;
            StatusMessage = "Auto-saving...";

            // Auto-increment version for existing guides
            if (!_isNewGuide)
            {
                Version = await GetNextVersionAsync();
            }

            var guide = BuildGuideModel();
            await _guideService.SaveGuideAsync(guide);

            StatusMessage = $"Auto-saved at {DateTime.Now:h:mm tt}";
            _logger.LogDebug("Auto-saved guide {GuideId} v{Version}", guide.GuideId, Version);

            // Restore previous status after 3 seconds
            await Task.Delay(3000);
            if (StatusMessage.StartsWith("Auto-saved"))
            {
                StatusMessage = previousStatus;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Auto-save failed");
            StatusMessage = "Auto-save failed";
        }
        finally
        {
            IsSaving = false;
        }
    }

    /// <summary>
    /// Tracks when user edits title.
    /// </summary>
    partial void OnTitleChanged(string value)
    {
        _lastUserEdit = DateTime.Now;
        ValidateProperty(value, nameof(Title));
    }

    /// <summary>
    /// Tracks when user edits description.
    /// </summary>
    partial void OnDescriptionChanged(string value)
    {
        _lastUserEdit = DateTime.Now;
        ValidateProperty(value, nameof(Description));
    }

    /// <summary>
    /// Validates selected category.
    /// </summary>
    partial void OnSelectedCategoryChanged(string value)
    {
        ValidateProperty(value, nameof(SelectedCategory));
    }

    /// <summary>
    /// Validates selected difficulty.
    /// </summary>
    partial void OnSelectedDifficultyChanged(string value)
    {
        ValidateProperty(value, nameof(SelectedDifficulty));
    }

    /// <summary>
    /// Notify when IsSaving changes (affects CanSaveDraft).
    /// </summary>
    partial void OnIsSavingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSaveDraft));
    }

    /// <summary>
    /// Notify when IsPublishing changes (affects CanPublish).
    /// </summary>
    partial void OnIsPublishingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanPublish));
    }

    /// <summary>
    /// Cleanup resources when ViewModel is disposed.
    /// </summary>
    public void Dispose()
    {
        _autoSaveTimer?.Dispose();
        _autoSaveTimer = null;
    }

    /// <summary>
    /// Loads an existing guide for editing.
    /// </summary>
    public async Task LoadGuideAsync(string guideId)
    {
        try
        {
            var guide = await _guideService.GetGuideAsync(guideId);
            if (guide == null)
            {
                _logger.LogWarning("Guide {GuideId} not found", guideId);
                StatusMessage = "Guide not found";
                return;
            }

            _originalGuideId = guideId;
            _isNewGuide = false;

            // Load metadata
            GuideId = guide.GuideId;
            Title = guide.Title ?? string.Empty;
            Description = guide.Description ?? string.Empty;
            SelectedCategory = guide.Category ?? GuideCategories.All[0];
            SelectedDifficulty = guide.Difficulty ?? GuideDifficulty.All[0];
            TargetAudience = guide.TargetAudience ?? string.Empty;
            EstimatedMinutes = guide.EstimatedMinutes ?? 30;
            Version = guide.Version ?? "1.0";
            Author = guide.Author ?? await _userService.GetCurrentUserNameAsync();

            // Load collections
            Tags = new ObservableCollection<string>(guide.Tags ?? new List<string>());
            Prerequisites = new ObservableCollection<string>(guide.Prerequisites ?? new List<string>());

            // Load steps
            Steps.Clear();
            if (guide.Steps != null)
            {
                foreach (var step in guide.Steps.OrderBy(s => s.OrderIndex))
                {
                    Steps.Add(new StepEditorItem
                    {
                        StepId = step.StepId,
                        OrderIndex = step.OrderIndex,
                        Title = step.Title ?? string.Empty,
                        Content = step.Content ?? string.Empty,
                        Notes = step.Notes ?? string.Empty,
                        WarningLevel = step.WarningLevel,
                        // MediaReference doesn't store URLs directly - they're managed separately
                        // For now, initialize as empty collection for editor
                        MediaUrls = new ObservableCollection<string>()
                    });
                }
            }

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(IsEditMode));
            StatusMessage = "Guide loaded";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide {GuideId}", guideId);
            StatusMessage = "Error loading guide";
        }
    }

    /// <summary>
    /// Starts creating a new guide.
    /// </summary>
    [RelayCommand]
    private async Task NewGuideAsync()
    {
        try
        {
            _isNewGuide = true;
            _originalGuideId = null;

            GuideId = Guid.NewGuid().ToString();
            Title = string.Empty;
            Description = string.Empty;
            SelectedCategory = GuideCategories.All[0];
            SelectedDifficulty = GuideDifficulty.All[0];
            TargetAudience = string.Empty;
            EstimatedMinutes = 30;
            Version = "1.0";
            Author = await _userService.GetCurrentUserNameAsync();

            Tags.Clear();
            Prerequisites.Clear();
            Steps.Clear();

            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(IsEditMode));
            StatusMessage = "Ready to create new guide";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating new guide");
            StatusMessage = "Error creating new guide";
        }
    }

    [RelayCommand]
    private void AddTag()
    {
        if (!string.IsNullOrWhiteSpace(NewTagInput) && !Tags.Contains(NewTagInput))
        {
            Tags.Add(NewTagInput.Trim());
            NewTagInput = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveTag(string tag)
    {
        Tags.Remove(tag);
    }

    [RelayCommand]
    private void AddPrerequisite()
    {
        if (!string.IsNullOrWhiteSpace(NewPrerequisiteInput) && !Prerequisites.Contains(NewPrerequisiteInput))
        {
            Prerequisites.Add(NewPrerequisiteInput.Trim());
            NewPrerequisiteInput = string.Empty;
        }
    }

    [RelayCommand]
    private void RemovePrerequisite(string prerequisite)
    {
        Prerequisites.Remove(prerequisite);
    }

    [RelayCommand]
    private void AddStep()
    {
        var newStep = new StepEditorItem
        {
            StepId = Guid.NewGuid().ToString(),
            OrderIndex = Steps.Count + 1,
            Title = $"Step {Steps.Count + 1}",
            Content = string.Empty
        };

        Steps.Add(newStep);
        SelectedStep = newStep;
    }

    [RelayCommand]
    private void RemoveStep(StepEditorItem step)
    {
        if (step != null && Steps.Contains(step))
        {
            Steps.Remove(step);
            ReorderSteps();
        }
    }

    [RelayCommand]
    private void MoveStepUp(StepEditorItem step)
    {
        var index = Steps.IndexOf(step);
        if (index > 0)
        {
            Steps.Move(index, index - 1);
            ReorderSteps();
        }
    }

    [RelayCommand]
    private void MoveStepDown(StepEditorItem step)
    {
        var index = Steps.IndexOf(step);
        if (index < Steps.Count - 1)
        {
            Steps.Move(index, index + 1);
            ReorderSteps();
        }
    }

    private void ReorderSteps()
    {
        for (int i = 0; i < Steps.Count; i++)
        {
            Steps[i].OrderIndex = i + 1;
        }
    }

    [RelayCommand]
    private async Task SaveDraftAsync()
    {
        // Validate all properties before saving
        ValidateAllProperties();

        // If there are validation errors, don't save
        if (HasErrors)
        {
            StatusMessage = "Please fix validation errors before saving";
            return;
        }

        IsSaving = true;
        StatusMessage = "Saving draft...";

        try
        {
            // Auto-increment version if editing existing guide
            if (!_isNewGuide)
            {
                Version = await GetNextVersionAsync();
            }

            var guide = BuildGuideModel();
            await _guideService.SaveGuideAsync(guide);

            // If this was a new guide, it's no longer new after first save
            if (_isNewGuide)
            {
                _isNewGuide = false;
                _originalGuideId = guide.GuideId;
                OnPropertyChanged(nameof(PageTitle));
                OnPropertyChanged(nameof(IsEditMode));
            }

            StatusMessage = $"Draft saved (v{Version})";
            _logger.LogInformation("Draft saved: {GuideId} v{Version}", guide.GuideId, Version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving draft");
            StatusMessage = "Error saving draft";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task PublishToSharePointAsync()
    {
        // Validate all properties
        ValidateAllProperties();

        if (!ValidateGuide())
        {
            StatusMessage = "Please fill in all required fields";
            return;
        }

        IsPublishing = true;
        StatusMessage = "Publishing guide locally...";

        try
        {
            // Auto-increment version before publishing
            Version = await GetNextVersionAsync();

            var guide = BuildGuideModel();
            guide.Version = Version;

            // Mark as published
            guide.IsPublished = true;
            guide.LastModified = DateTime.UtcNow;

            // Save locally
            await _guideService.SaveGuideAsync(guide);
            _logger.LogInformation("Saved guide locally: {GuideId} v{Version}", guide.GuideId, Version);

            // Upload to SharePoint (NoOp in local-only mode)
            var uploadSuccess = await _sharePointService.UploadGuideAsync(guide);

            StatusMessage = $"Published successfully (v{Version})";
            _logger.LogInformation("Published guide: {GuideId} v{Version}", guide.GuideId, Version);

            // Navigate back to guide list
            await Task.Delay(1500); // Show success message
            _navigationService.NavigateTo("GuideList");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing guide");
            StatusMessage = $"Error publishing guide: {ex.Message}";
        }
        finally
        {
            IsPublishing = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.NavigateTo("GuideList");
    }

    private bool ValidateGuide()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            _logger.LogWarning("Validation failed: Title is required");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            _logger.LogWarning("Validation failed: Description is required");
            return false;
        }

        if (Steps.Count == 0)
        {
            _logger.LogWarning("Validation failed: At least one step is required");
            return false;
        }

        return true;
    }

    private Guide BuildGuideModel()
    {
        return new Guide
        {
            GuideId = GuideId,
            Title = Title,
            Description = Description,
            Category = SelectedCategory,
            Difficulty = SelectedDifficulty,
            TargetAudience = string.IsNullOrWhiteSpace(TargetAudience) ? null : TargetAudience,
            EstimatedMinutes = EstimatedMinutes,
            Version = Version,
            Author = Author,
            Tags = Tags.ToList(),
            Metadata = new GuideMetadata
            {
                Prerequisites = Prerequisites.ToList()
            },
            Steps = Steps.Select(s => new Step
            {
                StepId = s.StepId,
                OrderIndex = s.OrderIndex,
                Title = s.Title,
                Content = s.Content,
                Notes = string.IsNullOrWhiteSpace(s.Notes) ? null : s.Notes,
                WarningLevel = s.WarningLevel,
                MediaReferences = s.MediaUrls
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Select((url, index) => new MediaReference
                    {
                        MediaId = Guid.NewGuid().ToString(),
                        MediaType = DetermineMediaType(url),
                        OrderIndex = index,
                        Caption = url // Store URL as caption for now until proper media upload is implemented
                    }).ToList()
            }).ToList(),
            LastModified = DateTime.UtcNow
        };
    }

    private string DetermineMediaType(string url)
    {
        var extension = Path.GetExtension(url)?.ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "image",
            ".mp4" or ".avi" or ".mov" or ".wmv" or ".webm" => "video",
            ".pdf" or ".doc" or ".docx" or ".txt" => "document",
            _ => "image" // Default to image
        };
    }

    [RelayCommand]
    private async Task BrowseImageAsync()
    {
        try
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
            };

            // Add supported image file types
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".webp");

            // Initialize the picker with the window handle
            var window = App.GetService<MainWindow>();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Generate a unique media ID for this file
                var mediaId = Guid.NewGuid().ToString();

                // Read the file data
                var buffer = await Windows.Storage.FileIO.ReadBufferAsync(file);
                var fileData = new byte[buffer.Length];
                using (var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer))
                {
                    dataReader.ReadBytes(fileData);
                }

                // Cache the media file locally
                await _cacheService.CacheFileAsync("media", mediaId, fileData, string.Empty);

                _logger.LogInformation("Cached media file {MediaId} ({Size} KB)", mediaId, fileData.Length / 1024.0);

                // Add the mediaId to the current step's media URLs
                if (SelectedStep != null && !SelectedStep.MediaUrls.Contains(mediaId))
                {
                    SelectedStep.MediaUrls.Add(mediaId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error browsing and caching image");
            // In production, show error message to user
        }
    }
}

/// <summary>
/// Represents a step in the editor with editable properties.
/// </summary>
public partial class StepEditorItem : ObservableObject
{
    [ObservableProperty]
    private string _stepId = string.Empty;

    [ObservableProperty]
    private int _orderIndex;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WarningLevelIndex))]
    private WarningLevel _warningLevel = WarningLevel.Info;

    /// <summary>
    /// Warning level as integer index for ComboBox binding.
    /// </summary>
    public int WarningLevelIndex
    {
        get => (int)WarningLevel;
        set
        {
            if (WarningLevel != (WarningLevel)value)
            {
                WarningLevel = (WarningLevel)value;
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<string> _mediaUrls = new();

    [RelayCommand]
    private void RemoveMediaUrl(string url)
    {
        MediaUrls.Remove(url);
    }
}
