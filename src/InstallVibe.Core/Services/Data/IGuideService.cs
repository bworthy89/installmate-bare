using InstallVibe.Core.Models.Domain;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Service for managing guides in local storage.
/// </summary>
public interface IGuideService
{
    Task<Guide?> GetGuideAsync(string guideId);
    Task<List<Guide>> GetAllGuidesAsync();
    Task SaveGuideAsync(Guide guide);
    Task DeleteGuideAsync(string guideId);
    Task<bool> ExistsAsync(string guideId);

    /// <summary>
    /// Gets the most recent guides (created in the last 30 days).
    /// </summary>
    /// <param name="count">Maximum number of guides to return.</param>
    /// <returns>List of recent guides ordered by creation date (newest first).</returns>
    Task<List<Guide>> GetNewGuidesAsync(int count = 3);

    /// <summary>
    /// Gets guides that have been started but not completed by the user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>List of in-progress guides ordered by last updated date.</returns>
    Task<List<Guide>> GetInProgressGuidesAsync(string userId);

    /// <summary>
    /// Gets guides that have been completed by the user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="count">Maximum number of guides to return.</param>
    /// <returns>List of completed guides ordered by completion date (most recent first).</returns>
    Task<List<Guide>> GetCompletedGuidesAsync(string userId, int count = 3);

    /// <summary>
    /// Gets guides by their IDs.
    /// </summary>
    /// <param name="guideIds">List of guide IDs.</param>
    /// <returns>List of guides matching the provided IDs.</returns>
    Task<List<Guide>> GetGuidesByIdsAsync(List<string> guideIds);
}
