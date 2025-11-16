using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Implements tag management functionality.
/// </summary>
public class TagService : ITagService
{
    private readonly IGuideService _guideService;
    private readonly ILogger<TagService> _logger;

    public TagService(
        IGuideService guideService,
        ILogger<TagService> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<TagInfo>> GetAllTagsAsync()
    {
        try
        {
            var guides = await _guideService.GetAllGuidesAsync();

            var tags = guides
                .SelectMany(g => g.Tags)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .GroupBy(tag => tag, StringComparer.OrdinalIgnoreCase)
                .Select(group => new TagInfo
                {
                    Name = group.Key,
                    GuideCount = group.Count()
                })
                .OrderBy(t => t.Name)
                .ToList();

            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tags");
            return new List<TagInfo>();
        }
    }

    public async Task<int> GetTagUsageCountAsync(string tag)
    {
        try
        {
            var guides = await _guideService.GetAllGuidesAsync();
            return guides.Count(g => g.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tag usage count for {Tag}", tag);
            return 0;
        }
    }

    public Task<List<string>> SuggestTagsAsync(string guideTitle, string guideDescription)
    {
        // Simple keyword extraction for tag suggestions
        var suggestions = new List<string>();

        try
        {
            var text = $"{guideTitle} {guideDescription}".ToLower();

            // Common technical keywords to suggest as tags
            var keywords = new Dictionary<string, List<string>>
            {
                { "windows", new List<string> { "windows", "win", "microsoft" } },
                { "office", new List<string> { "office", "365", "microsoft 365", "m365" } },
                { "installation", new List<string> { "install", "setup", "deploy" } },
                { "configuration", new List<string> { "config", "configure", "settings" } },
                { "network", new List<string> { "network", "wifi", "ethernet", "vpn" } },
                { "hardware", new List<string> { "hardware", "device", "printer", "scanner" } },
                { "software", new List<string> { "software", "app", "application", "program" } },
                { "security", new List<string> { "security", "antivirus", "firewall", "encryption" } },
                { "troubleshooting", new List<string> { "troubleshoot", "fix", "repair", "diagnose" } },
                { "update", new List<string> { "update", "upgrade", "patch" } }
            };

            foreach (var (tag, patterns) in keywords)
            {
                if (patterns.Any(pattern => text.Contains(pattern)))
                {
                    suggestions.Add(tag);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suggesting tags");
        }

        return Task.FromResult(suggestions);
    }
}
