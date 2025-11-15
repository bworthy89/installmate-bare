using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.Progress;
using InstallVibe.Core.Services.Media;

namespace InstallVibe.Core.Services.Engine;

/// <summary>
/// Main orchestrator for guide loading, rendering, progress tracking, and refresh logic.
/// </summary>
public interface IGuideEngine
{
    /// <summary>
    /// Loads a guide by ID, prioritizing local cache with fallback to SharePoint.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="forceRefresh">Force refresh from SharePoint even if cached.</param>
    /// <returns>Loaded guide with all metadata, or null if not found.</returns>
    Task<Guide?> LoadGuideAsync(string guideId, bool forceRefresh = false);

    /// <summary>
    /// Gets the current user's progress for a guide.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="userId">User identifier.</param>
    /// <returns>Progress object, or null if no progress exists.</returns>
    Task<GuideProgress?> GetProgressAsync(string guideId, string userId);

    /// <summary>
    /// Starts or resumes a guide for a user.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="userId">User identifier.</param>
    /// <returns>Guide progress object.</returns>
    Task<GuideProgress> StartGuideAsync(string guideId, string userId);

    /// <summary>
    /// Updates the completion status of a step.
    /// </summary>
    /// <param name="progressId">Progress identifier.</param>
    /// <param name="stepId">Step identifier.</param>
    /// <param name="status">New status.</param>
    /// <returns>Updated progress.</returns>
    Task<GuideProgress> UpdateStepStatusAsync(string progressId, string stepId, StepStatus status);

    /// <summary>
    /// Marks a step as completed and advances to the next step.
    /// </summary>
    /// <param name="progressId">Progress identifier.</param>
    /// <param name="stepId">Step identifier.</param>
    /// <returns>Updated progress with new current step.</returns>
    Task<GuideProgress> CompleteStepAsync(string progressId, string stepId);

    /// <summary>
    /// Checks if there are updates available for a guide.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <returns>Refresh result with update information.</returns>
    Task<GuideRefreshResult> CheckForUpdatesAsync(string guideId);

    /// <summary>
    /// Refreshes a guide from SharePoint and updates local cache.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="syncProgress">Whether to sync existing progress to new version.</param>
    /// <returns>Refresh result.</returns>
    Task<GuideRefreshResult> RefreshGuideAsync(string guideId, bool syncProgress = true);

    /// <summary>
    /// Ensures all media for a guide is cached locally.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <returns>List of media IDs that failed to cache.</returns>
    Task<List<string>> EnsureMediaCachedAsync(string guideId, IProgress<MediaCacheProgress>? progress = null);

    /// <summary>
    /// Gets a specific step from a guide.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="stepId">Step identifier.</param>
    /// <returns>Step object, or null if not found.</returns>
    Task<Step?> GetStepAsync(string guideId, string stepId);

    /// <summary>
    /// Gets the next step in a guide based on current progress.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="currentStepId">Current step identifier.</param>
    /// <returns>Next step, or null if at end.</returns>
    Task<Step?> GetNextStepAsync(string guideId, string currentStepId);

    /// <summary>
    /// Gets the previous step in a guide based on current progress.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="currentStepId">Current step identifier.</param>
    /// <returns>Previous step, or null if at beginning.</returns>
    Task<Step?> GetPreviousStepAsync(string guideId, string currentStepId);
}
