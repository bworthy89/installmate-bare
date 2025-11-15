namespace InstallVibe.Core.Contracts;

/// <summary>
/// Service for navigation between pages and views.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to a page by type.
    /// </summary>
    bool NavigateTo(string pageKey, object? parameter = null);

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    bool GoBack();

    /// <summary>
    /// Checks if navigation back is possible.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Clears the navigation history.
    /// </summary>
    void ClearHistory();
}
