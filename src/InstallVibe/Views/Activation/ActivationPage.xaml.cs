using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Activation;

namespace InstallVibe.Views.Activation;

public sealed partial class ActivationPage : Page
{
    public ActivationViewModel ViewModel { get; }

    public ActivationPage()
    {
        // Get ViewModel from DI container
        ViewModel = App.GetService<ActivationViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
