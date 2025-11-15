using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using InstallVibe.ViewModels.Editor;
using InstallVibe.Core.Models.Domain;
using Windows.ApplicationModel.DataTransfer;

namespace InstallVibe.Views.Editor;

/// <summary>
/// Page for creating and editing installation guides.
/// </summary>
public sealed partial class GuideEditorPage : Page
{
    public GuideEditorViewModel ViewModel { get; }

    private Step? _draggedStep;
    private Border? _dragOverBorder;

    public GuideEditorPage()
    {
        this.InitializeComponent();
        ViewModel = App.GetService<GuideEditorViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // e.Parameter can be a guide ID string to load existing guide
        var guideId = e.Parameter as string;
        await ViewModel.InitializeAsync(guideId);

        // Set up step editor
        if (StepEditorControl != null)
        {
            StepEditorControl.DataContext = ViewModel;
        }
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Set up keyboard shortcuts
        this.KeyDown += Page_KeyDown;
    }

    private void Page_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        // Ctrl+S: Save Draft
        if (e.Key == Windows.System.VirtualKey.S &&
            Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            ViewModel.SaveDraftCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+P: Preview
        else if (e.Key == Windows.System.VirtualKey.P &&
            Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down) &&
            !Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            ViewModel.TogglePreviewModeCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+Shift+P: Publish
        else if (e.Key == Windows.System.VirtualKey.P &&
            Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down) &&
            Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
        {
            ViewModel.PublishGuideCommand.Execute(null);
            e.Handled = true;
        }
        // Escape: Cancel
        else if (e.Key == Windows.System.VirtualKey.Escape)
        {
            ViewModel.CancelCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void StepCard_DragStarting(UIElement sender, DragStartingEventArgs args)
    {
        if (sender is Border border && border.DataContext is Step step)
        {
            _draggedStep = step;
            args.AllowedOperations = DataPackageOperation.Move;
            args.Data.RequestedOperation = DataPackageOperation.Move;

            // Set drag UI
            args.DragUI.SetContentFromDataPackage();
        }
    }

    private void StepCard_DragOver(object sender, DragEventArgs e)
    {
        if (_draggedStep == null)
            return;

        e.AcceptedOperation = DataPackageOperation.Move;
        e.DragUIOverride.IsCaptionVisible = false;
        e.DragUIOverride.IsGlyphVisible = false;

        // Highlight drop target
        if (sender is Border border)
        {
            if (_dragOverBorder != null && _dragOverBorder != border)
            {
                _dragOverBorder.BorderThickness = new Thickness(1);
                _dragOverBorder.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Resources["CardStrokeColorDefaultBrush"];
            }

            border.BorderThickness = new Thickness(2);
            border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Resources["DragOverBrush"];
            _dragOverBorder = border;
        }
    }

    private void StepCard_DragLeave(object sender, DragEventArgs e)
    {
        // Remove highlight
        if (sender is Border border)
        {
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Resources["CardStrokeColorDefaultBrush"];

            if (_dragOverBorder == border)
            {
                _dragOverBorder = null;
            }
        }
    }

    private async void StepCard_Drop(object sender, DragEventArgs e)
    {
        if (_draggedStep == null)
            return;

        // Remove highlight
        if (sender is Border border)
        {
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Resources["CardStrokeColorDefaultBrush"];

            if (border.DataContext is Step targetStep)
            {
                var oldIndex = ViewModel.Steps.IndexOf(_draggedStep);
                var newIndex = ViewModel.Steps.IndexOf(targetStep);

                if (oldIndex != newIndex && oldIndex >= 0 && newIndex >= 0)
                {
                    await ViewModel.ReorderStepAsync(oldIndex, newIndex);
                }
            }
        }

        _draggedStep = null;
        _dragOverBorder = null;
    }
}
