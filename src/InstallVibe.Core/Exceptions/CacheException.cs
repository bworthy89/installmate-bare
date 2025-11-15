namespace InstallVibe.Core.Exceptions;

/// <summary>
/// Exception thrown during cache operations.
/// </summary>
public class CacheException : Exception
{
    public CacheException() : base() { }

    public CacheException(string message) : base(message) { }

    public CacheException(string message, Exception innerException) 
        : base(message, innerException) { }

    /// <summary>
    /// Gets or sets the cache key that caused the error.
    /// </summary>
    public string? CacheKey { get; set; }

    /// <summary>
    /// Gets or sets the cache operation that failed.
    /// </summary>
    public string? Operation { get; set; }
}
