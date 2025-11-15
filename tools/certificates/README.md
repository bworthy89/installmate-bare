# Code Signing Certificates

This folder contains code signing certificates for MSIX packages.

## Certificate Requirements

- Valid code signing certificate (.pfx format)
- Must be trusted by Windows
- Should have extended validation (EV) for best results

## Security

**IMPORTANT**: Certificate files (.pfx, .cer) should NEVER be committed to source control.

- Add to .gitignore
- Store securely in a password manager or key vault
- Use strong password protection
- Limit access to authorized personnel only

## Certificate Storage

### Development
- Store certificate locally on build machine
- Use Windows Certificate Store for added security

### CI/CD
- Store in Azure Key Vault or similar
- Use GitHub Secrets or equivalent
- Inject during build pipeline

## Creating a Self-Signed Certificate (Testing Only)

For development and testing purposes only:

```powershell
# Create self-signed certificate
New-SelfSignedCertificate -Type Custom -Subject "CN=InstallVibe Test" `
    -KeyUsage DigitalSignature -FriendlyName "InstallVibe Test Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# Export to PFX
$cert = Get-ChildItem -Path Cert:\CurrentUser\My | Where-Object {$_.Subject -match "InstallVibe Test"}
$password = ConvertTo-SecureString -String "YourPassword" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "InstallVibeTest.pfx" -Password $password
```

**Note**: Self-signed certificates require users to install the certificate before installing the app. For production, use a proper code signing certificate from a trusted CA.

## Signing the Package

See `tools/scripts/sign.ps1` for signing automation.
