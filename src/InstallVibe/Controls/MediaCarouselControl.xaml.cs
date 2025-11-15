using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using InstallVibe.Core.Models.Domain;

namespace InstallVibe.Controls;

/// <summary>
/// Media carousel control with zoom, navigation, and full screen support.
/// </summary>
public sealed partial class MediaCarouselControl : UserControl
{
    private ObservableCollection<MediaReference> _mediaItems = new();
    private int _currentIndex = 0;

    public MediaCarouselControl()
    {
        this.InitializeComponent();
        ImageScrollViewer.ViewChanged += ImageScrollViewer_ViewChanged;
        this.KeyDown += MediaCarouselControl_KeyDown;
    }

    #region Dependency Properties

    public static readonly DependencyProperty MediaItemsProperty =
        DependencyProperty.Register(
            nameof(MediaItems),
            typeof(ObservableCollection<MediaReference>),
            typeof(MediaCarouselControl),
            new PropertyMetadata(null, OnMediaItemsChanged));

    public ObservableCollection<MediaReference> MediaItems
    {
        get => (ObservableCollection<MediaReference>)GetValue(MediaItemsProperty);
        set => SetValue(MediaItemsProperty, value);
    }

    private static void OnMediaItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MediaCarouselControl control && e.NewValue is ObservableCollection<MediaReference> items)
        {
            control._mediaItems = items;
            control.UpdateDisplay();
        }
    }

    #endregion

    #region Properties

    public string ZoomPercentage => $"{(int)(ImageScrollViewer.ZoomFactor * 100)}%";

    public int CurrentIndex
    {
        get => _currentIndex + 1;
    }

    public int TotalCount => _mediaItems?.Count ?? 0;

    #endregion

    #region Display Methods

    private void UpdateDisplay()
    {
        if (_mediaItems == null || _mediaItems.Count == 0)
        {
            MediaImage.Source = null;
            return;
        }

        var currentItem = _mediaItems[_currentIndex];
        LoadImage(currentItem.Url);

        PreviousButton.IsEnabled = _currentIndex > 0;
        NextButton.IsEnabled = _currentIndex < _mediaItems.Count - 1;

        // Update property bindings
        Bindings.Update();
    }

    private void LoadImage(string url)
    {
        try
        {
            var bitmap = new BitmapImage(new Uri(url));
            MediaImage.Source = bitmap;
            FullScreenImage.Source = bitmap;
        }
        catch
        {
            // Handle image load error
        }
    }

    #endregion

    #region Navigation

    private void Previous_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            UpdateDisplay();
        }
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (_currentIndex < _mediaItems.Count - 1)
        {
            _currentIndex++;
            UpdateDisplay();
        }
    }

    #endregion

    #region Zoom Controls

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        var currentZoom = ImageScrollViewer.ZoomFactor;
        var newZoom = Math.Min(currentZoom * 1.25f, 5.0f);
        ImageScrollViewer.ChangeView(null, null, newZoom);
        FullScreenScrollViewer.ChangeView(null, null, newZoom);
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        var currentZoom = ImageScrollViewer.ZoomFactor;
        var newZoom = Math.Max(currentZoom / 1.25f, 0.5f);
        ImageScrollViewer.ChangeView(null, null, newZoom);
        FullScreenScrollViewer.ChangeView(null, null, newZoom);
    }

    private void ResetZoom_Click(object sender, RoutedEventArgs e)
    {
        ImageScrollViewer.ChangeView(null, null, 1.0f);
        FullScreenScrollViewer.ChangeView(null, null, 1.0f);
    }

    private void ImageScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
    {
        Bindings.Update();
    }

    #endregion

    #region Full Screen

    private void FullScreen_Click(object sender, RoutedEventArgs e)
    {
        FullScreenOverlay.Visibility = Visibility.Visible;
        MainCarousel.Visibility = Visibility.Collapsed;
    }

    private void CloseFullScreen_Click(object sender, RoutedEventArgs e)
    {
        FullScreenOverlay.Visibility = Visibility.Collapsed;
        MainCarousel.Visibility = Visibility.Visible;
    }

    #endregion

    #region Touch and Keyboard Support

    private void MediaImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        // Toggle between fit and 2x zoom
        var currentZoom = ImageScrollViewer.ZoomFactor;
        var newZoom = Math.Abs(currentZoom - 1.0f) < 0.1f ? 2.0f : 1.0f;
        ImageScrollViewer.ChangeView(null, null, newZoom);
    }

    private void FullScreenImage_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        var currentZoom = FullScreenScrollViewer.ZoomFactor;
        var newZoom = Math.Abs(currentZoom - 1.0f) < 0.1f ? 2.0f : 1.0f;
        FullScreenScrollViewer.ChangeView(null, null, newZoom);
    }

    private void MediaImage_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        // Enable manipulation for touch gestures
        (sender as UIElement)?.CapturePointer(e.Pointer);
    }

    private void MediaCarouselControl_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Left:
                Previous_Click(this, null);
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Right:
                Next_Click(this, null);
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Add:
            case Windows.System.VirtualKey.Number0 when e.KeyStatus.IsKeyReleased &&
                (Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control) &
                Windows.UI.Core.CoreVirtualKeyStates.Down) != 0:
                ZoomIn_Click(this, null);
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Subtract:
                ZoomOut_Click(this, null);
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Number0:
                ResetZoom_Click(this, null);
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.F:
                if (FullScreenOverlay.Visibility == Visibility.Collapsed)
                    FullScreen_Click(this, null);
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.Escape:
                if (FullScreenOverlay.Visibility == Visibility.Visible)
                    CloseFullScreen_Click(this, null);
                e.Handled = true;
                break;
        }
    }

    #endregion
}
