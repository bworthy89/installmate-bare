using Microsoft.UI.Xaml;
using InstallVibe.Services.Navigation;

namespace InstallVibe.Views.Shell;

/// <summary>
/// Main application window
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly INavigationService _navigationService;

    public MainWindow(INavigationService navigationService)
    {
        _navigationService = navigationService;
        InitializeComponent();

        // Set up navigation frame
        _navigationService.Frame = RootFrame;

        // Navigate to activation page by default
        _navigationService.NavigateTo("Activation");
    }
}
