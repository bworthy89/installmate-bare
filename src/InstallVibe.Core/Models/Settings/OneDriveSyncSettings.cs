namespace InstallVibe.Core.Models.Settings;

/// <summary>
/// Settings for OneDrive/SharePoint folder synchronization.
/// </summary>
public class OneDriveSyncSettings
{
    /// <summary>
    /// Whether OneDrive sync is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// SharePoint site ID (obtained from Graph API).
    /// </summary>
    public string? SiteId { get; set; }

    /// <summary>
    /// Drive ID for the target OneDrive/SharePoint document library.
    /// </summary>
    public string? DriveId { get; set; }

    /// <summary>
    /// Folder path within the drive to sync from (e.g., "/InstallVibe/Guides").
    /// </summary>
    public string FolderPath { get; set; } = "/InstallVibe/Guides";

    /// <summary>
    /// Auto-sync interval in minutes.
    /// </summary>
    public int SyncIntervalMinutes { get; set; } = 15;

    /// <summary>
    /// Last successful sync timestamp.
    /// </summary>
    public DateTime? LastSyncTime { get; set; }

    /// <summary>
    /// Delta token for incremental sync (tracks last change position).
    /// </summary>
    public string? DeltaToken { get; set; }

    /// <summary>
    /// Whether to run sync automatically on application startup.
    /// </summary>
    public bool SyncOnStartup { get; set; } = true;
}
