using CommunityToolkit.Mvvm.ComponentModel;
using InstallVibe.Core.Services.Data;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

namespace InstallVibe.ViewModels.Shell;

/// <summary>
/// ViewModel for the main navigation shell.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly IProgressService _progressService;
    private readonly ILogger<ShellViewModel> _logger;

    [ObservableProperty]
    private string _title = "InstallVibe";

    [ObservableProperty]
    private bool _canGoBack = false;

    [ObservableProperty]
    private int _inProgressCount = 0;

    [ObservableProperty]
    private bool _hasInProgress = false;

    [ObservableProperty]
    private bool _isAdminMode = true; // TODO: Replace with actual role check

    public ShellViewModel(
        INavigationService navigationService,
        IProgressService progressService,
        ILogger<ShellViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Initialize(Frame contentFrame)
    {
        _navigationService.Frame = contentFrame;

        // Subscribe to frame navigation events
        if (contentFrame != null)
        {
            contentFrame.Navigated += OnFrameNavigated;
        }

        // Load initial data
        _ = LoadInProgressCountAsync();
    }

    public void NavigateTo(string pageKey)
    {
        _navigationService.NavigateTo(pageKey);
    }

    public void GoBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
        }
    }

    private void OnFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        CanGoBack = _navigationService.CanGoBack;
    }

    private async Task LoadInProgressCountAsync()
    {
        try
        {
            // TODO: Get actual user ID from authentication service
            var userId = Environment.UserName;
            var progressList = await _progressService.GetAllProgressAsync(userId);
            var inProgressGuides = progressList.Where(p => p.CompletedDate == null).ToList();

            InProgressCount = inProgressGuides.Count;
            HasInProgress = InProgressCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load in-progress count");
        }
    }
}
