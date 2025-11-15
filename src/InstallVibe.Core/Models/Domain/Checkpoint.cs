namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Verification checkpoint within a step.
/// </summary>
public class Checkpoint
{
    /// <summary>
    /// Checkpoint identifier.
    /// </summary>
    public string CheckpointId { get; set; } = string.Empty;

    /// <summary>
    /// What to verify.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Expected outcome.
    /// </summary>
    public string? ExpectedResult { get; set; }

    /// <summary>
    /// Order index.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Whether this checkpoint is mandatory.
    /// </summary>
    public bool IsMandatory { get; set; } = true;
}
