namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Represents a reference to media (image, video, document) within a step.
/// </summary>
public class MediaReference
{
    /// <summary>
    /// Unique media identifier.
    /// </summary>
    public string MediaId { get; set; } = string.Empty;

    /// <summary>
    /// Alias for MediaId (for compatibility).
    /// </summary>
    public string Id
    {
        get => MediaId;
        set => MediaId = value;
    }

    /// <summary>
    /// Type of media (image, video, document).
    /// </summary>
    public string MediaType { get; set; } = "image";

    /// <summary>
    /// Alias for MediaType (for compatibility).
    /// </summary>
    public string Type
    {
        get => MediaType;
        set => MediaType = value;
    }

    /// <summary>
    /// URL or path to the media file.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Thumbnail URL for preview.
    /// </summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// Media title.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Media description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Duration in seconds (for video/audio).
    /// </summary>
    public int? Duration { get; set; }

    /// <summary>
    /// Upload timestamp.
    /// </summary>
    public DateTime? UploadedDate { get; set; }

    /// <summary>
    /// Caption or description for the media.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Order index for display within the step.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Alternative text for accessibility.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Width hint for rendering (optional).
    /// </summary>
    public int? WidthHint { get; set; }

    /// <summary>
    /// Height hint for rendering (optional).
    /// </summary>
    public int? HeightHint { get; set; }
}
