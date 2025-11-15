using InstallVibe.Core.Models.Progress;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Service for managing guide progress tracking.
/// </summary>
public interface IProgressService
{
    Task<GuideProgress?> GetProgressAsync(string guideId, string userId);
    Task SaveProgressAsync(GuideProgress progress);
    Task<List<GuideProgress>> GetAllProgressAsync(string userId);
    Task DeleteProgressAsync(string progressId);
    Task UpdateStepProgressAsync(string progressId, string stepId, StepStatus status);
}
