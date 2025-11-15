using System.ComponentModel.DataAnnotations;

namespace InstallVibe.Data.Entities;

/// <summary>
/// Database entity for user progress tracking.
/// </summary>
public class ProgressEntity
{
    [Key]
    [MaxLength(100)]
    public string ProgressId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string GuideId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string UserId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CurrentStepId { get; set; }

    [Required]
    public string StepProgress { get; set; } = "{}";  // JSON

    public DateTime StartedDate { get; set; }

    public DateTime LastUpdated { get; set; }

    public DateTime? CompletedDate { get; set; }

    public string? Notes { get; set; }

    public int PercentComplete { get; set; }

    // Navigation property
    public GuideEntity Guide { get; set; } = null!;
}
