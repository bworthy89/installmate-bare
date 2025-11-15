using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Dashboard;

namespace InstallVibe.Views.Dashboard;

/// <summary>
/// Dashboard page showing recent guides and quick actions.
/// </summary>
public sealed partial class DashboardPage : Page
{
    public DashboardViewModel ViewModel { get; }

    public DashboardPage()
    {
        ViewModel = App.GetService<DashboardViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
