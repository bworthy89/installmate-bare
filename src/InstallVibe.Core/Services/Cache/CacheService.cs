using InstallVibe.Core.Models.Cache;
using InstallVibe.Data.Context;
using InstallVibe.Infrastructure.Security.Cryptography;
using InstallVibe.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Cache;

/// <summary>
/// Implements file system cache management with LRU eviction and integrity checking.
/// </summary>
public class CacheService : ICacheService
{
    private readonly InstallVibeContext _context;
    private readonly IHashService _hashService;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        InstallVibeContext context,
        IHashService hashService,
        ILogger<CacheService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Ensure directories exist
        PathConstants.EnsureDirectoriesExist();
    }

    /// <inheritdoc/>
    public async Task<bool> IsCachedAsync(string entityType, string entityId)
    {
        try
        {
            var path = await GetCachePathAsync(entityType, entityId);
            return path != null && File.Exists(path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache for {EntityType}:{EntityId}", entityType, entityId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetCachePathAsync(string entityType, string entityId)
    {
        return entityType.ToLower() switch
        {
            "guide" => await Task.FromResult(PathConstants.GetGuideJsonPath(entityId)),
            "step" => await GetStepCachePathAsync(entityId),
            "media" => await GetMediaCachePathAsync(entityId),
            _ => null
        };
    }

    /// <inheritdoc/>
    public async Task CacheFileAsync(string entityType, string entityId, byte[] data, string checksum)
    {
        try
        {
            // Verify checksum before caching
            var computedChecksum = Convert.ToHexString(_hashService.ComputeSha256(data)).ToLowerInvariant();
            if (computedChecksum != checksum.ToLowerInvariant())
            {
                throw new InvalidOperationException($"Checksum mismatch for {entityType}:{entityId}");
            }

            // Get cache path
            var path = await GetCachePathAsync(entityType, entityId);
            if (path == null)
            {
                throw new InvalidOperationException($"Unknown entity type: {entityType}");
            }

            // Ensure directory exists
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Write file
            await File.WriteAllBytesAsync(path, data);

            // Update database metadata
            await UpdateCacheMetadataAsync(entityType, entityId, path, data.Length, checksum);

            _logger.LogInformation("Cached {EntityType}:{EntityId} ({Size} bytes)", 
                entityType, entityId, data.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching {EntityType}:{EntityId}", entityType, entityId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> ReadCachedFileAsync(string entityType, string entityId)
    {
        try
        {
            var path = await GetCachePathAsync(entityType, entityId);
            if (path == null || !File.Exists(path))
            {
                throw new FileNotFoundException($"Cached file not found for {entityType}:{entityId}");
            }

            // Read file
            var data = await File.ReadAllBytesAsync(path);

            // Verify integrity
            var expectedChecksum = await GetExpectedChecksumAsync(entityType, entityId);
            if (!string.IsNullOrEmpty(expectedChecksum))
            {
                var computedChecksum = Convert.ToHexString(_hashService.ComputeSha256(data)).ToLowerInvariant();
                if (computedChecksum != expectedChecksum.ToLowerInvariant())
                {
                    _logger.LogWarning("Checksum mismatch for {EntityType}:{EntityId}, file may be corrupted", 
                        entityType, entityId);
                    
                    // Mark for re-download
                    await InvalidateCacheAsync(entityType, entityId);
                    throw new InvalidOperationException($"Corrupted cache file: {entityType}:{entityId}");
                }
            }

            // Update last accessed time
            await UpdateLastAccessedAsync(entityType, entityId);

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading cached {EntityType}:{EntityId}", entityType, entityId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateCacheAsync(string entityType, string entityId)
    {
        try
        {
            var path = await GetCachePathAsync(entityType, entityId);
            if (path != null && File.Exists(path))
            {
                File.Delete(path);
                _logger.LogInformation("Invalidated cache for {EntityType}:{EntityId}", entityType, entityId);
            }

            // Update sync status in database
            if (entityType.ToLower() == "guide")
            {
                var guide = await _context.Guides.FindAsync(entityId);
                if (guide != null)
                {
                    guide.SyncStatus = "pending";
                    await _context.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for {EntityType}:{EntityId}", entityType, entityId);
        }
    }

    /// <inheritdoc/>
    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        var stats = new CacheStatistics();

        try
        {
            // Get guide statistics
            var guides = await _context.Guides.Where(g => !g.IsDeleted).ToListAsync();
            stats.GuideCount = guides.Count;
            stats.LargestGuideSize = guides.Max(g => g.FileSize ?? 0);

            // Get media statistics
            var media = await _context.MediaCache.ToListAsync();
            stats.MediaCount = media.Count;
            stats.TotalSizeBytes = guides.Sum(g => g.FileSize ?? 0) + media.Sum(m => m.FileSize);
            stats.LargestMediaSize = media.Any() ? media.Max(m => m.FileSize) : 0;
            stats.OldestCachedDate = media.Any() ? media.Min(m => m.CachedDate) : null;

            // Calculate usage percentage
            stats.UsagePercentage = (stats.TotalSizeBytes / (double)CacheConstants.MaxTotalCacheSize) * 100;

            // Check if cleanup needed
            stats.NeedsCleanup = stats.TotalSizeBytes > CacheConstants.CacheWarningThreshold;

            // Generate warnings
            if (stats.UsagePercentage > 80)
            {
                stats.Warnings.Add($"Cache is {stats.UsagePercentage:F1}% full");
            }

            if (stats.LargestGuideSize > CacheConstants.MaxPerGuideSize)
            {
                stats.Warnings.Add($"Some guides exceed recommended size ({stats.LargestGuideSize / 1024 / 1024} MB)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cache statistics");
        }

        return stats;
    }

    /// <inheritdoc/>
    public async Task CleanupCacheAsync(bool force = false)
    {
        try
        {
            var stats = await GetCacheStatisticsAsync();

            if (!force && !stats.NeedsCleanup)
            {
                _logger.LogInformation("Cache cleanup not needed (Usage: {Usage:F1}%)", stats.UsagePercentage);
                return;
            }

            _logger.LogInformation("Starting cache cleanup (Current size: {Size} MB)", stats.TotalSizeMB);

            int cleanedCount = 0;

            // Step 1: Clear temp files
            await ClearTempFilesAsync();

            // Step 2: Remove orphaned media (not referenced by any guide)
            cleanedCount += await RemoveOrphanedMediaAsync();

            // Step 3: LRU eviction if still needed
            if (stats.TotalSizeBytes > CacheConstants.CacheWarningThreshold)
            {
                cleanedCount += await PerformLruEvictionAsync();
            }

            _logger.LogInformation("Cache cleanup completed. Removed {Count} items", cleanedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache cleanup");
        }
    }

    /// <inheritdoc/>
    public async Task<int> VerifyIntegrityAsync()
    {
        int corruptedCount = 0;

        try
        {
            _logger.LogInformation("Starting cache integrity verification");

            // Verify guides
            var guides = await _context.Guides
                .Where(g => !g.IsDeleted && g.Checksum != null)
                .ToListAsync();

            foreach (var guide in guides)
            {
                if (!File.Exists(guide.LocalPath))
                {
                    _logger.LogWarning("Guide file missing: {GuideId}", guide.GuideId);
                    guide.SyncStatus = "pending";
                    corruptedCount++;
                    continue;
                }

                try
                {
                    var data = await File.ReadAllBytesAsync(guide.LocalPath);
                    var checksum = Convert.ToHexString(_hashService.ComputeSha256(data)).ToLowerInvariant();

                    if (checksum != guide.Checksum?.ToLowerInvariant())
                    {
                        _logger.LogWarning("Checksum mismatch for guide: {GuideId}", guide.GuideId);
                        guide.SyncStatus = "error";
                        corruptedCount++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error verifying guide: {GuideId}", guide.GuideId);
                    guide.SyncStatus = "error";
                    corruptedCount++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Integrity check completed. Found {Count} corrupted files", corruptedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during integrity verification");
        }

        return corruptedCount;
    }

    /// <inheritdoc/>
    public async Task ClearTempFilesAsync()
    {
        try
        {
            if (Directory.Exists(PathConstants.TempPath))
            {
                var tempFiles = Directory.GetFiles(PathConstants.TempPath, "*", SearchOption.AllDirectories);
                var cutoffDate = DateTime.UtcNow.AddDays(-CacheConstants.TempFilesExpiration);

                foreach (var file in tempFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTimeUtc < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }

                _logger.LogInformation("Cleared {Count} temp files", tempFiles.Length);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing temp files");
        }
    }

    // Private helper methods

    private async Task<string?> GetStepCachePathAsync(string stepId)
    {
        var step = await _context.Steps.FindAsync(stepId);
        return step?.LocalPath;
    }

    private async Task<string?> GetMediaCachePathAsync(string mediaId)
    {
        var media = await _context.MediaCache.FindAsync(mediaId);
        return media?.LocalPath;
    }

    private async Task<string?> GetExpectedChecksumAsync(string entityType, string entityId)
    {
        return entityType.ToLower() switch
        {
            "guide" => (await _context.Guides.FindAsync(entityId))?.Checksum,
            "step" => (await _context.Steps.FindAsync(entityId))?.Checksum,
            "media" => (await _context.MediaCache.FindAsync(entityId))?.Checksum,
            _ => null
        };
    }

    private async Task UpdateCacheMetadataAsync(string entityType, string entityId, string path, long size, string checksum)
    {
        if (entityType.ToLower() == "guide")
        {
            var guide = await _context.Guides.FindAsync(entityId);
            if (guide != null)
            {
                guide.LocalPath = path;
                guide.FileSize = size;
                guide.Checksum = checksum;
                guide.CachedDate = DateTime.UtcNow;
                guide.SyncStatus = "synced";
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task UpdateLastAccessedAsync(string entityType, string entityId)
    {
        if (entityType.ToLower() == "media")
        {
            var media = await _context.MediaCache.FindAsync(entityId);
            if (media != null)
            {
                media.LastAccessed = DateTime.UtcNow;
                media.AccessCount++;
                await _context.SaveChangesAsync();
            }
        }
    }

    private async Task<int> RemoveOrphanedMediaAsync()
    {
        int count = 0;

        try
        {
            // Find media not referenced by any step
            var referencedMediaIds = await _context.Steps
                .Where(s => s.MediaReferences != null)
                .Select(s => s.MediaReferences!)
                .ToListAsync();

            var allMediaIds = await _context.MediaCache.Select(m => m.MediaId).ToListAsync();

            // This is simplified - in production, parse JSON to extract media IDs
            var orphanedMedia = allMediaIds; // TODO: Implement proper JSON parsing

            foreach (var mediaId in orphanedMedia.Take(100)) // Limit batch size
            {
                var media = await _context.MediaCache.FindAsync(mediaId);
                if (media != null &&
                    (DateTime.UtcNow - media.LastAccessed).TotalDays > CacheConstants.OrphanedMediaExpiration)
                {
                    if (File.Exists(media.LocalPath))
                    {
                        File.Delete(media.LocalPath);
                    }

                    _context.MediaCache.Remove(media);
                    count++;
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing orphaned media");
        }

        return count;
    }

    private async Task<int> PerformLruEvictionAsync()
    {
        int count = 0;

        try
        {
            // Get least recently accessed media
            var lruMedia = await _context.MediaCache
                .OrderBy(m => m.LastAccessed)
                .Take(CacheConstants.LruBatchSize)
                .ToListAsync();

            long freedSpace = 0;
            var targetFreeSpace = CacheConstants.MaxTotalCacheSize - CacheConstants.CacheWarningThreshold;

            foreach (var media in lruMedia)
            {
                if (freedSpace >= targetFreeSpace)
                    break;

                // Don't evict recently cached files
                if ((DateTime.UtcNow - media.CachedDate).TotalDays < CacheConstants.MinCacheRetention)
                    continue;

                if (File.Exists(media.LocalPath))
                {
                    File.Delete(media.LocalPath);
                    freedSpace += media.FileSize;
                }

                _context.MediaCache.Remove(media);
                count++;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("LRU eviction freed {Size} MB", freedSpace / 1024 / 1024);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LRU eviction");
        }

        return count;
    }
}
