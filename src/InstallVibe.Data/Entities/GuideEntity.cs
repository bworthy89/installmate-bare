using System.ComponentModel.DataAnnotations;

namespace InstallVibe.Data.Entities;

/// <summary>
/// Database entity for cached guides.
/// </summary>
public class GuideEntity
{
    [Key]
    [MaxLength(100)]
    public string GuideId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Category { get; set; }

    public string? Description { get; set; }

    [MaxLength(20)]
    public string? RequiredLicense { get; set; }

    public bool Published { get; set; }

    public DateTime LastModified { get; set; }

    [Required]
    [MaxLength(1000)]
    public string LocalPath { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? SharePointPath { get; set; }

    public DateTime? CachedDate { get; set; }

    [MaxLength(64)]
    public string? Checksum { get; set; }

    [MaxLength(20)]
    public string SyncStatus { get; set; } = "synced";

    public long? FileSize { get; set; }

    public int StepCount { get; set; }

    public DateTime CreatedDate { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation property
    public ICollection<StepEntity> Steps { get; set; } = new List<StepEntity>();
    public ICollection<ProgressEntity> Progress { get; set; } = new List<ProgressEntity>();
}
