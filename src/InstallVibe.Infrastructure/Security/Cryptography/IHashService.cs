namespace InstallVibe.Infrastructure.Security.Cryptography;

/// <summary>
/// Provides cryptographic hashing services.
/// </summary>
public interface IHashService
{
    /// <summary>
    /// Computes SHA256 hash of the input string.
    /// </summary>
    string ComputeSha256(string input);

    /// <summary>
    /// Computes SHA256 hash of byte array.
    /// </summary>
    byte[] ComputeSha256(byte[] input);

    /// <summary>
    /// Computes CRC16 checksum of byte array.
    /// </summary>
    ushort ComputeCrc16(byte[] input);

    /// <summary>
    /// Verifies CRC16 checksum.
    /// </summary>
    bool VerifyCrc16(byte[] data, ushort expectedChecksum);
}
