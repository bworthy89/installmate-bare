using System.Security.Cryptography;
using System.Text;

namespace InstallVibe.Infrastructure.Security.Cryptography;

/// <summary>
/// Implements encryption using Windows Data Protection API (DPAPI).
/// Data is encrypted for the current user and machine, preventing cross-machine copying.
/// </summary>
public class DpapiEncryption : IDpapiEncryption
{
    /// <inheritdoc/>
    public string Encrypt(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            throw new ArgumentException("Plaintext cannot be null or empty", nameof(plaintext));

        try
        {
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var encryptedBytes = ProtectedData.Protect(
                plaintextBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encryptedBytes);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Failed to encrypt data using DPAPI", ex);
        }
    }

    /// <inheritdoc/>
    public string Decrypt(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            throw new ArgumentException("Ciphertext cannot be null or empty", nameof(ciphertext));

        try
        {
            var encryptedBytes = Convert.FromBase64String(ciphertext);
            var decryptedBytes = ProtectedData.Unprotect(
                encryptedBytes,
                optionalEntropy: null,
                scope: DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Failed to decrypt data using DPAPI. Data may have been tampered with or created on a different machine/user.", ex);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("Invalid ciphertext format", ex);
        }
    }
}
