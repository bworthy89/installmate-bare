using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Editor;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Editor;

/// <summary>
/// ViewModel for the guide editor page.
/// </summary>
public partial class GuideEditorViewModel : ObservableObject
{
    private readonly IGuideEditorService _guideEditorService;
    private readonly IMediaUploadService _mediaUploadService;
    private readonly ISharePointService _sharePointService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<GuideEditorViewModel> _logger;

    [ObservableProperty]
    private Guide? _currentGuide;

    [ObservableProperty]
    private ObservableCollection<Step> _steps = new();

    [ObservableProperty]
    private Step? _selectedStep;

    [ObservableProperty]
    private bool _isNewGuide = true;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isSaving = false;

    [ObservableProperty]
    private bool _isPublishing = false;

    [ObservableProperty]
    private bool _isPreviewMode = false;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    [ObservableProperty]
    private bool _hasValidationErrors = false;

    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = new();

    // Guide metadata properties
    [ObservableProperty]
    private string _guideTitle = string.Empty;

    [ObservableProperty]
    private string _guideDescription = string.Empty;

    [ObservableProperty]
    private string _guideCategory = string.Empty;

    [ObservableProperty]
    private string _guideVersion = "1.0.0";

    [ObservableProperty]
    private string _guideDifficulty = "Intermediate";

    [ObservableProperty]
    private int _estimatedMinutes = 30;

    [ObservableProperty]
    private bool _requiresInternet = false;

    [ObservableProperty]
    private bool _requiresNetwork = false;

    [ObservableProperty]
    private string _tagInput = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _tags = new();

