namespace InstallVibe.Core.Constants;

/// <summary>
/// Pre-defined guide categories for consistent organization.
/// </summary>
public static class GuideCategories
{
    public const string Installation = "Installation";
    public const string Configuration = "Configuration";
    public const string Troubleshooting = "Troubleshooting";
    public const string Maintenance = "Maintenance";
    public const string BestPractices = "Best Practices";
    public const string Migration = "Migration";
    public const string Security = "Security";
    public const string Performance = "Performance";
    public const string Software = "Software";
    public const string Hardware = "Hardware";
    public const string Network = "Network";
    public const string Cloud = "Cloud";

    /// <summary>
    /// Returns all available categories.
    /// </summary>
    public static List<string> All => new()
    {
        Installation,
        Configuration,
        Troubleshooting,
        Maintenance,
        BestPractices,
        Migration,
        Security,
        Performance,
        Software,
        Hardware,
        Network,
        Cloud
    };
}
