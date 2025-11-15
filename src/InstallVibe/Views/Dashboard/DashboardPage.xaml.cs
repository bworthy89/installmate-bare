using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using InstallVibe.ViewModels.Dashboard;

namespace InstallVibe.Views.Dashboard;

/// <summary>
/// Dashboard page showing new guides, in-progress guides, completed guides,
/// pinned guides, and quick actions.
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

    private void GuideCard_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is string guideId)
        {
            ViewModel.OpenGuideCommand.Execute(guideId);
        }
    }

    private void LastAccessedCard_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ViewModel.LastAccessedGuide != null)
        {
            ViewModel.OpenGuideCommand.Execute(ViewModel.LastAccessedGuide.GuideId);
        }
    }

    private void Card_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            VisualStateManager.GoToState(element as Control ?? this, "PointerOver", true);
        }
    }

    private void Card_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is FrameworkElement element)
        {
            VisualStateManager.GoToState(element as Control ?? this, "Normal", true);
        }
    }
}
