using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Guides;
using System.Collections.Generic;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Enhanced guides page with filtering and resume capability.
/// </summary>
public sealed partial class GuidesPageEnhanced : Page
{
    public GuidesViewModel ViewModel { get; }

    public GuidesPageEnhanced()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<GuidesViewModel>();

        // Initialize loading skeletons
        var skeletonItems = new List<int> { 1, 2, 3, 4, 5 };
        LoadingSkeletons.ItemsSource = skeletonItems;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Play entrance animation
        PageEntranceStoryboard.Begin();

        // Set up keyboard shortcuts
        this.KeyDown += Page_KeyDown;
    }

    private void Page_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.F5)
        {
            ViewModel.RefreshGuidesCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void FilterCategory_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string category)
        {
            ViewModel.FilterByCategory(category);
        }
    }

    private void SortBy_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem item && item.Tag is string sortType)
        {
            ViewModel.SortBy(sortType);
        }
    }
}
