using InstallVibe.Core.Models.Settings;
using InstallVibe.Core.Models.Sync;
using InstallVibe.Core.Services.Export;
using InstallVibe.Infrastructure.Security.Graph;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Text.Json;

namespace InstallVibe.Core.Services.OneDrive;

/// <summary>
/// Implementation of OneDrive/SharePoint sync service for .ivguide files.
/// </summary>
public class OneDriveSyncService : IOneDriveSyncService
{
    private readonly IGraphClientFactory _graphClientFactory;
    private readonly IGuideArchiveService _guideArchiveService;
    private readonly ILogger<OneDriveSyncService> _logger;
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private PeriodicTimer? _autoSyncTimer;
    private CancellationTokenSource? _autoSyncCts;
    private OneDriveSyncSettings _settings;
    private readonly string _settingsFilePath;

    public bool IsAutoSyncRunning { get; private set; }

    public OneDriveSyncService(
        IGraphClientFactory graphClientFactory,
        IGuideArchiveService guideArchiveService,
        ILogger<OneDriveSyncService> logger,
        OneDriveSyncSettings settings)
    {
        _graphClientFactory = graphClientFactory ?? throw new ArgumentNullException(nameof(graphClientFactory));
        _guideArchiveService = guideArchiveService ?? throw new ArgumentNullException(nameof(guideArchiveService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));

        // Settings file path
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InstallVibe", "Config");
        Directory.CreateDirectory(appDataPath);
        _settingsFilePath = Path.Combine(appDataPath, "onedrive-sync-settings.json");

        // Load persisted settings
        _ = LoadPersistedSettingsAsync();
    }

    public async Task<OneDriveSyncResult> SyncNowAsync(CancellationToken cancellationToken = default)
    {
        var result = new OneDriveSyncResult
        {
            SyncStartTime = DateTime.UtcNow
        };

        // Check if sync is enabled
        if (!_settings.Enabled)
        {
            _logger.LogInformation("OneDrive sync is disabled");
            result.Success = true;
            result.SyncEndTime = DateTime.UtcNow;
            return result;
        }

        // Validate configuration
        if (string.IsNullOrWhiteSpace(_settings.SiteId) || string.IsNullOrWhiteSpace(_settings.DriveId))
        {
            var error = "OneDrive sync is not configured. SiteId and DriveId are required.";
            _logger.LogWarning(error);
            result.Errors.Add(error);
            result.SyncEndTime = DateTime.UtcNow;
            return result;
        }

        // Acquire sync lock to prevent concurrent syncs
        if (!await _syncLock.WaitAsync(0, cancellationToken))
        {
            _logger.LogInformation("Sync already in progress, skipping");
            result.Errors.Add("Sync operation already in progress");
            result.SyncEndTime = DateTime.UtcNow;
            return result;
        }

        try
        {
            _logger.LogInformation(
                "Starting OneDrive sync from {SiteId}/{DriveId}/{FolderPath}",
                _settings.SiteId,
                _settings.DriveId,
                _settings.FolderPath);

            // Get Graph client
            var graphClient = _graphClientFactory.CreateClient();

            // Query delta API
            var ivguideFiles = await QueryDeltaFilesAsync(graphClient, cancellationToken);

            _logger.LogInformation("Found {FileCount} .ivguide files to sync", ivguideFiles.Count);

            // Download and import each file
            foreach (var file in ivguideFiles)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Sync cancelled by user");
                    result.Errors.Add("Sync cancelled by user");
                    break;
                }

                await ProcessFileAsync(graphClient, file, result, cancellationToken);
            }

            // Update last sync time
            _settings.LastSyncTime = DateTime.UtcNow;
            await PersistSettingsAsync();

            result.Success = result.FilesFailed == 0;
            result.SyncEndTime = DateTime.UtcNow;

