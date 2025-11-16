namespace InstallVibe.Core.Services.User;

/// <summary>
/// Service for managing user context and information.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the current user's identifier.
    /// </summary>
    /// <returns>The current user ID.</returns>
    Task<string> GetCurrentUserIdAsync();

    /// <summary>
    /// Gets the current user's display name.
    /// </summary>
    /// <returns>The current user's display name.</returns>
    Task<string> GetCurrentUserNameAsync();

    /// <summary>
    /// Checks if the current user has administrator privileges.
    /// </summary>
    /// <returns>True if the user is an administrator, false otherwise.</returns>
    Task<bool> IsAdminAsync();
}
