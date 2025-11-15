using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Guides;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Page showing guide details.
/// </summary>
public sealed partial class GuideDetailPage : Page
{
    public GuideDetailViewModel ViewModel { get; }

    public GuideDetailPage()
    {
        ViewModel = App.GetService<GuideDetailViewModel>();
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
