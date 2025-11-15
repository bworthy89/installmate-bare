using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Models.SharePoint;

/// <summary>
/// Result of online product key validation against SharePoint.
/// </summary>
public class ProductKeyValidationResult
{
    /// <summary>
    /// Whether the key is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// License type associated with the key.
    /// </summary>
    public LicenseType? LicenseType { get; set; }

    /// <summary>
    /// Customer ID.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Expiration date (null = perpetual).
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Whether the key has been revoked.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Current activation count.
    /// </summary>
    public int ActivationCount { get; set; }

    /// <summary>
    /// Maximum allowed activations.
    /// </summary>
    public int MaxActivations { get; set; }

    /// <summary>
    /// Whether activation limit has been reached.
    /// </summary>
    public bool LimitReached => ActivationCount >= MaxActivations;

    /// <summary>
    /// Whether the key has expired.
    /// </summary>
    public bool IsExpired => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Whether validation was performed online or offline.
    /// </summary>
    public bool WasOnlineValidation { get; set; }

    /// <summary>
    /// Validation timestamp.
    /// </summary>
    public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;
}
