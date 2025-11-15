using InstallVibe.Data.Context;
using InstallVibe.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Service for managing user's favorite/pinned guides.
/// </summary>
public class FavoritesService : IFavoritesService
{
    private readonly InstallVibeContext _context;
    private readonly ILogger<FavoritesService> _logger;

    public FavoritesService(
        InstallVibeContext context,
        ILogger<FavoritesService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<string>> GetPinnedGuideIdsAsync(string userId)
    {
        try
        {
            var pinnedGuides = await _context.Favorites
                .Where(f => f.UserId == userId)
                .OrderBy(f => f.SortOrder)
                .ThenByDescending(f => f.CreatedDate)
                .Select(f => f.GuideId)
                .ToListAsync();

            _logger.LogDebug("Retrieved {Count} pinned guides for user {UserId}", pinnedGuides.Count, userId);
            return pinnedGuides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get pinned guides for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> IsPinnedAsync(string userId, string guideId)
    {
        try
        {
            var exists = await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.GuideId == guideId);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check if guide {GuideId} is pinned for user {UserId}", guideId, userId);
            throw;
        }
    }

    public async Task PinGuideAsync(string userId, string guideId)
    {
        try
        {
            // Check if already pinned
            var existing = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.GuideId == guideId);

            if (existing != null)
            {
                _logger.LogDebug("Guide {GuideId} is already pinned for user {UserId}", guideId, userId);
                return;
            }

            // Get the current max sort order
            var maxSortOrder = await _context.Favorites
                .Where(f => f.UserId == userId)
                .MaxAsync(f => (int?)f.SortOrder) ?? 0;

            var favorite = new FavoriteEntity
            {
                UserId = userId,
                GuideId = guideId,
                SortOrder = maxSortOrder + 1,
                CreatedDate = DateTime.UtcNow
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Pinned guide {GuideId} for user {UserId}", guideId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pin guide {GuideId} for user {UserId}", guideId, userId);
            throw;
        }
    }

    public async Task UnpinGuideAsync(string userId, string guideId)
    {
        try
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.GuideId == guideId);

            if (favorite == null)
            {
                _logger.LogDebug("Guide {GuideId} is not pinned for user {UserId}", guideId, userId);
                return;
            }

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Unpinned guide {GuideId} for user {UserId}", guideId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unpin guide {GuideId} for user {UserId}", guideId, userId);
            throw;
        }
    }

    public async Task<bool> TogglePinAsync(string userId, string guideId)
    {
        try
        {
            var isPinned = await IsPinnedAsync(userId, guideId);

            if (isPinned)
            {
                await UnpinGuideAsync(userId, guideId);
                return false;
            }
            else
            {
                await PinGuideAsync(userId, guideId);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle pin for guide {GuideId} and user {UserId}", guideId, userId);
            throw;
        }
    }
}
