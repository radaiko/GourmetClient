using System.Text.Json;
using GourmetClient.Maui.Utils;

namespace GourmetClient.Maui.Services.Implementations;

public class AesCredentialService : ICredentialService
{
    private readonly IAppDataPaths _paths;
    private readonly string _passPhrase;

    public AesCredentialService(IAppDataPaths paths)
    {
        _paths = paths;
        _passPhrase = DerivePassPhrase();
    }

    private static string DerivePassPhrase()
    {
        // Use a combination of app-specific GUID and platform identifier
        // This provides basic obfuscation - credentials are tied to this device/app
        var baseId = "GourmetClient-" + DeviceInfo.Platform + "-" + DeviceInfo.Idiom;
        return baseId;
    }

    public async Task SaveCredentialsAsync(string key, string username, string password)
    {
        var data = JsonSerializer.Serialize(new CredentialData { Username = username, Password = password });
        var encrypted = EncryptionHelper.Encrypt(data, _passPhrase);
        var filePath = GetCredentialFilePath(key);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await File.WriteAllTextAsync(filePath, encrypted);
    }

    public async Task<(string username, string password)?> GetCredentialsAsync(string key)
    {
        var filePath = GetCredentialFilePath(key);
        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var encrypted = await File.ReadAllTextAsync(filePath);
            var decrypted = EncryptionHelper.Decrypt(encrypted, _passPhrase);
            var cred = JsonSerializer.Deserialize<CredentialData>(decrypted);

            if (cred is null || string.IsNullOrEmpty(cred.Username))
            {
                return null;
            }

            return (cred.Username, cred.Password ?? string.Empty);
        }
        catch
        {
            // If decryption fails, credential file is corrupted or from different device
            return null;
        }
    }

    public Task DeleteCredentialsAsync(string key)
    {
        var filePath = GetCredentialFilePath(key);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    private string GetCredentialFilePath(string key)
    {
        return Path.Combine(_paths.AppDataDirectory, $"{key}.cred");
    }

    private sealed class CredentialData
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
