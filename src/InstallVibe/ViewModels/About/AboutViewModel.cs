using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Services.Activation;
using Windows.ApplicationModel;

namespace InstallVibe.ViewModels.About;

/// <summary>
/// ViewModel for the about page.
/// </summary>
public partial class AboutViewModel : ObservableObject
{
    private readonly IActivationService _activationService;

    [ObservableProperty]
    private string _appName = "InstallVibe";

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private string _appDescription = "A Windows desktop application for guided installation workflows";

    [ObservableProperty]
    private string _copyright = $"© {DateTime.Now.Year} InstallVibe. All rights reserved.";

    [ObservableProperty]
    private string _licenseType = string.Empty;

    [ObservableProperty]
    private string _licenseStatus = string.Empty;

    [ObservableProperty]
    private bool _isLicensed = false;

    public AboutViewModel(IActivationService activationService)
    {
        _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
        LoadVersionInfo();
    }

    public async Task InitializeAsync()
    {
        await LoadLicenseInfoAsync();
    }

    private void LoadVersionInfo()
    {
        try
        {
            var package = Package.Current;
            var version = package.Id.Version;
            AppVersion = $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        catch
        {
            // Fallback for non-packaged scenarios
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            AppVersion = version != null ? $"Version {version.Major}.{version.Minor}.{version.Build}.{version.Revision}" : "Version 1.0.0.0";
        }
    }

    private async Task LoadLicenseInfoAsync()
    {
        try
        {
            var licenseInfo = await _activationService.GetLicenseInfoAsync();
            IsLicensed = licenseInfo.IsActivated;

            if (IsLicensed)
            {
                LicenseType = $"{licenseInfo.LicenseType} License";

                if (licenseInfo.IsPerpetual)
                {
                    LicenseStatus = "Perpetual License";
                }
                else if (licenseInfo.DaysRemaining.HasValue)
                {
                    LicenseStatus = $"Expires in {licenseInfo.DaysRemaining} days";
                }
                else
                {
                    LicenseStatus = "Active";
                }
            }
            else
            {
                LicenseType = "Unlicensed";
                LicenseStatus = "Not activated";
            }
        }
        catch
        {
            IsLicensed = false;
            LicenseType = "Unknown";
            LicenseStatus = "Unable to verify license";
        }
    }

    [RelayCommand]
    private async Task OpenGitHubAsync()
    {
        // Open GitHub repository or documentation
        await Windows.System.Launcher.LaunchUriAsync(new Uri("https://github.com/yourusername/installvibe"));
    }

    [RelayCommand]
    private async Task OpenLicenseAsync()
    {
        // Navigate to activation page or show license details
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        // Check for updates via update service
    }
}
