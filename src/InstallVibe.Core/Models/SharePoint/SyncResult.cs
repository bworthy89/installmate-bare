namespace InstallVibe.Core.Models.SharePoint;

/// <summary>
/// Result of a sync operation.
/// </summary>
public class SyncResult
{
    /// <summary>
    /// Whether the sync completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Sync start timestamp.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Sync end timestamp.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Duration of the sync operation.
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Total number of guides checked.
    /// </summary>
    public int TotalGuidesChecked { get; set; }

    /// <summary>
    /// Number of guides downloaded.
    /// </summary>
    public int GuidesDownloaded { get; set; }

    /// <summary>
    /// Number of guides updated.
    /// </summary>
    public int GuidesUpdated { get; set; }

    /// <summary>
    /// Number of guides deleted (unpublished).
    /// </summary>
    public int GuidesDeleted { get; set; }

    /// <summary>
    /// Number of media files downloaded.
    /// </summary>
    public int MediaDownloaded { get; set; }

    /// <summary>
    /// Total bytes downloaded.
    /// </summary>
    public long BytesDownloaded { get; set; }

    /// <summary>
    /// List of errors encountered during sync.
    /// </summary>
    public List<SyncError> Errors { get; set; } = new();

    /// <summary>
    /// Error message if sync failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether sync was performed offline (using cached data).
    /// </summary>
    public bool WasOffline { get; set; }
}

/// <summary>
/// Sync error details.
/// </summary>
public class SyncError
{
    /// <summary>
    /// Guide or media ID that failed.
    /// </summary>
    public string EntityId { get; set; } = string.Empty;

    /// <summary>
    /// Type of entity (Guide, Media).
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exception type if applicable.
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Timestamp when error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Progress update during sync operation.
/// </summary>
public class SyncProgress
{
    /// <summary>
    /// Current operation description.
    /// </summary>
    public string CurrentOperation { get; set; } = string.Empty;

    /// <summary>
    /// Number of items processed.
    /// </summary>
    public int ItemsProcessed { get; set; }

    /// <summary>
    /// Total number of items to process.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Percentage complete (0-100).
    /// </summary>
    public double PercentComplete => TotalItems > 0 ? (ItemsProcessed * 100.0) / TotalItems : 0;

    /// <summary>
    /// Bytes downloaded so far.
    /// </summary>
    public long BytesDownloaded { get; set; }
}
