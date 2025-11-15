namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Represents a single step in an installation guide.
/// </summary>
public class Step
{
    /// <summary>
    /// Unique step identifier within the guide.
    /// </summary>
    public string StepId { get; set; } = string.Empty;

    /// <summary>
    /// Alias for StepId (for compatibility).
    /// </summary>
    public string Id
    {
        get => StepId;
        set => StepId = value;
    }

    /// <summary>
    /// Step title/heading.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Order index (1-based) for display.
    /// </summary>
    public int OrderIndex { get; set; }

    /// <summary>
    /// Step content (markdown supported).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Detailed instructions for this step.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Media references (images, videos, documents) for this step.
    /// </summary>
    public List<MediaReference> MediaReferences { get; set; } = new();

    /// <summary>
    /// Alias for MediaReferences (for compatibility).
    /// </summary>
    public List<MediaReference> Media
    {
        get => MediaReferences;
        set => MediaReferences = value;
    }

    /// <summary>
    /// Verification checkpoints for this step.
    /// </summary>
    public List<Checkpoint> Checkpoints { get; set; } = new();

    /// <summary>
    /// Actions to perform in this step.
    /// </summary>
    public List<StepAction> Actions { get; set; } = new();

    /// <summary>
    /// Validation rules for step completion.
    /// </summary>
    public StepValidation? Validation { get; set; }

    /// <summary>
    /// Estimated duration for this step in minutes.
    /// </summary>
    public int? EstimatedDuration { get; set; }

    /// <summary>
    /// Optional notes or additional information.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Warning level for this step (info, warning, critical).
    /// </summary>
    public WarningLevel WarningLevel { get; set; } = WarningLevel.Info;

    /// <summary>
    /// Whether this step is optional.
    /// </summary>
    public bool IsOptional { get; set; } = false;

    /// <summary>
    /// Expected duration in minutes.
    /// </summary>
    public int? ExpectedDurationMinutes { get; set; }

    /// <summary>
    /// Prerequisites that must be completed before this step.
    /// </summary>
    public List<string> Prerequisites { get; set; } = new();
}

/// <summary>
/// Warning level for a step.
/// </summary>
public enum WarningLevel
{
    /// <summary>
    /// Informational step.
    /// </summary>
    Info,

    /// <summary>
    /// Warning - caution required.
    /// </summary>
    Warning,

    /// <summary>
    /// Critical - important step that could cause issues if done incorrectly.
    /// </summary>
    Critical
}
