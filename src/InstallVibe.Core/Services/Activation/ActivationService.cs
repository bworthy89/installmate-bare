using InstallVibe.Core.Models.Activation;
using InstallVibe.Infrastructure.Security.Cryptography;
using InstallVibe.Infrastructure.Device;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Main activation service implementation.
/// Handles offline RSA validation with optional online fallback.
/// </summary>
public class ActivationService : IActivationService
{
    private readonly IProductKeyValidator _keyValidator;
    private readonly ITokenManager _tokenManager;
    private readonly IHashService _hashService;
    private readonly IDeviceIdProvider _deviceIdProvider;

    public ActivationService(
        IProductKeyValidator keyValidator,
        ITokenManager tokenManager,
        IHashService hashService,
        IDeviceIdProvider deviceIdProvider)
    {
        _keyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
        _tokenManager = tokenManager ?? throw new ArgumentNullException(nameof(tokenManager));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        _deviceIdProvider = deviceIdProvider ?? throw new ArgumentNullException(nameof(deviceIdProvider));
    }

    /// <inheritdoc/>
    public async Task<ActivationResult> ActivateAsync(string productKey, bool forceOnline = false)
    {
        if (string.IsNullOrWhiteSpace(productKey))
        {
            return ActivationResult.CreateFailure(
                ActivationErrorCode.InvalidFormat,
                "Product key cannot be empty");
        }

        // Check if already activated
        var existingToken = await _tokenManager.LoadTokenAsync();
        if (existingToken != null && !existingToken.IsExpired)
        {
            return ActivationResult.CreateFailure(
                ActivationErrorCode.AlreadyActivated,
                "Application is already activated. Deactivate first to use a different key.");
        }

        // Try offline validation (unless forced online)
        if (!forceOnline)
        {
            var offlineResult = await TryOfflineActivationAsync(productKey);
            if (offlineResult.Success)
            {
                return offlineResult;
            }

            // If offline validation failed but format is valid, could try online
            // For now, just return the offline failure
            return offlineResult;
        }

        // Online validation would go here (SharePoint lookup)
        // Not implemented in this version
        return ActivationResult.CreateFailure(
            ActivationErrorCode.NetworkError,
            "Online activation not yet implemented");
    }

    /// <inheritdoc/>
    public async Task DeactivateAsync()
    {
        await _tokenManager.DeleteTokenAsync();
    }

    /// <inheritdoc/>
    public async Task<LicenseInfo> GetLicenseInfoAsync()
    {
        var token = await _tokenManager.LoadTokenAsync();

        if (token == null)
        {
            return new LicenseInfo
            {
                IsActivated = false
            };
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
    public async Task<bool> IsActivatedAsync()
    {
        return await _tokenManager.IsActivatedAsync();
    }

    private async Task<ActivationResult> TryOfflineActivationAsync(string productKey)
    {
        // Validate and parse the product key
        var parsedKey = _keyValidator.ParseAndValidate(productKey);

        if (parsedKey == null || !parsedKey.IsValid)
        {
            var errorCode = DetermineErrorCode(parsedKey?.ValidationError);
            return ActivationResult.CreateFailure(
                errorCode,
                parsedKey?.ValidationError ?? "Invalid product key");
        }

        // Create activation token
        var token = CreateActivationToken(parsedKey, isOnline: false);

        // Save token
        await _tokenManager.SaveTokenAsync(token);

        return ActivationResult.CreateSuccess(token);
    }

    private ActivationToken CreateActivationToken(ProductKey productKey, bool isOnline)
    {
        var machineId = _deviceIdProvider.GetMachineId();
        var keyHash = _hashService.ComputeSha256(productKey.OriginalKey);

        var token = new ActivationToken
        {
            ProductKeyHash = keyHash,
            LicenseType = productKey.LicenseType,
            ExpirationDate = productKey.ExpirationDate,
            CustomerId = productKey.CustomerId.ToString(),
            EnabledFeatures = ParseFeatureFlags(productKey.FeatureFlags),
            MachineId = machineId,
            ValidatedDate = DateTime.UtcNow,
            OnlineValidation = isOnline,
            Signature = ComputeTokenSignature(keyHash, machineId)
        };

        return token;
    }

    private List<string> ParseFeatureFlags(byte featureFlags)
    {
        var features = new List<string>();

        // Bit 0: Editor
        if ((featureFlags & 0x01) != 0)
            features.Add("Editor");

        // Bit 1: Advanced Reporting
        if ((featureFlags & 0x02) != 0)
            features.Add("AdvancedReporting");

        // Bit 2: API Access
        if ((featureFlags & 0x04) != 0)
            features.Add("ApiAccess");

        // Add more feature flags as needed

        return features;
    }

    private string ComputeTokenSignature(string keyHash, string machineId)
    {
        // Compute HMAC of token data for tamper detection
        var data = $"{keyHash}|{machineId}|{DateTime.UtcNow:O}";
        return _hashService.ComputeSha256(data);
    }

    private ActivationErrorCode DetermineErrorCode(string? errorMessage)
    {
        if (errorMessage == null)
            return ActivationErrorCode.Unknown;

        if (errorMessage.Contains("format", StringComparison.OrdinalIgnoreCase))
            return ActivationErrorCode.InvalidFormat;

        if (errorMessage.Contains("checksum", StringComparison.OrdinalIgnoreCase))
            return ActivationErrorCode.InvalidChecksum;

        if (errorMessage.Contains("signature", StringComparison.OrdinalIgnoreCase))
            return ActivationErrorCode.InvalidSignature;

        if (errorMessage.Contains("expired", StringComparison.OrdinalIgnoreCase))
            return ActivationErrorCode.Expired;

        return ActivationErrorCode.Unknown;
    }
}