    public GuideEditorViewModel(
        IGuideEditorService guideEditorService,
        IMediaUploadService mediaUploadService,
        ISharePointService sharePointService,
        INavigationService navigationService,
        ILogger<GuideEditorViewModel> logger)
    {
        _guideEditorService = guideEditorService ?? throw new ArgumentNullException(nameof(guideEditorService));
        _mediaUploadService = mediaUploadService ?? throw new ArgumentNullException(nameof(mediaUploadService));
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InitializeAsync(string? guideId = null)
    {
        IsLoading = true;

        try
        {
            if (string.IsNullOrEmpty(guideId))
            {
                // Create new guide
                await CreateNewGuideAsync();
            }
            else
            {
                // Load existing guide
                await LoadGuideAsync(guideId);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CreateNewGuideAsync()
    {
        _logger.LogInformation("Creating new guide");

        CurrentGuide = await _guideEditorService.CreateNewGuideAsync();
        IsNewGuide = true;
        HasUnsavedChanges = false;

        LoadGuideMetadata();
        RefreshSteps();
    }

    [RelayCommand]
    private async Task LoadGuideAsync(string guideId)
    {
        _logger.LogInformation("Loading guide {GuideId}", guideId);

        CurrentGuide = await _guideEditorService.LoadDraftAsync(guideId);

        if (CurrentGuide == null)
        {
            _logger.LogWarning("Failed to load guide {GuideId}", guideId);
            await CreateNewGuideAsync();
            return;
        }

        IsNewGuide = false;
        HasUnsavedChanges = false;

        LoadGuideMetadata();
        RefreshSteps();
    }

    [RelayCommand]
    private async Task SaveDraftAsync()
    {
        if (CurrentGuide == null) return;

        IsSaving = true;
        try
        {
            UpdateGuideMetadata();

            var success = await _guideEditorService.SaveDraftAsync(CurrentGuide);

            if (success)
            {
                HasUnsavedChanges = false;
                _logger.LogInformation("Draft saved successfully");
            }
            else
            {
                _logger.LogWarning("Failed to save draft");
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task PublishGuideAsync()
    {
        if (CurrentGuide == null) return;

        // Validate before publishing
        if (!ValidateGuide())
        {
            _logger.LogWarning("Guide validation failed, cannot publish");
            return;
        }

        IsPublishing = true;
        try
        {
            UpdateGuideMetadata();

            // Increment version
            CurrentGuide.Metadata.Version = _guideEditorService.IncrementVersion(
                CurrentGuide.Metadata.Version,
                isMajor: false);

            // Save draft first
            await _guideEditorService.SaveDraftAsync(CurrentGuide);

            // Publish to SharePoint
            var success = await _sharePointService.UploadGuideAsync(CurrentGuide);

            if (success)
            {
                HasUnsavedChanges = false;
                GuideVersion = CurrentGuide.Metadata.Version;
                _logger.LogInformation("Guide published successfully");

                // Navigate back to guide list
                _navigationService.GoBack();
            }
            else
            {
                _logger.LogWarning("Failed to publish guide");
                ValidationMessage = "Failed to publish guide to SharePoint. Please check your connection.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing guide");
            ValidationMessage = $"Error publishing guide: {ex.Message}";
        }
        finally
        {
            IsPublishing = false;
        }
    }

    [RelayCommand]
    private async Task AddStepAsync()
    {
        if (CurrentGuide == null) return;

        var position = Steps.Count;
        var newStep = await _guideEditorService.AddStepAsync(CurrentGuide, position);

        RefreshSteps();
        SelectedStep = newStep;
        HasUnsavedChanges = true;

        _logger.LogInformation("Added new step at position {Position}", position);
    }

    [RelayCommand]
    private async Task DeleteStepAsync(Step? step)
    {
        if (CurrentGuide == null || step == null) return;

        await _guideEditorService.DeleteStepAsync(CurrentGuide, step.Id);

        RefreshSteps();
        SelectedStep = null;
        HasUnsavedChanges = true;

        _logger.LogInformation("Deleted step {StepId}", step.Id);
    }

    [RelayCommand]
    private async Task DuplicateStepAsync(Step? step)
    {
        if (CurrentGuide == null || step == null) return;

        var duplicatedStep = await _guideEditorService.DuplicateStepAsync(CurrentGuide, step);

        RefreshSteps();
        SelectedStep = duplicatedStep;
        HasUnsavedChanges = true;

        _logger.LogInformation("Duplicated step {StepId}", step.Id);
    }

    [RelayCommand]
    private async Task MoveStepUpAsync(Step? step)
    {
        if (CurrentGuide == null || step == null) return;

        var index = Steps.IndexOf(step);
        if (index <= 0) return;

        await _guideEditorService.ReorderStepsAsync(CurrentGuide, index, index - 1);

        RefreshSteps();
        SelectedStep = step;
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task MoveStepDownAsync(Step? step)
    {
        if (CurrentGuide == null || step == null) return;

        var index = Steps.IndexOf(step);
        if (index >= Steps.Count - 1) return;

        await _guideEditorService.ReorderStepsAsync(CurrentGuide, index, index + 1);

        RefreshSteps();
        SelectedStep = step;
        HasUnsavedChanges = true;
    }

    public async Task ReorderStepAsync(int oldIndex, int newIndex)
    {
        if (CurrentGuide == null) return;

        await _guideEditorService.ReorderStepsAsync(CurrentGuide, oldIndex, newIndex);

        RefreshSteps();
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void TogglePreviewMode()
    {
        IsPreviewMode = !IsPreviewMode;
        _logger.LogInformation("Preview mode: {IsPreviewMode}", IsPreviewMode);
    }

    [RelayCommand]
    private void AddTag()
    {
        if (string.IsNullOrWhiteSpace(TagInput)) return;

        var tag = TagInput.Trim();
        if (!Tags.Contains(tag))
        {
            Tags.Add(tag);
            HasUnsavedChanges = true;
        }

        TagInput = string.Empty;
    }

    [RelayCommand]
    private void RemoveTag(string tag)
    {
        if (Tags.Contains(tag))
        {
            Tags.Remove(tag);
            HasUnsavedChanges = true;
        }
    }

    [RelayCommand]
    private void IncrementMajorVersion()
    {
        if (CurrentGuide == null) return;

        GuideVersion = _guideEditorService.IncrementVersion(GuideVersion, isMajor: true);
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void IncrementMinorVersion()
    {
        if (CurrentGuide == null) return;

        GuideVersion = _guideEditorService.IncrementVersion(GuideVersion, isMajor: false);
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void Cancel()
    {
        if (HasUnsavedChanges)
        {
            // TODO: Show confirmation dialog
            _logger.LogInformation("Canceling with unsaved changes");
        }

        _navigationService.GoBack();
    }

    private void LoadGuideMetadata()
    {
        if (CurrentGuide == null) return;

        GuideTitle = CurrentGuide.Metadata.Title;
        GuideDescription = CurrentGuide.Metadata.Description ?? string.Empty;
        GuideCategory = CurrentGuide.Metadata.Category ?? string.Empty;
        GuideVersion = CurrentGuide.Metadata.Version;
        GuideDifficulty = CurrentGuide.Metadata.DifficultyLevel;
        EstimatedMinutes = (int)CurrentGuide.Metadata.EstimatedDuration.TotalMinutes;
        RequiresInternet = CurrentGuide.Requirements?.InternetRequired ?? false;
        RequiresNetwork = CurrentGuide.Requirements?.NetworkRequired ?? false;

        Tags.Clear();
        if (CurrentGuide.Metadata.Tags != null)
        {
            foreach (var tag in CurrentGuide.Metadata.Tags)
            {
                Tags.Add(tag);
            }
        }
    }

    private void UpdateGuideMetadata()
    {
        if (CurrentGuide == null) return;

        CurrentGuide.Metadata.Title = GuideTitle;
        CurrentGuide.Metadata.Description = GuideDescription;
        CurrentGuide.Metadata.Category = GuideCategory;
        CurrentGuide.Metadata.Version = GuideVersion;
        CurrentGuide.Metadata.DifficultyLevel = GuideDifficulty;
        CurrentGuide.Metadata.EstimatedDuration = TimeSpan.FromMinutes(EstimatedMinutes);
        CurrentGuide.Metadata.Tags = new List<string>(Tags);
        CurrentGuide.Metadata.LastModified = DateTime.UtcNow;

        if (CurrentGuide.Requirements != null)
        {
            CurrentGuide.Requirements.InternetRequired = RequiresInternet;
            CurrentGuide.Requirements.NetworkRequired = RequiresNetwork;
        }
    }

    private void RefreshSteps()
    {
        Steps.Clear();

        if (CurrentGuide?.Steps != null)
        {
            foreach (var step in CurrentGuide.Steps.OrderBy(s => s.OrderIndex))
            {
                Steps.Add(step);
            }
        }
    }

    private bool ValidateGuide()
    {
        ValidationErrors.Clear();
        HasValidationErrors = false;

        if (CurrentGuide == null)
        {
            ValidationErrors.Add("No guide loaded");
            HasValidationErrors = true;
            return false;
        }

        // Validate title
        if (string.IsNullOrWhiteSpace(GuideTitle))
        {
            ValidationErrors.Add("Guide title is required");
        }

        // Validate description
        if (string.IsNullOrWhiteSpace(GuideDescription))
        {
            ValidationErrors.Add("Guide description is required");
        }

        // Validate category
        if (string.IsNullOrWhiteSpace(GuideCategory))
        {
            ValidationErrors.Add("Guide category is required");
        }

        // Validate steps
        if (!Steps.Any())
        {
            ValidationErrors.Add("Guide must have at least one step");
        }
        else
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                var step = Steps[i];

                if (string.IsNullOrWhiteSpace(step.Title))
                {
                    ValidationErrors.Add($"Step {i + 1}: Title is required");
                }

                if (string.IsNullOrWhiteSpace(step.Instructions))
                {
                    ValidationErrors.Add($"Step {i + 1}: Instructions are required");
                }
            }
        }

        HasValidationErrors = ValidationErrors.Any();

        if (HasValidationErrors)
        {
            ValidationMessage = $"Validation failed with {ValidationErrors.Count} error(s)";
            _logger.LogWarning("Guide validation failed with {ErrorCount} errors", ValidationErrors.Count);
        }
        else
        {
            ValidationMessage = string.Empty;
        }

        return !HasValidationErrors;
    }

    partial void OnGuideTitleChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnGuideDescriptionChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnGuideCategoryChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnGuideDifficultyChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnEstimatedMinutesChanged(int value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnRequiresInternetChanged(bool value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnRequiresNetworkChanged(bool value)
    {
        HasUnsavedChanges = true;
    }
}
