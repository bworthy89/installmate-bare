namespace InstallVibe.Core.Models.Progress;

/// <summary>
/// Represents progress information for an individual step.
/// </summary>
public class StepProgress
{
    public string StepId { get; set; } = string.Empty;
    public StepStatus Status { get; set; }
    public DateTime? StartedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
}
