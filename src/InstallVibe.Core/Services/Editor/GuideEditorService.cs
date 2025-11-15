using InstallVibe.Core.Models.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace InstallVibe.Core.Services.Editor;

/// <summary>
/// Service for creating and editing installation guides.
/// </summary>
public class GuideEditorService : IGuideEditorService
{
    private readonly ILogger<GuideEditorService> _logger;
    private readonly string _draftsPath;

    public GuideEditorService(ILogger<GuideEditorService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Store drafts in local app data
        _draftsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InstallVibe", "Drafts");

        // Ensure drafts directory exists
        Directory.CreateDirectory(_draftsPath);
    }

    public Task<Guide> CreateNewGuideAsync()
    {
        _logger.LogInformation("Creating new guide");

        var guide = new Guide
        {
            Id = Guid.NewGuid().ToString(),
            Metadata = new GuideMetadata
            {
                Title = "New Guide",
                Description = string.Empty,
                Version = "1.0.0",
                Author = Environment.UserName,
                Category = string.Empty,
                Tags = new List<string>(),
                EstimatedDuration = TimeSpan.FromMinutes(30),
                DifficultyLevel = "Intermediate",
                CreatedDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                UsageCount = 0
            },
            Steps = new List<Step>(),
            Requirements = new Requirements
            {
                MinimumOSVersion = "Windows 10",
                RequiredSoftware = new List<string>(),
                RequiredPermissions = new List<string> { "Administrator" },
                RequiredFiles = new List<string>(),
                NetworkRequired = false,
                InternetRequired = false
            }
        };

        return Task.FromResult(guide);
    }

