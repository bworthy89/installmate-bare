using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.Core.Models.Update;
using InstallVibe.Core.Services.Update;
using Microsoft.Extensions.Logging;

namespace InstallVibe.Controls;

/// <summary>
/// Control that displays an update notification banner.
/// </summary>
public sealed partial class UpdateBannerControl : UserControl
{
    private readonly IUpdateService _updateService;
    private readonly ILogger<UpdateBannerControl> _logger;
    private UpdateInfo? _currentUpdate;

    public bool IsOptionalUpdate { get; private set; } = true;

    public event EventHandler? UpdateDismissed;
    public event EventHandler<UpdateInfo>? UpdateRequested;

    public UpdateBannerControl()
    {
        this.InitializeComponent();

        _updateService = App.GetService<IUpdateService>();
        _logger = App.GetService<ILogger<UpdateBannerControl>>();

        // Subscribe to update service events
        _updateService.UpdateDownloadProgress += OnUpdateDownloadProgress;
        _updateService.UpdateReadyToApply += OnUpdateReadyToApply;
    }

    /// <summary>
    /// Shows the update banner with the given update information.
    /// </summary>
    public void ShowUpdate(UpdateInfo updateInfo)
    {
        if (updateInfo == null)
            return;

        _currentUpdate = updateInfo;

        // Set severity based on update type
        UpdateInfoBar.Severity = updateInfo.Type switch
        {
            UpdateType.Critical => InfoBarSeverity.Error,
            UpdateType.Recommended => InfoBarSeverity.Warning,
            _ => InfoBarSeverity.Informational
        };

        // Set title based on update type
        UpdateInfoBar.Title = updateInfo.Type switch
        {
            UpdateType.Critical => "Critical Update Required",
            UpdateType.Recommended => "Update Recommended",
            _ => "Update Available"
        };

        // Set message
        UpdateMessageText.Text = updateInfo.Type switch
        {
            UpdateType.Critical => "A critical security update is available. Please update immediately to ensure system security and stability.",
            UpdateType.Recommended => "A recommended update is available with important improvements and fixes.",
            _ => "A new version is available with enhancements and bug fixes."
        };

        // Set version information
        CurrentVersionText.Text = _updateService.GetCurrentVersion();
        NewVersionText.Text = updateInfo.Version;

        // Set release notes
        if (!string.IsNullOrEmpty(updateInfo.ReleaseNotes))
        {
            ReleaseNotesText.Text = updateInfo.ReleaseNotes;
            ReleaseNotesExpander.Visibility = Visibility.Visible;
        }
        else
        {
            ReleaseNotesExpander.Visibility = Visibility.Collapsed;
        }

        // Configure buttons based on update type
        IsOptionalUpdate = !updateInfo.IsMandatory && updateInfo.Type != UpdateType.Critical;
        UpdateInfoBar.IsClosable = IsOptionalUpdate;

        if (!IsOptionalUpdate)
        {
            UpdateLaterButton.Visibility = Visibility.Collapsed;
            UpdateMessageText.Text += " This update is mandatory and must be installed.";
        }

        // Show the banner
        UpdateInfoBar.IsOpen = true;

        _logger.LogInformation("Showing update banner for version {Version} (type: {Type})",
            updateInfo.Version, updateInfo.Type);
    }

    /// <summary>
    /// Hides the update banner.
    /// </summary>
    public void HideBanner()
    {
        UpdateInfoBar.IsOpen = false;
    }

    private async void UpdateNow_Click(object sender, RoutedEventArgs e)
    {
        if (_currentUpdate == null)
            return;

        _logger.LogInformation("User requested to update now");

        // Disable buttons
        UpdateNowButton.IsEnabled = false;
        UpdateLaterButton.IsEnabled = false;

        try
        {
            // Show download progress
            DownloadProgressPanel.Visibility = Visibility.Visible;
            DownloadProgressText.Text = "Preparing update...";

            // Apply the update (this will download and launch AppInstaller)
            var success = await _updateService.ApplyUpdateAsync(_currentUpdate);

            if (!success)
            {
                _logger.LogError("Failed to apply update");

                // Show error
                UpdateInfoBar.Severity = InfoBarSeverity.Error;
                UpdateInfoBar.Title = "Update Failed";
                UpdateMessageText.Text = "Failed to apply the update. Please try again or download manually.";

                // Re-enable buttons
                UpdateNowButton.IsEnabled = true;
                UpdateLaterButton.IsEnabled = true;
                DownloadProgressPanel.Visibility = Visibility.Collapsed;
            }
            // If successful, the app will be restarted by the update service
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying update");

            UpdateInfoBar.Severity = InfoBarSeverity.Error;
            UpdateInfoBar.Title = "Update Error";
            UpdateMessageText.Text = $"An error occurred: {ex.Message}";

            UpdateNowButton.IsEnabled = true;
            UpdateLaterButton.IsEnabled = true;
            DownloadProgressPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateLater_Click(object sender, RoutedEventArgs e)
    {
        if (_currentUpdate == null)
            return;

        _logger.LogInformation("User dismissed update {Version}", _currentUpdate.Version);

        // Dismiss the update
        _updateService.DismissUpdate(_currentUpdate.Version);

        // Hide the banner
        HideBanner();

        // Raise event
        UpdateDismissed?.Invoke(this, EventArgs.Empty);
    }

    private void ViewDetails_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to update details page or open release notes URL
        if (_currentUpdate != null)
        {
            ReleaseNotesExpander.IsExpanded = !ReleaseNotesExpander.IsExpanded;
        }
    }

    private void UpdateInfoBar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        if (_currentUpdate != null && IsOptionalUpdate)
        {
            _updateService.DismissUpdate(_currentUpdate.Version);
            UpdateDismissed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnUpdateDownloadProgress(object? sender, int progress)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            DownloadProgressBar.Value = progress;
            DownloadProgressText.Text = $"Downloading update... {progress}%";
        });
    }

    private void OnUpdateReadyToApply(object? sender, UpdateInfo updateInfo)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            DownloadProgressText.Text = "Update downloaded. Installing...";
        });
    }
}
