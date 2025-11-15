# Product Key Activation System - Design & Implementation

## Overview

InstallVibe uses a hybrid offline-first/online-fallback activation system based on RSA digital signatures. Product keys can be validated offline using an embedded public key, with optional online validation via SharePoint.

---

## Product Key Format

### Structure

```
Format: XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
        [   Payload   ][Signature ]

Example: JK7M9-3PQ8R-W5TYN-8HC4V-XBMLG
```

### Encoding

**Base58 Alphabet**: `123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz`
- Excludes: 0, O, I, l (ambiguous characters)
- Case-sensitive
- No special characters

### Payload Structure (12 bytes)

```
Offset  Size  Field              Description
------  ----  -----------------  ----------------------------------
0       1     License Type       0x01 = Tech, 0x02 = Admin
1       4     Expiration Date    Unix timestamp (UTC) or 0xFFFFFFFF
5       4     Customer ID        Unique customer identifier
9       1     Feature Flags      Bitfield for features
10      2     Checksum           CRC16 of bytes 0-9
```

### Signature (approx. 256 bytes → 10 chars Base58 compressed)

- RSA-2048 signature of payload
- Signed with private key (offline, never distributed)
- Verified with public key (embedded in app)
- Compressed representation in key format

**Note**: The 10-character signature is a representation/hash of the full RSA signature, not the full signature itself. The actual signature is validated separately during the parsing process.

---

## RSA Key Generation (External Process)

### Generate Keys (Once, Offline)

```bash
# Generate 2048-bit RSA private key
openssl genrsa -out private_key.pem 2048

# Extract public key
openssl rsa -in private_key.pem -pubout -out public_key.pem

# View public key
openssl rsa -pubin -in public_key.pem -text -noout
```

### Security Requirements

- **Private Key**:
  - Store on air-gapped machine
  - Encrypt with strong passphrase
  - Never commit to source control
  - Never include in application
  - Backup to secure offline storage
  - Access restricted to key generation personnel only

- **Public Key**:
  - Embedded in application source code
  - Part of the distributed application
  - Used for signature verification only

---

## Product Key Validation Logic

### Validation Flow

```
User enters product key
    |
    v
Parse key format (5 groups of 5 characters)
    |
    v
Decode Base58 -> Payload + Signature Hash
    |
    v
Verify CRC16 checksum in payload
    |
    v
Extract full signature from embedded lookup table
    |
    v
Verify RSA signature of payload
    |
    +-- Valid? --> Extract license info
    |                  |
    |                  v
    |              Check expiration date
    |                  |
    |                  +-- Not expired? --> SUCCESS
    |                  |
    |                  +-- Expired? --> FAIL (Expired)
    |
    +-- Invalid? --> Optional: Try online validation
                         |
                         +-- SharePoint lookup
                         |
                         +-- Valid in DB? --> SUCCESS
                         |
                         +-- Not found? --> FAIL (Invalid key)
```

### Validation Rules

1. **Format Check**: Must match `XXXXX-XXXXX-XXXXX-XXXXX-XXXXX`
2. **Character Check**: All characters must be in Base58 alphabet
3. **Checksum Verification**: CRC16 must match payload
4. **Signature Verification**: RSA signature must be valid
5. **Expiration Check**: Current date must be before expiration
6. **License Type Check**: Must be valid enum value (0x01 or 0x02)

### Offline vs Online Validation

**Offline (Primary)**:
- No network required
- RSA signature proves authenticity
- Instant validation
- Cannot be revoked without app update

**Online (Fallback)**:
- Network required
- SharePoint database lookup
- Slower validation
- Supports key revocation
- Tracks activation count

---

## Activation Token Structure

After successful validation, an activation token is created and stored locally.

### Token Model

```csharp
public class ActivationToken
{
    public string ProductKeyHash { get; set; }        // SHA256(original key)
    public LicenseType LicenseType { get; set; }      // Tech or Admin
    public DateTime? ExpirationDate { get; set; }     // Null = perpetual
    public string CustomerId { get; set; }            // Customer ID from key
    public List<string> EnabledFeatures { get; set; } // Feature flags
    public string MachineId { get; set; }             // Device binding hash
    public DateTime ValidatedDate { get; set; }       // When activated
    public bool OnlineValidation { get; set; }        // Was validated online?
    public string Signature { get; set; }             // Token signature (HMAC)
}
```

