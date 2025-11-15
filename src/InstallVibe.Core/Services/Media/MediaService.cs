using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Media;

/// <summary>
/// Implements media file management with caching and SharePoint integration.
/// </summary>
public class MediaService : IMediaService
{
    private readonly ISharePointService _sharePointService;
    private readonly ICacheService _cacheService;
    private readonly InstallVibeContext _context;
    private readonly ILogger<MediaService> _logger;

    public MediaService(
        ISharePointService sharePointService,
        ICacheService cacheService,
        InstallVibeContext context,
        ILogger<MediaService> logger)
    {
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<byte[]?> GetMediaAsync(string mediaId, bool downloadIfMissing = true)
    {
        try
        {
            // Try to read from cache first
            if (await _cacheService.IsCachedAsync("media", mediaId))
            {
                try
                {
                    var data = await _cacheService.ReadCachedFileAsync("media", mediaId);
                    _logger.LogDebug("Retrieved media {MediaId} from cache ({Size} bytes)", mediaId, data.Length);
                    return data;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read cached media {MediaId}, will attempt download", mediaId);
                    await _cacheService.InvalidateCacheAsync("media", mediaId);
                }
            }

            // Download from SharePoint if not cached
            if (downloadIfMissing)
            {
                _logger.LogInformation("Media {MediaId} not cached, downloading from SharePoint", mediaId);
                var success = await DownloadMediaAsync(mediaId);
                if (success)
                {
                    return await _cacheService.ReadCachedFileAsync("media", mediaId);
                }
            }

            _logger.LogWarning("Media {MediaId} not found", mediaId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media {MediaId}", mediaId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<string?> GetMediaPathAsync(string mediaId)
    {
        try
        {
            var path = await _cacheService.GetCachePathAsync("media", mediaId);
            if (path != null && File.Exists(path))
            {
                return path;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media path for {MediaId}", mediaId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsMediaCachedAsync(string mediaId)
    {
        return await _cacheService.IsCachedAsync("media", mediaId);
    }

    /// <inheritdoc/>
    public async Task<List<string>> CacheGuideMediaAsync(Guide guide, IProgress<MediaCacheProgress>? progress = null)
    {
        var failedMediaIds = new List<string>();
        
        try
        {
            // Collect all unique media IDs from guide
            var mediaIds = guide.Steps
                .SelectMany(s => s.MediaReferences ?? new List<MediaReference>())
                .Select(mr => mr.MediaId)
                .Distinct()
                .ToList();

            if (mediaIds.Count == 0)
            {
                _logger.LogInformation("Guide {GuideId} has no media references", guide.GuideId);
                return failedMediaIds;
            }

            _logger.LogInformation("Caching {Count} media files for guide {GuideId}", mediaIds.Count, guide.GuideId);

            long totalBytesDownloaded = 0;

            for (int i = 0; i < mediaIds.Count; i++)
            {
                var mediaId = mediaIds[i];

                try
                {
                    // Report progress
                    progress?.Report(new MediaCacheProgress
                    {
                        CurrentMediaId = mediaId,
                        ProcessedCount = i,
                        TotalCount = mediaIds.Count,
                        BytesDownloaded = totalBytesDownloaded
                    });

                    // Skip if already cached
                    if (await IsMediaCachedAsync(mediaId))
                    {
                        _logger.LogDebug("Media {MediaId} already cached, skipping", mediaId);
                        continue;
                    }

                    // Download media
                    var success = await DownloadMediaAsync(mediaId);
                    if (success)
                    {
                        // Get file size for progress tracking
                        var data = await GetMediaAsync(mediaId, downloadIfMissing: false);
                        if (data != null)
                        {
                            totalBytesDownloaded += data.Length;
                        }
                    }
                    else
                    {
                        failedMediaIds.Add(mediaId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error caching media {MediaId}", mediaId);
                    failedMediaIds.Add(mediaId);
                }
            }

            // Report final progress
            progress?.Report(new MediaCacheProgress
            {
                CurrentMediaId = string.Empty,
                ProcessedCount = mediaIds.Count,
                TotalCount = mediaIds.Count,
                BytesDownloaded = totalBytesDownloaded
            });

            if (failedMediaIds.Count > 0)
            {
                _logger.LogWarning(
                    "Failed to cache {FailedCount} out of {TotalCount} media files for guide {GuideId}",
                    failedMediaIds.Count,
                    mediaIds.Count,
                    guide.GuideId);
            }
            else
            {
                _logger.LogInformation(
                    "Successfully cached all {Count} media files for guide {GuideId} ({Size} MB)",
                    mediaIds.Count,
                    guide.GuideId,
                    totalBytesDownloaded / (1024.0 * 1024.0));
            }

            return failedMediaIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching media for guide {GuideId}", guide.GuideId);
            return failedMediaIds;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DownloadMediaAsync(string mediaId)
    {
        try
        {
            _logger.LogInformation("Downloading media {MediaId} from SharePoint", mediaId);

            // Get metadata first
            var metadata = await _sharePointService.GetMediaMetadataAsync(mediaId);
            if (metadata == null)
            {
                _logger.LogWarning("Media metadata not found for {MediaId}", mediaId);
                return false;
            }

            // Download file
            var data = await _sharePointService.DownloadMediaAsync(mediaId);
            if (data == null || data.Length == 0)
            {
                _logger.LogWarning("Media download returned empty data for {MediaId}", mediaId);
                return false;
            }

            // Cache the file (SharePointService already caches it, but we verify)
            if (!string.IsNullOrEmpty(metadata.Checksum))
            {
                await _cacheService.CacheFileAsync("media", mediaId, data, metadata.Checksum);
            }

            _logger.LogInformation(
                "Successfully downloaded and cached media {MediaId} ({Size} KB)",
                mediaId,
                data.Length / 1024.0);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading media {MediaId}", mediaId);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task InvalidateMediaAsync(string mediaId)
    {
        try
        {
            await _cacheService.InvalidateCacheAsync("media", mediaId);
            _logger.LogInformation("Invalidated cached media {MediaId}", mediaId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating media {MediaId}", mediaId);
        }
    }

    /// <inheritdoc/>
    public async Task<MediaCacheStatistics> GetCacheStatisticsAsync()
    {
        var stats = new MediaCacheStatistics();

        try
        {
            var mediaCache = await _context.MediaCache.ToListAsync();

            stats.TotalMediaFiles = mediaCache.Count;
            stats.TotalSizeBytes = mediaCache.Sum(m => m.FileSize);
            stats.LargestFileSizeBytes = mediaCache.Any() ? mediaCache.Max(m => m.FileSize) : 0;

            // Count by type (based on file format)
            foreach (var media in mediaCache)
            {
                var format = media.FileFormat?.ToLowerInvariant() ?? "";
                
                if (IsImageFormat(format))
                    stats.ImageCount++;
                else if (IsVideoFormat(format))
                    stats.VideoCount++;
                else
                    stats.DocumentCount++;
            }

            _logger.LogDebug(
                "Media cache statistics: {Total} files, {Size} MB ({Images} images, {Videos} videos, {Docs} documents)",
                stats.TotalMediaFiles,
                stats.TotalSizeMB,
                stats.ImageCount,
                stats.VideoCount,
                stats.DocumentCount);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media cache statistics");
            return stats;
        }
    }

    // Private helper methods

    private static bool IsImageFormat(string format)
    {
        return format switch
        {
            "png" or "jpg" or "jpeg" or "gif" or "bmp" or "webp" or "svg" => true,
            _ => false
        };
    }

    private static bool IsVideoFormat(string format)
    {
        return format switch
        {
            "mp4" or "webm" or "avi" or "mov" or "wmv" or "flv" => true,
            _ => false
        };
    }
}
