using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Guides;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Page showing list of available guides.
/// </summary>
public sealed partial class GuideListPage : Page
{
    public GuideListViewModel ViewModel { get; }

    public GuideListPage()
    {
        ViewModel = App.GetService<GuideListViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }
}
