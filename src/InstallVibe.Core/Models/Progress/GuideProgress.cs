namespace InstallVibe.Core.Models.Progress;

/// <summary>
/// Represents progress tracking for a guide.
/// </summary>
public class GuideProgress
{
    public string ProgressId { get; set; } = string.Empty;
    public string GuideId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? CurrentStepId { get; set; }
    public Dictionary<string, StepStatus> StepProgress { get; set; } = new();
    public DateTime StartedDate { get; set; }
    public DateTime LastUpdated { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Notes { get; set; }
}
