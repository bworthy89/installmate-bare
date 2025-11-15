using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace InstallVibe.ViewModels.Admin;

/// <summary>
/// ViewModel for the admin editor page (guide management).
/// </summary>
public partial class AdminEditorViewModel : ObservableObject
{
    private readonly ISharePointService _sharePointService;
    private readonly IGuideService _guideService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<AdminEditorViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<Guide> _guides = new();

    [ObservableProperty]
    private Guide? _selectedGuide;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isSaving = false;

    public AdminEditorViewModel(
        ISharePointService sharePointService,
        IGuideService guideService,
        INavigationService navigationService,
        ILogger<AdminEditorViewModel> logger)
    {
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand]
    private async Task LoadGuidesAsync()
    {
        IsLoading = true;

        try
        {
            // Load guides from local storage (already synced from SharePoint)
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
    private async Task SyncGuideAsync(string guideId)
    {
        IsSaving = true;

        try
        {
            // Download guide from SharePoint and save to local storage
            var guide = await _sharePointService.GetGuideAsync(guideId);

            if (guide != null)
            {
                await _guideService.SaveGuideAsync(guide);
                _logger.LogInformation("Guide {GuideId} synced successfully", guideId);

                // Reload guides to reflect changes
                await LoadGuidesAsync();
            }
            else
            {
                _logger.LogWarning("Guide {GuideId} not found in SharePoint", guideId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing guide {GuideId}", guideId);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInformation("Navigating back to Dashboard");
        _navigationService.NavigateTo("Dashboard");
    }
}
