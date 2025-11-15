using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.Progress;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Media;
using InstallVibe.Core.Services.SharePoint;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Engine;

/// <summary>
/// Main orchestrator for guide loading, rendering, progress tracking, and refresh logic.
/// </summary>
public class GuideEngine : IGuideEngine
{
    private readonly IGuideService _guideService;
    private readonly IProgressService _progressService;
    private readonly ISharePointService _sharePointService;
    private readonly IMediaService _mediaService;
    private readonly ILogger<GuideEngine> _logger;

    public GuideEngine(
        IGuideService guideService,
        IProgressService progressService,
        ISharePointService sharePointService,
        IMediaService mediaService,
        ILogger<GuideEngine> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Guide?> LoadGuideAsync(string guideId, bool forceRefresh = false)
    {
        try
        {
            _logger.LogInformation("Loading guide {GuideId} (forceRefresh: {ForceRefresh})", guideId, forceRefresh);

            Guide? guide;

            if (forceRefresh)
            {
                // Force refresh from SharePoint
                _logger.LogInformation("Force refreshing guide {GuideId} from SharePoint", guideId);
                guide = await _sharePointService.GetGuideAsync(guideId);
                
                if (guide != null)
                {
                    // Save to local storage
                    await _guideService.SaveGuideAsync(guide);
                    _logger.LogInformation("Guide {GuideId} refreshed and saved locally", guideId);
                }
            }
            else
            {
                // Try local first
                guide = await _guideService.GetGuideAsync(guideId);

                if (guide == null)
                {
                    // Fallback to SharePoint
                    _logger.LogInformation("Guide {GuideId} not found locally, fetching from SharePoint", guideId);
                    guide = await _sharePointService.GetGuideAsync(guideId);

                    if (guide != null)
                    {
                        // Save to local storage
                        await _guideService.SaveGuideAsync(guide);
                        _logger.LogInformation("Guide {GuideId} downloaded and saved locally", guideId);
                    }
                }
                else
                {
                    _logger.LogInformation("Guide {GuideId} loaded from local cache", guideId);
                }
            }

            if (guide == null)
            {
                _logger.LogWarning("Guide {GuideId} not found", guideId);
                return null;
            }

            // Ensure steps are ordered correctly
            if (guide.Steps != null && guide.Steps.Count > 0)
            {
                guide.Steps = guide.Steps.OrderBy(s => s.OrderIndex).ToList();
            }

            return guide;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide {GuideId}", guideId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<GuideProgress?> GetProgressAsync(string guideId, string userId)
    {
        try
        {
            return await _progressService.GetProgressAsync(guideId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for guide {GuideId}, user {UserId}", guideId, userId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<GuideProgress> StartGuideAsync(string guideId, string userId)
    {
        try
        {
            // Check if progress already exists
            var existingProgress = await _progressService.GetProgressAsync(guideId, userId);
            if (existingProgress != null)
            {
                _logger.LogInformation("Resuming existing progress for guide {GuideId}, user {UserId}", guideId, userId);
                return existingProgress;
            }

            // Load guide to get first step
            var guide = await LoadGuideAsync(guideId);
            if (guide == null)
            {
                throw new InvalidOperationException($"Guide not found: {guideId}");
            }

            if (guide.Steps == null || guide.Steps.Count == 0)
            {
                throw new InvalidOperationException($"Guide {guideId} has no steps");
            }

            // Create new progress
            var progress = new GuideProgress
            {
                ProgressId = Guid.NewGuid().ToString(),
                GuideId = guideId,
                UserId = userId,
                CurrentStepId = guide.Steps.First().StepId,
                StepProgress = new Dictionary<string, StepStatus>(),
                StartedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow
            };

            // Initialize all steps as not started
            foreach (var step in guide.Steps)
            {
                progress.StepProgress[step.StepId] = StepStatus.NotStarted;
            }

            // Mark first step as in progress
            progress.StepProgress[guide.Steps.First().StepId] = StepStatus.InProgress;

            // Save progress
            await _progressService.SaveProgressAsync(progress);

            _logger.LogInformation(
                "Started guide {GuideId} for user {UserId} with {StepCount} steps",
                guideId,
                userId,
                guide.Steps.Count);

            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting guide {GuideId} for user {UserId}", guideId, userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GuideProgress> UpdateStepStatusAsync(string progressId, string stepId, StepStatus status)
    {
        try
        {
            await _progressService.UpdateStepProgressAsync(progressId, stepId, status);
            
            // Get updated progress
            var progress = await _progressService.GetProgressAsync(progressId, string.Empty);
            if (progress == null)
            {
                throw new InvalidOperationException($"Progress not found: {progressId}");
            }

            _logger.LogInformation(
                "Updated step {StepId} to status {Status} (Progress: {ProgressId})",
                stepId,
                status,
                progressId);

            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step status for {ProgressId}, step {StepId}", progressId, stepId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GuideProgress> CompleteStepAsync(string progressId, string stepId)
    {
        try
        {
            // Mark current step as completed
            await _progressService.UpdateStepProgressAsync(progressId, stepId, StepStatus.Completed);

            // Get updated progress
            var progress = await _progressService.GetProgressAsync(progressId, string.Empty);
            if (progress == null)
            {
                throw new InvalidOperationException($"Progress not found: {progressId}");
            }

            // Load guide to find next step
            var guide = await LoadGuideAsync(progress.GuideId);
            if (guide == null)
            {
                throw new InvalidOperationException($"Guide not found: {progress.GuideId}");
            }

            // Find current step index
            var currentStepIndex = guide.Steps.FindIndex(s => s.StepId == stepId);
            if (currentStepIndex < 0)
            {
                throw new InvalidOperationException($"Step not found: {stepId}");
            }

            // Check if there's a next step
            if (currentStepIndex < guide.Steps.Count - 1)
            {
                // Move to next step
                var nextStep = guide.Steps[currentStepIndex + 1];
                progress.CurrentStepId = nextStep.StepId;
                
                // Mark next step as in progress
                if (progress.StepProgress.ContainsKey(nextStep.StepId))
                {
                    progress.StepProgress[nextStep.StepId] = StepStatus.InProgress;
                }

                await _progressService.SaveProgressAsync(progress);

                _logger.LogInformation(
                    "Completed step {StepId} and advanced to {NextStepId}",
                    stepId,
                    nextStep.StepId);
            }
            else
            {
                // Last step completed - guide is finished
                _logger.LogInformation(
                    "Completed final step {StepId}, guide {GuideId} is finished",
                    stepId,
                    progress.GuideId);
            }

            return progress;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing step {StepId} for progress {ProgressId}", stepId, progressId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GuideRefreshResult> CheckForUpdatesAsync(string guideId)
    {
        var result = new GuideRefreshResult();

        try
        {
            // Check if online
            var isOnline = await _sharePointService.IsOnlineAsync();
            if (!isOnline)
            {
                result.Success = false;
                result.WasOffline = true;
                result.ErrorMessage = "SharePoint is offline";
                return result;
            }

            // Get local version
            var localGuide = await _guideService.GetGuideAsync(guideId);
            if (localGuide == null)
            {
                result.Success = false;
                result.ErrorMessage = "Guide not found locally";
                return result;
            }

            // Get SharePoint metadata
            var remoteMetadata = await _sharePointService.GetGuideMetadataAsync(guideId);
            if (remoteMetadata == null)
            {
                result.Success = false;
                result.ErrorMessage = "Guide not found on SharePoint";
                return result;
            }

            result.Success = true;
            result.PreviousVersion = localGuide.Version;
            result.PreviousLastModified = localGuide.LastModified;
            result.NewVersion = remoteMetadata.Version;
            result.NewLastModified = remoteMetadata.LastModified;

            // Check if update is available
            result.UpdateAvailable = remoteMetadata.LastModified > localGuide.LastModified ||
                                    remoteMetadata.Version != localGuide.Version;

            if (result.UpdateAvailable)
            {
                _logger.LogInformation(
                    "Update available for guide {GuideId}: v{OldVersion} ({OldDate}) -> v{NewVersion} ({NewDate})",
                    guideId,
                    result.PreviousVersion,
                    result.PreviousLastModified,
                    result.NewVersion,
                    result.NewLastModified);
            }
            else
            {
                _logger.LogInformation("Guide {GuideId} is up to date", guideId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates for guide {GuideId}", guideId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<GuideRefreshResult> RefreshGuideAsync(string guideId, bool syncProgress = true)
    {
        var result = new GuideRefreshResult();

        try
        {
            // Check for updates first
            var updateCheck = await CheckForUpdatesAsync(guideId);
            if (!updateCheck.Success)
            {
                return updateCheck;
            }

            if (!updateCheck.UpdateAvailable)
            {
                result.Success = true;
                result.UpdateAvailable = false;
                result.WasUpdated = false;
                return result;
            }

            // Get current version
            var oldGuide = await _guideService.GetGuideAsync(guideId);

            // Download new version
            var newGuide = await _sharePointService.GetGuideAsync(guideId);
            if (newGuide == null)
            {
                result.Success = false;
                result.ErrorMessage = "Failed to download updated guide";
                return result;
            }

            // Save new version
            await _guideService.SaveGuideAsync(newGuide);

            result.Success = true;
            result.UpdateAvailable = true;
            result.WasUpdated = true;
            result.PreviousVersion = oldGuide?.Version;
            result.PreviousLastModified = oldGuide?.LastModified;
            result.NewVersion = newGuide.Version;
            result.NewLastModified = newGuide.LastModified;

            // Calculate steps changed
            if (oldGuide != null)
            {
                var oldStepIds = oldGuide.Steps?.Select(s => s.StepId).ToHashSet() ?? new HashSet<string>();
                var newStepIds = newGuide.Steps?.Select(s => s.StepId).ToHashSet() ?? new HashSet<string>();
                
                result.StepsChanged = oldStepIds.SymmetricExceptDifference(newStepIds).Count();
            }

            _logger.LogInformation(
                "Refreshed guide {GuideId} from v{OldVersion} to v{NewVersion} ({StepsChanged} steps changed)",
                guideId,
                result.PreviousVersion,
                result.NewVersion,
                result.StepsChanged);

            // Sync progress if requested
            if (syncProgress && result.StepsChanged > 0)
            {
                // TODO: Implement progress sync logic
                // This would map old step IDs to new step IDs and preserve completion status
                result.ProgressSynced = false; // Placeholder
                _logger.LogWarning("Progress sync not yet implemented for guide {GuideId}", guideId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing guide {GuideId}", guideId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> EnsureMediaCachedAsync(string guideId, IProgress<MediaCacheProgress>? progress = null)
    {
        try
        {
            var guide = await LoadGuideAsync(guideId);
            if (guide == null)
            {
                _logger.LogWarning("Guide {GuideId} not found, cannot cache media", guideId);
                return new List<string>();
            }

            return await _mediaService.CacheGuideMediaAsync(guide, progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring media cached for guide {GuideId}", guideId);
            return new List<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<Step?> GetStepAsync(string guideId, string stepId)
    {
        try
        {
            var guide = await LoadGuideAsync(guideId);
            if (guide == null)
            {
                return null;
            }

            return guide.Steps?.FirstOrDefault(s => s.StepId == stepId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting step {StepId} from guide {GuideId}", stepId, guideId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Step?> GetNextStepAsync(string guideId, string currentStepId)
    {
        try
        {
            var guide = await LoadGuideAsync(guideId);
            if (guide == null || guide.Steps == null)
            {
                return null;
            }

            var currentIndex = guide.Steps.FindIndex(s => s.StepId == currentStepId);
            if (currentIndex < 0 || currentIndex >= guide.Steps.Count - 1)
            {
                return null; // No next step
            }

            return guide.Steps[currentIndex + 1];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting next step after {StepId} in guide {GuideId}", currentStepId, guideId);
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<Step?> GetPreviousStepAsync(string guideId, string currentStepId)
    {
        try
        {
            var guide = await LoadGuideAsync(guideId);
            if (guide == null || guide.Steps == null)
            {
                return null;
            }

            var currentIndex = guide.Steps.FindIndex(s => s.StepId == currentStepId);
            if (currentIndex <= 0)
            {
                return null; // No previous step
            }

            return guide.Steps[currentIndex - 1];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting previous step before {StepId} in guide {GuideId}", currentStepId, guideId);
            return null;
        }
    }
}
