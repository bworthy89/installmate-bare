namespace InstallVibe.Core.Exceptions;

/// <summary>
/// Exception thrown during product key activation failures.
/// </summary>
public class ActivationException : Exception
{
    public ActivationException() : base() { }

    public ActivationException(string message) : base(message) { }

    public ActivationException(string message, Exception innerException) 
        : base(message, innerException) { }

    /// <summary>
    /// Gets or sets the product key that failed activation.
    /// </summary>
    public string? ProductKey { get; set; }

    /// <summary>
    /// Gets or sets the activation error code.
    /// </summary>
    public string? ErrorCode { get; set; }
}
