using InstallVibe.Core.Models.Progress;
using InstallVibe.Data.Context;
using InstallVibe.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Manages guide progress tracking and persistence.
/// </summary>
public class ProgressService : IProgressService
{
    private readonly InstallVibeContext _context;
    private readonly ILogger<ProgressService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ProgressService(
        InstallVibeContext context,
        ILogger<ProgressService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<GuideProgress?> GetProgressAsync(string guideId, string userId)
    {
        try
        {
            var entity = await _context.Progress
                .FirstOrDefaultAsync(p => p.GuideId == guideId && p.UserId == userId);

            return entity != null ? MapEntityToModel(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for guide {GuideId}, user {UserId}", guideId, userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GuideProgress?> GetProgressByIdAsync(string progressId)
    {
        try
        {
            var entity = await _context.Progress.FindAsync(progressId);
            return entity != null ? MapEntityToModel(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress by ID {ProgressId}", progressId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveProgressAsync(GuideProgress progress)
    {
        try
        {
            var entity = await _context.Progress.FindAsync(progress.ProgressId);
            if (entity == null)
            {
                entity = new ProgressEntity
                {
                    ProgressId = progress.ProgressId,
                    GuideId = progress.GuideId,
                    UserId = progress.UserId,
                    StartedDate = progress.StartedDate
                };
                _context.Progress.Add(entity);
            }

            UpdateEntityFromModel(entity, progress);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Saved progress for guide {GuideId}", progress.GuideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving progress {ProgressId}", progress.ProgressId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<List<GuideProgress>> GetAllProgressAsync(string userId)
    {
        try
        {
            var entities = await _context.Progress
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.LastUpdated)
                .ToListAsync();

            return entities.Select(MapEntityToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all progress for user {UserId}", userId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DeleteProgressAsync(string progressId)
    {
        try
        {
            var entity = await _context.Progress.FindAsync(progressId);
            if (entity != null)
            {
                _context.Progress.Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted progress {ProgressId}", progressId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting progress {ProgressId}", progressId);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task UpdateStepProgressAsync(string progressId, string stepId, StepStatus status)
    {
        try
        {
            var entity = await _context.Progress.FindAsync(progressId);
            if (entity == null)
                throw new InvalidOperationException($"Progress not found: {progressId}");

            var stepProgress = JsonSerializer.Deserialize<Dictionary<string, StepStatus>>(
                entity.StepProgress, JsonOptions) ?? new Dictionary<string, StepStatus>();

            stepProgress[stepId] = status;

            entity.StepProgress = JsonSerializer.Serialize(stepProgress, JsonOptions);
            entity.CurrentStepId = stepId;
            entity.LastUpdated = DateTime.UtcNow;

            // Calculate percent complete
            var totalSteps = stepProgress.Count;
            var completedSteps = stepProgress.Count(kvp => kvp.Value == StepStatus.Completed);
            entity.PercentComplete = totalSteps > 0 ? (completedSteps * 100) / totalSteps : 0;

            // Check if all completed
            if (completedSteps == totalSteps && totalSteps > 0)
            {
                entity.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated step progress: {StepId} -> {Status}", stepId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating step progress for {ProgressId}", progressId);
            throw;
        }
    }

    // Private helper methods

    private GuideProgress MapEntityToModel(ProgressEntity entity)
    {
        var stepProgress = new Dictionary<string, StepStatus>();

        try
        {
            stepProgress = JsonSerializer.Deserialize<Dictionary<string, StepStatus>>(
                entity.StepProgress, JsonOptions) ?? new Dictionary<string, StepStatus>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize step progress for {ProgressId}", entity.ProgressId);
        }

        return new GuideProgress
        {
            ProgressId = entity.ProgressId,
            GuideId = entity.GuideId,
            UserId = entity.UserId,
            CurrentStepId = entity.CurrentStepId,
            StepProgress = stepProgress,
            StartedDate = entity.StartedDate,
            LastUpdated = entity.LastUpdated,
            CompletedDate = entity.CompletedDate,
            Notes = entity.Notes
        };
    }

    private void UpdateEntityFromModel(ProgressEntity entity, GuideProgress model)
    {
        entity.CurrentStepId = model.CurrentStepId;
        entity.StepProgress = JsonSerializer.Serialize(model.StepProgress, JsonOptions);
        entity.LastUpdated = DateTime.UtcNow;
        entity.CompletedDate = model.CompletedDate;
        entity.Notes = model.Notes;

        // Calculate percent complete
        var totalSteps = model.StepProgress.Count;
        var completedSteps = model.StepProgress.Count(kvp => kvp.Value == StepStatus.Completed);
        entity.PercentComplete = totalSteps > 0 ? (completedSteps * 100) / totalSteps : 0;
    }
}
