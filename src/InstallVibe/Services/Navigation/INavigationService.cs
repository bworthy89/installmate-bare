using Microsoft.UI.Xaml.Controls;

namespace InstallVibe.Services.Navigation;

/// <summary>
/// Service for managing navigation between pages.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets or sets the navigation frame.
    /// </summary>
    Frame? Frame { get; set; }

    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <typeparam name="TPage">Type of page to navigate to.</typeparam>
    /// <param name="parameter">Optional navigation parameter.</param>
    /// <returns>True if navigation succeeded.</returns>
    bool NavigateTo<TPage>(object? parameter = null) where TPage : Page;

    /// <summary>
    /// Navigates to the specified page type by name.
    /// </summary>
    /// <param name="pageKey">Page key/name.</param>
    /// <param name="parameter">Optional navigation parameter.</param>
    /// <returns>True if navigation succeeded.</returns>
    bool NavigateTo(string pageKey, object? parameter = null);

    /// <summary>
    /// Navigates back if possible.
    /// </summary>
    /// <returns>True if navigation succeeded.</returns>
    bool GoBack();

    /// <summary>
    /// Whether back navigation is possible.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Clears the navigation history.
    /// </summary>
    void ClearHistory();
}
