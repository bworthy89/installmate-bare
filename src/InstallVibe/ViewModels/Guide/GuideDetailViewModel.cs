using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Engine;
using InstallVibe.Core.Services.Export;
using InstallVibe.Core.Services.SharePoint;
using InstallVibe.Core.Services.User;
using InstallVibe.Services.Navigation;
using InstallVibe.Views.Shell;
using Microsoft.Extensions.Logging;
using Windows.Storage.Pickers;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for the guide detail page.
/// </summary>
public partial class GuideDetailViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly IGuideEngine _guideEngine;
    private readonly INavigationService _navigationService;
    private readonly IUserService _userService;
    private readonly ISharePointService _sharePointService;
    private readonly IGuideArchiveService _guideArchiveService;
    private readonly ILogger<GuideDetailViewModel> _logger;

    [ObservableProperty]
    private Guide? _guide;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isStarting = false;

    [ObservableProperty]
    private bool _isAdmin = false;

    public bool HasTags => Guide?.Tags?.Count > 0;
    public bool HasPrerequisites => Guide?.Prerequisites?.Count > 0;
    public bool HasSteps => Guide?.Steps?.Count > 0;

    partial void OnGuideChanged(Guide? value)
    {
        OnPropertyChanged(nameof(HasTags));
        OnPropertyChanged(nameof(HasPrerequisites));
        OnPropertyChanged(nameof(HasSteps));
    }

    public GuideDetailViewModel(
        IGuideService guideService,
        IGuideEngine guideEngine,
        INavigationService navigationService,
        IUserService userService,
        ISharePointService sharePointService,
        IGuideArchiveService guideArchiveService,
        ILogger<GuideDetailViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _guideEngine = guideEngine ?? throw new ArgumentNullException(nameof(guideEngine));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _sharePointService = sharePointService ?? throw new ArgumentNullException(nameof(sharePointService));
        _guideArchiveService = guideArchiveService ?? throw new ArgumentNullException(nameof(guideArchiveService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _ = CheckAdminStatusAsync();
    }

    private async Task CheckAdminStatusAsync()
    {
        try
        {
            IsAdmin = await _userService.IsAdminAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking admin status");
            IsAdmin = false;
        }
    }

    public async Task LoadGuideAsync(string guideId)
    {
        IsLoading = true;

        try
        {
            Guide = await _guideService.GetGuideAsync(guideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guide {GuideId}", guideId);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task StartGuideAsync()
    {
        if (Guide == null) return;

        IsStarting = true;

        try
        {
            // TODO: Get actual user ID from authentication service
            var userId = Environment.UserName; // Temporary: use Windows username
            await _guideEngine.StartGuideAsync(Guide.GuideId, userId);
            _navigationService.NavigateTo("Step", Guide.GuideId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting guide {GuideId}", Guide.GuideId);
        }
        finally
        {
            IsStarting = false;
        }
    }

    [RelayCommand]
    private void EditGuide()
    {
        if (Guide == null) return;

        _logger.LogInformation("Navigating to Guide Editor for guide {GuideId}", Guide.GuideId);
        _navigationService.NavigateTo("GuideEditor", Guide.GuideId);
    }

    [RelayCommand]
    private async Task DeleteGuideAsync()
    {
        if (Guide == null) return;

        try
        {
            _logger.LogInformation("Deleting guide {GuideId}", Guide.GuideId);

            // Delete from local database first
            await _guideService.DeleteGuideAsync(Guide.GuideId);
            _logger.LogInformation("Deleted guide locally: {GuideId}", Guide.GuideId);

            // Delete from SharePoint
            var deleteSuccess = await _sharePointService.DeleteGuideAsync(Guide.GuideId);

            if (deleteSuccess)
            {
                _logger.LogInformation("Deleted guide from SharePoint: {GuideId}", Guide.GuideId);
            }
            else
            {
                _logger.LogWarning("Failed to delete guide from SharePoint (offline or error): {GuideId}", Guide.GuideId);
            }

            // Navigate back to guide list after deletion
            _navigationService.NavigateTo("GuideList");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting guide {GuideId}", Guide.GuideId);
        }
    }

    [RelayCommand]
    private async Task ExportGuideAsync()
    {
        if (Guide == null) return;

        try
        {
            _logger.LogInformation("Exporting guide {GuideId}", Guide.GuideId);

            // Create file picker
            var picker = new FileSavePicker();
            var window = App.GetService<MainWindow>();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeChoices.Add("InstallVibe Guide", new[] { ".ivguide" });
            picker.SuggestedFileName = Guide.Title.Replace(" ", "_");

            var file = await picker.PickSaveFileAsync();
            if (file == null)
            {
                _logger.LogInformation("Export cancelled by user");
                return;
            }

            // Export guide
            var success = await _guideArchiveService.ExportGuideAsync(Guide.GuideId, file.Path);

            if (success)
            {
                _logger.LogInformation("Successfully exported guide to {Path}", file.Path);
                // TODO: Show success dialog
            }
            else
            {
                _logger.LogError("Failed to export guide {GuideId}", Guide.GuideId);
                // TODO: Show error dialog
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting guide {GuideId}", Guide?.GuideId);
            // TODO: Show error dialog
        }
    }
}
