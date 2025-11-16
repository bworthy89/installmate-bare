using Azure.Identity;
using InstallVibe.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System.Security.Cryptography.X509Certificates;

namespace InstallVibe.Infrastructure.Security.Graph;

/// <summary>
/// Factory for creating authenticated Microsoft Graph clients using certificate-based authentication.
/// </summary>
public class GraphClientFactory : IGraphClientFactory
{
    private readonly SharePointConfiguration _configuration;
    private readonly ILogger<GraphClientFactory> _logger;
    private X509Certificate2? _certificate;
    private GraphServiceClient? _cachedClient;
    private readonly object _lockObject = new();

    public GraphClientFactory(
        SharePointConfiguration configuration,
        ILogger<GraphClientFactory> logger,
        bool skipValidation = false)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Validate configuration on construction (unless explicitly skipped)
        if (!skipValidation)
        {
            _configuration.Validate();
        }
    }

    /// <inheritdoc/>
    public GraphServiceClient CreateClient()
    {
        // Return cached client if available
        if (_cachedClient != null)
            return _cachedClient;

        lock (_lockObject)
        {
            // Double-check pattern
            if (_cachedClient != null)
                return _cachedClient;

            try
            {
                // Check which authentication method to use
                bool useClientSecret = !string.IsNullOrWhiteSpace(_configuration.ClientSecret);
                bool useCertificate = !string.IsNullOrWhiteSpace(_configuration.CertificateThumbprint);

                if (useClientSecret)
                {
                    // Use client secret authentication (simpler)
                    _logger.LogInformation("Using client secret authentication");

                    var credential = new ClientSecretCredential(
                        _configuration.TenantId,
                        _configuration.ClientId,
                        _configuration.ClientSecret);

                    _cachedClient = new GraphServiceClient(credential);

                    _logger.LogInformation("GraphServiceClient created successfully with client secret");
                }
                else if (useCertificate)
                {
                    // Use certificate authentication (more secure)
                    _logger.LogInformation("Using certificate authentication");

                    var certificate = LoadCertificate();
                    if (certificate == null)
                    {
                        throw new InvalidOperationException(
                            $"Certificate with thumbprint {_configuration.CertificateThumbprint} not found in certificate store");
                    }

                    _certificate = certificate;

                    // Validate certificate
                    if (!certificate.HasPrivateKey)
                    {
                        throw new InvalidOperationException(
                            "Certificate must have a private key for app-only authentication");
                    }

                    if (certificate.NotAfter < DateTime.UtcNow)
                    {
                        _logger.LogWarning(
                            "Certificate expired on {ExpirationDate}",
                            certificate.NotAfter);
                        throw new InvalidOperationException(
                            $"Certificate expired on {certificate.NotAfter:yyyy-MM-dd}");
                    }

                    if (certificate.NotAfter < DateTime.UtcNow.AddDays(30))
                    {
                        _logger.LogWarning(
                            "Certificate expires soon on {ExpirationDate}",
                            certificate.NotAfter);
                    }

                    // Create credential
                    var credential = new ClientCertificateCredential(
                        _configuration.TenantId,
                        _configuration.ClientId,
                        certificate);

                    // Create Graph client
                    _cachedClient = new GraphServiceClient(credential);

                    _logger.LogInformation(
                        "GraphServiceClient created successfully with certificate (expires: {ExpirationDate})",
                        certificate.NotAfter);
                }
                else
                {
                    throw new InvalidOperationException(
                        "No authentication method configured. Set either ClientSecret or CertificateThumbprint in configuration.");
                }

                return _cachedClient;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create GraphServiceClient");
                throw;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateCertificateAsync()
    {
        try
        {
            var certificate = LoadCertificate();
            if (certificate == null)
            {
                _logger.LogWarning(
                    "Certificate with thumbprint {Thumbprint} not found",
                    _configuration.CertificateThumbprint);
                return false;
            }

            if (!certificate.HasPrivateKey)
            {
                _logger.LogWarning("Certificate does not have a private key");
                return false;
            }

            if (certificate.NotAfter < DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Certificate expired on {ExpirationDate}",
                    certificate.NotAfter);
                return false;
            }

            _logger.LogInformation(
                "Certificate validation successful (Expires: {ExpirationDate})",
                certificate.NotAfter);

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating certificate");
            return false;
        }
    }

    /// <inheritdoc/>
    public DateTime? GetCertificateExpirationDate()
    {
        try
        {
            var certificate = LoadCertificate();
            return certificate?.NotAfter;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting certificate expiration date");
            return null;
        }
    }

    /// <summary>
    /// Loads the certificate from the Windows Certificate Store.
    /// </summary>
    private X509Certificate2? LoadCertificate()
    {
        // Return cached certificate if available
        if (_certificate != null)
            return _certificate;

        try
        {
            // Open LocalMachine certificate store
            using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            // Find certificate by thumbprint
            var certificates = store.Certificates.Find(
                X509FindType.FindByThumbprint,
                _configuration.CertificateThumbprint,
                validOnly: false);

            if (certificates.Count == 0)
            {
                _logger.LogWarning(
                    "Certificate not found in LocalMachine\\My store. Thumbprint: {Thumbprint}",
                    _configuration.CertificateThumbprint);

                // Try CurrentUser store as fallback
                using var userStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                userStore.Open(OpenFlags.ReadOnly);

                certificates = userStore.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    _configuration.CertificateThumbprint,
                    validOnly: false);

                if (certificates.Count == 0)
                {
                    _logger.LogError(
                        "Certificate not found in CurrentUser\\My store either. Thumbprint: {Thumbprint}",
                        _configuration.CertificateThumbprint);
                    return null;
                }

                _logger.LogInformation(
                    "Certificate found in CurrentUser\\My store (Thumbprint: {Thumbprint})",
                    _configuration.CertificateThumbprint);
            }
            else
            {
                _logger.LogInformation(
                    "Certificate found in LocalMachine\\My store (Thumbprint: {Thumbprint})",
                    _configuration.CertificateThumbprint);
            }

            var certificate = certificates[0];

            // Log certificate details
            _logger.LogInformation(
                "Certificate loaded: Subject={Subject}, Issuer={Issuer}, NotBefore={NotBefore}, NotAfter={NotAfter}, HasPrivateKey={HasPrivateKey}",
                certificate.Subject,
                certificate.Issuer,
                certificate.NotBefore,
                certificate.NotAfter,
                certificate.HasPrivateKey);

            return certificate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading certificate from store");
            return null;
        }
    }
}
