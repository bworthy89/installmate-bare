using InstallVibe.Core.Models.Domain;

namespace InstallVibe.Core.Services.Editor;

/// <summary>
/// Service for uploading and managing media files for guides.
/// </summary>
public interface IMediaUploadService
{
    /// <summary>
    /// Uploads an image file.
    /// </summary>
    Task<MediaReference> UploadImageAsync(string filePath, string guideId);

    /// <summary>
    /// Uploads a video file.
    /// </summary>
    Task<MediaReference> UploadVideoAsync(string filePath, string guideId);

    /// <summary>
    /// Uploads media from a stream.
    /// </summary>
    Task<MediaReference> UploadMediaStreamAsync(Stream stream, string fileName, string guideId, string mediaType);

    /// <summary>
    /// Deletes a media file.
    /// </summary>
    Task<bool> DeleteMediaAsync(string mediaId);

    /// <summary>
    /// Gets all media for a guide.
    /// </summary>
    Task<IEnumerable<MediaReference>> GetGuideMediaAsync(string guideId);

    /// <summary>
    /// Validates media file (size, format, etc.).
    /// </summary>
    Task<(bool IsValid, string? ErrorMessage)> ValidateMediaAsync(string filePath);

    /// <summary>
    /// Gets supported image formats.
    /// </summary>
    string[] GetSupportedImageFormats();

    /// <summary>
    /// Gets supported video formats.
    /// </summary>
    string[] GetSupportedVideoFormats();
}
