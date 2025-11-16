using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.User;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace InstallVibe.ViewModels.Dashboard;

/// <summary>
/// ViewModel for the dashboard page showing new guides, in-progress guides,
/// completed guides, pinned guides, and quick actions.
/// </summary>
public partial class DashboardViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly IProgressService _progressService;
    private readonly IFavoritesService _favoritesService;
    private readonly IUserService _userService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<DashboardViewModel> _logger;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<GuideCardViewModel> _newGuides = new();

    [ObservableProperty]
    private ObservableCollection<GuideCardViewModel> _inProgressGuides = new();

    [ObservableProperty]
    private ObservableCollection<GuideCardViewModel> _recentlyCompletedGuides = new();

    [ObservableProperty]
    private ObservableCollection<GuideCardViewModel> _pinnedGuides = new();

    [ObservableProperty]
    private GuideCardViewModel? _lastAccessedGuide;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasNewGuides = false;

    [ObservableProperty]
    private bool _hasInProgressGuides = false;

    [ObservableProperty]
    private bool _hasCompletedGuides = false;

    [ObservableProperty]
    private bool _hasPinnedGuides = false;

    [ObservableProperty]
    private bool _hasLastAccessed = false;

    [ObservableProperty]
    private bool _isFirstTimeUser = false;

    public DashboardViewModel(
        IGuideService guideService,
        IProgressService progressService,
        IFavoritesService favoritesService,
        IUserService userService,
        INavigationService navigationService,
        ILogger<DashboardViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _progressService = progressService ?? throw new ArgumentNullException(nameof(progressService));
        _favoritesService = favoritesService ?? throw new ArgumentNullException(nameof(favoritesService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ = LoadDashboardDataAsync();
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // Get current user
            var userId = await _userService.GetCurrentUserIdAsync();
            var userName = await _userService.GetCurrentUserNameAsync();
            UserName = userName;

            _logger.LogInformation("Loading dashboard for user {UserId}", userId);

            // Load all sections concurrently
            var newGuidesTask = LoadNewGuidesAsync(userId);
            var inProgressTask = LoadInProgressGuidesAsync(userId);
            var completedTask = LoadCompletedGuidesAsync(userId);
            var pinnedTask = LoadPinnedGuidesAsync(userId);
            var lastAccessedTask = LoadLastAccessedGuideAsync(userId);

            await Task.WhenAll(newGuidesTask, inProgressTask, completedTask, pinnedTask, lastAccessedTask);

            // Check if this is a first-time user (no content in any section)
            IsFirstTimeUser = !HasNewGuides && !HasInProgressGuides &&
                             !HasCompletedGuides && !HasPinnedGuides &&
                             !HasLastAccessed;

            _logger.LogInformation("Dashboard loaded successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
            ErrorMessage = "Failed to load dashboard data. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadNewGuidesAsync(string userId)
    {
        try
        {
            var guides = await _guideService.GetNewGuidesAsync(3);
            var progressList = await _progressService.GetAllProgressAsync(userId);
            var pinnedGuideIds = await _favoritesService.GetPinnedGuideIdsAsync(userId);

            var guideCards = new List<GuideCardViewModel>();

            foreach (var guide in guides)
            {
                var progress = progressList.FirstOrDefault(p => p.GuideId == guide.GuideId);
                var isPinned = pinnedGuideIds.Contains(guide.GuideId);
                guideCards.Add(GuideCardViewModel.FromGuide(guide, progress, isPinned));
            }

            NewGuides = new ObservableCollection<GuideCardViewModel>(guideCards);
            HasNewGuides = guideCards.Count > 0;

            _logger.LogDebug("Loaded {Count} new guides", guideCards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load new guides");
        }
    }

    private async Task LoadInProgressGuidesAsync(string userId)
    {
        try
        {
            var guides = await _guideService.GetInProgressGuidesAsync(userId);
            var progressList = await _progressService.GetAllProgressAsync(userId);
            var pinnedGuideIds = await _favoritesService.GetPinnedGuideIdsAsync(userId);

            var guideCards = new List<GuideCardViewModel>();

            foreach (var guide in guides)
            {
                var progress = progressList.FirstOrDefault(p => p.GuideId == guide.GuideId);
                var isPinned = pinnedGuideIds.Contains(guide.GuideId);
                guideCards.Add(GuideCardViewModel.FromGuide(guide, progress, isPinned));
            }

            InProgressGuides = new ObservableCollection<GuideCardViewModel>(guideCards);
            HasInProgressGuides = guideCards.Count > 0;

            _logger.LogDebug("Loaded {Count} in-progress guides", guideCards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load in-progress guides");
        }
    }

    private async Task LoadCompletedGuidesAsync(string userId)
    {
        try
        {
            var guides = await _guideService.GetCompletedGuidesAsync(userId, 3);
            var progressList = await _progressService.GetAllProgressAsync(userId);
            var pinnedGuideIds = await _favoritesService.GetPinnedGuideIdsAsync(userId);

            var guideCards = new List<GuideCardViewModel>();

            foreach (var guide in guides)
            {
                var progress = progressList.FirstOrDefault(p => p.GuideId == guide.GuideId);
                var isPinned = pinnedGuideIds.Contains(guide.GuideId);
                guideCards.Add(GuideCardViewModel.FromGuide(guide, progress, isPinned));
            }

            RecentlyCompletedGuides = new ObservableCollection<GuideCardViewModel>(guideCards);
            HasCompletedGuides = guideCards.Count > 0;

            _logger.LogDebug("Loaded {Count} recently completed guides", guideCards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load completed guides");
        }
    }

    private async Task LoadPinnedGuidesAsync(string userId)
    {
        try
        {
            var pinnedGuideIds = await _favoritesService.GetPinnedGuideIdsAsync(userId);
            var guides = await _guideService.GetGuidesByIdsAsync(pinnedGuideIds);
            var progressList = await _progressService.GetAllProgressAsync(userId);

            var guideCards = new List<GuideCardViewModel>();

            // Maintain the pinned order
            foreach (var guideId in pinnedGuideIds)
            {
                var guide = guides.FirstOrDefault(g => g.GuideId == guideId);
                if (guide != null)
                {
                    var progress = progressList.FirstOrDefault(p => p.GuideId == guide.GuideId);
                    guideCards.Add(GuideCardViewModel.FromGuide(guide, progress, isPinned: true));
                }
            }

            PinnedGuides = new ObservableCollection<GuideCardViewModel>(guideCards);
            HasPinnedGuides = guideCards.Count > 0;

            _logger.LogDebug("Loaded {Count} pinned guides", guideCards.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load pinned guides");
        }
    }

    private async Task LoadLastAccessedGuideAsync(string userId)
    {
        try
        {
            var allProgress = await _progressService.GetAllProgressAsync(userId);

            // Get the most recently accessed guide
            var lastProgress = allProgress
                .Where(p => p.CompletedDate == null) // Only in-progress guides
                .OrderByDescending(p => p.LastUpdated)
                .FirstOrDefault();

            if (lastProgress != null)
            {
                var guide = await _guideService.GetGuideAsync(lastProgress.GuideId);
                if (guide != null)
                {
                    var pinnedGuideIds = await _favoritesService.GetPinnedGuideIdsAsync(userId);
                    var isPinned = pinnedGuideIds.Contains(guide.GuideId);

                    LastAccessedGuide = GuideCardViewModel.FromGuide(guide, lastProgress, isPinned);
                    HasLastAccessed = true;

                    _logger.LogDebug("Loaded last accessed guide: {GuideId}", guide.GuideId);
                }
            }
            else
            {
                HasLastAccessed = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load last accessed guide");
        }
    }

    [RelayCommand]
    private void OpenGuide(string guideId)
    {
        _logger.LogInformation("Opening guide {GuideId}", guideId);
        _navigationService.NavigateTo("GuideDetail", guideId);
    }

    [RelayCommand]
    private async Task TogglePinGuide(string guideId)
    {
        try
        {
            var userId = await _userService.GetCurrentUserIdAsync();
            var isPinned = await _favoritesService.TogglePinAsync(userId, guideId);

            _logger.LogInformation("Guide {GuideId} {Action}", guideId, isPinned ? "pinned" : "unpinned");

            // Reload dashboard to reflect changes
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle pin for guide {GuideId}", guideId);
        }
    }

    [RelayCommand]
    private void BrowseAllGuides()
    {
        _logger.LogInformation("Navigating to guide list");
        _navigationService.NavigateTo("GuideList");
    }

    [RelayCommand]
    private void OpenSettings()
    {
        _logger.LogInformation("Navigating to settings");
        _navigationService.NavigateTo("Settings");
    }

    [RelayCommand]
    private void OpenAdminPanel()
    {
        _logger.LogInformation("Navigating to admin panel");
        _navigationService.NavigateTo("AdminEditor");
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        _logger.LogInformation("Refreshing dashboard");
        await LoadDashboardDataAsync();
    }
}
