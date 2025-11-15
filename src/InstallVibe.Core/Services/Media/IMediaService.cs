using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.Media;

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
