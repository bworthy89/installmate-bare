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

    public GuidesViewModel(IGuideService guideService, INavigationService navigationService)
    {
        _guideService = guideService ?? throw new ArgumentNullException(nameof(guideService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
    }

    public async Task InitializeAsync()
    {
        await LoadGuidesAsync();
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

    private void ApplyFilter()
    {
        FilteredGuides.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? Guides
            : Guides.Where(g =>
                g.Metadata.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (g.Metadata.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

        foreach (var guide in filtered)
        {
            FilteredGuides.Add(guide);
        }
    }
}
