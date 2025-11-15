namespace InstallVibe.Core.Models.Progress;

/// <summary>
/// Represents user progress through a guide.
/// </summary>
public class GuideProgress
{
    /// <summary>
    /// Unique progress identifier (GUID).
    /// </summary>
    public string ProgressId { get; set; } = string.Empty;

    /// <summary>
    /// The guide being tracked.
    /// </summary>
    public string GuideId { get; set; } = string.Empty;

    /// <summary>
    /// The user this progress belongs to.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The current step the user is on.
    /// </summary>
    public string? CurrentStepId { get; set; }

    /// <summary>
    /// Dictionary mapping step IDs to their completion status.
    /// </summary>
    public Dictionary<string, StepStatus> StepProgress { get; set; } = new();

    /// <summary>
    /// When the user started this guide.
    /// </summary>
    public DateTime StartedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When progress was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the guide was completed (null if not yet completed).
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Optional notes from the user.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Calculates the completion percentage based on completed steps.
    /// </summary>
    public int PercentComplete
    {
        get
        {
            if (StepProgress.Count == 0) return 0;
            var completedCount = StepProgress.Count(kvp => kvp.Value == StepStatus.Completed);
            return (completedCount * 100) / StepProgress.Count;
        }
    }

    /// <summary>
    /// Whether this guide has been completed.
    /// </summary>
    public bool IsCompleted => CompletedDate.HasValue;
}
