namespace InstallVibe.Core.Models.Media;

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
