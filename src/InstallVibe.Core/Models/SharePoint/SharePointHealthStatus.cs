namespace InstallVibe.Core.Models.SharePoint;

/// <summary>
/// SharePoint connection health status.
/// </summary>
public class SharePointHealthStatus
{
    /// <summary>
    /// Whether SharePoint is currently reachable.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Last successful connection timestamp.
    /// </summary>
    public DateTime? LastSuccessfulConnection { get; set; }

    /// <summary>
    /// Last connection attempt timestamp.
    /// </summary>
    public DateTime LastConnectionAttempt { get; set; }

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public int? ResponseTimeMs { get; set; }

    /// <summary>
    /// Whether authentication is valid.
    /// </summary>
    public bool IsAuthenticated { get; set; }

    /// <summary>
    /// Whether certificate is valid and not expired.
    /// </summary>
    public bool IsCertificateValid { get; set; }

    /// <summary>
    /// Certificate expiration date.
    /// </summary>
    public DateTime? CertificateExpirationDate { get; set; }

    /// <summary>
    /// Days until certificate expires.
    /// </summary>
    public int? DaysUntilCertificateExpiry
    {
        get
        {
            if (CertificateExpirationDate.HasValue)
            {
                return (int)(CertificateExpirationDate.Value - DateTime.UtcNow).TotalDays;
            }
            return null;
        }
    }

    /// <summary>
    /// Site URL being accessed.
    /// </summary>
    public string? SiteUrl { get; set; }

    /// <summary>
    /// Last error message if connection failed.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Number of consecutive failures.
    /// </summary>
    public int ConsecutiveFailures { get; set; }

    /// <summary>
    /// Whether write operations are available (Admin only).
    /// </summary>
    public bool CanWrite { get; set; }

    /// <summary>
    /// API permissions status.
    /// </summary>
    public List<string> GrantedPermissions { get; set; } = new();

    /// <summary>
    /// Overall health status.
    /// </summary>
    public HealthLevel HealthLevel
    {
        get
        {
            if (!IsOnline || !IsAuthenticated)
                return HealthLevel.Critical;

            if (!IsCertificateValid)
                return HealthLevel.Warning;

            if (DaysUntilCertificateExpiry.HasValue && DaysUntilCertificateExpiry.Value < 30)
                return HealthLevel.Warning;

            if (ConsecutiveFailures > 0)
                return HealthLevel.Warning;

            return HealthLevel.Healthy;
        }
    }
}

/// <summary>
/// Health level enumeration.
/// </summary>
public enum HealthLevel
{
    /// <summary>
    /// All systems operational.
    /// </summary>
    Healthy,

    /// <summary>
    /// Non-critical issues detected.
    /// </summary>
    Warning,

    /// <summary>
    /// Critical issues, service unavailable.
    /// </summary>
    Critical
}
