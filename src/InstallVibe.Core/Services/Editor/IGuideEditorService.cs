using InstallVibe.Core.Models.Domain;

namespace InstallVibe.Core.Services.Editor;

/// <summary>
/// Service for creating and editing installation guides.
/// </summary>
public interface IGuideEditorService
{
    /// <summary>
    /// Creates a new empty guide with default metadata.
    /// </summary>
    Task<Guide> CreateNewGuideAsync();

    /// <summary>
    /// Saves a guide draft locally.
    /// </summary>
    Task<bool> SaveDraftAsync(Guide guide);

    /// <summary>
    /// Loads a guide draft from local storage.
    /// </summary>
    Task<Guide?> LoadDraftAsync(string guideId);

    /// <summary>
    /// Gets all draft guides.
    /// </summary>
    Task<IEnumerable<Guide>> GetAllDraftsAsync();

    /// <summary>
    /// Deletes a draft guide.
    /// </summary>
    Task<bool> DeleteDraftAsync(string guideId);

    /// <summary>
    /// Adds a new step to a guide.
    /// </summary>
    Task<Step> AddStepAsync(Guide guide, int position);

    /// <summary>
    /// Updates an existing step.
    /// </summary>
    Task<bool> UpdateStepAsync(Guide guide, Step step);

    /// <summary>
    /// Deletes a step from a guide.
    /// </summary>
    Task<bool> DeleteStepAsync(Guide guide, string stepId);

    /// <summary>
    /// Reorders steps in a guide.
    /// </summary>
    Task<bool> ReorderStepsAsync(Guide guide, int oldIndex, int newIndex);

    /// <summary>
    /// Duplicates an existing step.
    /// </summary>
    Task<Step> DuplicateStepAsync(Guide guide, Step step);

    /// <summary>
    /// Increments the guide version.
    /// </summary>
    string IncrementVersion(string currentVersion, bool isMajor = false);
}
