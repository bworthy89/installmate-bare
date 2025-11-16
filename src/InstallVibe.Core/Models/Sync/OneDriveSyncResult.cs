namespace InstallVibe.Core.Models.Sync;

/// <summary>
/// Result of a OneDrive sync operation.
/// </summary>
public class OneDriveSyncResult
{
    /// <summary>
    /// Whether the sync operation completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Number of .ivguide files downloaded from OneDrive.
    /// </summary>
    public int FilesDownloaded { get; set; }

    /// <summary>
    /// Number of guides successfully imported into InstallVibe.
    /// </summary>
    public int FilesImported { get; set; }

    /// <summary>
    /// Number of files that failed to import.
    /// </summary>
    public int FilesFailed { get; set; }

    /// <summary>
    /// List of error messages encountered during sync.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Sync operation start time.
    /// </summary>
    public DateTime SyncStartTime { get; set; }

    /// <summary>
    /// Sync operation end time.
    /// </summary>
    public DateTime SyncEndTime { get; set; }

    /// <summary>
    /// Duration of the sync operation.
    /// </summary>
    public TimeSpan Duration => SyncEndTime - SyncStartTime;

    /// <summary>
    /// New delta token to use for the next sync operation.
    /// </summary>
    public string? NewDeltaToken { get; set; }
}
