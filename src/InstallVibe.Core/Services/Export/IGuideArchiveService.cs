namespace InstallVibe.Core.Services.Export;

/// <summary>
/// Service for exporting and importing guides as archive files (.ivguide).
/// </summary>
public interface IGuideArchiveService
{
    /// <summary>
    /// Exports a guide to a .ivguide archive file (ZIP format).
    /// </summary>
    /// <param name="guideId">Guide identifier.</param>
    /// <param name="outputPath">Destination file path for the archive.</param>
    /// <returns>True if export was successful.</returns>
    Task<bool> ExportGuideAsync(string guideId, string outputPath);

    /// <summary>
    /// Imports a guide from a .ivguide archive file.
    /// </summary>
    /// <param name="archivePath">Path to the .ivguide archive file.</param>
    /// <param name="options">Import options (conflict resolution, etc.).</param>
    /// <returns>Import result with status and imported guide ID.</returns>
    Task<ImportResult> ImportGuideAsync(string archivePath, ImportOptions options);

    /// <summary>
    /// Validates a .ivguide archive file structure and integrity.
    /// </summary>
    /// <param name="archivePath">Path to the .ivguide archive file.</param>
    /// <returns>Validation result with details.</returns>
    Task<ValidationResult> ValidateArchiveAsync(string archivePath);
}

/// <summary>
/// Options for importing a guide archive.
/// </summary>
public class ImportOptions
{
    /// <summary>
    /// Action to take if a guide with the same GUID already exists.
    /// </summary>
    public ConflictResolution ConflictResolution { get; set; } = ConflictResolution.Cancel;

    /// <summary>
    /// Whether to regenerate GUIDs when importing as a copy.
    /// </summary>
    public bool RegenerateGuids { get; set; } = false;
}

/// <summary>
/// Conflict resolution strategy for guide imports.
/// </summary>
public enum ConflictResolution
{
    /// <summary>
    /// Cancel the import operation.
    /// </summary>
    Cancel,

    /// <summary>
    /// Overwrite the existing guide.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Import as a new copy with regenerated GUIDs.
    /// </summary>
    ImportAsCopy
}

/// <summary>
/// Result of a guide import operation.
/// </summary>
public class ImportResult
{
    /// <summary>
    /// Whether the import was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The ID of the imported guide (may be new GUID if imported as copy).
    /// </summary>
    public string? ImportedGuideId { get; set; }

    /// <summary>
    /// Error message if import failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of media files imported.
    /// </summary>
    public int MediaFilesImported { get; set; }

    /// <summary>
    /// Whether the guide was overwritten or created as new.
    /// </summary>
    public bool WasOverwritten { get; set; }
}

/// <summary>
/// Result of archive validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether the archive is valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of validation warnings.
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Guide ID from the archive (if readable).
    /// </summary>
    public string? GuideId { get; set; }

    /// <summary>
    /// Guide title from the archive (if readable).
    /// </summary>
    public string? GuideTitle { get; set; }

    /// <summary>
    /// Whether a guide with this ID already exists locally.
    /// </summary>
    public bool GuidAlreadyExists { get; set; }
}
