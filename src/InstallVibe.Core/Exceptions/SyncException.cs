namespace InstallVibe.Core.Exceptions;

/// <summary>
/// Exception thrown during sync operations.
/// </summary>
public class SyncException : Exception
{
    public SyncException() : base() { }

    public SyncException(string message) : base(message) { }

    public SyncException(string message, Exception innerException) 
        : base(message, innerException) { }

    /// <summary>
    /// Gets or sets the entity ID that failed to sync.
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Gets or sets the sync operation that failed.
    /// </summary>
    public string? SyncOperation { get; set; }
}
