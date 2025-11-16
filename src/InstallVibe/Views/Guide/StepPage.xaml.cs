using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Guides;
using InstallVibe.Converters;
using System.ComponentModel;

namespace InstallVibe.Views.Guides;

/// <summary>
/// Page for displaying and interacting with guide steps.
/// </summary>
public sealed partial class StepPage : Page
{
    public StepViewModel ViewModel { get; }

    public StepPage()
    {
        ViewModel = App.GetService<StepViewModel>();
        InitializeComponent();
        DataContext = ViewModel;

        // Subscribe to property changes to update markdown rendering
        ViewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is string guideId)
        {
            await ViewModel.LoadGuideAsync(guideId);
            RenderMarkdown();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.CurrentStep))
        {
            RenderMarkdown();
        }
    }

    private void RenderMarkdown()
    {
        if (ViewModel.CurrentStep?.Content != null)
        {
            MarkdownToXamlConverter.ConvertMarkdownToRichTextBlock(
                ViewModel.CurrentStep.Content,
                ContentRichTextBlock);
        }
    }
}
