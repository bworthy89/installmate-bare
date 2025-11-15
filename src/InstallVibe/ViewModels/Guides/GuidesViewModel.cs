using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Models.Domain;
using InstallVibe.Core.Services.Data;
using InstallVibe.Services.Navigation;

namespace InstallVibe.ViewModels.Guides;

/// <summary>
/// ViewModel for the guides list page.
/// </summary>
public partial class GuidesViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Guide> _guides = new();

    [ObservableProperty]
    private ObservableCollection<Guide> _filteredGuides = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _hasError = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private Guide? _selectedGuide;

    [ObservableProperty]
    private bool _hasLastGuide = false;

    [ObservableProperty]
    private string _lastGuideTitle = string.Empty;

    [ObservableProperty]
    private int _lastGuideStep = 0;

    [ObservableProperty]
    private int _lastGuideTotalSteps = 0;

    [ObservableProperty]
    private double _lastGuideProgress = 0.0;

    private string _currentCategory = string.Empty;
    private string _currentSortType = "TitleAsc";

    public GuidesViewModel(IGuideService guideService, INavigationService navigationService)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public async Task InitializeAsync()
    {
        await LoadGuidesAsync();
        await LoadLastGuideAsync();
    }

    private async Task LoadLastGuideAsync()
    {
        try
        {
            // TODO: Load last guide from progress service
            // For now, just set to false
            HasLastGuide = false;
        }
        catch
        {
            HasLastGuide = false;
        }
    }

    [RelayCommand]
    private async Task LoadGuidesAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = string.Empty;

        try
        {
            var guides = await _guideService.GetAllGuidesAsync();

            Guides.Clear();
            foreach (var guide in guides.OrderBy(g => g.Metadata.Title))
            {
                Guides.Add(guide);
            }

            ApplyFilter();
        }
        catch (Exception ex)
        {
            HasError = true;
            ErrorMessage = $"Failed to load guides: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshGuidesAsync()
    {
        // Trigger sync with SharePoint
        await LoadGuidesAsync();
    }

    [RelayCommand]
    private void OpenGuide(Guide guide)
    {
        if (guide == null) return;

        SelectedGuide = guide;
        _navigationService.NavigateTo("GuideDetail", guide);
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private void ResumeLastGuide()
    {
        // TODO: Navigate to last guide with progress
        // For now, just open the first guide if available
        if (Guides.Any())
        {
            OpenGuide(Guides.First());
        }
    }

    public void FilterByCategory(string category)
    {
        _currentCategory = category;
        ApplyFilter();
    }

    public void SortBy(string sortType)
    {
        _currentSortType = sortType;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        FilteredGuides.Clear();

        var filtered = Guides.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(g =>
                g.Metadata.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (g.Metadata.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(_currentCategory))
        {
            filtered = filtered.Where(g =>
                g.Metadata.Category?.Equals(_currentCategory, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        // Apply sorting
        filtered = _currentSortType switch
        {
            "TitleAsc" => filtered.OrderBy(g => g.Metadata.Title),
            "TitleDesc" => filtered.OrderByDescending(g => g.Metadata.Title),
            "Updated" => filtered.OrderByDescending(g => g.Metadata.LastModified),
            "Popular" => filtered.OrderByDescending(g => g.Metadata.UsageCount),
            _ => filtered.OrderBy(g => g.Metadata.Title)
        };

        foreach (var guide in filtered)
        {
            FilteredGuides.Add(guide);
        }
    }
}
