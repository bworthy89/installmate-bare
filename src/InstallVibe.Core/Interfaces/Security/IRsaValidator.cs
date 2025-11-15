namespace InstallVibe.Core.Interfaces.Security;

/// <summary>
/// Provides RSA signature validation services.
/// </summary>
public interface IRsaValidator
{
    /// <summary>
    /// Verifies an RSA signature against the given data.
    /// </summary>
    /// <param name="data">The data that was signed.</param>
    /// <param name="signature">The RSA signature to verify.</param>
    /// <returns>True if the signature is valid.</returns>
    bool VerifySignature(byte[] data, byte[] signature);
}
