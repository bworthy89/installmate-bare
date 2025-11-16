namespace InstallVibe.Core.Models.Settings;

/// <summary>
/// SharePoint connection settings.
/// </summary>
public class SharePointSettings
{
    public string? SiteUrl { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }

    /// <summary>
    /// Certificate thumbprint for certificate-based authentication (more secure).
    /// </summary>
    public string? CertificateThumbprint { get; set; }

    /// <summary>
    /// Client secret for secret-based authentication (simpler setup).
    /// Use either CertificateThumbprint OR ClientSecret, not both.
    /// </summary>
    public string? ClientSecret { get; set; }

    public bool UseAppOnlyAuth { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}
