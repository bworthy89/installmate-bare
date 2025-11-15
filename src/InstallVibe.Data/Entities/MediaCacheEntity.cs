using System.ComponentModel.DataAnnotations;

namespace InstallVibe.Data.Entities;

/// <summary>
/// Database entity for cached media files.
/// </summary>
public class MediaCacheEntity
{
    [Key]
    [MaxLength(100)]
    public string MediaId { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string LocalPath { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? SharePointPath { get; set; }

    public long FileSize { get; set; }

    [Required]
    [MaxLength(64)]
    public string Checksum { get; set; } = string.Empty;

    public DateTime CachedDate { get; set; }

    public DateTime LastAccessed { get; set; }

    public int AccessCount { get; set; }

    public bool IsShared { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }
}
