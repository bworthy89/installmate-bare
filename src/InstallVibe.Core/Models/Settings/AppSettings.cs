namespace InstallVibe.Core.Models.Settings;

/// <summary>
/// Application-wide settings.
/// </summary>
public class AppSettings
{
    public string? Theme { get; set; }
    public string? Language { get; set; }
    public bool AutoSync { get; set; } = true;
    public int SyncIntervalMinutes { get; set; } = 60;
    public bool OfflineMode { get; set; } = false;
    public int CacheSizeLimitMB { get; set; } = 500;
    public bool EnableLogging { get; set; } = true;
    public string? LogLevel { get; set; } = "Information";

    /// <summary>
    /// Feature flag: Use SharePoint integration (true) or local-only mode (false).
    /// Set to false to disable SharePoint and use local-only storage with import/export.
    /// </summary>
    public bool UseSharePoint { get; set; } = false;
}
