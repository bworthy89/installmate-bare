namespace InstallVibe.Core.Models.Cache;

/// <summary>
/// Represents a cached entry.
/// </summary>
public class CacheEntry
{
    public string Key { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string LocalPath { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string Checksum { get; set; } = string.Empty;
    public DateTime CachedDate { get; set; }
    public DateTime? LastAccessed { get; set; }
    public int AccessCount { get; set; }
}
