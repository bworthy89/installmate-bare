namespace InstallVibe.Core.Models.Cache;

/// <summary>
/// Statistics about the current cache usage.
/// </summary>
public class CacheStatistics
{
    public long TotalSizeBytes { get; set; }
    public long TotalSizeMB => TotalSizeBytes / (1024 * 1024);
    public int GuideCount { get; set; }
    public int MediaCount { get; set; }
    public long LargestGuideSize { get; set; }
    public long LargestMediaSize { get; set; }
    public DateTime? OldestCachedDate { get; set; }
    public double UsagePercentage { get; set; }
    public bool NeedsCleanup { get; set; }
    public List<string> Warnings { get; set; } = new();
}
