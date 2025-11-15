namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Represents a checklist item within a guide step.
/// </summary>
public class ChecklistItem
{
    public string ItemId { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}