### Token Storage

**Location**: `%LOCALAPPDATA%\InstallVibe\activation.dat`

**Encryption**: Windows DPAPI (Data Protection API)
- Scope: CurrentUser
- Machine-bound
- Cannot be copied to another machine
- Automatically decrypts for current user

**Format**: JSON serialized, then DPAPI encrypted

```
Plaintext Token (JSON) --> DPAPI Encrypt --> Base64 --> activation.dat
```

---

## Device Binding Strategy

### Hardware Profile Hash (No PII)

To prevent key sharing, tokens are bound to the device using a hardware profile hash.

#### Components (Ordered by stability)

1. **Machine GUID** (Registry: `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography\MachineGuid`)
2. **Processor ID** (WMI: `Win32_Processor.ProcessorId`)
3. **Motherboard Serial** (WMI: `Win32_BaseBoard.SerialNumber`)
4. **MAC Address** (Primary network adapter, persistent)

#### Hash Algorithm

```
Components = MachineGuid + ProcessorId + MotherboardSerial + MACAddress
MachineId = SHA256(Components)[0..16]  // First 16 bytes as hex string
```

#### Privacy Considerations

- **No PII**: No usernames, email addresses, or personal data
- **One-way hash**: Cannot reverse to get hardware info
- **Consistent**: Same hardware = same hash
- **Local only**: Never transmitted to server
- **GDPR compliant**: Hardware hash is not personal data under GDPR

#### Tolerance for Hardware Changes

- Changing RAM, GPU, storage: No impact (not included in hash)
- Changing motherboard or CPU: Requires reactivation
- Network adapter change: Requires reactivation (use most stable adapter)

---

## SharePoint Key Lookup Workflow

### SharePoint List Structure

**List Name**: `ProductKeys`

| Column | Type | Indexed | Description |
|--------|------|---------|-------------|
| KeyHash | Text | Yes | SHA256 hash of product key |
| LicenseType | Choice | No | Tech, Admin |
| CustomerId | Number | No | Customer identifier |
| ExpirationDate | DateTime | No | Expiration date or null |
| IsActive | Boolean | No | Can the key be used? |
| ActivationCount | Number | No | How many times activated |
| MaxActivations | Number | No | Max allowed activations |
| Features | Text | No | JSON array of enabled features |

### Online Validation Flow

```csharp
// Pseudocode
async Task<ActivationResult> ValidateOnlineAsync(string productKey)
{
    // 1. Hash the product key
    string keyHash = SHA256(productKey);

    // 2. Query SharePoint
    var item = await SharePoint.GetListItem("ProductKeys", keyHash);

    if (item == null)
        return ActivationResult.NotFound;

    // 3. Check if active
    if (!item.IsActive)
        return ActivationResult.Revoked;

    // 4. Check activation count
    if (item.ActivationCount >= item.MaxActivations)
        return ActivationResult.MaxActivationsReached;

    // 5. Check expiration
    if (item.ExpirationDate < DateTime.UtcNow)
        return ActivationResult.Expired;

    // 6. Increment activation count
    item.ActivationCount++;
    await SharePoint.UpdateListItem("ProductKeys", item);

    // 7. Create activation token
    var token = CreateToken(item);

    return ActivationResult.Success(token);
}
```

### When to Use Online Validation

- Offline validation fails (invalid signature)
- User explicitly chooses online activation
- Admin wants to track activations centrally
- Need to support key revocation

---

## Security Considerations

### Cryptographic Practices

1. **RSA-2048**
   - Industry standard key size
   - Secure until ~2030 (NIST recommendation)
   - Use PKCS#1 v2.1 (OAEP) padding for signing

2. **SHA-256 Hashing**
   - For product key hashing
   - For hardware profile hashing
   - Collision-resistant

3. **DPAPI Encryption**
   - Windows built-in encryption
   - Machine and user-bound
   - No key management required
   - Automatic key rotation by Windows

4. **CRC16 Checksum**
   - Detects accidental key corruption
   - NOT a security feature (easy to forge)
   - Supplements RSA signature

