namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Action to perform in a step.
/// </summary>
public class StepAction
{
    /// <summary>
    /// Action identifier.
    /// </summary>
    public string ActionId { get; set; } = string.Empty;

    /// <summary>
    /// Action type (command, script, manual, etc.).
    /// </summary>
    public string ActionType { get; set; } = "manual";

    /// <summary>
    /// Action description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Command or script to execute (if automated).
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// Order index.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Whether action can be automated.
    /// </summary>
    public bool IsAutomatable { get; set; } = false;
}
