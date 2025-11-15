using InstallVibe.Data.Entities;

namespace InstallVibe.Data.Repositories;

/// <summary>
/// Repository for managing application settings in the database.
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Gets a setting value by key.
    /// </summary>
    Task<SettingEntity?> GetAsync(string key);

    /// <summary>
    /// Gets all settings in a category.
    /// </summary>
    Task<List<SettingEntity>> GetByCategoryAsync(string category);

    /// <summary>
    /// Gets all settings.
    /// </summary>
    Task<List<SettingEntity>> GetAllAsync();

    /// <summary>
    /// Saves or updates a setting.
    /// </summary>
    Task SaveAsync(SettingEntity setting);

    /// <summary>
    /// Saves or updates multiple settings.
    /// </summary>
    Task SaveManyAsync(IEnumerable<SettingEntity> settings);

    /// <summary>
    /// Deletes a setting by key.
    /// </summary>
    Task DeleteAsync(string key);

    /// <summary>
    /// Deletes all settings in a category.
    /// </summary>
    Task DeleteCategoryAsync(string category);
}
