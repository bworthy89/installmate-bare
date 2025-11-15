namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Validation rules for step completion.
/// </summary>
public class StepValidation
{
    /// <summary>
    /// Validation type (manual, automated, etc.).
    /// </summary>
    public string ValidationType { get; set; } = "manual";

    /// <summary>
    /// Validation criteria description.
    /// </summary>
    public string Criteria { get; set; } = string.Empty;

    /// <summary>
    /// Whether validation is required.
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Automated validation script (if applicable).
    /// </summary>
    public string? ValidationScript { get; set; }

    /// <summary>
    /// Expected validation result.
    /// </summary>
    public string? ExpectedResult { get; set; }
}
