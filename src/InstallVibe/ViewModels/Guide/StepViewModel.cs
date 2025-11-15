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

    public StepViewModel(
        IGuideEngine guideEngine,
        INavigationService navigationService,
        ILogger<StepViewModel> logger)
    {
        _guideEngine = guideEngine ?? throw new ArgumentNullException(nameof(guideEngine));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
