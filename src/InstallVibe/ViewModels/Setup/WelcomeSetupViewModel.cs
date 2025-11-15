using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Setup;

/// <summary>
/// ViewModel for the welcome setup page (Step 1 of 2).
/// </summary>
public partial class WelcomeSetupViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<WelcomeSetupViewModel> _logger;

    public WelcomeSetupViewModel(
        INavigationService navigationService,
        ILogger<WelcomeSetupViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [RelayCommand]
    private void GetStarted()
    {
        _logger.LogInformation("User started setup wizard - navigating to license registration");
        _navigationService.NavigateTo("LicenseSetup");
    }
}
