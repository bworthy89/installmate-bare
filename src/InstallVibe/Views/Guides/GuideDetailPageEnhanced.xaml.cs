using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Guides;
using InstallVibe.Core.Models.Domain;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Enhanced guide detail page optimized for technician usability.
/// </summary>
public sealed partial class GuideDetailPageEnhanced : Page
{
    public GuideDetailViewModel ViewModel { get; }

    public GuideDetailPageEnhanced()
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

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Play entrance animation
        PageEntranceStoryboard.Begin();

        // Set up keyboard accelerators
        SetupKeyboardShortcuts();
    }

    private void SetupKeyboardShortcuts()
    {
        // Keyboard shortcuts are already set via AccessKey in XAML
        // This method can be used for additional custom shortcuts if needed
    }
}
