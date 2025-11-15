namespace InstallVibe.Infrastructure.Configuration;

/// <summary>
/// SharePoint Online configuration settings.
/// </summary>
public class SharePointConfiguration
{
    /// <summary>
    /// Azure AD tenant ID (directory ID).
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD application (client) ID.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Certificate thumbprint (SHA-1 hash) for app-only authentication.
    /// Certificate must be in Windows Certificate Store (LocalMachine\My).
    /// </summary>
    public string CertificateThumbprint { get; set; } = string.Empty;

    /// <summary>
    /// SharePoint site URL (e.g., https://yourtenant.sharepoint.com/sites/InstallVibe).
    /// </summary>
    public string SiteUrl { get; set; } = string.Empty;

    /// <summary>
    /// Name of the document library containing guides (default: "Guides").
    /// </summary>
    public string GuideLibrary { get; set; } = "Guides";

    /// <summary>
    /// Name of the document library containing media (default: "Media").
    /// </summary>
    public string MediaLibrary { get; set; } = "Media";

    /// <summary>
    /// Name of the list containing guide index (default: "GuideIndex").
    /// </summary>
    public string GuideIndexList { get; set; } = "GuideIndex";

    /// <summary>
    /// Optional: List ID for product key validation list.
    /// If null, online product key validation is disabled.
    /// </summary>
    public string? ProductKeysListId { get; set; }

    /// <summary>
    /// Number of retry attempts for failed requests.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Initial retry delay in milliseconds (exponential backoff).
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of concurrent downloads.
    /// </summary>
    public int MaxConcurrentDownloads { get; set; } = 5;

    /// <summary>
    /// Cache guide index for this many minutes (sliding expiration).
    /// </summary>
    public int GuideIndexCacheMinutes { get; set; } = 15;

    /// <summary>
    /// Whether to enable verbose logging for Graph API calls.
    /// </summary>
    public bool EnableVerboseLogging { get; set; } = false;

    /// <summary>
    /// Chunk size for large file uploads (in MB).
    /// </summary>
    public int UploadChunkSizeMB { get; set; } = 10;

    /// <summary>
    /// Validates the configuration.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(TenantId))
            throw new InvalidOperationException("SharePoint TenantId is required");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("SharePoint ClientId is required");

        if (string.IsNullOrWhiteSpace(CertificateThumbprint))
            throw new InvalidOperationException("SharePoint CertificateThumbprint is required");

        if (string.IsNullOrWhiteSpace(SiteUrl))
            throw new InvalidOperationException("SharePoint SiteUrl is required");

        if (!Uri.TryCreate(SiteUrl, UriKind.Absolute, out var uri) || !uri.Host.Contains("sharepoint.com"))
            throw new InvalidOperationException($"Invalid SharePoint SiteUrl: {SiteUrl}");

        if (RetryCount < 0 || RetryCount > 10)
            throw new InvalidOperationException("RetryCount must be between 0 and 10");

        if (TimeoutSeconds < 5 || TimeoutSeconds > 300)
            throw new InvalidOperationException("TimeoutSeconds must be between 5 and 300");
    }
}
