using Microsoft.UI.Xaml;
using InstallVibe.Services.Navigation;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;

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

        // Set window size
        SetWindowSize(1280, 800);

        // Set up navigation frame
        _navigationService.Frame = RootFrame;

        // Navigate to activation page by default
        _navigationService.NavigateTo("Activation");
    }

    private void SetWindowSize(int width, int height)
    {
        var hWnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow != null)
        {
            appWindow.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });
        }
    }
}
