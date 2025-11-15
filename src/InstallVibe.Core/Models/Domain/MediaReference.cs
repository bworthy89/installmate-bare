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
    /// Type of media (image, video, document).
    /// </summary>
    public string MediaType { get; set; } = "image";

    /// <summary>
    /// URL or path to the media file.
    /// </summary>
    public string? Url { get; set; }

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