### Threat Model & Mitigations

| Threat | Mitigation |
|--------|-----------|
| Key generation without private key | RSA signature verification fails |
| Key sharing between users | Device binding prevents use on different machines |
| Token copied to another machine | DPAPI encryption is machine-bound, won't decrypt |
| Offline validation bypass | App integrity checks (code signing), no debug backdoors |
| Private key theft | Private key never in source code, air-gapped storage |
| Expired key reactivation | Expiration date in signed payload, cannot be modified |
| License upgrade (Tech→Admin) | License type in signed payload, cannot be modified |
| Man-in-the-middle (online validation) | HTTPS only, certificate pinning for SharePoint |

### What This System Does NOT Prevent

1. **Application Piracy**: Determined attackers can:
   - Reverse engineer the app
   - Patch out activation checks
   - Extract the public key and create a key generator
   - **Mitigation**: Code obfuscation, server-side features, regular updates

2. **Key Sharing (Limited)**: Users can:
   - Share a key with a small number of people (activation limit)
   - **Mitigation**: Online validation with activation count limits

3. **Offline Key Generation**: Someone with:
   - The embedded public key
   - The activation algorithm
   - Could create a key generator
   - **Mitigation**: Additional online checks for high-value features

### Best Practices Implemented

✅ **Defense in Depth**: Offline + online validation
✅ **Zero Knowledge**: No private key in application
✅ **User Privacy**: No PII in device binding
✅ **Fail Secure**: Invalid keys rejected by default
✅ **Audit Trail**: Online validation logs activations
✅ **Revocation Support**: Online validation can disable keys
✅ **Expiration Support**: Time-limited licenses
✅ **Type Safety**: Strong typing, no string manipulation of crypto

### Security Anti-Patterns AVOIDED

❌ **No hardcoded master keys**
❌ **No backdoor validation codes**
❌ **No client-side only validation** (hybrid approach)
❌ **No weak crypto** (no MD5, SHA1, or < 2048-bit RSA)
❌ **No PII in telemetry**
❌ **No plaintext token storage**

---

## Pseudocode

### ProductKeyService

```csharp
public class ProductKeyService
{
    private readonly IRsaValidator _rsaValidator;
    private readonly ITokenManager _tokenManager;
    private readonly ISharePointService _sharePointService;
    private readonly IDeviceIdProvider _deviceIdProvider;

    public async Task<ActivationResult> ActivateAsync(string productKey, bool forceOnline = false)
    {
        // 1. Validate format
        if (!IsValidFormat(productKey))
            return ActivationResult.InvalidFormat;

        // 2. Try offline validation first (unless forced online)
        if (!forceOnline)
        {
            var offlineResult = ValidateOffline(productKey);
            if (offlineResult.Success)
            {
                var token = CreateToken(offlineResult, isOnline: false);
                await _tokenManager.SaveTokenAsync(token);
                return ActivationResult.Success(token);
            }
        }

        // 3. Try online validation
        var onlineResult = await ValidateOnlineAsync(productKey);
        if (onlineResult.Success)
        {
            var token = CreateToken(onlineResult, isOnline: true);
            await _tokenManager.SaveTokenAsync(token);
            return ActivationResult.Success(token);
        }

        // 4. Both failed
        return ActivationResult.Failed;
    }

    private OfflineValidationResult ValidateOffline(string productKey)
    {
        // Parse key
        var parsed = ParseProductKey(productKey);
        if (parsed == null)
            return OfflineValidationResult.ParseFailed;

        // Verify checksum
        if (!VerifyChecksum(parsed.Payload))
            return OfflineValidationResult.ChecksumFailed;

        // Verify RSA signature
        if (!_rsaValidator.VerifySignature(parsed.Payload, parsed.Signature))
            return OfflineValidationResult.SignatureFailed;

        // Check expiration
        if (IsExpired(parsed.ExpirationDate))
            return OfflineValidationResult.Expired;

        return OfflineValidationResult.Success(parsed);
    }

    private ActivationToken CreateToken(ValidationResult result, bool isOnline)
    {
        var machineId = _deviceIdProvider.GetMachineId();

        return new ActivationToken
        {
            ProductKeyHash = HashProductKey(result.OriginalKey),
            LicenseType = result.LicenseType,
            ExpirationDate = result.ExpirationDate,
            CustomerId = result.CustomerId,
            EnabledFeatures = ParseFeatureFlags(result.FeatureFlags),
            MachineId = machineId,
            ValidatedDate = DateTime.UtcNow,
            OnlineValidation = isOnline,
            Signature = SignToken(...)  // HMAC of token data
        };
    }
}
```

