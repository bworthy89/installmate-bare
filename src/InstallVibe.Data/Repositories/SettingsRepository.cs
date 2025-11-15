using InstallVibe.Data.Context;
using InstallVibe.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Data.Repositories;

/// <summary>
/// Repository implementation for managing application settings.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly InstallVibeContext _context;
    private readonly ILogger<SettingsRepository> _logger;

    public SettingsRepository(
        InstallVibeContext context,
        ILogger<SettingsRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<SettingEntity?> GetAsync(string key)
    {
        try
        {
            return await _context.Settings.FindAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting setting with key {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<SettingEntity>> GetByCategoryAsync(string category)
    {
        try
        {
            return await _context.Settings
                .Where(s => s.Category == category)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings for category {Category}", category);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<SettingEntity>> GetAllAsync()
    {
        try
        {
            return await _context.Settings.ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all settings");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync(SettingEntity setting)
    {
        try
        {
            setting.LastModified = DateTime.UtcNow;

            var existing = await _context.Settings.FindAsync(setting.Key);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(setting);
            }
            else
            {
                await _context.Settings.AddAsync(setting);
            }

            await _context.SaveChangesAsync();
            _logger.LogDebug("Saved setting {Key}", setting.Key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving setting {Key}", setting.Key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveManyAsync(IEnumerable<SettingEntity> settings)
    {
        try
        {
            foreach (var setting in settings)
            {
                setting.LastModified = DateTime.UtcNow;

                var existing = await _context.Settings.FindAsync(setting.Key);
                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(setting);
                }
                else
                {
                    await _context.Settings.AddAsync(setting);
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogDebug("Saved {Count} settings", settings.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving multiple settings");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteAsync(string key)
    {
        try
        {
            var setting = await _context.Settings.FindAsync(key);
            if (setting != null)
            {
                _context.Settings.Remove(setting);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Deleted setting {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting setting {Key}", key);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteCategoryAsync(string category)
    {
        try
        {
            var settings = await _context.Settings
                .Where(s => s.Category == category)
                .ToListAsync();

            if (settings.Any())
            {
                _context.Settings.RemoveRange(settings);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Deleted {Count} settings from category {Category}", settings.Count, category);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting settings from category {Category}", category);
            throw;
        }
    }
}
