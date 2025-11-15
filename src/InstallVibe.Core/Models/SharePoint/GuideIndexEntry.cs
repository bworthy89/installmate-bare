using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Models.SharePoint;

/// <summary>
/// Represents a guide entry from the SharePoint GuideIndex list.
/// </summary>
public class GuideIndexEntry
{
    /// <summary>
    /// Unique guide identifier (GUID).
    /// </summary>
    public string GuideId { get; set; } = string.Empty;

    /// <summary>
    /// Guide display title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Semantic version (e.g., "1.2.3").
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Guide category (Software, Hardware, Network, Cloud, Other).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Brief description (max 500 chars).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Required license type to view this guide.
    /// </summary>
    public LicenseType RequiredLicense { get; set; }

    /// <summary>
    /// Whether the guide is published and visible.
    /// </summary>
    public bool Published { get; set; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Guide author name.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// User/group who approved the guide (for Admin guides).
    /// </summary>
    public string? ApprovedBy { get; set; }

    /// <summary>
    /// Total number of steps in the guide.
    /// </summary>
    public int StepCount { get; set; }

    /// <summary>
    /// Estimated completion time in minutes.
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// Comma-separated tags for search.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// SHA256 hash of guide.json file.
    /// </summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// Size of guide.json file in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Relative path in Guides library (e.g., "/Guides/{guideId}").
    /// </summary>
    public string FolderPath { get; set; } = string.Empty;

    /// <summary>
    /// Number of media references in the guide.
    /// </summary>
    public int MediaCount { get; set; }

    /// <summary>
    /// Sync priority (Critical, High, Normal, Low).
    /// </summary>
    public string? SyncPriority { get; set; }

    /// <summary>
    /// Minimum InstallVibe client version required.
    /// </summary>
    public string? MinClientVersion { get; set; }

    /// <summary>
    /// Whether this guide is cached locally.
    /// </summary>
    public bool IsCached { get; set; }

    /// <summary>
    /// Whether local cached version matches SharePoint version.
    /// </summary>
    public bool IsUpToDate { get; set; }
}
