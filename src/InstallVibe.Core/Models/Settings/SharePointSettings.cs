namespace InstallVibe.Core.Models.Settings;

/// <summary>
/// SharePoint connection settings.
/// </summary>
public class SharePointSettings
{
    public string? SiteUrl { get; set; }
    public string? TenantId { get; set; }
    public string? ClientId { get; set; }
    public string? CertificateThumbprint { get; set; }
    public bool UseAppOnlyAuth { get; set; } = true;
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}
