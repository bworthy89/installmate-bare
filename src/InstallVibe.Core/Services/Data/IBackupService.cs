namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Service for creating and managing database backups.
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Creates a timestamped backup of the SQLite database.
    /// </summary>
    /// <returns>Path to the created backup file.</returns>
    Task<string> CreateBackupAsync();

    /// <summary>
    /// Restores the database from a backup file.
    /// </summary>
    /// <param name="backupPath">Path to the backup file.</param>
    /// <returns>True if restore was successful.</returns>
    Task<bool> RestoreBackupAsync(string backupPath);

    /// <summary>
    /// Lists all available database backups.
    /// </summary>
    /// <returns>List of backup file paths ordered by creation date (newest first).</returns>
    Task<List<string>> ListBackupsAsync();

    /// <summary>
    /// Deletes old backups, keeping only the specified number of most recent backups.
    /// </summary>
    /// <param name="keepCount">Number of recent backups to keep.</param>
    /// <returns>Number of backups deleted.</returns>
    Task<int> CleanupOldBackupsAsync(int keepCount = 10);

    /// <summary>
    /// Validates that a backup file is a valid SQLite database.
    /// </summary>
    /// <param name="backupPath">Path to the backup file.</param>
    /// <returns>True if backup is valid.</returns>
    Task<bool> ValidateBackupAsync(string backupPath);
}
