using InstallVibe.Core.Models.Domain;

namespace InstallVibe.Core.Services.Media;

/// <summary>
/// Service for managing media files (images, videos, documents).
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Gets a media file, downloading from SharePoint if not cached.
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    /// <param name="downloadIfMissing">Whether to download from SharePoint if not in cache.</param>
    /// <returns>Media file bytes, or null if not found.</returns>
    Task<byte[]?> GetMediaAsync(string mediaId, bool downloadIfMissing = true);

    /// <summary>
    /// Gets the local file path for a cached media file.
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    /// <returns>Local file path, or null if not cached.</returns>
    Task<string?> GetMediaPathAsync(string mediaId);

    /// <summary>
    /// Checks if a media file is cached locally.
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    /// <returns>True if cached, false otherwise.</returns>
    Task<bool> IsMediaCachedAsync(string mediaId);

    /// <summary>
    /// Downloads and caches media for all references in a guide.
    /// </summary>
    /// <param name="guide">Guide containing media references.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>List of media IDs that failed to download.</returns>
    Task<List<string>> CacheGuideMediaAsync(Guide guide, IProgress<MediaCacheProgress>? progress = null);

    /// <summary>
    /// Downloads and caches a single media file from SharePoint.
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> DownloadMediaAsync(string mediaId);

    /// <summary>
    /// Invalidates (deletes) a cached media file.
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    Task InvalidateMediaAsync(string mediaId);

    /// <summary>
    /// Gets statistics about cached media.
    /// </summary>
    /// <returns>Media cache statistics.</returns>
    Task<MediaCacheStatistics> GetCacheStatisticsAsync();
}

/// <summary>
/// Progress update for media caching operations.
/// </summary>
public class MediaCacheProgress
{
    /// <summary>
    /// Current media being processed.
    /// </summary>
    public string CurrentMediaId { get; set; } = string.Empty;

    /// <summary>
    /// Number of media files processed.
    /// </summary>
    public int ProcessedCount { get; set; }

    /// <summary>
    /// Total number of media files to process.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Percentage complete (0-100).
    /// </summary>
    public double PercentComplete => TotalCount > 0 ? (ProcessedCount * 100.0) / TotalCount : 0;

    /// <summary>
    /// Bytes downloaded so far.
    /// </summary>
    public long BytesDownloaded { get; set; }
}

/// <summary>
/// Statistics about cached media.
/// </summary>
public class MediaCacheStatistics
{
    /// <summary>
    /// Total number of cached media files.
    /// </summary>
    public int TotalMediaFiles { get; set; }

    /// <summary>
    /// Total size in bytes of all cached media.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Total size in megabytes.
    /// </summary>
    public double TotalSizeMB => TotalSizeBytes / (1024.0 * 1024.0);

    /// <summary>
    /// Number of images cached.
    /// </summary>
    public int ImageCount { get; set; }

    /// <summary>
    /// Number of videos cached.
    /// </summary>
    public int VideoCount { get; set; }

    /// <summary>
    /// Number of documents cached.
    /// </summary>
    public int DocumentCount { get; set; }

    /// <summary>
    /// Largest media file size in bytes.
    /// </summary>
    public long LargestFileSizeBytes { get; set; }
}
