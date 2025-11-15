namespace InstallVibe.Core.Exceptions;

/// <summary>
/// Exception thrown during SharePoint operations.
/// </summary>
public class SharePointException : Exception
{
    public SharePointException() : base() { }

    public SharePointException(string message) : base(message) { }

    public SharePointException(string message, Exception innerException) 
        : base(message, innerException) { }

    /// <summary>
    /// Gets or sets the SharePoint endpoint that caused the error.
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code if applicable.
    /// </summary>
    public int? StatusCode { get; set; }
}
