using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Engine;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for the guide detail page.
/// </summary>
public partial class GuideDetailViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly IGuideEngine _guideEngine;
    private readonly INavigationService _navigationService;
    private readonly ILogger<GuideDetailViewModel> _logger;

    [ObservableProperty]
    private Guide? _guide;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isStarting = false;

    public GuideDetailViewModel(
        IGuideService guideService,
        IGuideEngine guideEngine,
        INavigationService navigationService,
        ILogger<GuideDetailViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _guideEngine = guideEngine ?? throw new ArgumentNullException(nameof(guideEngine));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task LoadGuideAsync(string guideId)
    {
        IsLoading = true;

        try
        {
            Guide = await _guideService.GetGuideAsync(guideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide {GuideId}", guideId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartGuideAsync()
    {
        if (Guide == null) return;

        IsStarting = true;

        try
        {
            // TODO: Get actual user ID from authentication service
            var userId = Environment.UserName; // Temporary: use Windows username
            await _guideEngine.StartGuideAsync(Guide.GuideId, userId);
            _navigationService.NavigateTo("Step", Guide.GuideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting guide {GuideId}", Guide.GuideId);
        }
        finally
        {
            IsStarting = false;
        }
    }
}
