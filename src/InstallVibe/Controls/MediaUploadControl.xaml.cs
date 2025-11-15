using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using InstallVibe.ViewModels.Editor;
using InstallVibe.Core.Models.Domain;
using Windows.Storage.Pickers;
using System.Linq;

namespace InstallVibe.Controls;

/// <summary>
/// Control for uploading and managing media files.
/// </summary>
public sealed partial class MediaUploadControl : UserControl
{
    private StepEditorViewModel? _viewModel;

    public MediaUploadControl()
    {
        this.InitializeComponent();
        this.DataContextChanged += MediaUploadControl_DataContextChanged;
    }

    private void MediaUploadControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
        if (DataContext is StepEditorViewModel viewModel)
        {
            _viewModel = viewModel;
            MediaItemsControl.ItemsSource = viewModel.MediaItems;

            // Update empty state visibility
            viewModel.MediaItems.CollectionChanged += (s, e) =>
            {
                UpdateEmptyState();
            };

            UpdateEmptyState();
        }
    }

    private void UpdateEmptyState()
    {
        if (_viewModel == null) return;

        EmptyStatePanel.Visibility = _viewModel.MediaItems.Any()
            ? Visibility.Collapsed
            : Visibility.Visible;
    }

    private async void UploadImage_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        // Create file picker
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".webp");
        picker.FileTypeFilter.Add(".bmp");

        // Get the current window handle
        var window = App.Current.Services.GetRequiredService<MainWindow>();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ShowUploadProgress("Uploading image...");

            try
            {
                await _viewModel.UploadImageCommand.ExecuteAsync(file.Path);
            }
            finally
            {
                HideUploadProgress();
            }
        }
    }

    private async void UploadVideo_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null) return;

        // Create file picker
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".mp4");
        picker.FileTypeFilter.Add(".webm");
        picker.FileTypeFilter.Add(".mov");
        picker.FileTypeFilter.Add(".avi");

        // Get the current window handle
        var window = App.Current.Services.GetRequiredService<MainWindow>();
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            ShowUploadProgress("Uploading video...");

            try
            {
                await _viewModel.UploadVideoCommand.ExecuteAsync(file.Path);
            }
            finally
            {
                HideUploadProgress();
            }
        }
    }

    private async void DeleteMedia_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MediaReference media)
        {
            if (_viewModel != null)
            {
                await _viewModel.DeleteMediaCommand.ExecuteAsync(media);
            }
        }
    }

    private void MoveMediaUp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MediaReference media)
        {
            _viewModel?.MoveMediaUpCommand.Execute(media);
        }
    }

    private void MoveMediaDown_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is MediaReference media)
        {
            _viewModel?.MoveMediaDownCommand.Execute(media);
        }
    }

    private void ShowUploadProgress(string message)
    {
        UploadProgressText.Text = message;
        UploadProgressPanel.Visibility = Visibility.Visible;
    }

    private void HideUploadProgress()
    {
        UploadProgressPanel.Visibility = Visibility.Collapsed;
    }
}
