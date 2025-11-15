namespace InstallVibe.Core.Models.SharePoint;

/// <summary>
/// Represents a media file in the SharePoint Media library.
/// </summary>
public class SharePointMedia
{
    /// <summary>
    /// Unique media identifier (GUID).
    /// </summary>
    public string MediaId { get; set; } = string.Empty;

    /// <summary>
    /// Type of media (Image, Video, Document).
    /// </summary>
    public MediaType MediaType { get; set; }

    /// <summary>
    /// File format/extension (png, jpg, mp4, pdf, etc.).
    /// </summary>
    public string FileFormat { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// SHA256 hash of the file.
    /// </summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// List of GuideIds that reference this media.
    /// </summary>
    public List<string> ReferencedByGuides { get; set; } = new();

    /// <summary>
    /// Uploader name/email.
    /// </summary>
    public string? UploadedBy { get; set; }

    /// <summary>
    /// Upload timestamp.
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Direct download URL (valid for 1 hour).
    /// </summary>
    public string? DownloadUrl { get; set; }

    /// <summary>
    /// SharePoint item ID.
    /// </summary>
    public string? SharePointItemId { get; set; }

    /// <summary>
    /// Whether this media is cached locally.
    /// </summary>
    public bool IsCached { get; set; }
}

/// <summary>
/// Media type enumeration.
/// </summary>
public enum MediaType
{
    /// <summary>
    /// Image file (PNG, JPG, GIF, etc.).
    /// </summary>
    Image,

    /// <summary>
    /// Video file (MP4, WEBM, etc.).
    /// </summary>
    Video,

    /// <summary>
    /// Document file (PDF, etc.).
    /// </summary>
    Document
}
