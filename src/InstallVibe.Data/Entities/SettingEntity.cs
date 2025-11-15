using System.ComponentModel.DataAnnotations;

namespace InstallVibe.Data.Entities;

/// <summary>
/// Database entity for application settings.
/// </summary>
public class SettingEntity
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Value { get; set; } = string.Empty;

    public bool EncryptedValue { get; set; }

    public DateTime LastModified { get; set; }

    [MaxLength(50)]
    public string? Category { get; set; }
}
