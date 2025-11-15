using System.ComponentModel.DataAnnotations;

namespace InstallVibe.Data.Entities;

/// <summary>
/// Database entity for sync tracking.
/// </summary>
public class SyncMetadataEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntityType { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityId { get; set; } = string.Empty;

    public DateTime LastSyncDate { get; set; }

    [MaxLength(50)]
    public string? ServerVersion { get; set; }

    [MaxLength(50)]
    public string? LocalVersion { get; set; }

    [MaxLength(20)]
    public string SyncStatus { get; set; } = "synced";

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }
}
