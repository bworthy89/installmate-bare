using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Settings;

namespace InstallVibe.Views.Settings;

/// <summary>
/// Settings page for application configuration.
/// </summary>
public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

    public SettingsPage()
    {
        ViewModel = App.GetService<SettingsViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
