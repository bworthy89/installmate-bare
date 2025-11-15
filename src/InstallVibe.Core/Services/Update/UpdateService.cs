using InstallVibe.Core.Models.Update;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace InstallVibe.Core.Services.Update;

/// <summary>
/// Implementation of IUpdateService for AppInstaller/MSIX updates.
/// </summary>
public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _versionManifestUrl;
    private readonly string _localStatePath;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(4);

    private UpdateCheckResult? _cachedResult;
    private DateTime? _lastCheckTime;
    private HashSet<string> _dismissedVersions = new();

    public event EventHandler<UpdateCheckResult>? UpdateCheckCompleted;
    public event EventHandler<int>? UpdateDownloadProgress;
    public event EventHandler<UpdateInfo>? UpdateReadyToApply;

    public UpdateService(
        ILogger<UpdateService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _httpClient = httpClientFactory.CreateClient("UpdateClient");

        // Get version manifest URL from configuration
        _versionManifestUrl = _configuration["Update:VersionManifestUrl"]
            ?? throw new InvalidOperationException("Update:VersionManifestUrl not configured");

        // Local state path for caching and dismissed versions
        _localStatePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "InstallVibe", "Update");

        Directory.CreateDirectory(_localStatePath);

        LoadDismissedVersions();
    }

    public async Task<UpdateCheckResult> CheckForUpdatesAsync(bool forceCheck = false)
    {
        _logger.LogInformation("Checking for updates (forceCheck: {ForceCheck})", forceCheck);

        // Return cached result if available and not expired
        if (!forceCheck && _cachedResult != null && _lastCheckTime.HasValue)
        {
            var age = DateTime.UtcNow - _lastCheckTime.Value;
            if (age < _cacheExpiration)
            {
                _logger.LogInformation("Returning cached update check result (age: {Age})", age);
                return _cachedResult;
            }
        }

        var currentVersion = GetCurrentVersion();

        try
        {
            // Download version.json from SharePoint
            var manifest = await DownloadVersionManifestAsync();

            if (manifest == null)
            {
                var result = UpdateCheckResult.Failed(currentVersion, "Failed to download version manifest");
                UpdateCache(result);
                UpdateCheckCompleted?.Invoke(this, result);
                return result;
            }

            // Compare versions
            var current = new Version(currentVersion);
            var latest = new Version(manifest.Version);

            if (latest > current)
            {
                // Check minimum version requirement
                if (!string.IsNullOrEmpty(manifest.MinimumVersion))
                {
                    var minimum = new Version(manifest.MinimumVersion);
                    if (current < minimum)
                    {
                        _logger.LogWarning(
                            "Current version {Current} is below minimum required {Minimum} for update {Latest}",
                            currentVersion, manifest.MinimumVersion, manifest.Version);

                        var result = UpdateCheckResult.Failed(
                            currentVersion,
                            $"Your version is too old. Please manually update to version {manifest.MinimumVersion} or higher first.");

                        UpdateCache(result);
                        UpdateCheckCompleted?.Invoke(this, result);
                        return result;
                    }
                }

                var updateInfo = manifest.ToUpdateInfo();
                var result = UpdateCheckResult.UpdateAvailable(currentVersion, updateInfo);

                _logger.LogInformation(
                    "Update available: {Latest} (current: {Current}, type: {Type})",
                    manifest.Version, currentVersion, manifest.UpdateType);

                UpdateCache(result);
                UpdateCheckCompleted?.Invoke(this, result);
                return result;
            }
            else
            {
                _logger.LogInformation("No update available (current: {Current}, latest: {Latest})",
                    currentVersion, manifest.Version);

                var result = UpdateCheckResult.NoUpdateAvailable(currentVersion);
                UpdateCache(result);
                UpdateCheckCompleted?.Invoke(this, result);
                return result;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates");

            var result = UpdateCheckResult.Failed(currentVersion, $"Update check failed: {ex.Message}");
            UpdateCache(result);
            UpdateCheckCompleted?.Invoke(this, result);
            return result;
        }
    }

    public string GetCurrentVersion()
    {
        try
        {
            var package = Package.Current;
            var version = package.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get package version, using assembly version");

            // Fallback to assembly version
            var assemblyVersion = typeof(UpdateService).Assembly.GetName().Version;
            return assemblyVersion != null
                ? $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}"
                : "0.0.0";
        }
    }

    public async Task<string> DownloadUpdateAsync(UpdateInfo updateInfo, IProgress<int>? progress = null)
    {
        if (updateInfo == null)
            throw new ArgumentNullException(nameof(updateInfo));

        _logger.LogInformation("Downloading update {Version} from {Url}",
            updateInfo.Version, updateInfo.MsixPackageUrl);

        var tempPath = Path.Combine(Path.GetTempPath(), $"InstallVibe_Update_{updateInfo.Version}.msix");

        try
        {
            using var response = await _httpClient.GetAsync(updateInfo.MsixPackageUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? updateInfo.FileSize;
            var buffer = new byte[8192];
            var totalRead = 0L;

            using var fileStream = File.Create(tempPath);
            using var contentStream = await response.Content.ReadAsStreamAsync();

            while (true)
            {
                var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                if (read == 0)
                    break;

                await fileStream.WriteAsync(buffer, 0, read);
                totalRead += read;

                if (totalBytes > 0 && progress != null)
                {
                    var percentage = (int)((totalRead * 100) / totalBytes);
                    progress.Report(percentage);
                    UpdateDownloadProgress?.Invoke(this, percentage);
                }
            }

            _logger.LogInformation("Downloaded update to {Path} ({Bytes} bytes)", tempPath, totalRead);

            // Verify integrity
            if (!string.IsNullOrEmpty(updateInfo.FileHash))
            {
                var isValid = await VerifyUpdateIntegrityAsync(tempPath, updateInfo.FileHash);
                if (!isValid)
                {
                    File.Delete(tempPath);
                    throw new InvalidOperationException("Downloaded file failed integrity check");
                }
            }

            UpdateReadyToApply?.Invoke(this, updateInfo);
            return tempPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading update");

            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }

            throw;
        }
    }

    public async Task<bool> ApplyUpdateAsync(UpdateInfo updateInfo)
    {
        if (updateInfo == null)
            throw new ArgumentNullException(nameof(updateInfo));

        _logger.LogInformation("Applying update {Version}", updateInfo.Version);

        try
        {
            // Use AppInstaller URL if available (preferred method)
            if (!string.IsNullOrEmpty(updateInfo.AppInstallerUrl))
            {
                return await ApplyViaAppInstallerAsync(updateInfo.AppInstallerUrl);
            }
            // Otherwise download and install MSIX directly
            else if (!string.IsNullOrEmpty(updateInfo.MsixPackageUrl))
            {
                var tempFile = await DownloadUpdateAsync(updateInfo);
                return await InstallMsixPackageAsync(tempFile);
            }
            else
            {
                _logger.LogError("No valid update URL found");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying update");
            return false;
        }
    }

    public async Task<bool> VerifyUpdateIntegrityAsync(string filePath, string expectedHash)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found for integrity check: {FilePath}", filePath);
            return false;
        }

        try
        {
            using var sha256 = SHA256.Create();
            using var stream = File.OpenRead(filePath);

            var hashBytes = await Task.Run(() => sha256.ComputeHash(stream));
            var actualHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            var normalizedExpectedHash = expectedHash.Replace("-", "").ToLowerInvariant();

            var isValid = actualHash.Equals(normalizedExpectedHash, StringComparison.OrdinalIgnoreCase);

            if (isValid)
            {
                _logger.LogInformation("File integrity verified for {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning(
                    "File integrity check failed for {FilePath}. Expected: {Expected}, Actual: {Actual}",
                    filePath, normalizedExpectedHash, actualHash);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying file integrity for {FilePath}", filePath);
            return false;
        }
    }

    public DateTime? GetLastUpdateCheckTime() => _lastCheckTime;

    public void DismissUpdate(string updateVersion)
    {
        if (string.IsNullOrEmpty(updateVersion))
            return;

        _dismissedVersions.Add(updateVersion);
        SaveDismissedVersions();

        _logger.LogInformation("Update {Version} dismissed", updateVersion);
    }

    public bool IsUpdateDismissed(string updateVersion)
    {
        return _dismissedVersions.Contains(updateVersion);
    }

    public async Task RestartApplicationAsync()
    {
        _logger.LogInformation("Restarting application");

        try
        {
            // Get the package family name
            var package = Package.Current;
            var appListEntry = (await package.GetAppListEntriesAsync())[0];

            // Launch the app
            await appListEntry.LaunchAsync();

            // Exit current instance
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restarting application");
            throw;
        }
    }

    #region Private Methods

    private async Task<VersionManifest?> DownloadVersionManifestAsync()
    {
        try
        {
            _logger.LogInformation("Downloading version manifest from {Url}", _versionManifestUrl);

            var response = await _httpClient.GetAsync(_versionManifestUrl);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var manifest = JsonSerializer.Deserialize<VersionManifest>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (manifest == null)
            {
                _logger.LogWarning("Failed to deserialize version manifest");
                return null;
            }

            _logger.LogInformation("Successfully downloaded version manifest for version {Version}", manifest.Version);
            return manifest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading version manifest");
            return null;
        }
    }

    private async Task<bool> ApplyViaAppInstallerAsync(string appInstallerUrl)
    {
        try
        {
            _logger.LogInformation("Launching AppInstaller with URL: {Url}", appInstallerUrl);

            // Launch the .appinstaller file using Windows protocol handler
            var uri = new Uri(appInstallerUrl);
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                _logger.LogInformation("Successfully launched AppInstaller");

                // Give AppInstaller time to start
                await Task.Delay(2000);

                // Exit the current application
                Environment.Exit(0);
            }
            else
            {
                _logger.LogWarning("Failed to launch AppInstaller");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error launching AppInstaller");
            return false;
        }
    }

    private async Task<bool> InstallMsixPackageAsync(string packagePath)
    {
        try
        {
            _logger.LogInformation("Installing MSIX package from {Path}", packagePath);

            var packageManager = new PackageManager();
            var packageUri = new Uri(packagePath);

            // Add package (this will update if already installed)
            var deploymentResult = await packageManager.AddPackageAsync(
                packageUri,
                null,
                DeploymentOptions.ForceTargetApplicationShutdown);

            if (deploymentResult.IsRegistered)
            {
                _logger.LogInformation("Successfully installed package");
                return true;
            }
            else
            {
                _logger.LogError("Package installation failed: {ErrorText}", deploymentResult.ErrorText);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error installing MSIX package");
            return false;
        }
    }

    private void UpdateCache(UpdateCheckResult result)
    {
        _cachedResult = result;
        _lastCheckTime = DateTime.UtcNow;
    }

    private void LoadDismissedVersions()
    {
        var filePath = Path.Combine(_localStatePath, "dismissed_updates.json");

        if (File.Exists(filePath))
        {
            try
            {
                var json = File.ReadAllText(filePath);
                var versions = JsonSerializer.Deserialize<HashSet<string>>(json);

                if (versions != null)
                {
                    _dismissedVersions = versions;
                    _logger.LogInformation("Loaded {Count} dismissed versions", _dismissedVersions.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading dismissed versions");
            }
        }
    }

    private void SaveDismissedVersions()
    {
        var filePath = Path.Combine(_localStatePath, "dismissed_updates.json");

        try
        {
            var json = JsonSerializer.Serialize(_dismissedVersions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving dismissed versions");
        }
    }

    #endregion
}
