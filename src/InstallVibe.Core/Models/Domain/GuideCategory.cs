namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Represents a guide category for organization.
/// </summary>
public class GuideCategory
{
    public string CategoryId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
    public int GuideCount { get; set; }
}
