using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.Progress;
using InstallVibe.Core.Services.Engine;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for individual guide step page.
/// </summary>
public partial class StepViewModel : ObservableObject
{
    private readonly IGuideEngine _guideEngine;
    private readonly INavigationService _navigationService;
    private readonly ILogger<StepViewModel> _logger;

    [ObservableProperty]
    private Step? _currentStep;

    [ObservableProperty]
    private string _currentGuideId = string.Empty;

    [ObservableProperty]
    private string _currentProgressId = string.Empty;

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private string _notes = string.Empty;

    [ObservableProperty]
    private int _totalSteps = 0;

    [ObservableProperty]
    private bool _hasPreviousStep = false;

    [ObservableProperty]
    private bool _hasNextStep = false;

    public StepViewModel(
        IGuideEngine guideEngine,
        INavigationService navigationService,
        ILogger<StepViewModel> logger)
    {
        _guideEngine = guideEngine ?? throw new ArgumentNullException(nameof(guideEngine));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LoadGuideAsync(string guideId)
    {
        try
        {
            _logger.LogInformation("Loading guide {GuideId} for step view", guideId);
            CurrentGuideId = guideId;

            // TODO: Get actual user ID from authentication service
            var userId = Environment.UserName; // Temporary: use Windows username

            // Load the guide to get step count
            var guide = await _guideEngine.LoadGuideAsync(guideId);
            if (guide != null)
            {
                TotalSteps = guide.StepCount;
            }

            // Start or resume guide to get progress
            var progress = await _guideEngine.StartGuideAsync(guideId, userId);
            CurrentProgressId = progress.ProgressId;

            // Load the current step
            if (!string.IsNullOrEmpty(progress.CurrentStepId))
            {
                CurrentStep = await _guideEngine.GetStepAsync(guideId, progress.CurrentStepId);

                if (CurrentStep != null)
                {
                    _logger.LogInformation("Loaded step {StepId} for guide {GuideId}", CurrentStep.StepId, guideId);
                    await UpdateNavigationState();
                }
                else
                {
                    _logger.LogWarning("Current step {StepId} not found for guide {GuideId}", progress.CurrentStepId, guideId);
                }
            }
            else
            {
                _logger.LogWarning("No current step ID found in progress for guide {GuideId}", guideId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide {GuideId} for step view", guideId);
        }
    }

    private async Task UpdateNavigationState()
    {
        if (string.IsNullOrEmpty(CurrentGuideId) || CurrentStep == null)
        {
            HasPreviousStep = false;
            HasNextStep = false;
            return;
        }

        var previousStep = await _guideEngine.GetPreviousStepAsync(CurrentGuideId, CurrentStep.StepId);
        var nextStep = await _guideEngine.GetNextStepAsync(CurrentGuideId, CurrentStep.StepId);

        HasPreviousStep = previousStep != null;
        HasNextStep = nextStep != null;
    }

    [RelayCommand]
    private async Task PreviousStepAsync()
    {
        if (CurrentStep == null || string.IsNullOrEmpty(CurrentGuideId)) return;

        try
        {
            var previousStep = await _guideEngine.GetPreviousStepAsync(CurrentGuideId, CurrentStep.StepId);
            if (previousStep != null)
            {
                CurrentStep = previousStep;
                await UpdateNavigationState();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to previous step");
        }
    }

    [RelayCommand]
    private async Task NextStepAsync()
    {
        if (CurrentStep == null || string.IsNullOrEmpty(CurrentGuideId)) return;

        try
        {
            var nextStep = await _guideEngine.GetNextStepAsync(CurrentGuideId, CurrentStep.StepId);
            if (nextStep != null)
            {
                CurrentStep = nextStep;
                await UpdateNavigationState();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to next step");
        }
    }

    [RelayCommand]
    private async Task CompleteStepAsync()
    {
        if (CurrentStep == null || string.IsNullOrEmpty(CurrentProgressId)) return;

        IsProcessing = true;

        try
        {
            await _guideEngine.CompleteStepAsync(CurrentProgressId, CurrentStep.StepId);

            var nextStep = await _guideEngine.GetNextStepAsync(CurrentGuideId, CurrentStep.StepId);

            if (nextStep != null)
            {
                CurrentStep = nextStep;
            }
            else
            {
                _navigationService.GoBack();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing step {StepId}", CurrentStep.StepId);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task SkipStepAsync()
    {
        if (CurrentStep == null || string.IsNullOrEmpty(CurrentProgressId)) return;

        IsProcessing = true;

        try
        {
            await _guideEngine.UpdateStepStatusAsync(
                CurrentProgressId,
                CurrentStep.StepId,
                StepStatus.Skipped);

            var nextStep = await _guideEngine.GetNextStepAsync(CurrentGuideId, CurrentStep.StepId);

            if (nextStep != null)
            {
                CurrentStep = nextStep;
            }
            else
            {
                _navigationService.GoBack();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error skipping step {StepId}", CurrentStep.StepId);
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
