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
}
