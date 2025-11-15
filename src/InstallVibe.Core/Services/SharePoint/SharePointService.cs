using InstallVibe.Core.Models.Activation;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.SharePoint;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Core.Services.Data;
using InstallVibe.Infrastructure.Configuration;
using InstallVibe.Infrastructure.Security.Cryptography;
using InstallVibe.Infrastructure.Security.Graph;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace InstallVibe.Core.Services.SharePoint;

/// <summary>
/// Implements SharePoint Online integration using Microsoft Graph API.
/// </summary>
public class SharePointService : ISharePointService
{
    private readonly IGraphClientFactory _graphClientFactory;
    private readonly SharePointConfiguration _configuration;
    private readonly IGuideService _guideService;
    private readonly ICacheService _cacheService;
    private readonly IHashService _hashService;
    private readonly ILogger<SharePointService> _logger;

    private string? _siteId;
    private string? _guideDriveId;
    private string? _mediaDriveId;
    private string? _guideIndexListId;

    private bool _isOnline = true;
    private DateTime _lastOnlineCheck = DateTime.MinValue;
    private int _consecutiveFailures = 0;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SharePointService(
        IGraphClientFactory graphClientFactory,
        SharePointConfiguration configuration,
        IGuideService guideService,
        ICacheService cacheService,
        IHashService hashService,
        ILogger<SharePointService> logger)
    {
        _graphClientFactory = graphClientFactory ?? throw new ArgumentNullException(nameof(graphClientFactory));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate configuration
        _configuration.Validate();
    }

    // ========== Guide Index ==========

    public async Task<List<GuideIndexEntry>> GetGuideIndexAsync(LicenseType? filterByLicense = null)
    {
        try
        {
            // Check if online
            if (!await IsOnlineAsync())
            {
                _logger.LogWarning("SharePoint is offline, returning cached guide list");
                return await GetCachedGuideIndexAsync(filterByLicense);
            }

            await EnsureSiteInitializedAsync();

            var client = _graphClientFactory.CreateClient();

            // Build filter
            var filter = "fields/Published eq true";
            if (filterByLicense.HasValue)
            {
                var licenseStr = filterByLicense.Value.ToString();
                filter += $" and fields/RequiredLicense eq '{licenseStr}'";
            }

            // Fetch list items
            var items = await client.Sites[_siteId].Lists[_guideIndexListId].Items
                .GetAsync(config =>
                {
                    config.QueryParameters.Expand = new[] { "fields" };
                    config.QueryParameters.Filter = filter;
                    config.QueryParameters.Orderby = new[] { "fields/Category", "fields/Title" };
                });

            var entries = new List<GuideIndexEntry>();

            if (items?.Value != null)
            {
                foreach (var item in items.Value)
                {
                    try
                    {
                        var entry = MapListItemToGuideIndexEntry(item);
                        entries.Add(entry);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to map list item to guide index entry");
                    }
                }
            }

            _logger.LogInformation("Fetched {Count} guide index entries from SharePoint", entries.Count);

            // Mark as online
            _isOnline = true;
            _consecutiveFailures = 0;

            return entries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching guide index from SharePoint");
            _consecutiveFailures++;
            _isOnline = false;

            // Fallback to cached data
            return await GetCachedGuideIndexAsync(filterByLicense);
        }
    }

