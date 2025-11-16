using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Shell;

namespace InstallVibe.Views.Shell;

/// <summary>
/// Main navigation shell with NavigationView
/// </summary>
public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
        // Get ViewModel from DI container
        ViewModel = App.GetService<ShellViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Initialize the shell with the content frame
        ViewModel.Initialize(ContentFrame);

        // Navigate to default page (passed as parameter or Dashboard)
        var startPage = e.Parameter as string ?? "Dashboard";
        ViewModel.NavigateTo(startPage);
    }

    private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        if (args.InvokedItemContainer is NavigationViewItem item && item.Tag is string pageKey)
        {
            ViewModel.NavigateTo(pageKey);
        }
    }

    private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        ViewModel.GoBack();
    }
}
