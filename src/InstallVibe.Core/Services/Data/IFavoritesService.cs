namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Service for managing user's favorite/pinned guides.
/// </summary>
public interface IFavoritesService
{
    /// <summary>
    /// Gets all pinned guides for a user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>List of guide IDs that are pinned, ordered by SortOrder.</returns>
    Task<List<string>> GetPinnedGuideIdsAsync(string userId);

    /// <summary>
    /// Checks if a guide is pinned by the user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="guideId">Guide ID.</param>
    /// <returns>True if pinned, false otherwise.</returns>
    Task<bool> IsPinnedAsync(string userId, string guideId);

    /// <summary>
    /// Pins a guide for the user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="guideId">Guide ID to pin.</param>
    Task PinGuideAsync(string userId, string guideId);

    /// <summary>
    /// Unpins a guide for the user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="guideId">Guide ID to unpin.</param>
    Task UnpinGuideAsync(string userId, string guideId);

    /// <summary>
    /// Toggles pin status for a guide.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="guideId">Guide ID.</param>
    /// <returns>True if now pinned, false if unpinned.</returns>
    Task<bool> TogglePinAsync(string userId, string guideId);
}
