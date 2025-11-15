namespace InstallVibe.Core.Models.Activation;

/// <summary>
/// Represents the result of an activation attempt.
/// </summary>
public class ActivationResult
{
    /// <summary>
    /// Indicates whether the activation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if activation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code for programmatic handling.
    /// </summary>
    public ActivationErrorCode ErrorCode { get; set; }

    /// <summary>
    /// The activation token if successful.
    /// </summary>
    public ActivationToken? Token { get; set; }

    /// <summary>
    /// Creates a successful activation result.
    /// </summary>
    public static ActivationResult CreateSuccess(ActivationToken token)
    {
        return new ActivationResult
        {
            Success = true,
            Token = token,
            ErrorCode = ActivationErrorCode.None
        };
    }

    /// <summary>
    /// Creates a failed activation result.
    /// </summary>
    public static ActivationResult CreateFailure(ActivationErrorCode errorCode, string errorMessage)
    {
        return new ActivationResult
        {
            Success = false,
            ErrorCode = errorCode,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Error codes for activation failures.
/// </summary>
public enum ActivationErrorCode
{
    None = 0,
    InvalidFormat,
    InvalidChecksum,
    InvalidSignature,
    Expired,
    AlreadyActivated,
    MaxActivationsReached,
    NetworkError,
    NotFound,
    Revoked,
    Unknown
}
