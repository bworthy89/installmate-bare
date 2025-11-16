using InstallVibe.Core.Models.Settings;
using InstallVibe.Core.Models.Sync;

namespace InstallVibe.Core.Services.OneDrive;

/// <summary>
/// Service for synchronizing .ivguide files from OneDrive/SharePoint to InstallVibe.
/// </summary>
public interface IOneDriveSyncService
{
    /// <summary>
    /// Manually triggers a sync operation.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Sync result with statistics and errors.</returns>
    Task<OneDriveSyncResult> SyncNowAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the automatic background sync timer.
    /// </summary>
    Task StartAutoSyncAsync();

    /// <summary>
    /// Stops the automatic background sync timer.
    /// </summary>
    Task StopAutoSyncAsync();

    /// <summary>
    /// Gets whether automatic sync is currently running.
    /// </summary>
    bool IsAutoSyncRunning { get; }

    /// <summary>
    /// Gets the current OneDrive sync settings.
    /// </summary>
    Task<OneDriveSyncSettings> GetSettingsAsync();

    /// <summary>
    /// Updates the OneDrive sync settings.
    /// </summary>
    /// <param name="settings">New settings to apply.</param>
    Task UpdateSettingsAsync(OneDriveSyncSettings settings);
}
