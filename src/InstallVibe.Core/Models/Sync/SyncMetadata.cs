namespace InstallVibe.Core.Models.Sync;

/// <summary>
/// Metadata for sync operations.
/// </summary>
public class SyncMetadata
{
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public DateTime LastSyncDate { get; set; }
    public string? LastSyncVersion { get; set; }
    public string? Checksum { get; set; }
    public string SyncStatus { get; set; } = "Pending";
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}
