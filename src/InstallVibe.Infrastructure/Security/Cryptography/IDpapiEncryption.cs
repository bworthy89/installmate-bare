namespace InstallVibe.Infrastructure.Security.Cryptography;

/// <summary>
/// Provides Windows DPAPI encryption services.
/// </summary>
public interface IDpapiEncryption
{
    /// <summary>
    /// Encrypts data using DPAPI (CurrentUser scope).
    /// </summary>
    string Encrypt(string plaintext);

    /// <summary>
    /// Decrypts data using DPAPI.
    /// </summary>
    string Decrypt(string ciphertext);
}
