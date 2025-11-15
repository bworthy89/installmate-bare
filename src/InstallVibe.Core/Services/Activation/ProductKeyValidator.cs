using InstallVibe.Core.Models.Activation;
using InstallVibe.Core.Interfaces.Security;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Validates product keys using offline RSA signature verification and Base58 encoding.
/// 
/// Key Format: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX (25 chars, 5 groups)
/// Groups 1-3: Payload (15 chars Base58)
/// Groups 4-5: Signature hash (10 chars Base58)
/// 
/// Payload (12 bytes):
/// - License Type (1 byte)
/// - Expiration (4 bytes, Unix timestamp)
/// - Customer ID (4 bytes)
/// - Feature Flags (1 byte)
/// - CRC16 Checksum (2 bytes)
/// </summary>
public class ProductKeyValidator : IProductKeyValidator
{
    private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private const int PayloadSize = 12;
    private const int SignatureSize = 256; // RSA-2048 signature
    private static readonly Regex KeyFormatRegex = new(@"^[" + Base58Alphabet + "]{5}-[" + Base58Alphabet + "]{5}-[" + Base58Alphabet + "]{5}-[" + Base58Alphabet + "]{5}-[" + Base58Alphabet + "]{5}$");

    private readonly IRsaValidator _rsaValidator;
    private readonly IHashService _hashService;

    // Signature lookup table (maps 10-char hash to full signature for demo)
    // In production, this would be a more sophisticated mapping or embedded database
    private readonly Dictionary<string, byte[]> _signatureLookup = new();

    public ProductKeyValidator(IRsaValidator rsaValidator, IHashService hashService)
    {
        _rsaValidator = rsaValidator ?? throw new ArgumentNullException(nameof(rsaValidator));
        _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
    }

    /// <inheritdoc/>
    public bool IsValidFormat(string productKeyString)
    {
        if (string.IsNullOrWhiteSpace(productKeyString))
            return false;

        return KeyFormatRegex.IsMatch(productKeyString.Trim().ToUpperInvariant());
    }

    /// <inheritdoc/>
    public ProductKey? ParseAndValidate(string productKeyString)
    {
        var key = new ProductKey { OriginalKey = productKeyString };

        // 1. Validate format
        if (!IsValidFormat(productKeyString))
        {
            key.ValidationError = "Invalid product key format";
            return key;
        }

        try
        {
            // 2. Decode Base58
            var cleaned = productKeyString.Replace("-", "").Trim();
            var payloadPart = cleaned[..15];
            var signatureHashPart = cleaned[15..25];

            var payloadBytes = DecodeBase58(payloadPart);
            if (payloadBytes == null || payloadBytes.Length != PayloadSize)
            {
                key.ValidationError = "Invalid payload encoding";
                return key;
            }

            key.Payload = payloadBytes;

            // 3. Parse payload
            if (!ParsePayload(payloadBytes, key))
            {
                return key;
            }

            // 4. Verify checksum
            var checksumOffset = PayloadSize - 2;
            var dataToCheck = payloadBytes[..checksumOffset];
            var embeddedChecksum = BitConverter.ToUInt16(payloadBytes, checksumOffset);

            if (!_hashService.VerifyCrc16(dataToCheck, embeddedChecksum))
            {
                key.ValidationError = "Checksum verification failed";
                return key;
            }

            // 5. Get full signature from lookup/database
            // NOTE: In a real implementation, you would have a proper signature storage mechanism
            // For this demo, we'll create a deterministic signature based on the hash
            var signatureHash = signatureHashPart;
            var fullSignature = GetSignatureFromHash(signatureHash, payloadBytes);

            if (fullSignature == null)
            {
                key.ValidationError = "Signature not found - key may be invalid";
                return key;
            }

            key.Signature = fullSignature;

            // 6. Verify RSA signature
            if (!_rsaValidator.VerifySignature(payloadBytes, fullSignature))
            {
                key.ValidationError = "RSA signature verification failed";
                return key;
            }

            // 7. Check expiration
            if (key.ExpirationDate.HasValue && key.ExpirationDate.Value < DateTime.UtcNow)
            {
                key.ValidationError = $"License expired on {key.ExpirationDate.Value:d}";
                return key;
            }

            // Success!
            key.IsValid = true;
            return key;
        }
        catch (Exception ex)
        {
            key.ValidationError = $"Validation error: {ex.Message}";
            return key;
        }
    }

    private bool ParsePayload(byte[] payload, ProductKey key)
    {
        try
        {
            // Byte 0: License Type
            var licenseTypeByte = payload[0];
            if (!Enum.IsDefined(typeof(LicenseType), licenseTypeByte))
            {
                key.ValidationError = "Invalid license type";
                return false;
            }
            key.LicenseType = (LicenseType)licenseTypeByte;

            // Bytes 1-4: Expiration (Unix timestamp, big-endian)
            var expirationUnix = BitConverter.ToUInt32(payload, 1);
            if (expirationUnix == 0xFFFFFFFF)
            {
                key.ExpirationDate = null; // Perpetual
            }
            else
            {
                key.ExpirationDate = DateTimeOffset.FromUnixTimeSeconds(expirationUnix).UtcDateTime;
            }

            // Bytes 5-8: Customer ID
            key.CustomerId = BitConverter.ToUInt32(payload, 5);

            // Byte 9: Feature Flags
            key.FeatureFlags = payload[9];

            return true;
        }
        catch (Exception ex)
        {
            key.ValidationError = $"Failed to parse payload: {ex.Message}";
            return false;
        }
    }

    private byte[]? DecodeBase58(string input)
    {
        try
        {
            var result = new List<byte>();
            var intData = System.Numerics.BigInteger.Zero;

            foreach (char c in input)
            {
                var digit = Base58Alphabet.IndexOf(c);
                if (digit < 0)
                    return null;

                intData = intData * 58 + digit;
            }

            var bytes = Enumerable.Reverse(intData.ToByteArray()).ToArray();

            // Remove leading zeros
            int leadingZeros = input.TakeWhile(c => c == Base58Alphabet[0]).Count();
            result.AddRange(Enumerable.Repeat((byte)0, leadingZeros));
            result.AddRange(bytes);

            return result.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private string EncodeBase58(byte[] input)
    {
        var intData = new System.Numerics.BigInteger(input.Reverse().Concat(new byte[] { 0 }).ToArray());
        var result = new StringBuilder();

        while (intData > 0)
        {
            var remainder = (int)(intData % 58);
            intData /= 58;
            result.Insert(0, Base58Alphabet[remainder]);
        }

        // Add leading '1's for leading zero bytes
        foreach (var b in input)
        {
            if (b == 0)
                result.Insert(0, Base58Alphabet[0]);
            else
                break;
        }

        return result.ToString();
    }

    private byte[]? GetSignatureFromHash(string signatureHash, byte[] payload)
    {
        // NOTE: This is a simplified implementation for demonstration
        // In production, you would:
        // 1. Have a lookup table of valid signature hashes
        // 2. Query a database
        // 3. Or use a more sophisticated storage mechanism

        // For demo purposes, create a deterministic signature
        // In reality, this would come from the key generation process
        var dummySignature = new byte[SignatureSize];
        var hashBytes = Encoding.UTF8.GetBytes(signatureHash + Convert.ToHexString(payload));
        var hash = _hashService.ComputeSha256(hashBytes);

        // Fill signature with pattern based on hash (this is NOT a real signature!)
        for (int i = 0; i < SignatureSize; i++)
        {
            dummySignature[i] = hash[i % hash.Length];
        }

        return dummySignature;
    }
}
