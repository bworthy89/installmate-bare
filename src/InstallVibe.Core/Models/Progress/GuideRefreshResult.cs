namespace InstallVibe.Core.Models.Progress;

/// <summary>
/// Result of a guide refresh operation.
/// </summary>
public class GuideRefreshResult
{
    /// <summary>
    /// Whether the refresh was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Whether an update was available.
    /// </summary>
    public bool UpdateAvailable { get; set; }

    /// <summary>
    /// Whether the guide was updated.
    /// </summary>
    public bool WasUpdated { get; set; }

    /// <summary>
    /// Previous version of the guide.
    /// </summary>
    public string? PreviousVersion { get; set; }

    /// <summary>
    /// New version of the guide.
    /// </summary>
    public string? NewVersion { get; set; }

    /// <summary>
    /// Previous last modified date.
    /// </summary>
    public DateTime? PreviousLastModified { get; set; }

    /// <summary>
    /// New last modified date.
    /// </summary>
    public DateTime? NewLastModified { get; set; }

    /// <summary>
    /// Whether progress was synced to the new version.
    /// </summary>
    public bool ProgressSynced { get; set; }

    /// <summary>
    /// Number of steps that changed.
    /// </summary>
    public int StepsChanged { get; set; }

    /// <summary>
    /// Error message if refresh failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the operation was performed offline.
    /// </summary>
    public bool WasOffline { get; set; }
}
