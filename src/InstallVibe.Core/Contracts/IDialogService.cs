namespace InstallVibe.Core.Contracts;

/// <summary>
/// Service for displaying dialogs and user prompts.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an information dialog.
    /// </summary>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Shows a confirmation dialog.
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Shows an error dialog.
    /// </summary>
    Task ShowErrorAsync(string title, string message);

    /// <summary>
    /// Shows an input dialog.
    /// </summary>
    Task<string?> ShowInputAsync(string title, string message, string defaultValue = "");
}
