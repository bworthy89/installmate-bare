using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Services.Settings;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Settings;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
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
        ISettingsService settingsService,
        ILogger<SettingsViewModel> logger)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        LoadSettings();
    }

    private async void LoadSettings()
    {
        try
        {
            var appSettings = await _settingsService.GetAppSettingsAsync();

            Theme = appSettings.Theme ?? "System";
            AutoSync = appSettings.AutoSync;
            SyncInterval = appSettings.SyncIntervalMinutes;
            CacheLimit = appSettings.CacheSizeLimitMB;

            _logger.LogInformation("Settings loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading settings");
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        try
        {
            var appSettings = new Core.Models.Settings.AppSettings
            {
                Theme = Theme,
                AutoSync = AutoSync,
                SyncIntervalMinutes = SyncInterval,
                CacheSizeLimitMB = CacheLimit
            };

            await _settingsService.SaveAppSettingsAsync(appSettings);

            _logger.LogInformation("Settings saved: Theme={Theme}, AutoSync={AutoSync}, SyncInterval={SyncInterval}, CacheLimit={CacheLimit}",
                Theme, AutoSync, SyncInterval, CacheLimit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving settings");
        }
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        try
        {
            await _settingsService.ResetToDefaultsAsync();

            // Reset local values
            Theme = "System";
            AutoSync = true;
            SyncInterval = 60;
            CacheLimit = 500;

            _logger.LogInformation("Settings reset to defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting settings");
        }
    }
}
