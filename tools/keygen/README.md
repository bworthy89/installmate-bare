# Product Key Generator

Standalone tool for generating RSA-signed product keys.

## Overview

This tool generates product keys with the format:
```
XXXXX-XXXXX-XXXXX-XXXXX-XXXXX
```

Each key contains:
- License type (Tech or Admin)
- Expiration date
- Customer ID
- RSA signature for offline validation

## Security

**IMPORTANT**: The RSA private key (`private_key.pem`) must NEVER be committed to source control or distributed with the application.

- Store private key offline on a secure machine
- Use a separate build machine for key generation
- Only the public key is embedded in the application

## Usage

```powershell
# Generate a Tech key
dotnet run --project KeyGenerator.csproj -- --type Tech --customer 12345

# Generate an Admin key with expiration
dotnet run --project KeyGenerator.csproj -- --type Admin --customer 12345 --expires 2025-12-31

# Generate perpetual Admin key
dotnet run --project KeyGenerator.csproj -- --type Admin --customer 12345 --perpetual
```

## Building the Tool

```powershell
dotnet build KeyGenerator.csproj
```

## Generating RSA Keys

```powershell
# Generate private key (2048-bit)
openssl genrsa -out private_key.pem 2048

# Extract public key
openssl rsa -in private_key.pem -pubout -out public_key.pem
```

The public key content should be embedded in:
`src/InstallVibe.Infrastructure/Security/Keys/PublicKeys.cs`
