namespace InstallVibe.Core.Contracts;

/// <summary>
/// Service for displaying notifications to the user.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Shows an information notification.
    /// </summary>
    void ShowInformation(string message, string? title = null);

    /// <summary>
    /// Shows a success notification.
    /// </summary>
    void ShowSuccess(string message, string? title = null);

    /// <summary>
    /// Shows a warning notification.
    /// </summary>
    void ShowWarning(string message, string? title = null);

    /// <summary>
    /// Shows an error notification.
    /// </summary>
    void ShowError(string message, string? title = null);
}
