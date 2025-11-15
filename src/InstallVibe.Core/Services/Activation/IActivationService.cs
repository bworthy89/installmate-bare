using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Main activation service that orchestrates product key validation and token management.
/// </summary>
public interface IActivationService
{
    /// <summary>
    /// Activates the application with a product key.
    /// </summary>
    Task<ActivationResult> ActivateAsync(string productKey, bool forceOnline = false);

    /// <summary>
    /// Deactivates the application (removes activation token).
    /// </summary>
    Task DeactivateAsync();

    /// <summary>
    /// Gets the current activation status.
    /// </summary>
    Task<LicenseInfo> GetLicenseInfoAsync();

    /// <summary>
    /// Checks if the application is currently activated.
    /// </summary>
    Task<bool> IsActivatedAsync();
}
