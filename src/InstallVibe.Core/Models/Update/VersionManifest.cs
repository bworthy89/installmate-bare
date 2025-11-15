using System.Text.Json.Serialization;

namespace InstallVibe.Core.Models.Update;

/// <summary>
/// Represents the version.json manifest file from SharePoint.
/// This file contains metadata about the latest available version.
/// </summary>
public class VersionManifest
{
    /// <summary>
    /// The latest version number (semantic versioning: major.minor.patch).
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Release date in ISO 8601 format.
    /// </summary>
    [JsonPropertyName("releaseDate")]
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Type of update: "critical", "recommended", or "optional".
    /// </summary>
    [JsonPropertyName("updateType")]
    public string UpdateType { get; set; } = "optional";

    /// <summary>
    /// URL to the .appinstaller file for automatic updates.
    /// </summary>
    [JsonPropertyName("appInstallerUrl")]
    public string AppInstallerUrl { get; set; } = string.Empty;

    /// <summary>
    /// Direct URL to the MSIX package.
    /// </summary>
    [JsonPropertyName("msixPackageUrl")]
    public string MsixPackageUrl { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 hash of the MSIX package for integrity verification.
    /// </summary>
    [JsonPropertyName("fileHash")]
    public string FileHash { get; set; } = string.Empty;

    /// <summary>
    /// Size of the MSIX package in bytes.
    /// </summary>
    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }

    /// <summary>
    /// Minimum version required before applying this update.
    /// </summary>
    [JsonPropertyName("minimumVersion")]
    public string? MinimumVersion { get; set; }

    /// <summary>
    /// Release notes describing changes in this version.
    /// </summary>
    [JsonPropertyName("releaseNotes")]
    public string ReleaseNotes { get; set; } = string.Empty;

    /// <summary>
    /// Whether this update requires application restart.
    /// </summary>
    [JsonPropertyName("requiresRestart")]
    public bool RequiresRestart { get; set; } = true;

    /// <summary>
    /// Whether this update is mandatory.
    /// </summary>
    [JsonPropertyName("isMandatory")]
    public bool IsMandatory { get; set; }

    /// <summary>
    /// URL to the release notes page (optional).
    /// </summary>
    [JsonPropertyName("releaseNotesUrl")]
    public string? ReleaseNotesUrl { get; set; }

    /// <summary>
    /// List of changes in this version.
    /// </summary>
    [JsonPropertyName("changes")]
    public List<ChangelogEntry> Changes { get; set; } = new();

    /// <summary>
    /// Converts this manifest to an UpdateInfo object.
    /// </summary>
    public UpdateInfo ToUpdateInfo()
    {
        var updateType = UpdateType.ToLowerInvariant() switch
        {
            "critical" => Models.Update.UpdateType.Critical,
            "recommended" => Models.Update.UpdateType.Recommended,
            _ => Models.Update.UpdateType.Optional
        };

        return new UpdateInfo
        {
            Version = Version,
            ReleaseDate = ReleaseDate,
            Type = updateType,
            DownloadUrl = MsixPackageUrl,
            AppInstallerUrl = AppInstallerUrl,
            FileHash = FileHash,
            FileSize = FileSize,
            MinimumVersion = MinimumVersion,
            ReleaseNotes = ReleaseNotes,
            RequiresRestart = RequiresRestart,
            IsMandatory = IsMandatory,
            MsixPackageUrl = MsixPackageUrl
        };
    }
}

/// <summary>
/// Represents a single changelog entry.
/// </summary>
public class ChangelogEntry
{
    /// <summary>
    /// Type of change: "feature", "bugfix", "improvement", "security".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "improvement";

    /// <summary>
    /// Description of the change.
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Issue or ticket number (optional).
    /// </summary>
    [JsonPropertyName("issueNumber")]
    public string? IssueNumber { get; set; }
}
