using System.Security.Cryptography;
using InstallVibe.Core.Interfaces.Security;

namespace InstallVibe.Infrastructure.Security.Cryptography;

/// <summary>
/// Implements RSA signature validation using embedded public key.
/// </summary>
public class RsaValidator : IRsaValidator
{
    private readonly RSA _rsa;

    public RsaValidator()
    {
        _rsa = RSA.Create();
        LoadPublicKey();
    }

    /// <inheritdoc/>
    public bool VerifySignature(byte[] data, byte[] signature)
    {
        try
        {
            return _rsa.VerifyData(
                data,
                signature,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }
        catch
        {
            // Any exception during verification means invalid signature
            return false;
        }
    }

    private void LoadPublicKey()
    {
        var publicKeyPem = Keys.PublicKeys.RsaPublicKey;
        _rsa.ImportFromPem(publicKeyPem);
    }

    public void Dispose()
    {
        _rsa?.Dispose();
    }
}
