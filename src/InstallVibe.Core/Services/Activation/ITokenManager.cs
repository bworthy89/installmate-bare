using InstallVibe.Core.Models.Activation;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Manages activation token storage and retrieval.
/// </summary>
public interface ITokenManager
{
    /// <summary>
    /// Saves an activation token to encrypted storage.
    /// </summary>
    Task SaveTokenAsync(ActivationToken token);

    /// <summary>
    /// Loads the activation token from storage.
    /// </summary>
    Task<ActivationToken?> LoadTokenAsync();

    /// <summary>
    /// Deletes the activation token (deactivation).
    /// </summary>
    Task DeleteTokenAsync();

    /// <summary>
    /// Checks if a valid activation token exists.
    /// </summary>
    Task<bool> IsActivatedAsync();
}
