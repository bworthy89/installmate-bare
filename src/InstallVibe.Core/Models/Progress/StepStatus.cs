namespace InstallVibe.Core.Models.Progress;

/// <summary>
/// Represents the completion status of a step.
/// </summary>
public enum StepStatus
{
    /// <summary>
    /// Step has not been started yet.
    /// </summary>
    NotStarted = 0,

    /// <summary>
    /// Step is currently in progress.
    /// </summary>
    InProgress = 1,

    /// <summary>
    /// Step has been completed successfully.
    /// </summary>
    Completed = 2,

    /// <summary>
    /// Step was skipped (typically for optional steps).
    /// </summary>
    Skipped = 3
}
