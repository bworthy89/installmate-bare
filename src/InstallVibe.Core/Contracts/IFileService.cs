namespace InstallVibe.Core.Contracts;

/// <summary>
/// Service for file operations.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Reads all bytes from a file.
    /// </summary>
    Task<byte[]?> ReadAllBytesAsync(string path);

    /// <summary>
    /// Writes all bytes to a file.
    /// </summary>
    Task WriteAllBytesAsync(string path, byte[] bytes);

    /// <summary>
    /// Reads all text from a file.
    /// </summary>
    Task<string?> ReadAllTextAsync(string path);

    /// <summary>
    /// Writes all text to a file.
    /// </summary>
    Task WriteAllTextAsync(string path, string text);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// Checks if a directory exists.
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates a directory.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    void DeleteFile(string path);

    /// <summary>
    /// Deletes a directory.
    /// </summary>
    void DeleteDirectory(string path, bool recursive = false);
}
