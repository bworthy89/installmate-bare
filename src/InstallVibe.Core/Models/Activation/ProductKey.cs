namespace InstallVibe.Core.Models.Activation;

/// <summary>
/// Represents a parsed product key with its payload and signature.
/// </summary>
public class ProductKey
{
    /// <summary>
    /// Original product key string as entered by the user.
    /// </summary>
    public string OriginalKey { get; set; } = string.Empty;

    /// <summary>
    /// License type (Tech or Admin).
    /// </summary>
    public LicenseType LicenseType { get; set; }

    /// <summary>
    /// Expiration date (UTC). Null means perpetual license.
    /// </summary>
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Customer identifier.
    /// </summary>
    public uint CustomerId { get; set; }

    /// <summary>
    /// Feature flags bitfield.
    /// </summary>
    public byte FeatureFlags { get; set; }

    /// <summary>
    /// Raw payload bytes (12 bytes).
    /// </summary>
    public byte[] Payload { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// RSA signature of the payload.
    /// </summary>
    public byte[] Signature { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Indicates whether this key has been validated.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation error message if IsValid is false.
    /// </summary>
    public string? ValidationError { get; set; }
}
