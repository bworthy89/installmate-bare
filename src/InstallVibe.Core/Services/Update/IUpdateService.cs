using InstallVibe.Core.Models.Update;

namespace InstallVibe.Core.Services.Update;

/// <summary>
/// Service for checking and applying application updates via AppInstaller/MSIX.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Checks if an update is available from SharePoint.
    /// </summary>
    /// <param name="forceCheck">If true, bypasses cache and performs a fresh check.</param>
    /// <returns>Result of the update check.</returns>
    Task<UpdateCheckResult> CheckForUpdatesAsync(bool forceCheck = false);

    /// <summary>
    /// Gets the current application version.
    /// </summary>
    /// <returns>Current version string (e.g., "1.0.0").</returns>
    string GetCurrentVersion();

    /// <summary>
    /// Downloads the update package to a temporary location.
    /// </summary>
    /// <param name="updateInfo">Information about the update to download.</param>
    /// <param name="progress">Progress reporter (0-100).</param>
    /// <returns>Path to the downloaded file.</returns>
    Task<string> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<int>? progress = null);

    /// <summary>
    /// Applies the update using Windows AppInstaller.
    /// This will launch the .appinstaller file and exit the current application.
    /// </summary>
    /// <param name="updateInfo">Information about the update to apply.</param>
    /// <returns>True if update was successfully initiated.</returns>
    Task<bool> ApplyUpdateAsync(UpdateInfo updateInfo);

    /// <summary>
    /// Verifies the integrity of a downloaded update package.
    /// </summary>
    /// <param name="filePath">Path to the downloaded file.</param>
    /// <param name="expectedHash">Expected SHA256 hash.</param>
    /// <returns>True if the file hash matches.</returns>
    Task<bool> VerifyUpdateIntegrityAsync(string filePath, string expectedHash);

    /// <summary>
    /// Gets the last time an update check was performed.
    /// </summary>
    /// <returns>DateTime of last check, or null if never checked.</returns>
    DateTime? GetLastUpdateCheckTime();

    /// <summary>
    /// Dismisses the current update notification.
    /// </summary>
    /// <param name="updateVersion">Version of the update to dismiss.</param>
    void DismissUpdate(string updateVersion);

    /// <summary>
    /// Checks if an update notification has been dismissed.
    /// </summary>
    /// <param name="updateVersion">Version to check.</param>
    /// <returns>True if the update has been dismissed.</returns>
    bool IsUpdateDismissed(string updateVersion);

    /// <summary>
    /// Restarts the application.
    /// </summary>
    Task RestartApplicationAsync();

    /// <summary>
    /// Event raised when an update check completes.
    /// </summary>
    event EventHandler<UpdateCheckResult>? UpdateCheckCompleted;

    /// <summary>
    /// Event raised when an update download progresses.
    /// </summary>
    event EventHandler<int>? UpdateDownloadProgress;

    /// <summary>
    /// Event raised when an update is ready to be applied.
    /// </summary>
    event EventHandler<UpdateInfo>? UpdateReadyToApply;
}
