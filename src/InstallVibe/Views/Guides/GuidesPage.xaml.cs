using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Guides;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Page for browsing and selecting installation guides.
/// </summary>
public sealed partial class GuidesPage : Page
{
    public GuidesViewModel ViewModel { get; }

    public GuidesPage()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<GuidesViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }
}
