using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for the guide list page.
/// </summary>
public partial class GuideListViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<GuideListViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<Guide> _guides = new();

    [ObservableProperty]
    private Guide? _selectedGuide;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _searchText = string.Empty;

    public GuideListViewModel(
        IGuideService guideService,
        INavigationService navigationService,
        ILogger<GuideListViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ = LoadGuidesAsync();
    }

    [RelayCommand]
    private async Task LoadGuidesAsync()
    {
        IsLoading = true;

        try
        {
            var guides = await _guideService.GetAllGuidesAsync();
            Guides = new ObservableCollection<Guide>(guides);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guides");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectGuide(Guide guide)
    {
        if (guide != null)
        {
            _navigationService.NavigateTo("GuideDetail", guide.GuideId);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInformation("Navigating back to Dashboard");
        _navigationService.NavigateTo("Dashboard");
    }
}
