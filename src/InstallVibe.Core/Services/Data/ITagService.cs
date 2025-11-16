namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Service for managing guide tags.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Gets all tags currently in use by guides.
    /// </summary>
    /// <returns>List of tags with usage count.</returns>
    Task<List<TagInfo>> GetAllTagsAsync();

    /// <summary>
    /// Gets the count of guides using a specific tag.
    /// </summary>
    /// <param name="tag">Tag name.</param>
    /// <returns>Number of guides using this tag.</returns>
    Task<int> GetTagUsageCountAsync(string tag);

    /// <summary>
    /// Gets tag suggestions based on guide content (title, description).
    /// </summary>
    /// <param name="guideTitle">Guide title.</param>
    /// <param name="guideDescription">Guide description.</param>
    /// <returns>List of suggested tags.</returns>
    Task<List<string>> SuggestTagsAsync(string guideTitle, string guideDescription);
}

/// <summary>
/// Information about a tag and its usage.
/// </summary>
public class TagInfo
{
    /// <summary>
    /// Tag name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Number of guides using this tag.
    /// </summary>
    public int GuideCount { get; set; }
}
