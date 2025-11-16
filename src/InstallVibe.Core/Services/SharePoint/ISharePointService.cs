using InstallVibe.Core.Models.Activation;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Models.SharePoint;

namespace InstallVibe.Core.Services.SharePoint;

/// <summary>
/// Service for interacting with SharePoint Online using Microsoft Graph API.
/// </summary>
public interface ISharePointService
{
    // ========== Guide Index ==========

    /// <summary>
    /// Fetches the guide index from SharePoint.
    /// </summary>
    /// <param name="filterByLicense">Optional filter by license type.</param>
    /// <returns>List of guide index entries.</returns>
    Task<List<GuideIndexEntry>> GetGuideIndexAsync(LicenseType? filterByLicense = null);

    /// <summary>
    /// Gets metadata for a specific guide from the index.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <returns>Guide index entry, or null if not found.</returns>
    Task<GuideIndexEntry?> GetGuideMetadataAsync(string guideId);

    // ========== Guide Content ==========

    /// <summary>
    /// Downloads guide.json file as raw bytes.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <returns>Raw JSON bytes.</returns>
    Task<byte[]> DownloadGuideJsonAsync(string guideId);

    /// <summary>
    /// Downloads and deserializes a guide.
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <returns>Deserialized guide object, or null if not found.</returns>
    Task<Guide?> GetGuideAsync(string guideId);

    /// <summary>
    /// Uploads a guide to SharePoint (Admin only).
    /// </summary>
    /// <param name="guide">Guide to upload.</param>
    /// <returns>True if successful.</returns>
    Task<bool> UploadGuideAsync(Guide guide);

    /// <summary>
    /// Deletes a guide from SharePoint (Admin only).
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <returns>True if successful.</returns>
    Task<bool> DeleteGuideAsync(string guideId);

    // ========== Sync Operations ==========

    /// <summary>
    /// Syncs updated guides from SharePoint to local storage.
    /// </summary>
    /// <param name="since">Optional: Only sync guides modified after this date.</param>
    /// <param name="progress">Optional: Progress reporter.</param>
    /// <returns>Sync result with statistics.</returns>
    Task<SyncResult> SyncUpdatedGuidesAsync(DateTime? since = null, IProgress<SyncProgress>? progress = null);

    // ========== Media Operations ==========

    /// <summary>
    /// Gets metadata for a media file.
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    /// <returns>Media metadata, or null if not found.</returns>
    Task<SharePointMedia?> GetMediaMetadataAsync(string mediaId);

    /// <summary>
    /// Downloads a media file.
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    /// <returns>Raw file bytes.</returns>
    Task<byte[]> DownloadMediaAsync(string mediaId);

    /// <summary>
    /// Uploads a media file to SharePoint (Admin only).
    /// </summary>
    /// <param name="mediaId">Media identifier.</param>
    /// <param name="content">File content stream.</param>
    /// <param name="mediaType">Type of media (Image, Video, Document).</param>
    /// <param name="fileExtension">File extension (e.g., "png", "mp4").</param>
    /// <returns>SharePoint item ID of uploaded file.</returns>
    Task<string> UploadMediaAsync(string mediaId, Stream content, MediaType mediaType, string fileExtension);

    // ========== Product Key Validation (Optional) ==========

    /// <summary>
    /// Validates a product key against SharePoint ProductKeys list.
    /// </summary>
    /// <param name="productKey">Product key to validate.</param>
    /// <returns>Validation result.</returns>
    Task<ProductKeyValidationResult> ValidateProductKeyOnlineAsync(string productKey);

    // ========== Health & Status ==========

    /// <summary>
    /// Checks if SharePoint is currently reachable.
    /// </summary>
    /// <returns>True if online, false otherwise.</returns>
    Task<bool> IsOnlineAsync();

    /// <summary>
    /// Gets detailed health status of SharePoint connection.
    /// </summary>
    /// <returns>Health status information.</returns>
    Task<SharePointHealthStatus> GetHealthStatusAsync();
}
