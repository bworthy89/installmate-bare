using System.ComponentModel.DataAnnotations;

namespace InstallVibe.Data.Entities;

/// <summary>
/// Database entity for guide steps.
/// </summary>
public class StepEntity
{
    [Key]
    [MaxLength(100)]
    public string StepId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string GuideId { get; set; } = string.Empty;

    public int StepNumber { get; set; }

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    public string? MediaReferences { get; set; }  // JSON array

    [MaxLength(1000)]
    public string? LocalPath { get; set; }

    [MaxLength(64)]
    public string? Checksum { get; set; }

    public DateTime? CachedDate { get; set; }

    // Navigation property
    public GuideEntity Guide { get; set; } = null!;
}
