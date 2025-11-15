using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Manages license status and permissions.
/// </summary>
public interface ILicenseManager
{
    /// <summary>
    /// Gets the current license information.
    /// </summary>
    Task<LicenseInfo> GetLicenseInfoAsync();

    /// <summary>
    /// Checks if a specific feature is enabled.
    /// </summary>
    Task<bool> IsFeatureEnabledAsync(string featureName);

    /// <summary>
    /// Checks if the current license allows admin features.
    /// </summary>
    Task<bool> IsAdminLicenseAsync();
}
