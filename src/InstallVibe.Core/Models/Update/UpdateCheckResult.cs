namespace InstallVibe.Core.Models.Update;

/// <summary>
/// Result of checking for updates.
/// </summary>
public class UpdateCheckResult
{
    /// <summary>
    /// Whether an update is available.
    /// </summary>
    public bool IsUpdateAvailable { get; set; }

    /// <summary>
    /// Information about the available update (null if no update available).
    /// </summary>
    public UpdateInfo? UpdateInfo { get; set; }

    /// <summary>
    /// Current version of the application.
    /// </summary>
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// When the update check was performed.
    /// </summary>
    public DateTime CheckedAt { get; set; }

    /// <summary>
    /// Error message if the update check failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether the update check succeeded.
    /// </summary>
    public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Whether the available update is a newer version than current.
    /// </summary>
    public bool IsNewerVersion
    {
        get
        {
            if (!IsUpdateAvailable || UpdateInfo == null)
                return false;

            try
            {
                var current = new Version(CurrentVersion);
                var latest = new Version(UpdateInfo.Version);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Creates a successful result with no update available.
    /// </summary>
    public static UpdateCheckResult NoUpdateAvailable(string currentVersion)
    {
        return new UpdateCheckResult
        {
            IsUpdateAvailable = false,
            CurrentVersion = currentVersion,
            CheckedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a successful result with an update available.
    /// </summary>
    public static UpdateCheckResult UpdateAvailable(string currentVersion, UpdateInfo updateInfo)
    {
        return new UpdateCheckResult
        {
            IsUpdateAvailable = true,
            CurrentVersion = currentVersion,
            UpdateInfo = updateInfo,
            CheckedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static UpdateCheckResult Failed(string currentVersion, string errorMessage)
    {
        return new UpdateCheckResult
        {
            IsUpdateAvailable = false,
            CurrentVersion = currentVersion,
            ErrorMessage = errorMessage,
            CheckedAt = DateTime.UtcNow
        };
    }
}