            _logger.LogInformation(
                "OneDrive sync completed: Downloaded={Downloaded}, Imported={Imported}, Failed={Failed}, Duration={Duration}s",
                result.FilesDownloaded,
                result.FilesImported,
                result.FilesFailed,
                result.Duration.TotalSeconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OneDrive sync");
            result.Errors.Add($"Sync error: {ex.Message}");
            result.SyncEndTime = DateTime.UtcNow;
            return result;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task StartAutoSyncAsync()
    {
        if (IsAutoSyncRunning)
        {
            _logger.LogInformation("Auto-sync already running");
            return;
        }

        if (!_settings.Enabled)
        {
            _logger.LogInformation("Auto-sync not started because OneDrive sync is disabled");
            return;
        }

        _logger.LogInformation(
            "Starting auto-sync with interval of {Interval} minutes",
            _settings.SyncIntervalMinutes);

        _autoSyncCts = new CancellationTokenSource();
        _autoSyncTimer = new PeriodicTimer(TimeSpan.FromMinutes(_settings.SyncIntervalMinutes));
        IsAutoSyncRunning = true;

        // Start background task
        _ = Task.Run(async () =>
        {
            try
            {
                while (await _autoSyncTimer.WaitForNextTickAsync(_autoSyncCts.Token))
                {
                    _logger.LogDebug("Auto-sync timer triggered");

                    try
                    {
                        await SyncNowAsync(_autoSyncCts.Token);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during auto-sync");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Auto-sync cancelled");
            }
            finally
            {
                IsAutoSyncRunning = false;
            }
        }, _autoSyncCts.Token);

        await Task.CompletedTask;
    }

    public async Task StopAutoSyncAsync()
    {
        if (!IsAutoSyncRunning)
        {
            return;
        }

        _logger.LogInformation("Stopping auto-sync");

        _autoSyncCts?.Cancel();
        _autoSyncTimer?.Dispose();
        _autoSyncTimer = null;
        _autoSyncCts?.Dispose();
        _autoSyncCts = null;
        IsAutoSyncRunning = false;

        await Task.CompletedTask;
    }

    public async Task<OneDriveSyncSettings> GetSettingsAsync()
    {
        await Task.CompletedTask;
        return _settings;
    }

    public async Task UpdateSettingsAsync(OneDriveSyncSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        await PersistSettingsAsync();

        // Restart auto-sync if settings changed
        if (IsAutoSyncRunning)
        {
            await StopAutoSyncAsync();
            await StartAutoSyncAsync();
        }
    }

    // Private helper methods

    private async Task<List<DriveItem>> QueryDeltaFilesAsync(
        GraphServiceClient graphClient,
        CancellationToken cancellationToken)
    {
        var ivguideFiles = new List<DriveItem>();

        try
        {
            // Query children of the folder
            DriveItemCollectionResponse? childrenResponse = null;

            // Build the path - for root folder use "root", otherwise use the specified path
            string itemPath = "root";
            if (!string.IsNullOrWhiteSpace(_settings.FolderPath) && _settings.FolderPath != "/")
            {
                itemPath = _settings.FolderPath.TrimStart('/');
            }

            try
            {
                // Use the Items endpoint with path
                if (itemPath == "root")
                {
                    childrenResponse = await graphClient.Drives[_settings.DriveId]
                        .Items["root"]
                        .Children
                        .GetAsync(cancellationToken: cancellationToken);
                }
                else
                {
                    // Try to navigate to the folder by path
                    childrenResponse = await graphClient.Drives[_settings.DriveId]
                        .Items["root:/" + itemPath + ":/"]
                        .Children
                        .GetAsync(cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not query folder path: {FolderPath}, trying root", _settings.FolderPath);

                // Fallback to root
                try
                {
                    childrenResponse = await graphClient.Drives[_settings.DriveId]
                        .Items["root"]
                        .Children
                        .GetAsync(cancellationToken: cancellationToken);
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "Failed to query root folder");
                    throw;
                }
            }

            if (childrenResponse?.Value != null)
            {
                foreach (var item in childrenResponse.Value)
                {
                    // Filter for .ivguide files
                    if (item.File != null &&
                        item.Name != null &&
                        item.Name.EndsWith(".ivguide", StringComparison.OrdinalIgnoreCase))
                    {
                        ivguideFiles.Add(item);
                        _logger.LogDebug("Found .ivguide file: {FileName}", item.Name);
                    }
                }
            }

            // Store timestamp for tracking (simplified - no delta token in this version)
            _settings.DeltaToken = DateTime.UtcNow.ToString("o");
            _logger.LogDebug("Updated sync timestamp");

            return ivguideFiles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying files");
            throw;
        }
    }

    private async Task ProcessFileAsync(
        GraphServiceClient graphClient,
        DriveItem file,
        OneDriveSyncResult result,
        CancellationToken cancellationToken)
    {
        string? tempFilePath = null;

        try
        {
            _logger.LogInformation("Processing file: {FileName} ({FileSize} bytes)", file.Name, file.Size);

            // Download file to temp location
            tempFilePath = Path.Combine(Path.GetTempPath(), $"onedrive_sync_{Guid.NewGuid()}.ivguide");

            // Retry logic for download
            const int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // Download file stream
                    if (string.IsNullOrWhiteSpace(file.Id))
                    {
                        throw new InvalidOperationException("File ID is null or empty");
                    }

                    var stream = await graphClient.Drives[_settings.DriveId]
                        .Items[file.Id!]
                        .Content
                        .GetAsync(cancellationToken: cancellationToken);

                    if (stream == null)
                    {
                        throw new InvalidOperationException("Failed to download file stream");
                    }

                    // Stream to temp file
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                    {
                        await stream.CopyToAsync(fileStream, cancellationToken);
                    }

                    result.FilesDownloaded++;
                    _logger.LogDebug("Downloaded file to: {TempPath}", tempFilePath);
                    break;
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    _logger.LogWarning(
                        ex,
                        "Download attempt {Attempt}/{MaxRetries} failed for {FileName}, retrying...",
                        attempt,
                        maxRetries,
                        file.Name);

                    // Exponential backoff
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                }
            }

            // Import using existing GuideArchiveService
            var importOptions = new ImportOptions
            {
                ConflictResolution = ConflictResolution.Overwrite // Default to overwrite (use OneDrive version)
            };

            var importResult = await _guideArchiveService.ImportGuideAsync(tempFilePath, importOptions);

            if (importResult.Success)
            {
                result.FilesImported++;
                _logger.LogInformation(
                    "Successfully imported guide {GuideId} from {FileName}",
                    importResult.ImportedGuideId,
                    file.Name);
            }
            else
            {
                result.FilesFailed++;
                var error = $"Failed to import {file.Name}: {importResult.ErrorMessage}";
                result.Errors.Add(error);
                _logger.LogError(error);
            }
        }
        catch (Exception ex)
        {
            result.FilesFailed++;
            var error = $"Error processing {file.Name}: {ex.Message}";
            result.Errors.Add(error);
            _logger.LogError(ex, "Error processing file {FileName}", file.Name);
        }
        finally
        {
            // Clean up temp file
            if (tempFilePath != null && File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete temp file: {TempPath}", tempFilePath);
                }
            }
        }
    }

    private async Task LoadPersistedSettingsAsync()
    {
        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var persisted = JsonSerializer.Deserialize<OneDriveSyncSettings>(json);

                if (persisted != null)
                {
                    // Merge persisted settings (keep runtime values from DI, update persisted values)
                    _settings.LastSyncTime = persisted.LastSyncTime;
                    _settings.DeltaToken = persisted.DeltaToken;

                    _logger.LogDebug("Loaded persisted OneDrive sync settings");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load persisted OneDrive sync settings");
        }
    }

    private async Task PersistSettingsAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(_settingsFilePath, json);
            _logger.LogDebug("Persisted OneDrive sync settings");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist OneDrive sync settings");
        }
    }
}
