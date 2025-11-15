using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Setup;

namespace InstallVibe.Views.Setup;

/// <summary>
/// Welcome page for installation setup wizard (Step 1 of 2).
/// </summary>
public sealed partial class WelcomeSetupPage : Page
{
    public WelcomeSetupViewModel ViewModel { get; }

    public WelcomeSetupPage()
    {
        ViewModel = App.GetService<WelcomeSetupViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
