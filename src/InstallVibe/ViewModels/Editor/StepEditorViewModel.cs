using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Editor;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Editor;

/// <summary>
/// ViewModel for editing a single step.
/// </summary>
public partial class StepEditorViewModel : ObservableObject
{
    private readonly IMediaUploadService _mediaUploadService;
    private readonly ILogger<StepEditorViewModel> _logger;

    [ObservableProperty]
    private Step? _currentStep;

    [ObservableProperty]
    private string _stepTitle = string.Empty;

    [ObservableProperty]
    private string _stepInstructions = string.Empty;

    [ObservableProperty]
    private int _estimatedMinutes = 5;

    [ObservableProperty]
    private string _validationType = "Manual";

    [ObservableProperty]
    private bool _validationRequired = false;

    [ObservableProperty]
    private string _validationScript = string.Empty;

    [ObservableProperty]
    private string _successCriteria = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Checkpoint> _checkpoints = new();

    [ObservableProperty]
    private ObservableCollection<MediaReference> _mediaItems = new();

    [ObservableProperty]
    private ObservableCollection<StepAction> _actions = new();

    [ObservableProperty]
    private Checkpoint? _selectedCheckpoint;

    [ObservableProperty]
    private MediaReference? _selectedMedia;

    [ObservableProperty]
    private StepAction? _selectedAction;

    [ObservableProperty]
    private bool _hasUnsavedChanges = false;

    [ObservableProperty]
    private bool _isUploadingMedia = false;

    [ObservableProperty]
    private string _uploadProgress = string.Empty;

