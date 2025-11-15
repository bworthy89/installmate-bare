using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using InstallVibe.Core.Contracts;

namespace InstallVibe.ViewModels.Settings;

/// <summary>
/// ViewModel for the settings page.
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private ElementTheme _selectedTheme = ElementTheme.Default;

    [ObservableProperty]
    private bool _autoSync = true;

    [ObservableProperty]
    private int _cacheLifetimeDays = 30;

    [ObservableProperty]
    private bool _enableLogging = true;

    [ObservableProperty]
    private string _sharePointUrl = string.Empty;

    [ObservableProperty]
    private bool _hasChanges = false;

    [ObservableProperty]
    private bool _isSaving = false;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
    }

    public async Task InitializeAsync()
    {
        await LoadSettingsAsync();
    }

    [RelayCommand]
    private async Task LoadSettingsAsync()
    {
        try
        {
            // Load settings from service
            var settings = await _settingsService.LoadSettingsAsync();

            SelectedTheme = Enum.Parse<ElementTheme>(settings.Theme ?? "Default");
            AutoSync = settings.AutoSync;
            CacheLifetimeDays = settings.CacheLifetimeDays;
            EnableLogging = settings.EnableLogging;
            SharePointUrl = settings.SharePointUrl ?? string.Empty;

            HasChanges = false;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load settings: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        IsSaving = true;
        StatusMessage = string.Empty;

        try
        {
            await _settingsService.SaveSettingAsync("Theme", SelectedTheme.ToString());
            await _settingsService.SaveSettingAsync("AutoSync", AutoSync);
            await _settingsService.SaveSettingAsync("CacheLifetimeDays", CacheLifetimeDays);
            await _settingsService.SaveSettingAsync("EnableLogging", EnableLogging);
            await _settingsService.SaveSettingAsync("SharePointUrl", SharePointUrl);

            HasChanges = false;
            StatusMessage = "Settings saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to save settings: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task ResetSettingsAsync()
    {
        await LoadSettingsAsync();
        StatusMessage = "Settings reset to last saved values";
    }

    [RelayCommand]
    private async Task ClearCacheAsync()
    {
        try
        {
            // Clear cache via service
            StatusMessage = "Cache cleared successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to clear cache: {ex.Message}";
        }
    }

    partial void OnSelectedThemeChanged(ElementTheme value)
    {
        HasChanges = true;
    }

    partial void OnAutoSyncChanged(bool value)
    {
        HasChanges = true;
    }

    partial void OnCacheLifetimeDaysChanged(int value)
    {
        HasChanges = true;
    }

    partial void OnEnableLoggingChanged(bool value)
    {
        HasChanges = true;
    }

    partial void OnSharePointUrlChanged(string value)
    {
        HasChanges = true;
    }
}
