namespace InstallVibe.Core.Models.Activation;

/// <summary>
/// Represents a validated activation token stored locally.
/// Encrypted and persisted to disk after successful activation.
/// </summary>
public class ActivationToken
{
    /// <summary>
    /// SHA256 hash of the original product key (not the key itself).
    /// </summary>
    public string ProductKeyHash { get; set; } = string.Empty;

    /// <summary>
    /// License type granted by this activation.
    /// </summary>
    public LicenseType LicenseType { get; set; }

    /// <summary>
    /// When this license expires (UTC). Null means perpetual.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Customer identifier from the product key.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// List of feature identifiers enabled for this license.
    /// </summary>
    public List<string> EnabledFeatures { get; set; } = new();

    /// <summary>
    /// Machine identifier (hardware hash) this token is bound to.
    /// </summary>
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// When this token was created (activation date).
    /// </summary>
    public DateTime ValidatedDate { get; set; }

    /// <summary>
    /// Whether this token was validated online via SharePoint.
    /// </summary>
    public bool OnlineValidation { get; set; }

    /// <summary>
    /// HMAC signature of the token data for tamper detection.
    /// </summary>
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Checks if this token has expired.
    /// </summary>
    public bool IsExpired =>
        ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    /// <summary>
    /// Checks if this is a perpetual license.
    /// </summary>
    public bool IsPerpetual => !ExpirationDate.HasValue;

    /// <summary>
    /// Gets the number of days until expiration. Null if perpetual.
    /// </summary>
    public int? DaysUntilExpiration
    {
        get
        {
            if (!ExpirationDate.HasValue)
                return null;

            var days = (ExpirationDate.Value - DateTime.UtcNow).Days;
            return Math.Max(0, days);
        }
    }
}
