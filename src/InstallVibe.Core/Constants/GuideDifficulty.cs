namespace InstallVibe.Core.Constants;

/// <summary>
/// Pre-defined difficulty levels for guides.
/// </summary>
public static class GuideDifficulty
{
    public const string Easy = "Easy";
    public const string Medium = "Medium";
    public const string Hard = "Hard";

    /// <summary>
    /// Returns all available difficulty levels.
    /// </summary>
    public static List<string> All => new()
    {
        Easy,
        Medium,
        Hard
    };

    /// <summary>
    /// Gets the description for a difficulty level.
    /// </summary>
    public static string GetDescription(string difficulty) => difficulty switch
    {
        Easy => "Basic tasks, minimal prerequisites, 15-30 minutes",
        Medium => "Moderate complexity, some prerequisites, 30-60 minutes",
        Hard => "Complex tasks, multiple prerequisites, 60+ minutes",
        _ => "Unknown difficulty level"
    };
}
