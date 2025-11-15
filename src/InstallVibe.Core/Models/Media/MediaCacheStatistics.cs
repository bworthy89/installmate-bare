namespace InstallVibe.Core.Models.Media;

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