    public StepEditorViewModel(
        IMediaUploadService mediaUploadService,
        ILogger<StepEditorViewModel> logger)
    {
        _mediaUploadService = mediaUploadService ?? throw new ArgumentNullException(nameof(mediaUploadService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LoadStep(Step step)
    {
        if (step == null)
            throw new ArgumentNullException(nameof(step));

        CurrentStep = step;
        HasUnsavedChanges = false;

        // Load step properties
        StepTitle = step.Title;
        StepInstructions = step.Instructions;
        EstimatedMinutes = (int)step.EstimatedDuration.TotalMinutes;

        // Load validation
        ValidationType = step.Validation?.Type ?? "Manual";
        ValidationRequired = step.Validation?.Required ?? false;
        ValidationScript = step.Validation?.ValidationScript ?? string.Empty;
        SuccessCriteria = step.Validation?.SuccessCriteria ?? string.Empty;

        // Load checkpoints
        Checkpoints.Clear();
        if (step.Checkpoints != null)
        {
            foreach (var checkpoint in step.Checkpoints)
            {
                Checkpoints.Add(checkpoint);
            }
        }

        // Load media
        MediaItems.Clear();
        if (step.Media != null)
        {
            foreach (var media in step.Media)
            {
                MediaItems.Add(media);
            }
        }

        // Load actions
        Actions.Clear();
        if (step.Actions != null)
        {
            foreach (var action in step.Actions)
            {
                Actions.Add(action);
            }
        }
    }

    public void SaveChanges()
    {
        if (CurrentStep == null) return;

        CurrentStep.Title = StepTitle;
        CurrentStep.Instructions = StepInstructions;
        CurrentStep.EstimatedDuration = TimeSpan.FromMinutes(EstimatedMinutes);

        CurrentStep.Validation = new StepValidation
        {
            Type = ValidationType,
            Required = ValidationRequired,
            ValidationScript = ValidationScript,
            SuccessCriteria = SuccessCriteria
        };

        CurrentStep.Checkpoints = new List<Checkpoint>(Checkpoints);
        CurrentStep.Media = new List<MediaReference>(MediaItems);
        CurrentStep.Actions = new List<StepAction>(Actions);

        HasUnsavedChanges = false;

        _logger.LogInformation("Saved changes to step {StepId}", CurrentStep.Id);
    }

    [RelayCommand]
    private void AddCheckpoint()
    {
        var checkpoint = new Checkpoint
        {
            Id = Guid.NewGuid().ToString(),
            Description = "New checkpoint",
            IsRequired = true,
            IsCompleted = false
        };

        Checkpoints.Add(checkpoint);
        SelectedCheckpoint = checkpoint;
        HasUnsavedChanges = true;

        _logger.LogInformation("Added new checkpoint");
    }

    [RelayCommand]
    private void DeleteCheckpoint(Checkpoint? checkpoint)
    {
        if (checkpoint == null) return;

        Checkpoints.Remove(checkpoint);
        SelectedCheckpoint = null;
        HasUnsavedChanges = true;

        _logger.LogInformation("Deleted checkpoint {CheckpointId}", checkpoint.Id);
    }

    [RelayCommand]
    private void MoveCheckpointUp(Checkpoint? checkpoint)
    {
        if (checkpoint == null) return;

        var index = Checkpoints.IndexOf(checkpoint);
        if (index <= 0) return;

        Checkpoints.Move(index, index - 1);
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void MoveCheckpointDown(Checkpoint? checkpoint)
    {
        if (checkpoint == null) return;

        var index = Checkpoints.IndexOf(checkpoint);
        if (index >= Checkpoints.Count - 1) return;

        Checkpoints.Move(index, index + 1);
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private async Task UploadImageAsync(string filePath)
    {
        if (CurrentStep == null || string.IsNullOrEmpty(filePath))
            return;

        IsUploadingMedia = true;
        UploadProgress = "Validating image...";

        try
        {
            // Validate the media file
            var (isValid, errorMessage) = await _mediaUploadService.ValidateMediaAsync(filePath);

            if (!isValid)
            {
                _logger.LogWarning("Image validation failed: {ErrorMessage}", errorMessage);
                UploadProgress = $"Validation failed: {errorMessage}";
                return;
            }

            UploadProgress = "Uploading image...";

            // Upload the image (using step ID as guide ID for organization)
            var mediaReference = await _mediaUploadService.UploadImageAsync(filePath, CurrentStep.Id);

            MediaItems.Add(mediaReference);
            SelectedMedia = mediaReference;
            HasUnsavedChanges = true;

            UploadProgress = "Upload complete!";
            _logger.LogInformation("Uploaded image {MediaId}", mediaReference.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image");
            UploadProgress = $"Upload failed: {ex.Message}";
        }
        finally
        {
            IsUploadingMedia = false;
        }
    }

    [RelayCommand]
    private async Task UploadVideoAsync(string filePath)
    {
        if (CurrentStep == null || string.IsNullOrEmpty(filePath))
            return;

        IsUploadingMedia = true;
        UploadProgress = "Validating video...";

        try
        {
            // Validate the media file
            var (isValid, errorMessage) = await _mediaUploadService.ValidateMediaAsync(filePath);

            if (!isValid)
            {
                _logger.LogWarning("Video validation failed: {ErrorMessage}", errorMessage);
                UploadProgress = $"Validation failed: {errorMessage}";
                return;
            }

            UploadProgress = "Uploading video...";

            // Upload the video (using step ID as guide ID for organization)
            var mediaReference = await _mediaUploadService.UploadVideoAsync(filePath, CurrentStep.Id);

            MediaItems.Add(mediaReference);
            SelectedMedia = mediaReference;
            HasUnsavedChanges = true;

            UploadProgress = "Upload complete!";
            _logger.LogInformation("Uploaded video {MediaId}", mediaReference.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload video");
            UploadProgress = $"Upload failed: {ex.Message}";
        }
        finally
        {
            IsUploadingMedia = false;
        }
    }

    [RelayCommand]
    private async Task DeleteMediaAsync(MediaReference? media)
    {
        if (media == null) return;

        try
        {
            await _mediaUploadService.DeleteMediaAsync(media.Id);

            MediaItems.Remove(media);
            SelectedMedia = null;
            HasUnsavedChanges = true;

            _logger.LogInformation("Deleted media {MediaId}", media.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete media {MediaId}", media.Id);
        }
    }

    [RelayCommand]
    private void MoveMediaUp(MediaReference? media)
    {
        if (media == null) return;

        var index = MediaItems.IndexOf(media);
        if (index <= 0) return;

        MediaItems.Move(index, index - 1);
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void MoveMediaDown(MediaReference? media)
    {
        if (media == null) return;

        var index = MediaItems.IndexOf(media);
        if (index >= MediaItems.Count - 1) return;

        MediaItems.Move(index, index + 1);
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void AddAction()
    {
        var action = new StepAction
        {
            Id = Guid.NewGuid().ToString(),
            Type = "Manual",
            Description = "New action",
            Command = string.Empty,
            Parameters = new Dictionary<string, string>(),
            IsRequired = true
        };

        Actions.Add(action);
        SelectedAction = action;
        HasUnsavedChanges = true;

        _logger.LogInformation("Added new action");
    }

    [RelayCommand]
    private void DeleteAction(StepAction? action)
    {
        if (action == null) return;

        Actions.Remove(action);
        SelectedAction = null;
        HasUnsavedChanges = true;

        _logger.LogInformation("Deleted action {ActionId}", action.Id);
    }

    [RelayCommand]
    private void MoveActionUp(StepAction? action)
    {
        if (action == null) return;

        var index = Actions.IndexOf(action);
        if (index <= 0) return;

        Actions.Move(index, index - 1);
        HasUnsavedChanges = true;
    }

    [RelayCommand]
    private void MoveActionDown(StepAction? action)
    {
        if (action == null) return;

        var index = Actions.IndexOf(action);
        if (index >= Actions.Count - 1) return;

        Actions.Move(index, index + 1);
        HasUnsavedChanges = true;
    }

    partial void OnStepTitleChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnStepInstructionsChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnEstimatedMinutesChanged(int value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnValidationTypeChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnValidationRequiredChanged(bool value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnValidationScriptChanged(string value)
    {
        HasUnsavedChanges = true;
    }

    partial void OnSuccessCriteriaChanged(string value)
    {
        HasUnsavedChanges = true;
    }
}
