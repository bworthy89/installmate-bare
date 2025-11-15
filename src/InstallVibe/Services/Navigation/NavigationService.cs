using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;

namespace InstallVibe.Services.Navigation;

/// <summary>
/// Implements page navigation service.
/// </summary>
public class NavigationService : INavigationService
{
    private readonly ILogger<NavigationService> _logger;
    private readonly Dictionary<string, Type> _pages = new();

    public Frame? Frame { get; set; }

    public bool CanGoBack => Frame?.CanGoBack ?? false;

    public NavigationService(ILogger<NavigationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Registers a page type with a key.
    /// </summary>
    public void RegisterPage<TPage>(string pageKey) where TPage : Page
    {
        _pages[pageKey] = typeof(TPage);
        _logger.LogDebug("Registered page: {PageKey} -> {PageType}", pageKey, typeof(TPage).Name);
    }

    /// <inheritdoc/>
    public bool NavigateTo<TPage>(object? parameter = null) where TPage : Page
    {
        return NavigateTo(typeof(TPage), parameter);
    }

    /// <inheritdoc/>
    public bool NavigateTo(string pageKey, object? parameter = null)
    {
        if (!_pages.TryGetValue(pageKey, out var pageType))
        {
            _logger.LogWarning("Page key not found: {PageKey}", pageKey);
            return false;
        }

        return NavigateTo(pageType, parameter);
    }

    /// <inheritdoc/>
    public bool GoBack()
    {
        if (Frame?.CanGoBack == true)
        {
            Frame.GoBack();
            _logger.LogDebug("Navigated back");
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public void ClearHistory()
    {
        if (Frame != null)
        {
            Frame.BackStack.Clear();
            _logger.LogDebug("Navigation history cleared");
        }
    }

    private bool NavigateTo(Type pageType, object? parameter)
    {
        if (Frame == null)
        {
            _logger.LogError("Navigation frame is null");
            return false;
        }

        try
        {
            var result = Frame.Navigate(pageType, parameter);
            if (result)
            {
                _logger.LogInformation("Navigated to {PageType}", pageType.Name);
            }
            else
            {
                _logger.LogWarning("Navigation to {PageType} failed", pageType.Name);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to {PageType}", pageType.Name);
            return false;
        }
    }
}
