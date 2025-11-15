using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using InstallVibe.Core.Services.Activation;
using InstallVibe.Core.Models.Activation;

namespace InstallVibe.ViewModels.Activation;

/// <summary>
/// ViewModel for the product key activation page.
/// </summary>
public partial class ActivationViewModel : ObservableObject
{
    private readonly IActivationService _activationService;

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

    public ActivationViewModel(IActivationService activationService)
    {
        _activationService = activationService ?? throw new ArgumentNullException(nameof(activationService));
        _ = CheckActivationStatusAsync();
    }

    [RelayCommand]
    private async Task ActivateAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(ProductKey))
        {
            ErrorMessage = "Please enter a product key";
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
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Activation failed";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Activation error: {ex.Message}";
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
