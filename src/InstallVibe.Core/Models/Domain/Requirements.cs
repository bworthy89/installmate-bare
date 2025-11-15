namespace InstallVibe.Core.Models.Domain;

/// <summary>
/// Requirements for a guide.
/// </summary>
public class Requirements
{
    /// <summary>
    /// Hardware requirements.
    /// </summary>
    public List<string> Hardware { get; set; } = new();

    /// <summary>
    /// Software requirements.
    /// </summary>
    public List<string> Software { get; set; } = new();

    /// <summary>
    /// Network requirements.
    /// </summary>
    public List<string> Network { get; set; } = new();

    /// <summary>
    /// Credentials or accounts needed.
    /// </summary>
    public List<string> Credentials { get; set; } = new();

    /// <summary>
    /// Additional notes.
    /// </summary>
    public string? Notes { get; set; }
}
