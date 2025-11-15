using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace InstallVibe.ViewModels.Dashboard;

/// <summary>
/// ViewModel for the dashboard page showing recent guides and quick actions.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly IProgressService _progressService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<Guide> _recentGuides = new();

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public DashboardViewModel(
        IGuideService guideService,
        IProgressService progressService,
        INavigationService navigationService,
        ILogger<DashboardViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var guides = await _guideService.GetAllGuidesAsync();
            RecentGuides = new ObservableCollection<Guide>(guides.Take(5));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            ErrorMessage = "Failed to load dashboard data";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
