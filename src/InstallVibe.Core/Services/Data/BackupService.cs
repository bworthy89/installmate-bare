using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Core.Services.Data;

/// <summary>
/// Implements database backup and restore functionality for SQLite databases.
/// </summary>
public class BackupService : IBackupService
{
    private readonly string _databasePath;
    private readonly string _backupDirectory;
    private readonly ILogger<BackupService> _logger;

    public BackupService(ILogger<BackupService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Database path
        _databasePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InstallVibe", "Data", "installvibe.db");

        // Backup directory
        _backupDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InstallVibe", "Backups");

        // Ensure backup directory exists
        Directory.CreateDirectory(_backupDirectory);
    }

    /// <inheritdoc/>
    public async Task<string> CreateBackupAsync()
    {
        try
        {
            if (!File.Exists(_databasePath))
            {
                throw new FileNotFoundException("Database file not found", _databasePath);
            }

            // Generate timestamped backup filename
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"installvibe_backup_{timestamp}.db";
            var backupPath = Path.Combine(_backupDirectory, backupFileName);

            _logger.LogInformation("Creating database backup: {BackupPath}", backupPath);

            // Use SQLite's backup API for safe backup (handles in-use database)
            using (var sourceConnection = new SqliteConnection($"Data Source={_databasePath}"))
            using (var destConnection = new SqliteConnection($"Data Source={backupPath}"))
            {
                await sourceConnection.OpenAsync();
                await destConnection.OpenAsync();

                // Perform backup
                sourceConnection.BackupDatabase(destConnection);
            }

            _logger.LogInformation("Database backup created successfully: {BackupPath}", backupPath);

            // Cleanup old backups
            await CleanupOldBackupsAsync(10);

            return backupPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating database backup");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RestoreBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                _logger.LogError("Backup file not found: {BackupPath}", backupPath);
                return false;
            }

            // Validate backup before restoring
            if (!await ValidateBackupAsync(backupPath))
            {
                _logger.LogError("Backup validation failed: {BackupPath}", backupPath);
                return false;
            }

            _logger.LogInformation("Restoring database from backup: {BackupPath}", backupPath);

            // Create a safety backup of current database first
            if (File.Exists(_databasePath))
            {
                var safetyBackupPath = Path.Combine(_backupDirectory, $"pre_restore_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.db");
                File.Copy(_databasePath, safetyBackupPath, true);
                _logger.LogInformation("Created safety backup before restore: {SafetyBackupPath}", safetyBackupPath);
            }

            // Copy backup to database location
            File.Copy(backupPath, _databasePath, true);

            _logger.LogInformation("Database restored successfully from backup");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring database backup");
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<List<string>> ListBackupsAsync()
    {
        try
        {
            if (!Directory.Exists(_backupDirectory))
            {
                return new List<string>();
            }

            var backupFiles = Directory.GetFiles(_backupDirectory, "installvibe_backup_*.db")
                .OrderByDescending(f => File.GetCreationTimeUtc(f))
                .ToList();

            return await Task.FromResult(backupFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing backups");
            return new List<string>();
        }
    }

    /// <inheritdoc/>
    public async Task<int> CleanupOldBackupsAsync(int keepCount = 10)
    {
        try
        {
            var backups = await ListBackupsAsync();
            var backupsToDelete = backups.Skip(keepCount).ToList();

            int deletedCount = 0;
            foreach (var backupPath in backupsToDelete)
            {
                try
                {
                    File.Delete(backupPath);
                    deletedCount++;
                    _logger.LogDebug("Deleted old backup: {BackupPath}", backupPath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete backup: {BackupPath}", backupPath);
                }
            }

            if (deletedCount > 0)
            {
                _logger.LogInformation("Cleaned up {Count} old backups", deletedCount);
            }

            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up old backups");
            return 0;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateBackupAsync(string backupPath)
    {
        try
        {
            if (!File.Exists(backupPath))
            {
                return false;
            }

            // Try to open the database and query it
            using var connection = new SqliteConnection($"Data Source={backupPath}");
            await connection.OpenAsync();

            // Verify it's a valid SQLite database by checking for sqlite_master table
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table';";
            var result = await command.ExecuteScalarAsync();

            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Backup validation failed for: {BackupPath}", backupPath);
            return false;
        }
    }
}
