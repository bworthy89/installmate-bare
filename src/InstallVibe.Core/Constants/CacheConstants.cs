namespace InstallVibe.Core.Constants;

/// <summary>
/// Cache size limits and retention policies.
/// </summary>
public static class CacheConstants
{
    // Size Limits (in bytes)
    public const long MaxTotalCacheSize = 10L * 1024 * 1024 * 1024;  // 10 GB
    public const long CacheWarningThreshold = 8L * 1024 * 1024 * 1024; // 8 GB
    public const long MaxPerGuideSize = 500L * 1024 * 1024;           // 500 MB
    public const long MaxPerMediaSize = 100L * 1024 * 1024;           // 100 MB
    public const long MaxDatabaseSize = 500L * 1024 * 1024;           // 500 MB
    public const long MinFreeSpace = 1L * 1024 * 1024 * 1024;         // 1 GB

    // Expiration Times (in days)
    public const int GuideContentExpiration = 90;      // 90 days
    public const int OrphanedMediaExpiration = 30;     // 30 days
    public const int TempFilesExpiration = 1;          // 1 day
    public const int LogFilesExpiration = 30;          // 30 days
    public const int MinCacheRetention = 1;            // Minimum 1 day

    // Eviction Settings
    public const int LruBatchSize = 100;               // Process 100 items at a time
    public const int MaxEvictionIterations = 10;      // Max iterations before giving up

    // Integrity Check
    public const int IntegrityCheckIntervalDays = 7;   // Weekly integrity check
    public const int MaxIntegrityCheckDuration = 60;   // Max 60 seconds for check

    // Backup Settings
    public const int MaxBackupCount = 7;               // Keep last 7 backups
    public const int AutoBackupIntervalDays = 1;       // Daily backups
}
