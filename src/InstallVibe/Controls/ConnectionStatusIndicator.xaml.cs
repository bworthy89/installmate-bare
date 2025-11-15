using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Networking.Connectivity;

namespace InstallVibe.Controls;

/// <summary>
/// Connection status indicator showing online/offline state and sync status.
/// </summary>
public sealed partial class ConnectionStatusIndicator : UserControl
{
    private DispatcherTimer _statusCheckTimer;

    public ConnectionStatusIndicator()
    {
        this.InitializeComponent();

        // Monitor network status
        NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

        // Start periodic status check
        _statusCheckTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(30)
        };
        _statusCheckTimer.Tick += StatusCheckTimer_Tick;
        _statusCheckTimer.Start();

        UpdateStatus();
    }

    #region Properties

    public bool IsOnline { get; private set; } = true;
    public bool IsSyncing { get; private set; } = false;

    public string StatusText => IsOnline ? (IsSyncing ? "Syncing" : "Online") : "Offline";

    public string StatusIcon => IsOnline ? "\uE753" : "\uF384"; // Globe or Airplane

    public string StatusTooltip => IsOnline
        ? "Connected to SharePoint. Guides are up to date."
        : "Working offline. Changes will sync when connection is restored.";

    public Brush StatusBackgroundBrush => IsOnline
        ? new SolidColorBrush(Color.FromArgb(26, 16, 124, 16))  // Light green
        : new SolidColorBrush(Color.FromArgb(26, 255, 140, 0)); // Light orange

    public Brush StatusBorderBrush => IsOnline
        ? new SolidColorBrush(Color.FromArgb(128, 16, 124, 16))
        : new SolidColorBrush(Color.FromArgb(128, 255, 140, 0));

    public Brush StatusForegroundBrush => IsOnline
        ? new SolidColorBrush(Color.FromArgb(255, 16, 124, 16))  // Dark green
        : new SolidColorBrush(Color.FromArgb(255, 204, 102, 0)); // Dark orange

    #endregion

    #region Status Monitoring

    private void NetworkInformation_NetworkStatusChanged(object sender)
    {
        DispatcherQueue.TryEnqueue(() => UpdateStatus());
    }

    private void StatusCheckTimer_Tick(object sender, object e)
    {
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var profile = NetworkInformation.GetInternetConnectionProfile();
        var previousStatus = IsOnline;

        IsOnline = profile != null &&
                   profile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;

        // Refresh bindings
        Bindings.Update();

        // Notify if status changed
        if (previousStatus != IsOnline)
        {
            StatusChanged?.Invoke(this, IsOnline);
        }
    }

    public void SetSyncingState(bool isSyncing)
    {
        IsSyncing = isSyncing;
        Bindings.Update();
    }

    #endregion

    #region Events

    public event EventHandler<bool> StatusChanged;

    #endregion
}
