using InstallVibe.Core.Models.Cache;

namespace InstallVibe.Core.Services.Cache;

/// <summary>
/// Service for managing file system cache.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Checks if an entity is cached locally.
    /// </summary>
    Task<bool> IsCachedAsync(string entityType, string entityId);

    /// <summary>
    /// Gets the local file path for a cached entity.
    /// </summary>
    Task<string?> GetCachePathAsync(string entityType, string entityId);

    /// <summary>
    /// Caches a file with checksum verification.
    /// </summary>
    Task CacheFileAsync(string entityType, string entityId, byte[] data, string checksum);

    /// <summary>
    /// Reads a cached file with integrity verification.
    /// </summary>
    Task<byte[]> ReadCachedFileAsync(string entityType, string entityId);

    /// <summary>
    /// Invalidates and removes a cached entity.
    /// </summary>
    Task InvalidateCacheAsync(string entityType, string entityId);

    /// <summary>
    /// Gets current cache usage statistics.
    /// </summary>
    Task<CacheStatistics> GetCacheStatisticsAsync();

    /// <summary>
    /// Performs cache cleanup using LRU eviction.
    /// </summary>
    Task CleanupCacheAsync(bool force = false);

    /// <summary>
    /// Verifies integrity of all cached files.
    /// </summary>
    Task<int> VerifyIntegrityAsync();

    /// <summary>
    /// Clears all temporary files.
    /// </summary>
    Task ClearTempFilesAsync();
}
