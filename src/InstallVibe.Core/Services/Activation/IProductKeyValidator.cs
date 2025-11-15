using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Validates product keys using offline RSA signature verification.
/// </summary>
public interface IProductKeyValidator
{
    /// <summary>
    /// Parses and validates a product key.
    /// </summary>
    ProductKey? ParseAndValidate(string productKeyString);

    /// <summary>
    /// Checks if a product key string has valid format.
    /// </summary>
    bool IsValidFormat(string productKeyString);
}
