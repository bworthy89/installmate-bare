using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Services.Activation;
using InstallVibe.Core.Models.Activation;
using InstallVibe.Services.Navigation;
using Microsoft.Extensions.Logging;

namespace InstallVibe.ViewModels.Activation;

/// <summary>
/// ViewModel for the product key activation page (setup wizard step 2).
/// </summary>
public partial class ActivationViewModel : ObservableObject
{
    private readonly IActivationService _activationService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<ActivationViewModel> _logger;

    [ObservableProperty]
    private string _productKey = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isActivating = false;

    [ObservableProperty]
    private bool _isActivated = false;

    [ObservableProperty]
    private string _licenseInfo = string.Empty;

    public ActivationViewModel(
        IActivationService activationService,
        INavigationService navigationService,
        ILogger<ActivationViewModel> logger)
    {
        _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = CheckActivationStatusAsync();
    }

    [RelayCommand]
    private async Task ActivateAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ProductKey))
        {
            ErrorMessage = "Please enter a license key";
            return;
        }

        IsActivating = true;

        try
        {
            var result = await _activationService.ActivateAsync(ProductKey.Trim());

            if (result.Success && result.Token != null)
            {
                IsActivated = true;
                UpdateLicenseInfo(result.Token);
                ProductKey = string.Empty; // Clear the key from display

                _logger.LogInformation("Setup completed successfully - navigating to Dashboard");

                // Show success message briefly, then navigate to Dashboard
                await Task.Delay(2000);
                _navigationService.NavigateTo("Dashboard");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Setup failed. Please check your license key and try again.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Setup failed with exception");
            ErrorMessage = $"Setup error: {ex.Message}";
        }
        finally
        {
            IsActivating = false;
        }
    }

    [RelayCommand]
    private async Task DeactivateAsync()
    {
        try
        {
            await _activationService.DeactivateAsync();
            IsActivated = false;
            LicenseInfo = string.Empty;
            ProductKey = string.Empty;
            ErrorMessage = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Deactivation error: {ex.Message}";
        }
    }

    private async Task CheckActivationStatusAsync()
    {
        try
        {
            var licenseInfo = await _activationService.GetLicenseInfoAsync();
            IsActivated = licenseInfo.IsActivated;

            if (IsActivated)
            {
                LicenseInfo = $"{licenseInfo.LicenseType} License";

                if (licenseInfo.IsPerpetual)
                {
                    LicenseInfo += " (Perpetual)";
                }
                else if (licenseInfo.DaysRemaining.HasValue)
                {
                    LicenseInfo += $" (Expires in {licenseInfo.DaysRemaining} days)";
                }
            }
        }
        catch
        {
            IsActivated = false;
        }
    }

    private void UpdateLicenseInfo(ActivationToken token)
    {
        LicenseInfo = $"{token.LicenseType} License";

        if (token.IsPerpetual)
        {
            LicenseInfo += " (Perpetual)";
        }
        else if (token.DaysUntilExpiration.HasValue)
        {
            LicenseInfo += $" (Expires in {token.DaysUntilExpiration} days)";
        }
    }
}
