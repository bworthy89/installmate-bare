namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Additional metadata for a guide.
/// </summary>
public class GuideMetadata
{
    /// <summary>
    /// Prerequisites required before starting this guide.
    /// </summary>
    public List<string> Prerequisites { get; set; } = new();

    /// <summary>
    /// Related guide IDs.
    /// </summary>
    public List<string> RelatedGuides { get; set; } = new();

    /// <summary>
    /// Change log entries for version history.
    /// </summary>
    public List<ChangeLogEntry> ChangeLog { get; set; } = new();

    /// <summary>
    /// Custom metadata key-value pairs.
    /// </summary>
    public Dictionary<string, string> CustomMetadata { get; set; } = new();
}

/// <summary>
/// Represents a change log entry for a guide version.
/// </summary>
public class ChangeLogEntry
{
    /// <summary>
    /// Version this change applies to.
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Date of the change.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Description of changes.
    /// </summary>
    public string Changes { get; set; } = string.Empty;

    /// <summary>
    /// Author of the change.
    /// </summary>
    public string? Author { get; set; }
}
