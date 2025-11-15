using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Settings;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string _theme = "System";

    [ObservableProperty]
    private bool _autoSync = true;

    [ObservableProperty]
    private int _syncInterval = 60;

    [ObservableProperty]
    private int _cacheLimit = 500;

    public SettingsViewModel(
        INavigationService navigationService,
        ILogger<SettingsViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoadSettings();
    }

    private void LoadSettings()
    {
        // TODO: Load settings from ISettingsService when implemented
        _logger.LogInformation("Settings loaded");
    }

    [RelayCommand]
    private void SaveSettings()
    {
        // TODO: Save settings via ISettingsService when implemented
        _logger.LogInformation("Settings saved: Theme={Theme}, AutoSync={AutoSync}, SyncInterval={SyncInterval}, CacheLimit={CacheLimit}",
            Theme, AutoSync, SyncInterval, CacheLimit);
    }

    [RelayCommand]
    private void ResetSettings()
    {
        Theme = "System";
        AutoSync = true;
        SyncInterval = 60;
        CacheLimit = 500;
        _logger.LogInformation("Settings reset to defaults");
    }

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInformation("Navigating back to Dashboard");
        _navigationService.NavigateTo("Dashboard");
    }
}
