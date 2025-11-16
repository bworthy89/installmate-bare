namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Service for managing guide categories.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories currently in use by guides.
    /// </summary>
    /// <returns>List of categories with usage count.</returns>
    Task<List<CategoryInfo>> GetAllCategoriesAsync();

    /// <summary>
    /// Gets the count of guides using a specific category.
    /// </summary>
    /// <param name="category">Category name.</param>
    /// <returns>Number of guides using this category.</returns>
    Task<int> GetCategoryUsageCountAsync(string category);
}

/// <summary>
/// Information about a category and its usage.
/// </summary>
public class CategoryInfo
{
    /// <summary>
    /// Category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of guides using this category.
    /// </summary>
    public int GuideCount { get; set; }
}
