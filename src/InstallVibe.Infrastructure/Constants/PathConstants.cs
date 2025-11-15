namespace InstallVibe.Infrastructure.Constants;

/// <summary>
/// Defines all file system paths used by InstallVibe.
/// </summary>
public static class PathConstants
{
    /// <summary>
    /// Base application data folder.
    /// </summary>
    public static string AppDataPath { get; } =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "InstallVibe");

    /// <summary>
    /// Configuration folder for activation and settings.
    /// </summary>
    public static string ConfigPath { get; } = Path.Combine(AppDataPath, "Config");

    /// <summary>
    /// Database folder.
    /// </summary>
    public static string DataPath { get; } = Path.Combine(AppDataPath, "Data");

    /// <summary>
    /// Cache root folder.
    /// </summary>
    public static string CachePath { get; } = Path.Combine(AppDataPath, "Cache");

    /// <summary>
    /// Guides cache folder.
    /// </summary>
    public static string GuidesCachePath { get; } = Path.Combine(CachePath, "Guides");

    /// <summary>
    /// Media cache folder.
    /// </summary>
    public static string MediaCachePath { get; } = Path.Combine(CachePath, "Media");

    /// <summary>
    /// Temporary files folder.
    /// </summary>
    public static string TempPath { get; } = Path.Combine(CachePath, "Temp");

    /// <summary>
    /// Logs folder.
    /// </summary>
    public static string LogsPath { get; } = Path.Combine(AppDataPath, "Logs");

    /// <summary>
    /// Backup folder.
    /// </summary>
    public static string BackupPath { get; } = Path.Combine(AppDataPath, "Backup");

    /// <summary>
    /// SQLite database file path.
    /// </summary>
    public static string DatabasePath { get; } = Path.Combine(DataPath, "installvibe.db");

    /// <summary>
    /// Activation token file path.
    /// </summary>
    public static string ActivationTokenPath { get; } = Path.Combine(ConfigPath, "activation.dat");

    /// <summary>
    /// User settings file path.
    /// </summary>
    public static string SettingsPath { get; } = Path.Combine(ConfigPath, "settings.json");

    /// <summary>
    /// Ensures all required directories exist.
    /// </summary>
    public static void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(ConfigPath);
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(GuidesCachePath);
        Directory.CreateDirectory(MediaCachePath);
        Directory.CreateDirectory(Path.Combine(MediaCachePath, "Images"));
        Directory.CreateDirectory(Path.Combine(MediaCachePath, "Videos"));
        Directory.CreateDirectory(Path.Combine(MediaCachePath, "Documents"));
        Directory.CreateDirectory(TempPath);
        Directory.CreateDirectory(LogsPath);
        Directory.CreateDirectory(BackupPath);
    }

    /// <summary>
    /// Gets the path for a specific guide's cache folder.
    /// </summary>
    public static string GetGuideCachePath(string guideId)
    {
        return Path.Combine(GuidesCachePath, guideId);
    }

    /// <summary>
    /// Gets the path for a guide's JSON file.
    /// </summary>
    public static string GetGuideJsonPath(string guideId)
    {
        return Path.Combine(GetGuideCachePath(guideId), "guide.json");
    }

    /// <summary>
    /// Gets the path for a guide's steps folder.
    /// </summary>
    public static string GetGuideStepsPath(string guideId)
    {
        return Path.Combine(GetGuideCachePath(guideId), "steps");
    }

    /// <summary>
    /// Gets the path for a guide's media folder.
    /// </summary>
    public static string GetGuideMediaPath(string guideId)
    {
        return Path.Combine(GetGuideCachePath(guideId), "media");
    }

    /// <summary>
    /// Gets the path for a specific step JSON file.
    /// </summary>
    public static string GetStepJsonPath(string guideId, string stepId)
    {
        return Path.Combine(GetGuideStepsPath(guideId), $"{stepId}.json");
    }
}
