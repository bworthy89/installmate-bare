using Microsoft.Win32;
using InstallVibe.Core.Interfaces.Device;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace InstallVibe.Infrastructure.Device;

/// <summary>
/// Generates a stable machine identifier based on hardware profile.
/// Uses non-PII hardware characteristics: Machine GUID, Processor ID, Motherboard Serial, MAC Address.
/// </summary>
public class DeviceIdProvider : IDeviceIdProvider
{
    /// <inheritdoc/>
    public string GetMachineId()
    {
        try
        {
            var components = new List<string>
            {
                GetMachineGuid(),
                GetProcessorId(),
                GetMotherboardSerial(),
                GetMacAddress()
            };

            // Combine all components
            var combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));

            // Hash to create fixed-length identifier
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));

            // Return first 16 bytes as hex string
            return Convert.ToHexString(hashBytes[..16]).ToLowerInvariant();
        }
        catch
        {
            // Fallback to a less stable but still unique ID
            return GetFallbackId();
        }
    }

    private string GetMachineGuid()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var guid = key?.GetValue("MachineGuid") as string;
            return guid ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string GetProcessorId()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                var processorId = obj["ProcessorId"]?.ToString();
                if (!string.IsNullOrEmpty(processorId))
                    return processorId;
            }
        }
        catch
        {
            // Ignore WMI errors
        }

        return string.Empty;
    }

    private string GetMotherboardSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                var serial = obj["SerialNumber"]?.ToString();
                if (!string.IsNullOrEmpty(serial))
                    return serial;
            }
        }
        catch
        {
            // Ignore WMI errors
        }

        return string.Empty;
    }

    private string GetMacAddress()
    {
        try
        {
            var nics = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up
                         && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                         && !string.IsNullOrEmpty(n.GetPhysicalAddress()?.ToString()))
                .OrderBy(n => n.Name)
                .ToList();

            if (nics.Any())
            {
                return nics.First().GetPhysicalAddress().ToString();
            }
        }
        catch
        {
            // Ignore network errors
        }

        return string.Empty;
    }

    private string GetFallbackId()
    {
        // Last resort: use machine name + user name
        var fallback = $"{Environment.MachineName}|{Environment.UserName}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(fallback));
        return Convert.ToHexString(hashBytes[..16]).ToLowerInvariant();
    }
}
