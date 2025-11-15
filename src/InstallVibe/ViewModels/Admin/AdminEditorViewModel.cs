using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.SharePoint;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace InstallVibe.ViewModels.Admin;

/// <summary>
/// ViewModel for the admin editor page (guide management).
/// </summary>
public partial class AdminEditorViewModel : ObservableObject
{
    private readonly ISharePointService _sharePointService;
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
        ILogger<AdminEditorViewModel> logger)
    {
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand]
    private async Task LoadGuidesAsync()
    {
        IsLoading = true;

        try
        {
            var guides = await _sharePointService.GetGuidesAsync();
            Guides = new ObservableCollection<Guide>(guides);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guides from SharePoint");
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
            await _sharePointService.SyncGuideAsync(guideId);
            _logger.LogInformation("Guide {GuideId} synced successfully", guideId);
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
}
