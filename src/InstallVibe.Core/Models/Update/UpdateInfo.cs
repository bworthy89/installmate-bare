namespace InstallVibe.Core.Models.Update;

/// <summary>
/// Represents information about an available update.
/// </summary>
public class UpdateInfo
{
    /// <summary>
    /// The version number of the available update.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// The release date of the update.
    /// </summary>
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// The type of update (critical, recommended, optional).
    /// </summary>
    public UpdateType Type { get; set; }

    /// <summary>
    /// URL to download the update package (.msix or .appinstaller).
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL to the .appinstaller file for automatic updates.
    /// </summary>
    public string AppInstallerUrl { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 hash of the update package for integrity verification.
    /// </summary>
    public string FileHash { get; set; } = string.Empty;

    /// <summary>
    /// Size of the update package in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Minimum version required before applying this update.
    /// </summary>
    public string? MinimumVersion { get; set; }

    /// <summary>
    /// Release notes describing changes in this update.
    /// </summary>
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Whether this update requires application restart.
    /// </summary>
    public bool RequiresRestart { get; set; } = true;

    /// <summary>
    /// Whether this update is mandatory and must be installed.
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Direct link to the MSIX package.
    /// </summary>
    public string MsixPackageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Types of updates based on severity and importance.
/// </summary>
public enum UpdateType
{
    /// <summary>
    /// Optional update with minor improvements.
    /// </summary>
    Optional,

    /// <summary>
    /// Recommended update with important fixes or features.
    /// </summary>
    Recommended,

    /// <summary>
    /// Critical update that must be installed (security or stability).
    /// </summary>
    Critical
}
