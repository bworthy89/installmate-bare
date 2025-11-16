using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Represents a complete installation guide with all its steps and metadata.
/// </summary>
public class Guide
{
    /// <summary>
    /// Unique guide identifier (GUID).
    /// </summary>
    public string GuideId { get; set; } = string.Empty;

    /// <summary>
    /// Display title of the guide.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Semantic version (e.g., "1.2.3").
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Category (Software, Hardware, Network, Cloud, Other).
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Brief description of the guide.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Required license type to view this guide.
    /// </summary>
    public LicenseType RequiredLicense { get; set; } = LicenseType.Tech;

    /// <summary>
    /// Whether the guide is published and visible.
    /// </summary>
    public bool IsPublished { get; set; } = true;

    /// <summary>
    /// Publishing status: Draft, Published, or Archived.
    /// </summary>
    public PublishStatus Status { get; set; } = PublishStatus.Draft;

    /// <summary>
    /// Date when the guide was published (null if never published).
    /// </summary>
    public DateTime? PublishedDate { get; set; }

    /// <summary>
    /// Last modification timestamp.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date when the guide was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Guide author name.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Estimated completion time in minutes.
    /// </summary>
    public int? EstimatedMinutes { get; set; }

    /// <summary>
    /// Tags for search and categorization.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Target audience for this guide (e.g., "System Administrators", "Help Desk Technicians").
    /// </summary>
    public string? TargetAudience { get; set; }

    /// <summary>
    /// Difficulty level: Easy, Medium, or Hard.
    /// </summary>
    public string Difficulty { get; set; } = "Medium";

    /// <summary>
    /// Ordered list of steps in the guide.
    /// </summary>
    public List<Step> Steps { get; set; } = new();

    /// <summary>
    /// Additional metadata (prerequisites, related guides, changelog).
    /// </summary>
    public GuideMetadata Metadata { get; set; } = new();

    /// <summary>
    /// Total number of steps.
    /// </summary>
    public int TotalSteps => Steps?.Count ?? 0;

    /// <summary>
    /// Convenience property for step count (alias for TotalSteps).
    /// </summary>
    public int StepCount => TotalSteps;

    /// <summary>
    /// Convenience property for prerequisites.
    /// </summary>
    public List<string> Prerequisites => Metadata?.Prerequisites ?? new();

    /// <summary>
    /// Estimated time formatted as a string.
    /// </summary>
    public string EstimatedTime => EstimatedMinutes.HasValue ? $"{EstimatedMinutes} min" : "Not specified";

    /// <summary>
    /// Total number of media references across all steps.
    /// </summary>
    public int TotalMediaReferences => Steps?.SelectMany(s => s.MediaReferences ?? new List<MediaReference>()).Count() ?? 0;
}
