namespace InstallVibe.Core.Contracts;

/// <summary>
/// Service for managing application settings.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Gets a setting value.
    /// </summary>
    T? Get<T>(string key, T? defaultValue = default);

    /// <summary>
    /// Sets a setting value.
    /// </summary>
    Task SetAsync<T>(string key, T value);

    /// <summary>
    /// Removes a setting.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a setting exists.
    /// </summary>
    bool Contains(string key);

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    Task SaveAsync();
}
