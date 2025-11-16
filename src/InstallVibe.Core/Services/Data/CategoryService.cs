using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Implements category management functionality.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly IGuideService _guideService;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(
        IGuideService guideService,
        ILogger<CategoryService> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<CategoryInfo>> GetAllCategoriesAsync()
    {
        try
        {
            var guides = await _guideService.GetAllGuidesAsync();

            var categories = guides
                .Where(g => !string.IsNullOrWhiteSpace(g.Category))
                .GroupBy(g => g.Category)
                .Select(group => new CategoryInfo
                {
                    Name = group.Key,
                    GuideCount = group.Count()
                })
                .OrderBy(c => c.Name)
                .ToList();

            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all categories");
            return new List<CategoryInfo>();
        }
    }

    public async Task<int> GetCategoryUsageCountAsync(string category)
    {
        try
        {
            var guides = await _guideService.GetAllGuidesAsync();
            return guides.Count(g => g.Category == category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category usage count for {Category}", category);
            return 0;
        }
    }
}
