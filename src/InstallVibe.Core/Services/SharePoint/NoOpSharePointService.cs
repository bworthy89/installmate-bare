using InstallVibe.Core.Models.Activation;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.SharePoint;
using InstallVibe.Core.Services.Data;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.SharePoint;

/// <summary>
/// No-operation implementation of ISharePointService for local-only mode.
/// All SharePoint operations return minimal/empty results as the application
/// operates entirely from local storage with import/export functionality.
/// </summary>
public class NoOpSharePointService : ISharePointService
{
    private readonly IGuideService _guideService;
    private readonly ILogger<NoOpSharePointService> _logger;

    public NoOpSharePointService(
        IGuideService guideService,
        ILogger<NoOpSharePointService> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ========== Guide Index ==========

    public async Task<List<GuideIndexEntry>> GetGuideIndexAsync(LicenseType? filterByLicense = null)
    {
        _logger.LogDebug("NoOp: GetGuideIndexAsync - returning local guides as index");

        // Return guides from local database formatted as index entries
        var guides = await _guideService.GetAllGuidesAsync();

        return guides
            .Where(g => !filterByLicense.HasValue || g.RequiredLicense == filterByLicense.Value)
            .Select(g => new GuideIndexEntry
            {
                GuideId = g.GuideId,
                Title = g.Title,
                Version = g.Version,
                Category = g.Category,
                Description = g.Description,
                RequiredLicense = g.RequiredLicense,
                Published = g.IsPublished,
                LastModified = g.LastModified,
                StepCount = g.Steps?.Count ?? 0,
                Author = g.Author,
                EstimatedMinutes = g.EstimatedMinutes,
                Tags = g.Tags,
                Checksum = string.Empty, // Not used in local-only mode
                IsCached = true,
                IsUpToDate = true
            })
            .ToList();
    }

    public async Task<GuideIndexEntry?> GetGuideMetadataAsync(string guideId)
    {
        _logger.LogDebug("NoOp: GetGuideMetadataAsync - returning local guide metadata for {GuideId}", guideId);

        var guide = await _guideService.GetGuideAsync(guideId);
        if (guide == null)
            return null;

        return new GuideIndexEntry
        {
            GuideId = guide.GuideId,
            Title = guide.Title,
            Version = guide.Version,
            Category = guide.Category,
            Description = guide.Description,
            RequiredLicense = guide.RequiredLicense,
            Published = guide.IsPublished,
            LastModified = guide.LastModified,
            StepCount = guide.Steps?.Count ?? 0,
            Author = guide.Author,
            EstimatedMinutes = guide.EstimatedMinutes,
            Tags = guide.Tags,
            Checksum = string.Empty,
            IsCached = true,
            IsUpToDate = true
        };
    }

    // ========== Guide Content ==========

    public Task<byte[]> DownloadGuideJsonAsync(string guideId)
    {
        _logger.LogDebug("NoOp: DownloadGuideJsonAsync - not supported in local-only mode");
        throw new NotSupportedException("Download operations are not supported in local-only mode. Use import/export functionality.");
    }

    public async Task<Guide?> GetGuideAsync(string guideId)
    {
        _logger.LogDebug("NoOp: GetGuideAsync - delegating to local GuideService for {GuideId}", guideId);
        return await _guideService.GetGuideAsync(guideId);
    }

    public async Task<bool> UploadGuideAsync(Guide guide)
    {
        _logger.LogInformation("NoOp: UploadGuideAsync - guide already saved locally (local-only mode)");
        // In local-only mode, guide is already saved to database by calling code
        // No SharePoint upload needed
        return true;
    }

    public async Task<bool> DeleteGuideAsync(string guideId)
    {
        _logger.LogInformation("NoOp: DeleteGuideAsync - guide will be deleted locally only (local-only mode)");
        // In local-only mode, guide is already deleted from database by calling code
        // No SharePoint deletion needed
        return true;
    }

    // ========== Sync Operations ==========

    public Task<SyncResult> SyncUpdatedGuidesAsync(DateTime? since = null, IProgress<SyncProgress>? progress = null)
    {
        _logger.LogDebug("NoOp: SyncUpdatedGuidesAsync - no sync in local-only mode");

        return Task.FromResult(new SyncResult
        {
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow,
            Success = true,
            TotalGuidesChecked = 0,
            GuidesDownloaded = 0,
            GuidesUpdated = 0,
            BytesDownloaded = 0,
            WasOffline = false,
            ErrorMessage = "No sync performed in local-only mode",
            Errors = new List<SyncError>()
        });
    }

    // ========== Media Operations ==========

    public Task<SharePointMedia?> GetMediaMetadataAsync(string mediaId)
    {
        _logger.LogDebug("NoOp: GetMediaMetadataAsync - not supported in local-only mode");
        return Task.FromResult<SharePointMedia?>(null);
    }

    public Task<byte[]> DownloadMediaAsync(string mediaId)
    {
        _logger.LogDebug("NoOp: DownloadMediaAsync - not supported in local-only mode");
        throw new NotSupportedException("Download operations are not supported in local-only mode. Media is managed locally.");
    }

    public Task<string> UploadMediaAsync(string mediaId, Stream content, MediaType mediaType, string fileExtension)
    {
        _logger.LogDebug("NoOp: UploadMediaAsync - not supported in local-only mode");
        throw new NotSupportedException("Upload operations are not supported in local-only mode. Media is managed locally.");
    }

    // ========== Product Key Validation ==========

    public Task<ProductKeyValidationResult> ValidateProductKeyOnlineAsync(string productKey)
    {
        _logger.LogDebug("NoOp: ValidateProductKeyOnlineAsync - not supported in local-only mode");

        return Task.FromResult(new ProductKeyValidationResult
        {
            IsValid = false,
            WasOnlineValidation = false,
            ErrorMessage = "Online product key validation is not available in local-only mode"
        });
    }

    // ========== Health & Status ==========

    public Task<bool> IsOnlineAsync()
    {
        _logger.LogDebug("NoOp: IsOnlineAsync - always returns false in local-only mode");
        return Task.FromResult(false);
    }

    public Task<SharePointHealthStatus> GetHealthStatusAsync()
    {
        _logger.LogDebug("NoOp: GetHealthStatusAsync - returning offline status for local-only mode");

        return Task.FromResult(new SharePointHealthStatus
        {
            IsOnline = false,
            IsAuthenticated = false,
            IsCertificateValid = false,
            LastConnectionAttempt = DateTime.UtcNow,
            LastSuccessfulConnection = null,
            ConsecutiveFailures = 0,
            ResponseTimeMs = 0,
            SiteUrl = "Local-only mode (no SharePoint connection)",
            CanWrite = false,
            GrantedPermissions = new List<string>(),
            CertificateExpirationDate = null,
            LastError = "SharePoint integration disabled (local-only mode)"
        });
    }
}
