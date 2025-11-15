namespace InstallVibe.Core.Models.Activation;

/// <summary>
/// Defines the type of license activation.
/// </summary>
public enum LicenseType : byte
{
    /// <summary>
    /// Technician license - read-only access to guides.
    /// </summary>
    Tech = 0x01,

    /// <summary>
    /// Administrator license - full access including guide editor.
    /// </summary>
    Admin = 0x02
}
