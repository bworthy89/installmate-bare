namespace InstallVibe.Core.Models.Settings;

/// <summary>
/// User-specific preferences.
/// </summary>
public class UserPreferences
{
    public bool ShowWelcomeScreen { get; set; } = true;
    public bool AutoDownloadMedia { get; set; } = true;
    public bool ConfirmBeforeDelete { get; set; } = true;
    public int RecentGuidesCount { get; set; } = 10;
    public string? DefaultGuideView { get; set; } = "List";
    public bool EnableNotifications { get; set; } = true;
    public bool PlaySounds { get; set; } = false;
}
