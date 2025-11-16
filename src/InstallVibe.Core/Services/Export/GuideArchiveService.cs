using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Cache;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Media;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace InstallVibe.Core.Services.Export;

/// <summary>
/// Implements guide export and import functionality using .ivguide archive format.
/// </summary>
public class GuideArchiveService : IGuideArchiveService
{
    private readonly IGuideService _guideService;
    private readonly IMediaService _mediaService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GuideArchiveService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public GuideArchiveService(
        IGuideService guideService,
        IMediaService mediaService,
        ICacheService cacheService,
        ILogger<GuideArchiveService> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _mediaService = mediaService ?? throw new ArgumentNullException(nameof(mediaService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<bool> ExportGuideAsync(string guideId, string outputPath)
    {
        try
        {
            _logger.LogInformation("Exporting guide {GuideId} to {OutputPath}", guideId, outputPath);

            // Get the guide
            var guide = await _guideService.GetGuideAsync(guideId);
            if (guide == null)
            {
                _logger.LogError("Guide {GuideId} not found", guideId);
                return false;
            }

            // Collect media files
            var mediaFiles = await _mediaService.CollectGuideMediaAsync(guideId);
            _logger.LogInformation("Collected {Count} media files for guide {GuideId}", mediaFiles.Count, guideId);

            // Serialize guide to JSON
            var guideJson = JsonSerializer.Serialize(guide, JsonOptions);
            var guideBytes = Encoding.UTF8.GetBytes(guideJson);

            // Compute checksum
            var checksum = ComputeSha256(guideBytes);

            // Build manifest
            var manifest = new ArchiveManifest
            {
                FormatVersion = "1.0",
                GuideId = guide.GuideId,
                Title = guide.Title,
                ExportDate = DateTime.UtcNow,
                MediaFiles = mediaFiles.Keys.Select(GetMediaFileName).ToList(),
                Checksum = checksum
            };

            var manifestJson = JsonSerializer.Serialize(manifest, JsonOptions);

            // Create temporary directory for archive contents
            var tempDir = Path.Combine(Path.GetTempPath(), $"ivguide_export_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Write manifest.json
                await File.WriteAllTextAsync(Path.Combine(tempDir, "manifest.json"), manifestJson);

                // Write guide.json
                await File.WriteAllBytesAsync(Path.Combine(tempDir, "guide.json"), guideBytes);

                // Write media files
                if (mediaFiles.Count > 0)
                {
                    var mediaDir = Path.Combine(tempDir, "media");
                    Directory.CreateDirectory(mediaDir);

                    foreach (var (mediaId, mediaData) in mediaFiles)
                    {
                        var fileName = GetMediaFileName(mediaId);
                        var filePath = Path.Combine(mediaDir, fileName);
                        await File.WriteAllBytesAsync(filePath, mediaData);
                    }
                }

                // Create ZIP archive
                if (File.Exists(outputPath))
                {
                    File.Delete(outputPath);
                }

                ZipFile.CreateFromDirectory(tempDir, outputPath, CompressionLevel.Optimal, false);

                _logger.LogInformation(
                    "Successfully exported guide {GuideId} to {OutputPath} ({Size} KB)",
                    guideId,
                    outputPath,
                    new FileInfo(outputPath).Length / 1024.0);

                return true;
            }
            finally
            {
                // Cleanup temporary directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting guide {GuideId}", guideId);
            return false;
        }
    }

    public async Task<ImportResult> ImportGuideAsync(string archivePath, ImportOptions options)
    {
        var result = new ImportResult();

        try
        {
            _logger.LogInformation("Importing guide from {ArchivePath}", archivePath);

            // Validate archive first
            var validation = await ValidateArchiveAsync(archivePath);
            if (!validation.IsValid)
            {
                result.ErrorMessage = $"Archive validation failed: {string.Join(", ", validation.Errors)}";
                _logger.LogError("Archive validation failed: {Errors}", result.ErrorMessage);
                return result;
            }

            // Create temporary extraction directory
            var tempDir = Path.Combine(Path.GetTempPath(), $"ivguide_import_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Extract archive - SECURITY: Use GetFullPath to prevent Zip Slip attacks
                using (var archive = ZipFile.OpenRead(archivePath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        var destinationPath = Path.GetFullPath(Path.Combine(tempDir, entry.FullName));

                        // Validate that extraction path is within temp directory (prevent Zip Slip)
                        if (!destinationPath.StartsWith(tempDir, StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogWarning("Zip Slip attack detected: {EntryPath}", entry.FullName);
                            result.ErrorMessage = "Archive contains invalid paths";
                            return result;
                        }

                        // Create directory if needed
                        var destDir = Path.GetDirectoryName(destinationPath);
                        if (destDir != null && !Directory.Exists(destDir))
                        {
                            Directory.CreateDirectory(destDir);
                        }

                        // Extract file
                        entry.ExtractToFile(destinationPath, overwrite: true);
                    }
                }

                // Read manifest
                var manifestPath = Path.Combine(tempDir, "manifest.json");
                var manifestJson = await File.ReadAllTextAsync(manifestPath);
                var manifest = JsonSerializer.Deserialize<ArchiveManifest>(manifestJson, JsonOptions);

                if (manifest == null)
                {
                    result.ErrorMessage = "Failed to parse manifest";
                    return result;
                }

                // Read guide.json
                var guidePath = Path.Combine(tempDir, "guide.json");
                var guideJson = await File.ReadAllTextAsync(guidePath);
                var guide = JsonSerializer.Deserialize<Guide>(guideJson, JsonOptions);

                if (guide == null)
                {
                    result.ErrorMessage = "Failed to parse guide";
                    return result;
                }

                // Verify checksum
                var guideBytes = await File.ReadAllBytesAsync(guidePath);
                var computedChecksum = ComputeSha256(guideBytes);
                if (!string.Equals(computedChecksum, manifest.Checksum, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Checksum mismatch for imported guide {GuideId}", guide.GuideId);
                    // Continue anyway but log warning
                }

                // Check for GUID conflict
                var existingGuide = await _guideService.GetGuideAsync(guide.GuideId);
                bool guidExists = existingGuide != null;

                if (guidExists)
                {
                    if (options.ConflictResolution == ConflictResolution.Cancel)
                    {
                        result.ErrorMessage = $"Guide with ID {guide.GuideId} already exists";
                        _logger.LogInformation("Import cancelled due to GUID conflict");
                        return result;
                    }
                    else if (options.ConflictResolution == ConflictResolution.ImportAsCopy || options.RegenerateGuids)
                    {
                        // Regenerate GUIDs
                        var oldGuideId = guide.GuideId;
                        guide.GuideId = Guid.NewGuid().ToString();

                        // Regenerate step GUIDs
                        foreach (var step in guide.Steps)
                        {
                            step.StepId = Guid.NewGuid().ToString();
                        }

                        _logger.LogInformation(
                            "Regenerated GUIDs for import as copy: {OldId} -> {NewId}",
                            oldGuideId,
                            guide.GuideId);
                    }
                    else if (options.ConflictResolution == ConflictResolution.Overwrite)
                    {
                        result.WasOverwritten = true;
                        _logger.LogInformation("Overwriting existing guide {GuideId}", guide.GuideId);
                    }
                }

                // Update metadata
                guide.LastModified = DateTime.UtcNow;

                // Import media files
                var mediaDir = Path.Combine(tempDir, "media");
                if (Directory.Exists(mediaDir))
                {
                    var mediaFilePaths = Directory.GetFiles(mediaDir);

                    foreach (var mediaFilePath in mediaFilePaths)
                    {
                        try
                        {
                            var fileName = Path.GetFileName(mediaFilePath);
                            var mediaId = GetMediaIdFromFileName(fileName);
                            var mediaData = await File.ReadAllBytesAsync(mediaFilePath);

                            // Cache the media file
                            await _cacheService.CacheFileAsync("media", mediaId, mediaData, string.Empty);
                            result.MediaFilesImported++;

                            _logger.LogDebug("Imported media file {MediaId}", mediaId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to import media file {FileName}", Path.GetFileName(mediaFilePath));
                        }
                    }
                }

                // Save guide to database
                await _guideService.SaveGuideAsync(guide);

                result.Success = true;
                result.ImportedGuideId = guide.GuideId;

                _logger.LogInformation(
                    "Successfully imported guide {GuideId} ({Title}) with {MediaCount} media files",
                    guide.GuideId,
                    guide.Title,
                    result.MediaFilesImported);

                return result;
            }
            finally
            {
                // Cleanup temporary directory
                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, recursive: true);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing guide from {ArchivePath}", archivePath);
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    public async Task<ValidationResult> ValidateArchiveAsync(string archivePath)
    {
        var result = new ValidationResult();

        try
        {
            if (!File.Exists(archivePath))
            {
                result.Errors.Add("Archive file not found");
                return result;
            }

            // Check file extension
            if (!archivePath.EndsWith(".ivguide", StringComparison.OrdinalIgnoreCase))
            {
                result.Warnings.Add("File does not have .ivguide extension");
            }

            // Try to open as ZIP
            using var archive = ZipFile.OpenRead(archivePath);

            // Check for required files
            var manifestEntry = archive.GetEntry("manifest.json");
            if (manifestEntry == null)
            {
                result.Errors.Add("Missing manifest.json");
                return result;
            }

            var guideEntry = archive.GetEntry("guide.json");
            if (guideEntry == null)
            {
                result.Errors.Add("Missing guide.json");
                return result;
            }

            // Read and validate manifest
            using (var manifestStream = manifestEntry.Open())
            using (var reader = new StreamReader(manifestStream))
            {
                var manifestJson = await reader.ReadToEndAsync();
                var manifest = JsonSerializer.Deserialize<ArchiveManifest>(manifestJson, JsonOptions);

                if (manifest == null)
                {
                    result.Errors.Add("Invalid manifest.json format");
                    return result;
                }

                result.GuideId = manifest.GuideId;
                result.GuideTitle = manifest.Title;

                // Check format version
                if (manifest.FormatVersion != "1.0")
                {
                    result.Warnings.Add($"Unsupported format version: {manifest.FormatVersion}");
                }
            }

            // Read and validate guide
            using (var guideStream = guideEntry.Open())
            using (var reader = new StreamReader(guideStream))
            {
                var guideJson = await reader.ReadToEndAsync();
                var guide = JsonSerializer.Deserialize<Guide>(guideJson, JsonOptions);

                if (guide == null)
                {
                    result.Errors.Add("Invalid guide.json format");
                    return result;
                }

                // Check if guide already exists
                var existingGuide = await _guideService.GetGuideAsync(guide.GuideId);
                result.GuidAlreadyExists = existingGuide != null;

                if (result.GuidAlreadyExists)
                {
                    result.Warnings.Add($"Guide with ID {guide.GuideId} already exists locally");
                }
            }

            // Check media files
            var mediaEntries = archive.Entries.Where(e => e.FullName.StartsWith("media/", StringComparison.OrdinalIgnoreCase)).ToList();
            if (mediaEntries.Count > 0)
            {
                _logger.LogDebug("Archive contains {Count} media files", mediaEntries.Count);
            }

            result.IsValid = result.Errors.Count == 0;

            _logger.LogInformation(
                "Archive validation complete: Valid={IsValid}, Errors={ErrorCount}, Warnings={WarningCount}",
                result.IsValid,
                result.Errors.Count,
                result.Warnings.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating archive {ArchivePath}", archivePath);
            result.Errors.Add($"Validation error: {ex.Message}");
            return result;
        }
    }

    // Private helper methods

    private static string ComputeSha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(data);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static string GetMediaFileName(string mediaId)
    {
        // Media files are stored with their ID as filename
        // Extension can be determined from the file, or we can store as .bin
        return $"{mediaId}.bin";
    }

    private static string GetMediaIdFromFileName(string fileName)
    {
        // Remove extension to get media ID
        return Path.GetFileNameWithoutExtension(fileName);
    }

    /// <summary>
    /// Archive manifest structure.
    /// </summary>
    private class ArchiveManifest
    {
        public string FormatVersion { get; set; } = string.Empty;
        public string GuideId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime ExportDate { get; set; }
        public List<string> MediaFiles { get; set; } = new();
        public string Checksum { get; set; } = string.Empty;
    }
}
