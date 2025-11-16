using InstallVibe.Core.Services.Activation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.User;

/// <summary>
/// Service for managing user context and information.
/// Currently uses Windows username as the user identifier.
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly ILicenseManager _licenseManager;
    private string? _cachedUserId;
    private string? _cachedUserName;

    public UserService(
        ILogger<UserService> logger,
        ILicenseManager licenseManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _licenseManager = licenseManager ?? throw new ArgumentNullException(nameof(licenseManager));
    }

    /// <summary>
    /// Gets the current user's identifier.
    /// Uses Windows username (Environment.UserName) as the user ID.
    /// </summary>
    public Task<string> GetCurrentUserIdAsync()
    {
        if (_cachedUserId != null)
        {
            return Task.FromResult(_cachedUserId);
        }

        try
        {
            _cachedUserId = Environment.UserName;
            _logger.LogDebug("Retrieved user ID: {UserId}", _cachedUserId);
            return Task.FromResult(_cachedUserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user ID");
            throw new InvalidOperationException("Failed to retrieve current user information", ex);
        }
    }

    /// <summary>
    /// Gets the current user's display name.
    /// Uses Windows username for now.
    /// </summary>
    public Task<string> GetCurrentUserNameAsync()
    {
        if (_cachedUserName != null)
        {
            return Task.FromResult(_cachedUserName);
        }

        try
        {
            _cachedUserName = Environment.UserName;
            _logger.LogDebug("Retrieved user name: {UserName}", _cachedUserName);
            return Task.FromResult(_cachedUserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current user name");
            throw new InvalidOperationException("Failed to retrieve current user information", ex);
        }
    }

    /// <summary>
    /// Checks if the current user has administrator privileges.
    /// Checks if the product key grants Admin license type.
    /// </summary>
    public async Task<bool> IsAdminAsync()
    {
        try
        {
            var isAdmin = await _licenseManager.IsAdminLicenseAsync();
            var userId = await GetCurrentUserIdAsync();
            _logger.LogDebug("Admin check for user {UserId}: {IsAdmin}", userId, isAdmin);
            return isAdmin;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check admin status");
            return false; // Deny access on error
        }
    }
}