    public async Task<bool> SaveDraftAsync(Guide guide)
    {
        if (guide == null)
        {
            _logger.LogWarning("Attempted to save null guide");
            return false;
        }

        try
        {
            var filePath = Path.Combine(_draftsPath, $"{guide.Id}.json");

            // Update last modified
            guide.Metadata.LastModified = DateTime.UtcNow;

            var json = JsonSerializer.Serialize(guide, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(filePath, json);

            _logger.LogInformation("Saved draft for guide {GuideId}", guide.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save draft for guide {GuideId}", guide.Id);
            return false;
        }
    }

    public async Task<Guide?> LoadDraftAsync(string guideId)
    {
        try
        {
            var filePath = Path.Combine(_draftsPath, $"{guideId}.json");

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Draft not found for guide {GuideId}", guideId);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var guide = JsonSerializer.Deserialize<Guide>(json);

            _logger.LogInformation("Loaded draft for guide {GuideId}", guideId);
            return guide;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load draft for guide {GuideId}", guideId);
            return null;
        }
    }

    public async Task<IEnumerable<Guide>> GetAllDraftsAsync()
    {
        try
        {
            var draftFiles = Directory.GetFiles(_draftsPath, "*.json");
            var drafts = new List<Guide>();

            foreach (var filePath in draftFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var guide = JsonSerializer.Deserialize<Guide>(json);

                    if (guide != null)
                    {
                        drafts.Add(guide);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load draft from {FilePath}", filePath);
                }
            }

            _logger.LogInformation("Loaded {Count} drafts", drafts.Count);
            return drafts.OrderByDescending(d => d.Metadata.LastModified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load drafts");
            return Enumerable.Empty<Guide>();
        }
    }

    public async Task<bool> DeleteDraftAsync(string guideId)
    {
        try
        {
            var filePath = Path.Combine(_draftsPath, $"{guideId}.json");

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Deleted draft for guide {GuideId}", guideId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete draft for guide {GuideId}", guideId);
            return false;
        }
    }

    public Task<Step> AddStepAsync(Guide guide, int position)
    {
        if (guide == null)
            throw new ArgumentNullException(nameof(guide));

        var step = new Step
        {
            Id = Guid.NewGuid().ToString(),
            OrderIndex = position,
            Title = "New Step",
            Instructions = string.Empty,
            Media = new List<MediaReference>(),
            Checkpoints = new List<Checkpoint>(),
            Actions = new List<StepAction>(),
            Validation = new StepValidation
            {
                Type = "Manual",
                Required = false,
                ValidationScript = string.Empty,
                SuccessCriteria = string.Empty
            },
            EstimatedDuration = TimeSpan.FromMinutes(5)
        };

        // Insert at position
        if (position >= 0 && position <= guide.Steps.Count)
        {
            guide.Steps.Insert(position, step);
        }
        else
        {
            guide.Steps.Add(step);
        }

        // Reindex all steps
        ReindexSteps(guide);

        _logger.LogInformation("Added new step at position {Position} for guide {GuideId}", position, guide.Id);
        return Task.FromResult(step);
    }

    public Task<bool> UpdateStepAsync(Guide guide, Step step)
    {
        if (guide == null || step == null)
            return Task.FromResult(false);

        var existingStep = guide.Steps.FirstOrDefault(s => s.Id == step.Id);
        if (existingStep == null)
        {
            _logger.LogWarning("Step {StepId} not found in guide {GuideId}", step.Id, guide.Id);
            return Task.FromResult(false);
        }

        var index = guide.Steps.IndexOf(existingStep);
        guide.Steps[index] = step;

        _logger.LogInformation("Updated step {StepId} in guide {GuideId}", step.Id, guide.Id);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteStepAsync(Guide guide, string stepId)
    {
        if (guide == null || string.IsNullOrEmpty(stepId))
            return Task.FromResult(false);

        var step = guide.Steps.FirstOrDefault(s => s.Id == stepId);
        if (step == null)
        {
            _logger.LogWarning("Step {StepId} not found in guide {GuideId}", stepId, guide.Id);
            return Task.FromResult(false);
        }

        guide.Steps.Remove(step);
        ReindexSteps(guide);

        _logger.LogInformation("Deleted step {StepId} from guide {GuideId}", stepId, guide.Id);
        return Task.FromResult(true);
    }

    public Task<bool> ReorderStepsAsync(Guide guide, int oldIndex, int newIndex)
    {
        if (guide == null)
            return Task.FromResult(false);

        if (oldIndex < 0 || oldIndex >= guide.Steps.Count ||
            newIndex < 0 || newIndex >= guide.Steps.Count)
        {
            _logger.LogWarning("Invalid reorder indices: {OldIndex} to {NewIndex}", oldIndex, newIndex);
            return Task.FromResult(false);
        }

        var step = guide.Steps[oldIndex];
        guide.Steps.RemoveAt(oldIndex);
        guide.Steps.Insert(newIndex, step);

        ReindexSteps(guide);

        _logger.LogInformation("Reordered step from {OldIndex} to {NewIndex} in guide {GuideId}",
            oldIndex, newIndex, guide.Id);
        return Task.FromResult(true);
    }

    public async Task<Step> DuplicateStepAsync(Guide guide, Step step)
    {
        if (guide == null || step == null)
            throw new ArgumentNullException();

        var duplicatedStep = new Step
        {
            Id = Guid.NewGuid().ToString(),
            OrderIndex = step.OrderIndex + 1,
            Title = $"{step.Title} (Copy)",
            Instructions = step.Instructions,
            Media = new List<MediaReference>(step.Media),
            Checkpoints = step.Checkpoints.Select(c => new Checkpoint
            {
                Id = Guid.NewGuid().ToString(),
                Description = c.Description,
                IsRequired = c.IsRequired,
                IsCompleted = false
            }).ToList(),
            Actions = new List<StepAction>(step.Actions),
            Validation = new StepValidation
            {
                Type = step.Validation.Type,
                Required = step.Validation.Required,
                ValidationScript = step.Validation.ValidationScript,
                SuccessCriteria = step.Validation.SuccessCriteria
            },
            EstimatedDuration = step.EstimatedDuration
        };

        var insertIndex = guide.Steps.IndexOf(step) + 1;
        guide.Steps.Insert(insertIndex, duplicatedStep);

        ReindexSteps(guide);

        _logger.LogInformation("Duplicated step {StepId} in guide {GuideId}", step.Id, guide.Id);
        return duplicatedStep;
    }

    public string IncrementVersion(string currentVersion, bool isMajor = false)
    {
        try
        {
            var parts = currentVersion.Split('.');
            if (parts.Length != 3)
            {
                _logger.LogWarning("Invalid version format: {Version}", currentVersion);
                return "1.0.0";
            }

            var major = int.Parse(parts[0]);
            var minor = int.Parse(parts[1]);
            var patch = int.Parse(parts[2]);

            if (isMajor)
            {
                major++;
                minor = 0;
                patch = 0;
            }
            else
            {
                patch++;
            }

            var newVersion = $"{major}.{minor}.{patch}";
            _logger.LogInformation("Incremented version from {OldVersion} to {NewVersion}",
                currentVersion, newVersion);

            return newVersion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to increment version {Version}", currentVersion);
            return "1.0.0";
        }
    }

    private void ReindexSteps(Guide guide)
    {
        for (int i = 0; i < guide.Steps.Count; i++)
        {
            guide.Steps[i].OrderIndex = i;
        }
    }
}