---

## Implementation Notes

### File Organization

```
src/InstallVibe.Core/
├── Models/Activation/
│   ├── LicenseType.cs              (enum)
│   ├── ProductKey.cs               (parsed key model)
│   ├── ActivationToken.cs          (stored token model)
│   └── ActivationResult.cs         (result wrapper)
│
├── Services/Activation/
│   ├── IActivationService.cs
│   ├── ActivationService.cs        (main orchestration)
│   ├── IProductKeyValidator.cs
│   ├── ProductKeyValidator.cs      (parsing + validation)
│   ├── ITokenManager.cs
│   └── TokenManager.cs             (DPAPI storage)

src/InstallVibe.Infrastructure/
├── Security/Cryptography/
│   ├── IRsaValidator.cs
│   ├── RsaValidator.cs             (RSA verification)
│   ├── IHashService.cs
│   ├── HashService.cs              (SHA256, CRC16)
│   ├── IDpapiEncryption.cs
│   └── DpapiEncryption.cs          (Windows DPAPI)
│
├── Security/Keys/
│   ├── PublicKeys.cs               (embedded public key)
│   └── KeyLoader.cs                (load public key into RSA)
│
└── Device/
    ├── IDeviceIdProvider.cs
    └── DeviceIdProvider.cs         (hardware hash)
```

---

## Testing Strategy

### Unit Tests

- Product key parsing (valid/invalid formats)
- Checksum calculation and verification
- RSA signature verification (with test keys)
- Device ID generation (consistent, no PII)
- Token serialization/deserialization
- DPAPI encryption/decryption

### Integration Tests

- End-to-end activation flow
- Token persistence and retrieval
- Machine binding validation
- Expiration handling

### Security Tests

- Invalid signature rejection
- Expired key rejection
- Tampered payload detection
- Token tampering detection
- Cross-machine token usage (should fail)

---

## Future Enhancements

1. **Grace Period**: Allow N days after expiration for renewal
2. **Trial Keys**: Time-limited trial activations
3. **Floating Licenses**: Network-based license server
4. **Subscription Keys**: Monthly/yearly auto-renewal
5. **Feature Toggles**: Granular feature control via flags
6. **License Transfer**: Deactivate on one machine, activate on another
7. **Telemetry**: Anonymous usage statistics (opt-in)

---

## Compliance & Legal

### GDPR Compliance

- ✅ Hardware hash is not personal data (cannot identify individuals)
- ✅ Customer ID alone is not personal data (no names/emails)
- ✅ No PII collected or transmitted
- ✅ Local storage only (DPAPI encrypted)
- ⚠️ If combined with SharePoint customer database, customer ID becomes pseudonymous data
  - Requires privacy policy disclosure
  - User consent may be required depending on jurisdiction

### Software Licensing

This activation system is suitable for:
- ✅ Commercial software licensing
- ✅ Internal enterprise software
- ✅ SaaS activation codes
- ⚠️ Not suitable for GPL/open-source (source code disclosure would expose validation logic)

---

## References

- [NIST SP 800-57: Key Management](https://csrc.nist.gov/publications/detail/sp/800-57-part-1/rev-5/final)
- [RFC 8017: PKCS #1 v2.2 RSA Cryptography](https://datatracker.ietf.org/doc/html/rfc8017)
- [Windows DPAPI Documentation](https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata)
- [Base58 Encoding](https://en.bitcoin.it/wiki/Base58Check_encoding)
- [CRC-16 Implementation](https://www.codeproject.com/Articles/1626/Cyclic-Redundancy-Check-CRC-for-C-programmers)

---

**Document Version**: 1.0
**Last Updated**: 2025-01-15
**Author**: InstallVibe Development Team
