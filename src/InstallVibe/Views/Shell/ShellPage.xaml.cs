using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Shell;

namespace InstallVibe.Views.Shell;

/// <summary>
/// Main navigation shell with NavigationView
/// </summary>
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        // Get ViewModel from DI container
        ViewModel = App.GetService<ShellViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
