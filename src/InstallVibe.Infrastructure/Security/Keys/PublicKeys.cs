namespace InstallVibe.Infrastructure.Security.Keys;

/// <summary>
/// Contains embedded RSA public keys for product key validation.
/// </summary>
/// <remarks>
/// SECURITY NOTE: This file contains ONLY the public key, which is safe to distribute.
/// The private key must NEVER be included in the application or source code.
/// 
/// To generate a new key pair:
/// 1. openssl genrsa -out private_key.pem 2048
/// 2. openssl rsa -in private_key.pem -pubout -out public_key.pem
/// 3. Copy the contents of public_key.pem below
/// 4. Store private_key.pem in secure offline storage
/// </remarks>
public static class PublicKeys
{
    /// <summary>
    /// RSA 2048-bit public key in PEM format for product key validation.
    /// </summary>
    /// <remarks>
    /// This is a SAMPLE public key for demonstration purposes.
    /// 
    /// ⚠️  IMPORTANT: Replace this with your actual production public key!
    /// 
    /// This sample key pair was generated for demonstration and should NOT be used in production.
    /// The corresponding private key is included in tools/keygen/README.md for testing purposes only.
    /// </remarks>
    public const string RsaPublicKey = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAy8Dbv8prpJ/0kKhlGeJY
ozo2t60EG8EocNeafxXkMjXCj5ykYGgl2RbSD1GwRTNaXBaOOGDExJw5WBVbh/DG
uyWPPwFr3WTVQq2YzXxkVVUr8/m5mzxBZFX/jNXLb5z0/vHSfKLPWp5v3dN8o0h0
pu1D2r7H6PnFW1r/DpdlJ0BCXLB7R1cGH3qPy/5r0nCQKG7sKlmL0sL8t0IcJ7x5
FojXJR9TpD0EAZ6BYgkW5e3i7Cc4mNxKc4SQVZ/PFT3l5f+jFJCLfE1qWk1wHRYB
+5pj5qH9xC8k8VkYPmzVGN4LPU3tIdG2fNLQU0xKEBRq2MbJJLvTvZKpwvO8PlJ3
HQIDAQAB
-----END PUBLIC KEY-----";

    /// <summary>
    /// Key version identifier. Increment when rotating keys.
    /// </summary>
    public const int KeyVersion = 1;
}
