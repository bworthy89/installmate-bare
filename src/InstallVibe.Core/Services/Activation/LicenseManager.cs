using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Manages license status and feature permissions.
/// </summary>
public class LicenseManager : ILicenseManager
{
    private readonly ITokenManager _tokenManager;

    public LicenseManager(ITokenManager tokenManager)
    {
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
    }

    /// <inheritdoc/>
    public async Task<LicenseInfo> GetLicenseInfoAsync()
    {
        var token = await _tokenManager.LoadTokenAsync();

        if (token == null)
        {
            return new LicenseInfo { IsActivated = false };
        }

        return new LicenseInfo
        {
            IsActivated = true,
            LicenseType = token.LicenseType,
            ExpirationDate = token.ExpirationDate,
            DaysRemaining = token.DaysUntilExpiration,
            CustomerId = token.CustomerId,
            EnabledFeatures = token.EnabledFeatures
        };
    }

    /// <inheritdoc/>
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        var token = await _tokenManager.LoadTokenAsync();

        if (token == null || token.IsExpired)
            return false;

        return token.EnabledFeatures.Contains(featureName, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc/>
    public async Task<bool> IsAdminLicenseAsync()
    {
        var token = await _tokenManager.LoadTokenAsync();

        if (token == null || token.IsExpired)
            return false;

        return token.LicenseType == LicenseType.Admin;
    }
}
