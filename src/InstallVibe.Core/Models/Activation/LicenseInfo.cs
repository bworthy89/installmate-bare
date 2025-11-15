namespace InstallVibe.Core.Models.Activation;

/// <summary>
/// Provides information about the current license status.
/// </summary>
public class LicenseInfo
{
    /// <summary>
    /// Indicates whether the application is activated.
    /// </summary>
    public bool IsActivated { get; set; }

    /// <summary>
    /// The current license type.
    /// </summary>
    public LicenseType? LicenseType { get; set; }

    /// <summary>
    /// When the license expires. Null if perpetual or not activated.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Number of days until expiration. Null if perpetual or not activated.
    /// </summary>
    public int? DaysRemaining { get; set; }

    /// <summary>
    /// Customer ID associated with this license.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// List of enabled features.
    /// </summary>
    public List<string> EnabledFeatures { get; set; } = new();

    /// <summary>
    /// Whether this is a perpetual license.
    /// </summary>
    public bool IsPerpetual => !ExpirationDate.HasValue;

    /// <summary>
    /// Whether the license has expired.
    /// </summary>
    public bool IsExpired =>
        ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    /// <summary>
    /// Whether the license is approaching expiration (within 30 days).
    /// </summary>
    public bool IsExpiringSoon =>
        DaysRemaining.HasValue && DaysRemaining.Value <= 30;

    /// <summary>
    /// Human-readable license status.
    /// </summary>
    public string GetStatusText()
    {
        if (!IsActivated)
            return "Not Activated";

        if (IsExpired)
            return "Expired";

        if (IsPerpetual)
            return $"{LicenseType} License (Perpetual)";

        if (IsExpiringSoon)
            return $"{LicenseType} License (Expires in {DaysRemaining} days)";

        return $"{LicenseType} License (Valid until {ExpirationDate:d})";
    }
}
