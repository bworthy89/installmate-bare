using InstallVibe.ViewModels.Guides;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace InstallVibe.Views.Guides;

public sealed partial class GuideEditorPage : Page
{
    public GuideEditorViewModel ViewModel { get; }

    public GuideEditorPage()
    {
        ViewModel = App.GetService<GuideEditorViewModel>();
        InitializeComponent();
        DataContext = ViewModel;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // If a guide ID is passed, load it for editing
        if (e.Parameter is string guideId && !string.IsNullOrEmpty(guideId))
        {
            await ViewModel.LoadGuideAsync(guideId);
        }
        else
        {
            // Otherwise, start with a new guide
            await ViewModel.NewGuideCommand.ExecuteAsync(null);
        }
    }
}
