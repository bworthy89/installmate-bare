using InstallVibe.Core.Models.Settings;
using InstallVibe.Data.Entities;
using InstallVibe.Data.Repositories;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InstallVibe.Core.Services.Settings;

/// <summary>
/// Service implementation for managing application and user settings.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ISettingsRepository _repository;
    private readonly ILogger<SettingsService> _logger;

    private const string AppSettingsCategory = "AppSettings";
    private const string UserPreferencesCategory = "UserPreferences";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SettingsService(
        ISettingsRepository repository,
        ILogger<SettingsService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<AppSettings> GetAppSettingsAsync()
    {
        try
        {
            var settings = await _repository.GetByCategoryAsync(AppSettingsCategory);

            if (!settings.Any())
            {
                // Return defaults if no settings exist
                return new AppSettings();
            }

            var appSettings = new AppSettings();

            foreach (var setting in settings)
            {
                MapSettingToAppSettings(setting, appSettings);
            }

            return appSettings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting app settings");
            return new AppSettings(); // Return defaults on error
        }
    }

    /// <inheritdoc/>
    public async Task SaveAppSettingsAsync(AppSettings settings)
    {
        try
        {
            var entities = new List<SettingEntity>
            {
                CreateSettingEntity("Theme", settings.Theme, AppSettingsCategory),
                CreateSettingEntity("Language", settings.Language, AppSettingsCategory),
                CreateSettingEntity("AutoSync", settings.AutoSync.ToString(), AppSettingsCategory),
                CreateSettingEntity("SyncIntervalMinutes", settings.SyncIntervalMinutes.ToString(), AppSettingsCategory),
                CreateSettingEntity("OfflineMode", settings.OfflineMode.ToString(), AppSettingsCategory),
                CreateSettingEntity("CacheSizeLimitMB", settings.CacheSizeLimitMB.ToString(), AppSettingsCategory),
                CreateSettingEntity("EnableLogging", settings.EnableLogging.ToString(), AppSettingsCategory),
                CreateSettingEntity("LogLevel", settings.LogLevel, AppSettingsCategory)
            };

            await _repository.SaveManyAsync(entities);
            _logger.LogInformation("Saved app settings");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving app settings");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<UserPreferences> GetUserPreferencesAsync(string userId)
    {
        try
        {
            var key = $"{UserPreferencesCategory}:{userId}";
            var setting = await _repository.GetAsync(key);

            if (setting == null)
            {
                return new UserPreferences();
            }

            return JsonSerializer.Deserialize<UserPreferences>(setting.Value, JsonOptions)
                   ?? new UserPreferences();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user preferences for user {UserId}", userId);
            return new UserPreferences(); // Return defaults on error
        }
    }

    /// <inheritdoc/>
    public async Task SaveUserPreferencesAsync(string userId, UserPreferences preferences)
    {
        try
        {
            var key = $"{UserPreferencesCategory}:{userId}";
            var value = JsonSerializer.Serialize(preferences, JsonOptions);

            var entity = new SettingEntity
            {
                Key = key,
                Value = value,
                Category = UserPreferencesCategory,
                EncryptedValue = false,
                LastModified = DateTime.UtcNow
            };

            await _repository.SaveAsync(entity);
            _logger.LogInformation("Saved user preferences for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving user preferences for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetSettingAsync<T>(string key, T? defaultValue = default)
    {
        try
        {
            var setting = await _repository.GetAsync(key);

            if (setting == null)
            {
                return defaultValue;
            }

            return DeserializeValue<T>(setting.Value, defaultValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting setting {Key}", key);
            return defaultValue;
        }
    }

    /// <inheritdoc/>
    public async Task SetSettingAsync<T>(string key, T value, string? category = null)
    {
        try
        {
            var serializedValue = SerializeValue(value);

            var entity = new SettingEntity
            {
                Key = key,
                Value = serializedValue,
                Category = category,
                EncryptedValue = false,
                LastModified = DateTime.UtcNow
            };

            await _repository.SaveAsync(entity);
            _logger.LogDebug("Set setting {Key} to {Value}", key, value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteSettingAsync(string key)
    {
        try
        {
            await _repository.DeleteAsync(key);
            _logger.LogInformation("Deleted setting {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting setting {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ResetToDefaultsAsync()
    {
        try
        {
            await _repository.DeleteCategoryAsync(AppSettingsCategory);
            await SaveAppSettingsAsync(new AppSettings());
            _logger.LogInformation("Reset all settings to defaults");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting settings to defaults");
            throw;
        }
    }

    #region Private Helper Methods

    private static SettingEntity CreateSettingEntity(string key, string? value, string category)
    {
        return new SettingEntity
        {
            Key = key,
            Value = value ?? string.Empty,
            Category = category,
            EncryptedValue = false,
            LastModified = DateTime.UtcNow
        };
    }

    private void MapSettingToAppSettings(SettingEntity setting, AppSettings appSettings)
    {
        switch (setting.Key)
        {
            case "Theme":
                appSettings.Theme = setting.Value;
                break;
            case "Language":
                appSettings.Language = setting.Value;
                break;
            case "AutoSync":
                appSettings.AutoSync = bool.TryParse(setting.Value, out var autoSync) && autoSync;
                break;
            case "SyncIntervalMinutes":
                appSettings.SyncIntervalMinutes = int.TryParse(setting.Value, out var interval) ? interval : 60;
                break;
            case "OfflineMode":
                appSettings.OfflineMode = bool.TryParse(setting.Value, out var offline) && offline;
                break;
            case "CacheSizeLimitMB":
                appSettings.CacheSizeLimitMB = int.TryParse(setting.Value, out var cacheSize) ? cacheSize : 500;
                break;
            case "EnableLogging":
                appSettings.EnableLogging = bool.TryParse(setting.Value, out var logging) && logging;
                break;
            case "LogLevel":
                appSettings.LogLevel = setting.Value;
                break;
        }
    }

    private static string SerializeValue<T>(T value)
    {
        if (value == null)
            return string.Empty;

        if (value is string str)
            return str;

        if (value.GetType().IsPrimitive || value is decimal)
            return value.ToString() ?? string.Empty;

        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private static T? DeserializeValue<T>(string value, T? defaultValue)
    {
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        var type = typeof(T);

        if (type == typeof(string))
            return (T)(object)value;

        if (type == typeof(bool))
            return (T)(object)(bool.TryParse(value, out var boolVal) && boolVal);

        if (type == typeof(int))
            return (T)(object)(int.TryParse(value, out var intVal) ? intVal : 0);

        if (type == typeof(decimal))
            return (T)(object)(decimal.TryParse(value, out var decVal) ? decVal : 0);

        try
        {
            return JsonSerializer.Deserialize<T>(value, JsonOptions);
        }
        catch
        {
            return defaultValue;
        }
    }

    #endregion
}
