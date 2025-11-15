using CommunityToolkit.Mvvm.ComponentModel;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Shell;

/// <summary>
/// ViewModel for the main navigation shell.
/// </summary>
public partial class ShellViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;
    private readonly ILogger<ShellViewModel> _logger;

    [ObservableProperty]
    private string _title = "InstallVibe";

    public ShellViewModel(
        INavigationService navigationService,
        ILogger<ShellViewModel> logger)
    {
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
