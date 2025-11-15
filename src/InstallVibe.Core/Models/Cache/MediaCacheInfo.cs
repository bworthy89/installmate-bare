namespace InstallVibe.Core.Models.Cache;

/// <summary>
/// Information about a cached media file.
/// </summary>
public class MediaCacheInfo
{
    public string MediaId { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string LocalPath { get; set; } = string.Empty;
    public string Checksum { get; set; } = string.Empty;
    public DateTime CachedDate { get; set; }
    public DateTime? LastAccessed { get; set; }
    public bool IsValid { get; set; } = true;
    public List<string> ReferencedByGuides { get; set; } = new();
}
