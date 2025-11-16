using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Constants;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Core.Services.User;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for the guide list page with filtering, sorting, and dual view modes.
/// </summary>
public partial class GuideListViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly INavigationService _navigationService;
    private readonly IUserService _userService;
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

    public List<string> AvailableCategories { get; private set; } = new();
    public List<string> AvailableDifficulties { get; private set; } = new();
    public List<string> AvailableTags { get; private set; } = new();
    public List<string> SortOptions { get; } = new() { "Title", "Date", "Difficulty", "Steps", "Time" };

    public GuideListViewModel(
        IGuideService guideService,
        INavigationService navigationService,
        IUserService userService,
        ILogger<GuideListViewModel> logger)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        InitializeFilterOptions();
        _ = LoadGuidesAsync();
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
}
