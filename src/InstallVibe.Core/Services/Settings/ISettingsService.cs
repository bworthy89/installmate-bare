using InstallVibe.Core.Models.Settings;

namespace InstallVibe.Core.Services.Settings;

/// <summary>
/// Service for managing application and user settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets the application settings.
    /// </summary>
    Task<AppSettings> GetAppSettingsAsync();

    /// <summary>
    /// Saves the application settings.
    /// </summary>
    Task SaveAppSettingsAsync(AppSettings settings);

    /// <summary>
    /// Gets user preferences for a specific user.
    /// </summary>
    Task<UserPreferences> GetUserPreferencesAsync(string userId);

    /// <summary>
    /// Saves user preferences for a specific user.
    /// </summary>
    Task SaveUserPreferencesAsync(string userId, UserPreferences preferences);

    /// <summary>
    /// Gets a specific setting value by key.
    /// </summary>
    Task<T?> GetSettingAsync<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Sets a specific setting value by key.
    /// </summary>
    Task SetSettingAsync<T>(string key, T value, string? category = null);

    /// <summary>
    /// Deletes a setting by key.
    /// </summary>
    Task DeleteSettingAsync(string key);

    /// <summary>
    /// Resets all settings to defaults.
    /// </summary>
    Task ResetToDefaultsAsync();
}
