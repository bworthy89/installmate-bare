namespace InstallVibe.Data.Entities;

/// <summary>
/// Entity representing a user's favorite/pinned guide.
/// </summary>
public class FavoriteEntity
{
    /// <summary>
    /// Unique identifier for this favorite entry.
    /// </summary>
    public string FavoriteId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// User who favorited the guide.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Guide that was favorited.
    /// </summary>
    public string GuideId { get; set; } = string.Empty;

    /// <summary>
    /// When the guide was favorited.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Display order for pinned guides (lower numbers appear first).
    /// </summary>
    public int SortOrder { get; set; } = 0;
}
