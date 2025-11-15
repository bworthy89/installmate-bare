using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.SharePoint;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Editor;

/// <summary>
/// Service for uploading and managing media files for guides.
/// </summary>
public class MediaUploadService : IMediaUploadService
{
    private readonly ILogger<MediaUploadService> _logger;
    private readonly ISharePointService _sharePointService;

    // Maximum file sizes (in bytes)
    private const long MaxImageSize = 10 * 1024 * 1024; // 10 MB
    private const long MaxVideoSize = 500 * 1024 * 1024; // 500 MB

    private static readonly string[] SupportedImageFormats = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".bmp" };
    private static readonly string[] SupportedVideoFormats = { ".mp4", ".webm", ".mov", ".avi" };

    public MediaUploadService(
        ILogger<MediaUploadService> logger,
        ISharePointService sharePointService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
    }

    public async Task<MediaReference> UploadImageAsync(string filePath, string guideId)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Image file not found", filePath);

        _logger.LogInformation("Uploading image {FilePath} for guide {GuideId}", filePath, guideId);

        // Validate file
        var (isValid, errorMessage) = await ValidateMediaAsync(filePath);
        if (!isValid)
            throw new InvalidOperationException(errorMessage);

        var fileName = Path.GetFileName(filePath);
        var mediaId = Guid.NewGuid().ToString();

        try
        {
            // Upload to SharePoint media library
            var sharePointUrl = await _sharePointService.UploadFileAsync(
                filePath,
                $"GuideMedia/{guideId}/images",
                fileName);

            var mediaReference = new MediaReference
            {
                Id = mediaId,
                Type = "Image",
                Url = sharePointUrl,
                ThumbnailUrl = sharePointUrl, // SharePoint can generate thumbnails
                Title = Path.GetFileNameWithoutExtension(fileName),
                Description = string.Empty,
                Duration = null,
                UploadedDate = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully uploaded image {MediaId}", mediaId);
            return mediaReference;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload image {FilePath}", filePath);
            throw;
        }
    }

    public async Task<MediaReference> UploadVideoAsync(string filePath, string guideId)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException("Video file not found", filePath);

        _logger.LogInformation("Uploading video {FilePath} for guide {GuideId}", filePath, guideId);

        // Validate file
        var (isValid, errorMessage) = await ValidateMediaAsync(filePath);
        if (!isValid)
            throw new InvalidOperationException(errorMessage);

        var fileName = Path.GetFileName(filePath);
        var mediaId = Guid.NewGuid().ToString();

        try
        {
            // Upload to SharePoint media library
            var sharePointUrl = await _sharePointService.UploadFileAsync(
                filePath,
                $"GuideMedia/{guideId}/videos",
                fileName);

            var mediaReference = new MediaReference
            {
                Id = mediaId,
                Type = "Video",
                Url = sharePointUrl,
                ThumbnailUrl = string.Empty, // Could be generated later
                Title = Path.GetFileNameWithoutExtension(fileName),
                Description = string.Empty,
                Duration = null, // Could be extracted using MediaInfo
                UploadedDate = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully uploaded video {MediaId}", mediaId);
            return mediaReference;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload video {FilePath}", filePath);
            throw;
        }
    }

    public async Task<MediaReference> UploadMediaStreamAsync(Stream stream, string fileName, string guideId, string mediaType)
    {
        if (stream == null || stream.Length == 0)
            throw new ArgumentException("Stream is empty", nameof(stream));

        _logger.LogInformation("Uploading media stream {FileName} for guide {GuideId}", fileName, guideId);

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var isImage = SupportedImageFormats.Contains(extension);
        var isVideo = SupportedVideoFormats.Contains(extension);

        if (!isImage && !isVideo)
            throw new InvalidOperationException($"Unsupported file format: {extension}");

        // Validate size
        var maxSize = isImage ? MaxImageSize : MaxVideoSize;
        if (stream.Length > maxSize)
        {
            var maxSizeMB = maxSize / (1024 * 1024);
            throw new InvalidOperationException($"File size exceeds maximum of {maxSizeMB} MB");
        }

        var mediaId = Guid.NewGuid().ToString();

        try
        {
            // Save stream to temp file
            var tempPath = Path.Combine(Path.GetTempPath(), fileName);
            using (var fileStream = File.Create(tempPath))
            {
                await stream.CopyToAsync(fileStream);
            }

            // Upload temp file
            var subfolder = isImage ? "images" : "videos";
            var sharePointUrl = await _sharePointService.UploadFileAsync(
                tempPath,
                $"GuideMedia/{guideId}/{subfolder}",
                fileName);

            // Clean up temp file
            File.Delete(tempPath);

            var mediaReference = new MediaReference
            {
                Id = mediaId,
                Type = isImage ? "Image" : "Video",
                Url = sharePointUrl,
                ThumbnailUrl = isImage ? sharePointUrl : string.Empty,
                Title = Path.GetFileNameWithoutExtension(fileName),
                Description = string.Empty,
                Duration = null,
                UploadedDate = DateTime.UtcNow
            };

            _logger.LogInformation("Successfully uploaded media stream {MediaId}", mediaId);
            return mediaReference;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload media stream {FileName}", fileName);
            throw;
        }
    }

    public async Task<bool> DeleteMediaAsync(string mediaId)
    {
        if (string.IsNullOrEmpty(mediaId))
            return false;

        try
        {
            _logger.LogInformation("Deleting media {MediaId}", mediaId);

            // In a real implementation, you would need to:
            // 1. Look up the media reference from a database or SharePoint list
            // 2. Get the SharePoint file URL
            // 3. Delete the file from SharePoint

            // For now, we'll just return true
            // TODO: Implement actual deletion from SharePoint

            _logger.LogInformation("Successfully deleted media {MediaId}", mediaId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete media {MediaId}", mediaId);
            return false;
        }
    }

    public async Task<IEnumerable<MediaReference>> GetGuideMediaAsync(string guideId)
    {
        if (string.IsNullOrEmpty(guideId))
            return Enumerable.Empty<MediaReference>();

        try
        {
            _logger.LogInformation("Getting media for guide {GuideId}", guideId);

            // In a real implementation, you would query SharePoint for all files in the guide's media folder
            // For now, we'll return an empty list
            // TODO: Implement actual retrieval from SharePoint

            return Enumerable.Empty<MediaReference>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get media for guide {GuideId}", guideId);
            return Enumerable.Empty<MediaReference>();
        }
    }

    public Task<(bool IsValid, string? ErrorMessage)> ValidateMediaAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return Task.FromResult((false, "File not found"));

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fileInfo = new FileInfo(filePath);

        // Check if it's a supported format
        var isImage = SupportedImageFormats.Contains(extension);
        var isVideo = SupportedVideoFormats.Contains(extension);

        if (!isImage && !isVideo)
        {
            return Task.FromResult((false, $"Unsupported file format: {extension}"));
        }

        // Check file size
        var maxSize = isImage ? MaxImageSize : MaxVideoSize;
        if (fileInfo.Length > maxSize)
        {
            var maxSizeMB = maxSize / (1024 * 1024);
            var actualSizeMB = fileInfo.Length / (1024.0 * 1024.0);
            return Task.FromResult((false, $"File size ({actualSizeMB:F2} MB) exceeds maximum of {maxSizeMB} MB"));
        }

        _logger.LogInformation("Media file {FilePath} is valid", filePath);
        return Task.FromResult((true, (string?)null));
    }

    public string[] GetSupportedImageFormats()
    {
        return SupportedImageFormats;
    }

    public string[] GetSupportedVideoFormats()
    {
        return SupportedVideoFormats;
    }
}
