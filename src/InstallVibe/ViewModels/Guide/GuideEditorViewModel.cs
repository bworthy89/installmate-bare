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
using System.Collections.ObjectModel;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for creating and editing guides with full WYSIWYG editing capabilities.
/// </summary>
public partial class GuideEditorViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly ISharePointService _sharePointService;
    private readonly ICacheService _cacheService;
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<GuideEditorViewModel> _logger;

    private string? _originalGuideId;
    private bool _isNewGuide;

    // Guide Metadata
    [ObservableProperty]
    private string _guideId = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = GuideCategories.All[0];

    [ObservableProperty]
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
        IsSaving = true;
        StatusMessage = "Saving draft...";

        try
        {
            var guide = BuildGuideModel();
            await _guideService.SaveGuideAsync(guide);

            StatusMessage = "Draft saved successfully";
            _logger.LogInformation("Draft saved: {GuideId}", guide.GuideId);
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
        if (!ValidateGuide())
        {
            StatusMessage = "Please fill in all required fields";
            return;
        }

        IsPublishing = true;
        StatusMessage = "Publishing guide locally...";

        try
        {
            var guide = BuildGuideModel();

            // Mark as published
            guide.IsPublished = true;
            guide.LastModified = DateTime.UtcNow;

            // Save locally
            await _guideService.SaveGuideAsync(guide);
            _logger.LogInformation("Saved guide locally: {GuideId}", guide.GuideId);

            // Upload to SharePoint (NoOp in local-only mode)
            var uploadSuccess = await _sharePointService.UploadGuideAsync(guide);

            StatusMessage = "Published successfully";
            _logger.LogInformation("Published guide: {GuideId}", guide.GuideId);

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

    [ObservableProperty]
    private string _newMediaUrlInput = string.Empty;

    [RelayCommand]
    private void AddMediaUrl()
    {
        if (!string.IsNullOrWhiteSpace(NewMediaUrlInput) && !MediaUrls.Contains(NewMediaUrlInput))
        {
            MediaUrls.Add(NewMediaUrlInput.Trim());
            NewMediaUrlInput = string.Empty;
        }
    }

    [RelayCommand]
    private void RemoveMediaUrl(string url)
    {
        MediaUrls.Remove(url);
    }
}
