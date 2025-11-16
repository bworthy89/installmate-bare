using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Guides;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Page for displaying and interacting with guide steps.
/// </summary>
public sealed partial class StepPage : Page
{
    public StepViewModel ViewModel { get; }

    public StepPage()
    {
        ViewModel = App.GetService<StepViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string guideId)
        {
            await ViewModel.LoadGuideAsync(guideId);
        }
    }
}
