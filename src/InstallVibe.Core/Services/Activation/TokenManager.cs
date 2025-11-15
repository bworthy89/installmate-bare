using InstallVibe.Core.Models.Activation;
using InstallVibe.Core.Interfaces.Security;
using InstallVibe.Core.Interfaces.Device;
using System.Text.Json;

namespace InstallVibe.Core.Services.Activation;

/// <summary>
/// Manages activation token storage using DPAPI encryption.
/// </summary>
public class TokenManager : ITokenManager
{
    private readonly IDpapiEncryption _encryption;
    private readonly IDeviceIdProvider _deviceIdProvider;
    private readonly string _tokenFilePath;

    public TokenManager(
        IDpapiEncryption encryption,
        IDeviceIdProvider deviceIdProvider)
    {
        _encryption = encryption ?? throw new ArgumentNullException(nameof(encryption));
        _deviceIdProvider = deviceIdProvider ?? throw new ArgumentNullException(nameof(deviceIdProvider));

        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var installVibeFolder = Path.Combine(appDataPath, "InstallVibe", "Config");
        Directory.CreateDirectory(installVibeFolder);
        _tokenFilePath = Path.Combine(installVibeFolder, "activation.dat");
    }

    /// <inheritdoc/>
    public async Task SaveTokenAsync(ActivationToken token)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        try
        {
            // Serialize to JSON
            var json = JsonSerializer.Serialize(token, new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // Encrypt using DPAPI
            var encrypted = _encryption.Encrypt(json);

            // Write to file
            await File.WriteAllTextAsync(_tokenFilePath, encrypted);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to save activation token", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<ActivationToken?> LoadTokenAsync()
    {
        if (!File.Exists(_tokenFilePath))
            return null;

        try
        {
            // Read encrypted file
            var encrypted = await File.ReadAllTextAsync(_tokenFilePath);

            // Decrypt using DPAPI
            var json = _encryption.Decrypt(encrypted);

            // Deserialize from JSON
            var token = JsonSerializer.Deserialize<ActivationToken>(json);

            if (token == null)
                return null;

            // Verify machine binding
            var currentMachineId = _deviceIdProvider.GetMachineId();
            if (token.MachineId != currentMachineId)
            {
                // Token is from a different machine
                throw new InvalidOperationException("Activation token is bound to a different machine");
            }

            // Check if expired
            if (token.IsExpired)
            {
                return null;
            }

            return token;
        }
        catch (InvalidOperationException)
        {
            throw; // Re-throw machine binding errors
        }
        catch (Exception ex)
        {
            // Token file corrupted or tampered with
            throw new InvalidOperationException("Failed to load activation token. The token may be corrupted or invalid.", ex);
        }
    }

    /// <inheritdoc/>
    public async Task DeleteTokenAsync()
    {
        if (File.Exists(_tokenFilePath))
        {
            try
            {
                File.Delete(_tokenFilePath);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to delete activation token", ex);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsActivatedAsync()
    {
        try
        {
            var token = await LoadTokenAsync();
            return token != null && !token.IsExpired;
        }
        catch
        {
            return false;
        }
    }
}