    public async Task<GuideIndexEntry?> GetGuideMetadataAsync(string guideId)
    {
        try
        {
            if (!await IsOnlineAsync())
            {
                _logger.LogWarning("SharePoint is offline, checking local cache for guide {GuideId}", guideId);
                return await GetCachedGuideMetadataAsync(guideId);
            }

            await EnsureSiteInitializedAsync();

            var client = _graphClientFactory.CreateClient();

            var items = await client.Sites[_siteId].Lists[_guideIndexListId].Items
                .GetAsync(config =>
                {
                    config.QueryParameters.Expand = new[] { "fields" };
                    config.QueryParameters.Filter = $"fields/GuideId eq '{guideId}'";
                });

            if (items?.Value?.Count > 0)
            {
                var entry = MapListItemToGuideIndexEntry(items.Value[0]);
                _logger.LogInformation("Fetched metadata for guide {GuideId}", guideId);
                return entry;
            }

            _logger.LogWarning("Guide {GuideId} not found in SharePoint index", guideId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching guide metadata for {GuideId}", guideId);
            _isOnline = false;
            return await GetCachedGuideMetadataAsync(guideId);
        }
    }

    // ========== Guide Content ==========

    public async Task<byte[]> DownloadGuideJsonAsync(string guideId)
    {
        try
        {
            if (!await IsOnlineAsync())
            {
                _logger.LogWarning("SharePoint is offline, reading from local cache");
                return await _cacheService.ReadCachedFileAsync("guide", guideId);
            }

            await EnsureSiteInitializedAsync();

            var client = _graphClientFactory.CreateClient();

            // Get file content
            var path = $"/Guides/{guideId}/guide.json";
            var stream = await client.Drives[_guideDriveId].Root
                .ItemWithPath(path)
                .Content
                .GetAsync();

            if (stream == null)
            {
                throw new FileNotFoundException($"Guide file not found: {path}");
            }

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var data = memoryStream.ToArray();

            _logger.LogInformation("Downloaded guide.json for {GuideId} ({Size} bytes)", guideId, data.Length);

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading guide.json for {GuideId}", guideId);
            _isOnline = false;

            // Fallback to cache
            return await _cacheService.ReadCachedFileAsync("guide", guideId);
        }
    }

    public async Task<Guide?> GetGuideAsync(string guideId)
    {
        try
        {
            var data = await DownloadGuideJsonAsync(guideId);
            var json = Encoding.UTF8.GetString(data);
            var guide = JsonSerializer.Deserialize<Guide>(json, JsonOptions);

            return guide;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing guide {GuideId}", guideId);
            return null;
        }
    }

    public async Task<bool> UploadGuideAsync(Guide guide)
    {
        try
        {
            if (!await IsOnlineAsync())
            {
                _logger.LogWarning("SharePoint is offline, cannot upload guide");
                return false;
            }

            await EnsureSiteInitializedAsync();

            var client = _graphClientFactory.CreateClient();

            // Serialize guide
            var json = JsonSerializer.Serialize(guide, JsonOptions);
            var data = Encoding.UTF8.GetBytes(json);
            var checksum = Convert.ToHexString(_hashService.ComputeSha256(data)).ToLowerInvariant();

            // Upload file
            var folderPath = $"/Guides/{guide.GuideId}";
            var filePath = $"{folderPath}/guide.json";

            using var stream = new MemoryStream(data);

            var uploadedFile = await client.Drives[_guideDriveId].Root
                .ItemWithPath(filePath)
                .Content
                .PutAsync(stream);

            _logger.LogInformation("Uploaded guide.json for {GuideId}", guide.GuideId);

            // Update GuideIndex list
            await UpdateGuideIndexEntryAsync(guide, checksum, data.Length);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading guide {GuideId}", guide.GuideId);
            return false;
        }
    }

    // ========== Sync Operations ==========

    public async Task<SyncResult> SyncUpdatedGuidesAsync(DateTime? since = null, IProgress<SyncProgress>? progress = null)
    {
        var result = new SyncResult
        {
            StartTime = DateTime.UtcNow
        };

        try
        {
            if (!await IsOnlineAsync())
            {
                result.Success = false;
                result.ErrorMessage = "SharePoint is offline";
                result.WasOffline = true;
                result.EndTime = DateTime.UtcNow;
                return result;
            }

            // Get updated guides from index
            var allGuides = await GetGuideIndexAsync();

            // Filter by date if specified
            var guidesToSync = since.HasValue
                ? allGuides.Where(g => g.LastModified > since.Value).ToList()
                : allGuides;

            result.TotalGuidesChecked = guidesToSync.Count;

            _logger.LogInformation("Syncing {Count} guides from SharePoint", guidesToSync.Count);

            // Download each guide
            for (int i = 0; i < guidesToSync.Count; i++)
            {
                var guideEntry = guidesToSync[i];

                try
                {
                    // Report progress
                    progress?.Report(new SyncProgress
                    {
                        CurrentOperation = $"Downloading guide: {guideEntry.Title}",
                        ItemsProcessed = i,
                        TotalItems = guidesToSync.Count
                    });

                    // Check if already cached and up-to-date
                    var existingGuide = await _guideService.GetGuideAsync(guideEntry.GuideId);
                    if (existingGuide != null && existingGuide.LastModified >= guideEntry.LastModified)
                    {
                        _logger.LogDebug("Guide {GuideId} is up-to-date, skipping", guideEntry.GuideId);
                        continue;
                    }

                    // Download guide
                    var data = await DownloadGuideJsonAsync(guideEntry.GuideId);

                    // Verify checksum
                    var computedChecksum = Convert.ToHexString(_hashService.ComputeSha256(data)).ToLowerInvariant();
                    if (computedChecksum != guideEntry.Checksum.ToLowerInvariant())
                    {
                        _logger.LogWarning("Checksum mismatch for guide {GuideId}", guideEntry.GuideId);
                        result.Errors.Add(new SyncError
                        {
                            EntityId = guideEntry.GuideId,
                            EntityType = "Guide",
                            Message = "Checksum mismatch",
                            Timestamp = DateTime.UtcNow
                        });
                        continue;
                    }

                    // Cache the file
                    await _cacheService.CacheFileAsync("guide", guideEntry.GuideId, data, guideEntry.Checksum);

                    // Deserialize and save to database
                    var json = Encoding.UTF8.GetString(data);
                    var guide = JsonSerializer.Deserialize<Guide>(json, JsonOptions);
                    if (guide != null)
                    {
                        await _guideService.SaveGuideAsync(guide);
                    }

                    result.BytesDownloaded += data.Length;

                    if (existingGuide == null)
                        result.GuidesDownloaded++;
                    else
                        result.GuidesUpdated++;

                    _logger.LogInformation("Synced guide {GuideId} ({Size} bytes)", guideEntry.GuideId, data.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing guide {GuideId}", guideEntry.GuideId);
                    result.Errors.Add(new SyncError
                    {
                        EntityId = guideEntry.GuideId,
                        EntityType = "Guide",
                        Message = ex.Message,
                        ExceptionType = ex.GetType().Name,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            result.Success = result.Errors.Count == 0;
            result.EndTime = DateTime.UtcNow;

            _logger.LogInformation(
                "Sync completed: {Downloaded} downloaded, {Updated} updated, {Errors} errors in {Duration}ms",
                result.GuidesDownloaded,
                result.GuidesUpdated,
                result.Errors.Count,
                result.Duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sync operation");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.EndTime = DateTime.UtcNow;
            return result;
        }
    }

    // ========== Media Operations ==========

    public async Task<SharePointMedia?> GetMediaMetadataAsync(string mediaId)
    {
        try
        {
            if (!await IsOnlineAsync())
            {
                _logger.LogWarning("SharePoint is offline, cannot fetch media metadata");
                return null;
            }

            await EnsureSiteInitializedAsync();

            var client = _graphClientFactory.CreateClient();

            // Search for media file by MediaId column
            var items = await client.Drives[_mediaDriveId].Items
                .GetAsync(config =>
                {
                    config.QueryParameters.Filter = $"fields/MediaId eq '{mediaId}'";
                    config.QueryParameters.Expand = new[] { "fields" };
                });

            if (items?.Value?.Count > 0)
            {
                var item = items.Value[0];
                var media = MapDriveItemToMedia(item);
                return media;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching media metadata for {MediaId}", mediaId);
            return null;
        }
    }

    public async Task<byte[]> DownloadMediaAsync(string mediaId)
    {
        try
        {
            if (!await IsOnlineAsync())
            {
                _logger.LogWarning("SharePoint is offline, reading from local cache");
                return await _cacheService.ReadCachedFileAsync("media", mediaId);
            }

            // Get metadata first to find the file path
            var metadata = await GetMediaMetadataAsync(mediaId);
            if (metadata == null)
            {
                throw new FileNotFoundException($"Media not found: {mediaId}");
            }

            var client = _graphClientFactory.CreateClient();

            // Download using item ID
            var stream = await client.Drives[_mediaDriveId].Items[metadata.SharePointItemId]
                .Content
                .GetAsync();

            if (stream == null)
            {
                throw new FileNotFoundException($"Media file content not found: {mediaId}");
            }

            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var data = memoryStream.ToArray();

            _logger.LogInformation("Downloaded media {MediaId} ({Size} bytes)", mediaId, data.Length);

            // Cache the file
            if (!string.IsNullOrEmpty(metadata.Checksum))
            {
                await _cacheService.CacheFileAsync("media", mediaId, data, metadata.Checksum);
            }

            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading media {MediaId}", mediaId);
            _isOnline = false;

            // Fallback to cache
            return await _cacheService.ReadCachedFileAsync("media", mediaId);
        }
    }

    public async Task<string> UploadMediaAsync(string mediaId, Stream content, MediaType mediaType, string fileExtension)
    {
        try
        {
            if (!await IsOnlineAsync())
            {
                throw new InvalidOperationException("SharePoint is offline, cannot upload media");
            }

            await EnsureSiteInitializedAsync();

            var client = _graphClientFactory.CreateClient();

            // Determine subfolder based on media type
            var subfolder = mediaType switch
            {
                MediaType.Image => "Images",
                MediaType.Video => "Videos",
                MediaType.Document => "Documents",
                _ => "Other"
            };

            var fileName = $"{mediaId}.{fileExtension}";
            var filePath = $"/Media/{subfolder}/{fileName}";

            // Check file size for upload strategy
            var fileSize = content.Length;
            DriveItem? uploadedItem;

            if (fileSize < 4 * 1024 * 1024) // < 4MB: Simple upload
            {
                uploadedItem = await client.Drives[_mediaDriveId].Root
                    .ItemWithPath(filePath)
                    .Content
                    .PutAsync(content);
            }
            else // >= 4MB: Resumable upload session
            {
                uploadedItem = await UploadLargeFileAsync(filePath, content);
            }

            if (uploadedItem?.Id == null)
            {
                throw new InvalidOperationException("Upload failed: No item ID returned");
            }

            _logger.LogInformation("Uploaded media {MediaId} to {Path}", mediaId, filePath);

            // Update metadata
            await UpdateMediaMetadataAsync(uploadedItem.Id, mediaId, mediaType, fileExtension);

            return uploadedItem.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media {MediaId}", mediaId);
            throw;
        }
    }

    // ========== Product Key Validation ==========

    public async Task<ProductKeyValidationResult> ValidateProductKeyOnlineAsync(string productKey)
    {
        var result = new ProductKeyValidationResult
        {
            WasOnlineValidation = false
        };

        try
        {
            // Check if online validation is enabled
            if (string.IsNullOrEmpty(_configuration.ProductKeysListId))
            {
                result.ErrorMessage = "Online product key validation is not configured";
                return result;
            }

            if (!await IsOnlineAsync())
            {
                result.ErrorMessage = "SharePoint is offline";
                return result;
            }

            await EnsureSiteInitializedAsync();

            // Compute key hash
            var keyHash = _hashService.ComputeSha256(productKey);

            var client = _graphClientFactory.CreateClient();

            // Query ProductKeys list
            var items = await client.Sites[_siteId].Lists[_configuration.ProductKeysListId].Items
                .GetAsync(config =>
                {
                    config.QueryParameters.Expand = new[] { "fields" };
                    config.QueryParameters.Filter = $"fields/ProductKeyHash eq '{keyHash}' and fields/IsRevoked eq false";
                });

            if (items?.Value?.Count > 0)
            {
                var item = items.Value[0];

                // Safely extract fields dictionary
                if (!item.AdditionalData.TryGetValue("fields", out var fieldsObj) ||
                    fieldsObj is not IDictionary<string, object> fields)
                {
                    _logger.LogWarning("Product key validation failed: Unable to extract fields from SharePoint item");
                    return result;
                }

                result.IsValid = true;
                result.WasOnlineValidation = true;

                // Parse LicenseType with error handling
                try
                {
                    result.LicenseType = fields.TryGetValue("LicenseType", out var licenseTypeValue)
                        ? Enum.Parse<LicenseType>(licenseTypeValue?.ToString() ?? "Tech", ignoreCase: true)
                        : LicenseType.Tech;
                }
                catch (ArgumentException ex)
                {
                    _logger.LogWarning(ex, "Invalid license type in SharePoint, defaulting to Tech");
                    result.LicenseType = LicenseType.Tech;
                }

                result.CustomerId = fields.TryGetValue("CustomerId", out var customerIdValue)
                    ? customerIdValue?.ToString()
                    : null;

                // Parse ExpirationDate with error handling
                if (fields.TryGetValue("ExpirationDate", out var expirationValue) && expirationValue != null)
                {
                    try
                    {
                        result.ExpirationDate = DateTime.Parse(expirationValue.ToString()!);
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogWarning(ex, "Invalid expiration date format in SharePoint: {Value}", expirationValue);
                    }
                }

                // Parse activation counts with error handling
                try
                {
                    result.ActivationCount = fields.TryGetValue("ActivationCount", out var activationCountValue)
                        ? int.Parse(activationCountValue?.ToString() ?? "0")
                        : 0;
                    result.MaxActivations = fields.TryGetValue("MaxActivations", out var maxActivationsValue)
                        ? int.Parse(maxActivationsValue?.ToString() ?? "5")
                        : 5;
                }
                catch (FormatException ex)
                {
                    _logger.LogWarning(ex, "Invalid activation count format in SharePoint, using defaults");
                    result.ActivationCount = 0;
                    result.MaxActivations = 5;
                }

                // Increment activation count
                if (result.ActivationCount < result.MaxActivations)
                {
                    await IncrementActivationCountAsync(item.Id!, result.ActivationCount + 1);
                }

                _logger.LogInformation("Product key validated online: {LicenseType}", result.LicenseType);
            }
            else
            {
                result.IsValid = false;
                result.ErrorMessage = "Product key not found or revoked";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating product key online");
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    // ========== Health & Status ==========

    public async Task<bool> IsOnlineAsync()
    {
        // Use cached status if checked recently (within 30 seconds)
        if (DateTime.UtcNow - _lastOnlineCheck < TimeSpan.FromSeconds(30))
        {
            return _isOnline;
        }

        try
        {
            var client = _graphClientFactory.CreateClient();

            // Quick connectivity check: Get site info
            var stopwatch = Stopwatch.StartNew();
            var siteUri = new Uri(_configuration.SiteUrl);
            var hostname = siteUri.Host;
            var serverRelativeUrl = siteUri.PathAndQuery.Trim('/');
            var site = await client.Sites[$"{hostname}:/{serverRelativeUrl}"]
                .GetAsync();
            stopwatch.Stop();

            _isOnline = site != null;
            _lastOnlineCheck = DateTime.UtcNow;

            if (_isOnline)
            {
                _consecutiveFailures = 0;
                _logger.LogDebug("SharePoint connectivity check successful ({Ms}ms)", stopwatch.ElapsedMilliseconds);
            }

            return _isOnline;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SharePoint connectivity check failed");
            _isOnline = false;
            _lastOnlineCheck = DateTime.UtcNow;
            _consecutiveFailures++;
            return false;
        }
    }

    public async Task<SharePointHealthStatus> GetHealthStatusAsync()
    {
        var status = new SharePointHealthStatus
        {
            LastConnectionAttempt = DateTime.UtcNow,
            SiteUrl = _configuration.SiteUrl,
            ConsecutiveFailures = _consecutiveFailures
        };

        try
        {
            // Check certificate
            status.IsCertificateValid = await _graphClientFactory.ValidateCertificateAsync();
            status.CertificateExpirationDate = _graphClientFactory.GetCertificateExpirationDate();

            // Check connectivity
            var stopwatch = Stopwatch.StartNew();
            status.IsOnline = await IsOnlineAsync();
            stopwatch.Stop();

            status.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;

            if (status.IsOnline)
            {
                status.LastSuccessfulConnection = DateTime.UtcNow;
                status.IsAuthenticated = true;

                // Check permissions (simplified - would need to query actual permissions)
                status.GrantedPermissions.Add("Sites.Read.All");
                status.CanWrite = true; // Would check actual permissions here
            }

            return status;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting health status");
            status.LastError = ex.Message;
            status.IsOnline = false;
            return status;
        }
    }

    // ========== Private Helper Methods ==========

    private async Task EnsureSiteInitializedAsync()
    {
        if (!string.IsNullOrEmpty(_siteId) && !string.IsNullOrEmpty(_guideDriveId))
            return;

        var client = _graphClientFactory.CreateClient();

        // Get site ID
        var siteUri = new Uri(_configuration.SiteUrl);
        var hostname = siteUri.Host;
        var serverRelativeUrl = siteUri.PathAndQuery.Trim('/');

        var site = await client.Sites[$"{hostname}:/{serverRelativeUrl}"].GetAsync();

        if (site?.Id == null)
        {
            throw new InvalidOperationException($"Failed to resolve SharePoint site: {_configuration.SiteUrl}");
        }

        _siteId = site.Id;

        // Get drives
        var drives = await client.Sites[_siteId].Drives.GetAsync();

        var guideDrive = drives?.Value?.FirstOrDefault(d => d.Name == _configuration.GuideLibrary);
        var mediaDrive = drives?.Value?.FirstOrDefault(d => d.Name == _configuration.MediaLibrary);

        if (guideDrive == null || mediaDrive == null)
        {
            throw new InvalidOperationException("Required document libraries not found");
        }

        _guideDriveId = guideDrive.Id;
        _mediaDriveId = mediaDrive.Id;

        // Get GuideIndex list ID
        var lists = await client.Sites[_siteId].Lists.GetAsync();
        var guideIndexList = lists?.Value?.FirstOrDefault(l => l.DisplayName == _configuration.GuideIndexList);

        if (guideIndexList == null)
        {
            throw new InvalidOperationException($"GuideIndex list not found: {_configuration.GuideIndexList}");
        }

        _guideIndexListId = guideIndexList.Id;

        _logger.LogInformation("SharePoint site initialized: {SiteId}", _siteId);
    }

    private GuideIndexEntry MapListItemToGuideIndexEntry(ListItem item)
    {
        // Safely extract fields dictionary
        if (!item.AdditionalData.TryGetValue("fields", out var fieldsObj) ||
            fieldsObj is not IDictionary<string, object> fields)
        {
            throw new InvalidOperationException("Unable to extract fields from SharePoint list item");
        }

        var entry = new GuideIndexEntry
        {
            GuideId = fields.TryGetValue("GuideId", out var guideId) ? guideId?.ToString() ?? string.Empty : string.Empty,
            Title = fields.TryGetValue("Title", out var title) ? title?.ToString() ?? string.Empty : string.Empty,
            Version = fields.TryGetValue("Version", out var version) ? version?.ToString() ?? string.Empty : string.Empty,
            Category = fields.TryGetValue("Category", out var category) ? category?.ToString() ?? string.Empty : string.Empty,
            Description = fields.TryGetValue("Description", out var description) ? description?.ToString() : null,
            Author = fields.TryGetValue("Author", out var author) ? author?.ToString() : null,
            ApprovedBy = fields.TryGetValue("ApprovedBy", out var approvedBy) ? approvedBy?.ToString() : null,
            Checksum = fields.TryGetValue("Checksum", out var checksum) ? checksum?.ToString() ?? string.Empty : string.Empty,
            FolderPath = fields.TryGetValue("FolderPath", out var folderPath) ? folderPath?.ToString() ?? string.Empty : string.Empty,
            SyncPriority = fields.TryGetValue("SyncPriority", out var syncPriority) ? syncPriority?.ToString() : null,
            MinClientVersion = fields.TryGetValue("MinClientVersion", out var minClientVersion) ? minClientVersion?.ToString() : null
        };

        // Parse RequiredLicense with error handling
        try
        {
            entry.RequiredLicense = fields.TryGetValue("RequiredLicense", out var requiredLicense)
                ? Enum.Parse<LicenseType>(requiredLicense?.ToString() ?? "Tech", ignoreCase: true)
                : LicenseType.Tech;
        }
        catch (ArgumentException)
        {
            entry.RequiredLicense = LicenseType.Tech;
        }

        // Parse Published with error handling
        entry.Published = fields.TryGetValue("Published", out var published) &&
                          bool.TryParse(published?.ToString(), out var publishedValue) &&
                          publishedValue;

        // Parse LastModified with error handling
        if (fields.TryGetValue("LastModified", out var lastModified) &&
            DateTime.TryParse(lastModified?.ToString(), out var lastModifiedValue))
        {
            entry.LastModified = lastModifiedValue;
        }
        else
        {
            entry.LastModified = DateTime.UtcNow;
        }

        // Parse integer fields with error handling
        entry.StepCount = fields.TryGetValue("StepCount", out var stepCount) &&
                          int.TryParse(stepCount?.ToString(), out var stepCountValue)
                          ? stepCountValue : 0;

        entry.EstimatedMinutes = fields.TryGetValue("EstimatedMinutes", out var estimatedMinutes) &&
                                 int.TryParse(estimatedMinutes?.ToString(), out var estimatedMinutesValue)
                                 ? estimatedMinutesValue : null;

        entry.FileSize = fields.TryGetValue("FileSize", out var fileSize) &&
                         long.TryParse(fileSize?.ToString(), out var fileSizeValue)
                         ? fileSizeValue : 0;

        entry.MediaCount = fields.TryGetValue("MediaCount", out var mediaCount) &&
                           int.TryParse(mediaCount?.ToString(), out var mediaCountValue)
                           ? mediaCountValue : 0;

        // Parse tags
        if (fields.TryGetValue("Tags", out var tags))
        {
            var tagsStr = tags?.ToString();
            if (!string.IsNullOrEmpty(tagsStr))
            {
                entry.Tags = tagsStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }
        }

        return entry;
    }

    private SharePointMedia MapDriveItemToMedia(DriveItem item)
    {
        // Safely extract fields dictionary
        IDictionary<string, object> fields;
        if (item.AdditionalData.TryGetValue("fields", out var fieldsObj) &&
            fieldsObj is IDictionary<string, object> fieldsDict)
        {
            fields = fieldsDict;
        }
        else
        {
            fields = new Dictionary<string, object>();
        }

        var media = new SharePointMedia
        {
            MediaId = fields.TryGetValue("MediaId", out var mediaId) ? mediaId?.ToString() ?? string.Empty : string.Empty,
            FileFormat = fields.TryGetValue("FileFormat", out var fileFormat) ? fileFormat?.ToString() ?? string.Empty : string.Empty,
            FileSizeBytes = item.Size ?? 0,
            Checksum = fields.TryGetValue("Checksum", out var checksum) ? checksum?.ToString() ?? string.Empty : string.Empty,
            UploadedBy = fields.TryGetValue("UploadedBy", out var uploadedBy) ? uploadedBy?.ToString() : null,
            UploadDate = item.CreatedDateTime?.DateTime ?? DateTime.UtcNow,
            DownloadUrl = item.AdditionalData.TryGetValue("@microsoft.graph.downloadUrl", out var downloadUrl)
                ? downloadUrl?.ToString()
                : null,
            SharePointItemId = item.Id ?? throw new InvalidOperationException("DriveItem missing required Id")
        };

        // Parse MediaType with error handling
        try
        {
            media.MediaType = fields.TryGetValue("MediaType", out var mediaType)
                ? Enum.Parse<MediaType>(mediaType?.ToString() ?? "Image", ignoreCase: true)
                : MediaType.Image;
        }
        catch (ArgumentException)
        {
            media.MediaType = MediaType.Image;
        }

        // Parse referenced guides
        if (fields.TryGetValue("ReferencedByGuides", out var referencedByGuides))
        {
            var guidesStr = referencedByGuides?.ToString();
            if (!string.IsNullOrEmpty(guidesStr))
            {
                media.ReferencedByGuides = guidesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
            }
        }

        return media;
    }

    private async Task<DriveItem> UploadLargeFileAsync(string filePath, Stream content)
    {
        var client = _graphClientFactory.CreateClient();

        // Create upload session
        var uploadSessionRequestBody = new Microsoft.Graph.Drives.Item.Items.Item.CreateUploadSession.CreateUploadSessionPostRequestBody
        {
            Item = new DriveItemUploadableProperties
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "@microsoft.graph.conflictBehavior", "replace" }
                }
            }
        };

        var uploadSession = await client.Drives[_mediaDriveId].Root
            .ItemWithPath(filePath)
            .CreateUploadSession
            .PostAsync(uploadSessionRequestBody);

        if (uploadSession?.UploadUrl == null)
        {
            throw new InvalidOperationException("Failed to create upload session");
        }

        // Upload in chunks
        var chunkSize = _configuration.UploadChunkSizeMB * 1024 * 1024;
        var buffer = new byte[chunkSize];
        long position = 0;
        var totalSize = content.Length;

        using var httpClient = new HttpClient();

        while (position < totalSize)
        {
            var bytesRead = await content.ReadAsync(buffer.AsMemory(0, (int)Math.Min(chunkSize, totalSize - position)));
            
            using var chunkContent = new ByteArrayContent(buffer, 0, bytesRead);
            chunkContent.Headers.ContentRange = new System.Net.Http.Headers.ContentRangeHeaderValue(position, position + bytesRead - 1, totalSize);

            var response = await httpClient.PutAsync(uploadSession.UploadUrl, chunkContent);
            response.EnsureSuccessStatusCode();

            position += bytesRead;

            _logger.LogDebug("Uploaded chunk: {Position}/{Total} bytes", position, totalSize);
        }

        // Get final item
        var finalResponse = await httpClient.GetAsync(uploadSession.UploadUrl);
        var jsonResponse = await finalResponse.Content.ReadAsStringAsync();
        var driveItem = JsonSerializer.Deserialize<DriveItem>(jsonResponse);

        return driveItem ?? throw new InvalidOperationException("Failed to retrieve uploaded item");
    }

    private async Task UpdateGuideIndexEntryAsync(Guide guide, string checksum, long fileSize)
    {
        var client = _graphClientFactory.CreateClient();

        var fields = new FieldValueSet
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "GuideId", guide.GuideId },
                { "Title", guide.Title },
                { "Version", guide.Version },
                { "Category", guide.Category },
                { "Description", guide.Description ?? string.Empty },
                { "RequiredLicense", guide.RequiredLicense.ToString() },
                { "Published", guide.IsPublished },
                { "LastModified", guide.LastModified.ToString("o") },
                { "StepCount", guide.Steps?.Count ?? 0 },
                { "Checksum", checksum },
                { "FileSize", fileSize },
                { "FolderPath", $"/Guides/{guide.GuideId}" },
                { "MediaCount", guide.Steps?.SelectMany(s => s.MediaReferences ?? new List<MediaReference>()).Count() ?? 0 }
            }
        };

        // Find existing item
        var items = await client.Sites[_siteId].Lists[_guideIndexListId].Items
            .GetAsync(config =>
            {
                config.QueryParameters.Filter = $"fields/GuideId eq '{guide.GuideId}'";
            });

        if (items?.Value?.Count > 0)
        {
            // Update existing
            await client.Sites[_siteId].Lists[_guideIndexListId].Items[items.Value[0].Id]
                .PatchAsync(new ListItem { Fields = fields });
        }
        else
        {
            // Create new
            await client.Sites[_siteId].Lists[_guideIndexListId].Items
                .PostAsync(new ListItem { Fields = fields });
        }
    }

    private async Task UpdateMediaMetadataAsync(string itemId, string mediaId, MediaType mediaType, string fileExtension)
    {
        var client = _graphClientFactory.CreateClient();

        var fields = new FieldValueSet
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "MediaId", mediaId },
                { "MediaType", mediaType.ToString() },
                { "FileFormat", fileExtension }
            }
        };

        await client.Drives[_mediaDriveId].Items[itemId].ListItem
            .PatchAsync(new ListItem { Fields = fields });
    }

    private async Task IncrementActivationCountAsync(string itemId, int newCount)
    {
        if (string.IsNullOrEmpty(_configuration.ProductKeysListId))
        {
            _logger.LogWarning("Cannot increment activation count: ProductKeysListId is not configured");
            return;
        }

        var client = _graphClientFactory.CreateClient();

        var fields = new FieldValueSet
        {
            AdditionalData = new Dictionary<string, object>
            {
                { "ActivationCount", newCount },
                { "LastActivatedDate", DateTime.UtcNow.ToString("o") }
            }
        };

        await client.Sites[_siteId].Lists[_configuration.ProductKeysListId].Items[itemId]
            .PatchAsync(new ListItem { Fields = fields });
    }

    private async Task<List<GuideIndexEntry>> GetCachedGuideIndexAsync(LicenseType? filterByLicense)
    {
        // Fallback: Return guides from local database
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
                IsCached = true,
                IsUpToDate = false // Unknown when offline
            })
            .ToList();
    }

    private async Task<GuideIndexEntry?> GetCachedGuideMetadataAsync(string guideId)
    {
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
            IsCached = true,
            IsUpToDate = false
        };
    }
}
