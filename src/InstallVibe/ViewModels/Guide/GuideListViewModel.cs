using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Constants;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.Export;
using InstallVibe.Core.Services.OneDrive;
using InstallVibe.Core.Services.User;
using InstallVibe.Services.Navigation;
using InstallVibe.Views.Shell;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Storage.Pickers;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for the guide list page with filtering, sorting, and dual view modes.
/// </summary>
public partial class GuideListViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly INavigationService _navigationService;
    private readonly IUserService _userService;
    private readonly IGuideArchiveService _guideArchiveService;
    private readonly IOneDriveSyncService _oneDriveSyncService;
    private readonly ILogger<GuideListViewModel> _logger;

    private List<Guide> _allGuides = new();

    [ObservableProperty]
    private ObservableCollection<Guide> _guides = new();

    [ObservableProperty]
    private ObservableCollection<Guide> _filteredGuides = new();

    [ObservableProperty]
    private Guide? _selectedGuide;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private string _selectedDifficulty = "All";

    [ObservableProperty]
    private ObservableCollection<string> _selectedTags = new();

    [ObservableProperty]
    private string _sortBy = "Title";

    [ObservableProperty]
    private bool _sortAscending = true;

    [ObservableProperty]
    private bool _isGridView = true;

    [ObservableProperty]
    private string _filterSummary = string.Empty;

    [ObservableProperty]
    private bool _hasActiveFilters = false;

    [ObservableProperty]
    private bool _isAdmin = false;

    [ObservableProperty]
    private bool _isSyncingFromOneDrive = false;

    [ObservableProperty]
    private string _syncStatus = "Not synced";

    [ObservableProperty]
    private bool _oneDriveSyncEnabled = false;

    public List<string> AvailableCategories { get; private set; } = new();
    public List<string> AvailableDifficulties { get; private set; } = new();
    public List<string> AvailableTags { get; private set; } = new();
    public List<string> SortOptions { get; } = new() { "Title", "Date", "Difficulty", "Steps", "Time" };

    public GuideListViewModel(
        IGuideService guideService,
        INavigationService navigationService,
        IUserService userService,
        IGuideArchiveService guideArchiveService,
        IOneDriveSyncService oneDriveSyncService,
        ILogger<GuideListViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _guideArchiveService = guideArchiveService ?? throw new ArgumentNullException(nameof(guideArchiveService));
        _oneDriveSyncService = oneDriveSyncService ?? throw new ArgumentNullException(nameof(oneDriveSyncService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeFilterOptions();
        _ = LoadGuidesAsync();
        _ = CheckAdminStatusAsync();
        _ = CheckOneDriveSyncStatusAsync();
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

    private async Task CheckOneDriveSyncStatusAsync()
    {
        try
        {
            var settings = await _oneDriveSyncService.GetSettingsAsync();
            OneDriveSyncEnabled = settings.Enabled;

            if (settings.LastSyncTime.HasValue)
            {
                var timeSince = DateTime.UtcNow - settings.LastSyncTime.Value;
                if (timeSince.TotalMinutes < 60)
                {
                    SyncStatus = $"Last synced {(int)timeSince.TotalMinutes}m ago";
                }
                else if (timeSince.TotalHours < 24)
                {
                    SyncStatus = $"Last synced {(int)timeSince.TotalHours}h ago";
                }
                else
                {
                    SyncStatus = $"Last synced {settings.LastSyncTime.Value:MMM d}";
                }
            }
            else
            {
                SyncStatus = "Never synced";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking OneDrive sync status");
            OneDriveSyncEnabled = false;
            SyncStatus = "Sync unavailable";
        }
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedCategoryChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSelectedDifficultyChanged(string value)
    {
        ApplyFilters();
    }

    partial void OnSortByChanged(string value)
    {
        ApplySorting();
    }

    partial void OnSortAscendingChanged(bool value)
    {
        ApplySorting();
    }

    [RelayCommand]
    private async Task LoadGuidesAsync()
    {
        IsLoading = true;

        try
        {
            var guides = await _guideService.GetAllGuidesAsync();
            _allGuides = guides;
            Guides = new ObservableCollection<Guide>(guides);

            ExtractAvailableTags();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading guides");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SelectGuide(Guide guide)
    {
        if (guide != null)
        {
            _navigationService.NavigateTo("GuideDetail", guide.GuideId);
        }
    }

    [RelayCommand]
    private void GoBack()
    {
        _logger.LogInformation("Navigating back to Dashboard");
        _navigationService.NavigateTo("Dashboard");
    }

    [RelayCommand]
    private void CreateGuide()
    {
        _logger.LogInformation("Navigating to Guide Editor for new guide");
        _navigationService.NavigateTo("GuideEditor");
    }

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedCategory = "All";
        SelectedDifficulty = "All";
        SelectedTags.Clear();
        SearchText = string.Empty;
        SortBy = "Title";
        SortAscending = true;
    }

    [RelayCommand]
    private void ToggleView()
    {
        IsGridView = !IsGridView;
        _logger.LogInformation("Toggled view mode to: {ViewMode}", IsGridView ? "Grid" : "List");
    }

    [RelayCommand]
    private void ToggleTag(string tag)
    {
        if (SelectedTags.Contains(tag))
        {
            SelectedTags.Remove(tag);
        }
        else
        {
            SelectedTags.Add(tag);
        }
        ApplyFilters();
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        SelectedCategory = category;
    }

    [RelayCommand]
    private void SelectDifficulty(string difficulty)
    {
        SelectedDifficulty = difficulty;
    }

    [RelayCommand]
    private void ToggleSortDirection()
    {
        SortAscending = !SortAscending;
    }

    private void InitializeFilterOptions()
    {
        AvailableCategories = new List<string> { "All" };
        AvailableCategories.AddRange(GuideCategories.All);

        AvailableDifficulties = new List<string> { "All" };
        AvailableDifficulties.AddRange(GuideDifficulty.All);
    }

    private void ExtractAvailableTags()
    {
        var allTags = _allGuides
            .SelectMany(g => g.Tags)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        AvailableTags = allTags;
        OnPropertyChanged(nameof(AvailableTags));
    }

    private void ApplyFilters()
    {
        try
        {
            var filtered = _allGuides.AsEnumerable();

            // Filter by category
            if (SelectedCategory != "All")
            {
                filtered = filtered.Where(g => g.Category == SelectedCategory);
            }

            // Filter by difficulty
            if (SelectedDifficulty != "All")
            {
                filtered = filtered.Where(g => g.Difficulty == SelectedDifficulty);
            }

            // Filter by tags (guide must have ALL selected tags)
            if (SelectedTags.Any())
            {
                filtered = filtered.Where(g => SelectedTags.All(tag => g.Tags.Contains(tag)));
            }

            // Filter by search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(g =>
                    (g.Title?.ToLower().Contains(searchLower) ?? false) ||
                    (g.Description?.ToLower().Contains(searchLower) ?? false) ||
                    (g.Author?.ToLower().Contains(searchLower) ?? false) ||
                    g.Tags.Any(t => t.ToLower().Contains(searchLower)));
            }

            var result = filtered.ToList();
            FilteredGuides = new ObservableCollection<Guide>(result);

            ApplySorting();
            UpdateFilterSummary();
            UpdateHasActiveFilters();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters");
        }
    }

    private void ApplySorting()
    {
        try
        {
            if (!FilteredGuides.Any()) return;

            IEnumerable<Guide> sorted = SortBy switch
            {
                "Title" => SortAscending
                    ? FilteredGuides.OrderBy(g => g.Title)
                    : FilteredGuides.OrderByDescending(g => g.Title),

                "Date" => SortAscending
                    ? FilteredGuides.OrderBy(g => g.LastModified)
                    : FilteredGuides.OrderByDescending(g => g.LastModified),

                "Difficulty" => SortAscending
                    ? FilteredGuides.OrderBy(g => GetDifficultyOrder(g.Difficulty))
                    : FilteredGuides.OrderByDescending(g => GetDifficultyOrder(g.Difficulty)),

                "Steps" => SortAscending
                    ? FilteredGuides.OrderBy(g => g.StepCount)
                    : FilteredGuides.OrderByDescending(g => g.StepCount),

                "Time" => SortAscending
                    ? FilteredGuides.OrderBy(g => g.EstimatedMinutes ?? 0)
                    : FilteredGuides.OrderByDescending(g => g.EstimatedMinutes ?? 0),

                _ => FilteredGuides
            };

            FilteredGuides = new ObservableCollection<Guide>(sorted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying sorting");
        }
    }

    private void UpdateFilterSummary()
    {
        var filteredCount = FilteredGuides.Count;
        var totalCount = _allGuides.Count;

        if (filteredCount == totalCount)
        {
            FilterSummary = $"Showing all {totalCount} guides";
        }
        else
        {
            FilterSummary = $"Showing {filteredCount} of {totalCount} guides";
        }
    }

    private void UpdateHasActiveFilters()
    {
        HasActiveFilters = SelectedCategory != "All" ||
                          SelectedDifficulty != "All" ||
                          SelectedTags.Any() ||
                          !string.IsNullOrWhiteSpace(SearchText);
    }

    private static int GetDifficultyOrder(string difficulty)
    {
        return difficulty switch
        {
            "Easy" => 1,
            "Medium" => 2,
            "Hard" => 3,
            _ => 0
        };
    }

    [RelayCommand]
    private async Task ImportGuideAsync()
    {
        try
        {
            _logger.LogInformation("Starting guide import");

            // Create file picker
            var picker = new FileOpenPicker();
            var window = App.GetService<MainWindow>();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".ivguide");

            var file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                _logger.LogInformation("Import cancelled by user");
                return;
            }

            // Validate archive first
            var validation = await _guideArchiveService.ValidateArchiveAsync(file.Path);
            if (!validation.IsValid)
            {
                _logger.LogError("Archive validation failed: {Errors}", string.Join(", ", validation.Errors));
                // TODO: Show error dialog
                await ShowErrorDialogAsync("Invalid Archive",
                    $"The selected file is not a valid guide archive:\n\n{string.Join("\n", validation.Errors)}");
                return;
            }

            // Check for GUID conflict
            var options = new ImportOptions
            {
                ConflictResolution = ConflictResolution.Cancel
            };

            if (validation.GuidAlreadyExists)
            {
                // Show conflict resolution dialog
                var resolution = await ShowConflictDialogAsync(
                    validation.GuideTitle ?? "Unknown Guide",
                    validation.GuideId ?? "");

                if (resolution == ConflictResolution.Cancel)
                {
                    _logger.LogInformation("Import cancelled due to GUID conflict");
                    return;
                }

                options.ConflictResolution = resolution;
                options.RegenerateGuids = resolution == ConflictResolution.ImportAsCopy;
            }

            // Import guide
            var result = await _guideArchiveService.ImportGuideAsync(file.Path, options);

            if (result.Success)
            {
                _logger.LogInformation(
                    "Successfully imported guide {GuideId} with {MediaCount} media files",
                    result.ImportedGuideId,
                    result.MediaFilesImported);

                // Reload guide list
                await LoadGuidesAsync();

                // TODO: Show success dialog
                await ShowSuccessDialogAsync("Import Successful",
                    $"Guide imported successfully with {result.MediaFilesImported} media files.");
            }
            else
            {
                _logger.LogError("Import failed: {Error}", result.ErrorMessage);
                // TODO: Show error dialog
                await ShowErrorDialogAsync("Import Failed",
                    $"Failed to import guide:\n\n{result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing guide");
            // TODO: Show error dialog
            await ShowErrorDialogAsync("Import Error",
                $"An error occurred during import:\n\n{ex.Message}");
        }
    }

    private async Task<ConflictResolution> ShowConflictDialogAsync(string guideTitle, string guideId)
    {
        // Create dialog
        var dialog = new ContentDialog
        {
            Title = "Guide Already Exists",
            Content = $"A guide with the same ID already exists:\n\nTitle: {guideTitle}\nID: {guideId}\n\nWhat would you like to do?",
            PrimaryButtonText = "Overwrite",
            SecondaryButtonText = "Import as Copy",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close
        };

        // Get XamlRoot from main window
        var window = App.GetService<MainWindow>();
        dialog.XamlRoot = window.Content.XamlRoot;

        var result = await dialog.ShowAsync();

        return result switch
        {
            ContentDialogResult.Primary => ConflictResolution.Overwrite,
            ContentDialogResult.Secondary => ConflictResolution.ImportAsCopy,
            _ => ConflictResolution.Cancel
        };
    }

    private async Task ShowSuccessDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close
        };

        var window = App.GetService<MainWindow>();
        dialog.XamlRoot = window.Content.XamlRoot;

        await dialog.ShowAsync();
    }

    private async Task ShowErrorDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close
        };

        var window = App.GetService<MainWindow>();
        dialog.XamlRoot = window.Content.XamlRoot;

        await dialog.ShowAsync();
    }

    [RelayCommand]
    private async Task SyncFromOneDriveAsync()
    {
        if (!OneDriveSyncEnabled)
        {
            _logger.LogInformation("OneDrive sync is not enabled");
            await ShowErrorDialogAsync("OneDrive Sync Disabled",
                "OneDrive sync is not enabled. Please configure OneDrive settings first.");
            return;
        }

        IsSyncingFromOneDrive = true;
        SyncStatus = "Syncing...";

        try
        {
            _logger.LogInformation("Starting manual OneDrive sync");

            var result = await _oneDriveSyncService.SyncNowAsync();

            if (result.Success || result.FilesImported > 0)
            {
                if (result.FilesImported > 0)
                {
                    SyncStatus = $"Synced {result.FilesImported} guide{(result.FilesImported == 1 ? "" : "s")} at {DateTime.Now:HH:mm}";

                    _logger.LogInformation(
                        "OneDrive sync completed successfully: {Imported} guides imported in {Duration}s",
                        result.FilesImported,
                        result.Duration.TotalSeconds);

                    // Reload guide list
                    await LoadGuidesAsync();

                    // Show success message
                    await ShowSuccessDialogAsync("Sync Successful",
                        $"Successfully imported {result.FilesImported} guide{(result.FilesImported == 1 ? "" : "s")} from OneDrive.\n\n" +
                        $"Downloaded: {result.FilesDownloaded}\n" +
                        $"Imported: {result.FilesImported}\n" +
                        $"Failed: {result.FilesFailed}\n" +
                        $"Duration: {result.Duration.TotalSeconds:F1}s");
                }
                else
                {
                    SyncStatus = $"No new guides at {DateTime.Now:HH:mm}";
                    _logger.LogInformation("OneDrive sync completed with no new guides");
                }

                // Update sync status
                await CheckOneDriveSyncStatusAsync();
            }
            else
            {
                SyncStatus = "Sync failed";
                _logger.LogError("OneDrive sync failed: {Errors}", string.Join(", ", result.Errors));

                await ShowErrorDialogAsync("Sync Failed",
                    $"OneDrive sync failed:\n\n{string.Join("\n", result.Errors.Take(5))}");
            }
        }
        catch (Exception ex)
        {
            SyncStatus = "Sync error";
            _logger.LogError(ex, "Error during manual OneDrive sync");

            await ShowErrorDialogAsync("Sync Error",
                $"An error occurred during sync:\n\n{ex.Message}");
        }
        finally
        {
            IsSyncingFromOneDrive = false;
        }
    }
}
