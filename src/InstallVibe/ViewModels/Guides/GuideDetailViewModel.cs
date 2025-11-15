using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Engine;
using InstallVibe.Services.Navigation;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for the guide detail/execution page.
/// </summary>
public partial class GuideDetailViewModel : ObservableObject
{
    private readonly IGuideEngine _guideEngine;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private Guide? _guide;

    [ObservableProperty]
    private Step? _currentStep;

    [ObservableProperty]
    private int _currentStepIndex = 0;

    [ObservableProperty]
    private int _totalSteps = 0;

    [ObservableProperty]
    private int _progressPercentage = 0;

    [ObservableProperty]
    private bool _canNavigateNext = false;

    [ObservableProperty]
    private bool _canNavigatePrevious = false;

    [ObservableProperty]
    private bool _isCompleted = false;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public GuideDetailViewModel(IGuideEngine guideEngine, INavigationService navigationService)
    {
        _guideEngine = guideEngine ?? throw new ArgumentNullException(nameof(guideEngine));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public async Task InitializeAsync(Guide guide)
    {
        if (guide == null)
            throw new ArgumentNullException(nameof(guide));

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            Guide = guide;
            TotalSteps = guide.Steps.Count;

            // Start or resume the guide
            await _guideEngine.StartGuideAsync(guide.Id);
            await LoadCurrentStepAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to initialize guide: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NextStepAsync()
    {
        if (!CanNavigateNext || Guide == null) return;

        try
        {
            await _guideEngine.CompleteCurrentStepAsync();
            await _guideEngine.MoveToNextStepAsync();
            await LoadCurrentStepAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to move to next step: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task PreviousStepAsync()
    {
        if (!CanNavigatePrevious) return;

        try
        {
            await _guideEngine.MoveToPreviousStepAsync();
            await LoadCurrentStepAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to move to previous step: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CompleteGuideAsync()
    {
        try
        {
            await _guideEngine.CompleteGuideAsync();
            IsCompleted = true;
            _navigationService.NavigateTo("Guides");
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to complete guide: {ex.Message}";
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.GoBack();
    }

    private async Task LoadCurrentStepAsync()
    {
        var stepInfo = await _guideEngine.GetCurrentStepAsync();

        if (stepInfo != null && Guide != null)
        {
            CurrentStep = Guide.Steps[stepInfo.CurrentStepIndex];
            CurrentStepIndex = stepInfo.CurrentStepIndex + 1; // 1-based for display
            ProgressPercentage = (int)((double)stepInfo.CurrentStepIndex / TotalSteps * 100);

            CanNavigateNext = stepInfo.CanMoveNext;
            CanNavigatePrevious = stepInfo.CanMovePrevious;
            IsCompleted = stepInfo.IsCompleted;
        }
    }

    [RelayCommand]
    private async Task MarkStepCompleteAsync()
    {
        try
        {
            await _guideEngine.CompleteCurrentStepAsync();
            await LoadCurrentStepAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to mark step complete: {ex.Message}";
        }
    }
}
