using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Guides;
using InstallVibe.Core.Models.Domain;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Page for executing and navigating through a guide's steps.
/// </summary>
public sealed partial class GuideDetailPage : Page
{
    public GuideDetailViewModel ViewModel { get; }

    public GuideDetailPage()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<GuideDetailViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is Guide guide)
        {
            await ViewModel.InitializeAsync(guide);
        }
    }
}
