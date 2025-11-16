using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Data.Context;
using InstallVibe.Data.Entities;
using InstallVibe.Infrastructure.Security.Cryptography;
using InstallVibe.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Manages guide storage, retrieval, and JSON serialization.
/// </summary>
public class GuideService : IGuideService
{
    private readonly InstallVibeContext _context;
    private readonly ICacheService _cacheService;
    private readonly IHashService _hashService;
    private readonly ILogger<GuideService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GuideService(
        InstallVibeContext context,
        ICacheService cacheService,
        IHashService hashService,
        ILogger<GuideService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<Guide?> GetGuideAsync(string guideId)
    {
        try
        {
            // Check database first
            var guideEntity = await _context.Guides
                .Include(g => g.Steps)
                .FirstOrDefaultAsync(g => g.GuideId == guideId && !g.IsDeleted);

            if (guideEntity == null)
                return null;

            // Read from cache
            if (await _cacheService.IsCachedAsync("guide", guideId))
            {
                try
                {
                    var data = await _cacheService.ReadCachedFileAsync("guide", guideId);
                    var json = System.Text.Encoding.UTF8.GetString(data);
                    var guide = JsonSerializer.Deserialize<Guide>(json, JsonOptions);

                    if (guide != null)
                        return guide;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read cached guide {GuideId}, will attempt recovery", guideId);
                    await HandleCorruptedGuide(guideId);
                }
            }

            // Fallback: return basic info from database
            return MapEntityToModel(guideEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting guide {GuideId}", guideId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Guide>> GetAllGuidesAsync()
    {
        try
        {
            var guideEntities = await _context.Guides
                .Where(g => !g.IsDeleted)
                .OrderByDescending(g => g.LastModified)
                .ToListAsync();

            var guides = new List<Guide>();

            foreach (var entity in guideEntities)
            {
                try
                {
                    var guide = await GetGuideAsync(entity.GuideId);
                    if (guide != null)
                        guides.Add(guide);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping guide {GuideId} due to error", entity.GuideId);
                }
            }

            return guides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all guides");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveGuideAsync(Guide guide)
    {
        try
        {
            // Serialize to JSON
            var json = JsonSerializer.Serialize(guide, JsonOptions);
            var data = System.Text.Encoding.UTF8.GetBytes(json);

            // Compute checksum
            var checksum = Convert.ToHexString(_hashService.ComputeSha256(data)).ToLowerInvariant();

            // Cache the file
            await _cacheService.CacheFileAsync("guide", guide.GuideId, data, checksum);

            // Update database
            var entity = await _context.Guides.FindAsync(guide.GuideId);
            if (entity == null)
            {
                entity = new GuideEntity
                {
                    GuideId = guide.GuideId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Guides.Add(entity);
            }

            UpdateEntityFromModel(entity, guide, checksum, data.Length);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Saved guide {GuideId}", guide.GuideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving guide {GuideId}", guide.GuideId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteGuideAsync(string guideId)
    {
        try
        {
            var entity = await _context.Guides.FindAsync(guideId);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            // Invalidate cache
            await _cacheService.InvalidateCacheAsync("guide", guideId);

            _logger.LogInformation("Deleted guide {GuideId}", guideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting guide {GuideId}", guideId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ExistsAsync(string guideId)
    {
        return await _context.Guides.AnyAsync(g => g.GuideId == guideId && !g.IsDeleted);
    }

    /// <inheritdoc/>
    public async Task<List<Guide>> GetNewGuidesAsync(int count = 3)
    {
        try
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

            var guideEntities = await _context.Guides
                .Where(g => !g.IsDeleted && g.CreatedDate >= thirtyDaysAgo)
                .OrderByDescending(g => g.CreatedDate)
                .Take(count)
                .ToListAsync();

            var guides = new List<Guide>();
            foreach (var entity in guideEntities)
            {
                try
                {
                    var guide = await GetGuideAsync(entity.GuideId);
                    if (guide != null)
                        guides.Add(guide);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping guide {GuideId} due to error", entity.GuideId);
                }
            }

            _logger.LogDebug("Retrieved {Count} new guides", guides.Count);
            return guides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting new guides");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Guide>> GetInProgressGuidesAsync(string userId)
    {
        try
        {
            // Get guide IDs that have progress but are not completed
            var inProgressGuideIds = await _context.Progress
                .Where(p => p.UserId == userId && p.CompletedDate == null)
                .OrderByDescending(p => p.LastUpdated)
                .Select(p => p.GuideId)
                .ToListAsync();

            var guides = new List<Guide>();
            foreach (var guideId in inProgressGuideIds)
            {
                try
                {
                    var guide = await GetGuideAsync(guideId);
                    if (guide != null)
                        guides.Add(guide);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping guide {GuideId} due to error", guideId);
                }
            }

            _logger.LogDebug("Retrieved {Count} in-progress guides for user {UserId}", guides.Count, userId);
            return guides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting in-progress guides for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Guide>> GetCompletedGuidesAsync(string userId, int count = 3)
    {
        try
        {
            // Get guide IDs that have been completed
            var completedGuideIds = await _context.Progress
                .Where(p => p.UserId == userId && p.CompletedDate != null)
                .OrderByDescending(p => p.CompletedDate)
                .Take(count)
                .Select(p => p.GuideId)
                .ToListAsync();

            var guides = new List<Guide>();
            foreach (var guideId in completedGuideIds)
            {
                try
                {
                    var guide = await GetGuideAsync(guideId);
                    if (guide != null)
                        guides.Add(guide);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping guide {GuideId} due to error", guideId);
                }
            }

            _logger.LogDebug("Retrieved {Count} completed guides for user {UserId}", guides.Count, userId);
            return guides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed guides for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<Guide>> GetGuidesByIdsAsync(List<string> guideIds)
    {
        try
        {
            var guides = new List<Guide>();
            foreach (var guideId in guideIds)
            {
                try
                {
                    var guide = await GetGuideAsync(guideId);
                    if (guide != null)
                        guides.Add(guide);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping guide {GuideId} due to error", guideId);
                }
            }

            _logger.LogDebug("Retrieved {Count} guides by IDs", guides.Count);
            return guides;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting guides by IDs");
            throw;
        }
    }

    // Private helper methods

    private Guide MapEntityToModel(GuideEntity entity)
    {
        return new Guide
        {
            GuideId = entity.GuideId,
            Title = entity.Title,
            Version = entity.Version,
            Category = entity.Category,
            Description = entity.Description,
            RequiredLicense = ParseLicenseType(entity.RequiredLicense),
            IsPublished = entity.Published,
            Status = ParsePublishStatus(entity.Status),
            PublishedDate = entity.PublishedDate,
            LastModified = entity.LastModified,
            CreatedDate = entity.CreatedDate,
            Steps = new List<Step>(),
            Metadata = new GuideMetadata()
        };
    }

    private void UpdateEntityFromModel(GuideEntity entity, Guide model, string checksum, long fileSize)
    {
        entity.Title = model.Title;
        entity.Version = model.Version;
        entity.Category = model.Category;
        entity.Description = model.Description;
        entity.RequiredLicense = model.RequiredLicense.ToString();
        entity.Published = model.IsPublished;
        entity.Status = model.Status.ToString();
        entity.PublishedDate = model.PublishedDate;
        entity.LastModified = model.LastModified;
        entity.LocalPath = PathConstants.GetGuideJsonPath(model.GuideId);
        entity.Checksum = checksum;
        entity.FileSize = fileSize;
        entity.CachedDate = DateTime.UtcNow;
        entity.StepCount = model.Steps?.Count ?? 0;
    }

    private Core.Models.Activation.LicenseType ParseLicenseType(string? license)
    {
        return license?.ToLower() switch
        {
            "admin" => Core.Models.Activation.LicenseType.Admin,
            _ => Core.Models.Activation.LicenseType.Tech
        };
    }

    private PublishStatus ParsePublishStatus(string? status)
    {
        return status?.ToLower() switch
        {
            "published" => PublishStatus.Published,
            "archived" => PublishStatus.Archived,
            _ => PublishStatus.Draft
        };
    }

    private async Task HandleCorruptedGuide(string guideId)
    {
        try
        {
            _logger.LogWarning("Handling corrupted guide {GuideId}", guideId);

            // Mark for re-download
            var entity = await _context.Guides.FindAsync(guideId);
            if (entity != null)
            {
                entity.SyncStatus = "error";
                await _context.SaveChangesAsync();
            }

            // Invalidate cache
            await _cacheService.InvalidateCacheAsync("guide", guideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling corrupted guide {GuideId}", guideId);
        }
    }
}
