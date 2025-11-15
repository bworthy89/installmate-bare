using Microsoft.UI.Xaml.Controls;
using InstallVibe.Core.Models.Update;
using InstallVibe.Core.Services.Update;

namespace InstallVibe.Dialogs;

/// <summary>
/// Dialog that prompts the user to restart after an update is ready.
/// </summary>
public sealed partial class UpdateRestartDialog : ContentDialog
{
    private readonly IUpdateService _updateService;
    private readonly UpdateInfo _updateInfo;

    public bool RestartNow { get; private set; }

    public UpdateRestartDialog(UpdateInfo updateInfo)
    {
        this.InitializeComponent();

        _updateService = App.GetService<IUpdateService>();
        _updateInfo = updateInfo ?? throw new ArgumentNullException(nameof(updateInfo));

        // Set version information
        CurrentVersionText.Text = _updateService.GetCurrentVersion();
        NewVersionText.Text = updateInfo.Version;

        // Set XamlRoot for proper display
        this.XamlRoot = App.Current.Services.GetRequiredService<MainWindow>().Content.XamlRoot;
    }

    private async void PrimaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // User chose to restart now
        RestartNow = true;

        // Get deferral to perform async operation
        var deferral = args.GetDeferral();

        try
        {
            // Restart the application
            await _updateService.RestartApplicationAsync();
        }
        finally
        {
            deferral.Complete();
        }
    }

    private void SecondaryButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // User chose to restart later
        RestartNow = false;
    }
}
