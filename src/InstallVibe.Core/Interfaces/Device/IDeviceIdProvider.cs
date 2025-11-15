namespace InstallVibe.Core.Interfaces.Device;

/// <summary>
/// Provides device identification based on hardware characteristics.
/// </summary>
public interface IDeviceIdProvider
{
    /// <summary>
    /// Gets a unique identifier for this machine based on hardware profile.
    /// </summary>
    /// <returns>A hex string representing the machine ID.</returns>
    string GetMachineId();
}
