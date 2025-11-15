using System.Security.Cryptography;
using System.Text;

namespace InstallVibe.Infrastructure.Security.Cryptography;

/// <summary>
/// Implements cryptographic hashing services using SHA256 and CRC16.
/// </summary>
public class HashService : IHashService
{
    /// <inheritdoc/>
    public string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = ComputeSha256(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <inheritdoc/>
    public byte[] ComputeSha256(byte[] input)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(input);
    }

    /// <inheritdoc/>
    public ushort ComputeCrc16(byte[] input)
    {
        const ushort polynomial = 0xA001; // CRC-16-ANSI
        ushort crc = 0xFFFF;

        foreach (byte b in input)
        {
            crc ^= b;
            for (int i = 0; i < 8; i++)
            {
                if ((crc & 0x0001) != 0)
                    crc = (ushort)((crc >> 1) ^ polynomial);
                else
                    crc = (ushort)(crc >> 1);
            }
        }

        return crc;
    }

    /// <inheritdoc/>
    public bool VerifyCrc16(byte[] data, ushort expectedChecksum)
    {
        var computed = ComputeCrc16(data);
        return computed == expectedChecksum;
    }
}
