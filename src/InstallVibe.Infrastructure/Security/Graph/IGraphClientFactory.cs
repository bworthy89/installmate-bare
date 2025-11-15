using Microsoft.Graph;

namespace InstallVibe.Infrastructure.Security.Graph;

/// <summary>
/// Factory for creating authenticated Microsoft Graph clients.
/// </summary>
public interface IGraphClientFactory
{
    /// <summary>
    /// Creates an authenticated GraphServiceClient using certificate-based authentication.
    /// </summary>
    /// <returns>Configured GraphServiceClient instance.</returns>
    GraphServiceClient CreateClient();

    /// <summary>
    /// Validates that the certificate is available and valid.
    /// </summary>
    /// <returns>True if certificate is valid, false otherwise.</returns>
    Task<bool> ValidateCertificateAsync();

    /// <summary>
    /// Gets the certificate expiration date.
    /// </summary>
    /// <returns>Certificate expiration date, or null if certificate not found.</returns>
    DateTime? GetCertificateExpirationDate();
}
