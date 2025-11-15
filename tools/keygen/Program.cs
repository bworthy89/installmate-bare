using System.Security.Cryptography;
using System.Text;

namespace KeyGenerator;

/// <summary>
/// Product Key Generator for InstallVibe.
/// 
/// ⚠️  SECURITY CRITICAL:  
/// This tool uses the RSA private key and must be run on a secure, offline machine.
/// Never distribute the private key or include it in the application.
/// 
/// Usage:
///   dotnet run -- --type Tech --customer 12345 --expires 2025-12-31
///   dotnet run -- --type Admin --customer 67890 --perpetual
/// </summary>
class Program
{
    private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

    static void Main(string[] args)
    {
        Console.WriteLine("=== InstallVibe Product Key Generator ===\n");

        // Parse arguments
        var licenseType = GetArgument(args, "--type", "Tech");
        var customerId = uint.Parse(GetArgument(args, "--customer", "10001"));
        var expiresArg = GetArgument(args, "--expires", null);
        var isPerpetual = Array.Exists(args, arg => arg == "--perpetual");

        // Validate license type
        byte licenseTypeByte = licenseType.ToLower() switch
        {
            "tech" => 0x01,
            "admin" => 0x02,
            _ => throw new ArgumentException("License type must be 'Tech' or 'Admin'")
        };

        // Parse expiration
        DateTime? expirationDate = null;
        if (!isPerpetual && !string.IsNullOrEmpty(expiresArg))
        {
            expirationDate = DateTime.Parse(expiresArg).ToUniversalTime();
        }

        // Generate key
        var productKey = GenerateProductKey(licenseTypeByte, customerId, expirationDate, 0x00);

        Console.WriteLine($"Product Key: {productKey}");
        Console.WriteLine($"License Type: {licenseType}");
        Console.WriteLine($"Customer ID: {customerId}");
        Console.WriteLine($"Expiration: {(expirationDate.HasValue ? expirationDate.Value.ToString("yyyy-MM-dd") : "Perpetual")}");
        Console.WriteLine("\n✅ Key generated successfully!");
        Console.WriteLine("\n⚠️  Store this key securely. It can only be generated once.");
    }

    static string GenerateProductKey(byte licenseType, uint customerId, DateTime? expirationDate, byte featureFlags)
    {
        // Build payload (12 bytes)
        var payload = new byte[12];

        // Byte 0: License Type
        payload[0] = licenseType;

        // Bytes 1-4: Expiration (Unix timestamp or 0xFFFFFFFF for perpetual)
        uint expirationUnix = expirationDate.HasValue
            ? (uint)((DateTimeOffset)expirationDate.Value).ToUnixTimeSeconds()
            : 0xFFFFFFFF;

        Buffer.BlockCopy(BitConverter.GetBytes(expirationUnix), 0, payload, 1, 4);

        // Bytes 5-8: Customer ID
        Buffer.BlockCopy(BitConverter.GetBytes(customerId), 0, payload, 5, 4);

        // Byte 9: Feature Flags
        payload[9] = featureFlags;

        // Bytes 10-11: CRC16 Checksum
        ushort checksum = ComputeCrc16(payload[..10]);
        Buffer.BlockCopy(BitConverter.GetBytes(checksum), 0, payload, 10, 2);

        // Sign payload with RSA private key
        var signature = SignPayload(payload);

        // Encode payload (15 chars Base58)
        var payloadEncoded = EncodeBase58(payload).PadRight(15, Base58Alphabet[0])[..15];

        // Create signature hash (10 chars Base58)
        var sigHash = ComputeSha256(signature);
        var sigHashEncoded = EncodeBase58(sigHash[..8]).PadRight(10, Base58Alphabet[0])[..10];

        // Format as XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
        var combined = payloadEncoded + sigHashEncoded;
        return $"{combined[..5]}-{combined[5..10]}-{combined[10..15]}-{combined[15..20]}-{combined[20..25]}";
    }

    static byte[] SignPayload(byte[] payload)
    {
        // ⚠️  SECURITY CRITICAL: Load private key from secure location
        // This is a DEMO implementation. In production:
        // 1. Store private_key.pem on an air-gapped machine
        // 2. Load it from a secure key vault
        // 3. Never commit it to source control

        var privateKeyPem = LoadPrivateKey();

        using var rsa = RSA.Create();
        rsa.ImportFromPem(privateKeyPem);

        return rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    static string LoadPrivateKey()
    {
        var keyPath = "private_key.pem";

        if (!File.Exists(keyPath))
        {
            Console.WriteLine("\n❌ ERROR: private_key.pem not found!");
            Console.WriteLine("Generate a key pair first:");
            Console.WriteLine("  openssl genrsa -out private_key.pem 2048");
            Console.WriteLine("  openssl rsa -in private_key.pem -pubout -out public_key.pem");
            Environment.Exit(1);
        }

        return File.ReadAllText(keyPath);
    }

    static ushort ComputeCrc16(byte[] data)
    {
        const ushort polynomial = 0xA001;
        ushort crc = 0xFFFF;

        foreach (byte b in data)
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

    static byte[] ComputeSha256(byte[] data)
    {
        using var sha256 = SHA256.Create();
        return sha256.ComputeHash(data);
    }

    static string EncodeBase58(byte[] input)
    {
        var intData = new System.Numerics.BigInteger(input.Reverse().Concat(new byte[] { 0 }).ToArray());
        var result = new StringBuilder();

        while (intData > 0)
        {
            var remainder = (int)(intData % 58);
            intData /= 58;
            result.Insert(0, Base58Alphabet[remainder]);
        }

        foreach (var b in input)
        {
            if (b == 0)
                result.Insert(0, Base58Alphabet[0]);
            else
                break;
        }

        return result.ToString();
    }

    static string GetArgument(string[] args, string name, string? defaultValue)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == name)
                return args[i + 1];
        }
        return defaultValue ?? throw new ArgumentException($"Missing required argument: {name}");
    }
}
