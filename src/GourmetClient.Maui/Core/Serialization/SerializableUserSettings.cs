using GourmetClient.Maui.Core.Settings;
using GourmetClient.Maui.Utils;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Serialization;

internal class SerializableUserSettings
{
    public static SerializableUserSettings FromUserSettings(UserSettings userSettings)
    {
        return new SerializableUserSettings
        {
            CacheValidityMinutes = (int)userSettings.CacheValidity.TotalMinutes,
            GourmetLoginUsername = userSettings.GourmetLoginUsername,
            GourmetLoginPassword = Encrypt(userSettings.GourmetLoginPassword),
            VentopayUsername = userSettings.VentopayUsername,
            VentopayPassword = Encrypt(userSettings.VentopayPassword)
        };
    }

    [JsonPropertyName("CacheValidityMinutes")]
    public int? CacheValidityMinutes { get; set; }

    [JsonPropertyName("GourmetLoginUsername")]
    public string? GourmetLoginUsername { get; set; }

    [JsonPropertyName("GourmetLoginPassword")]
    public string? GourmetLoginPassword { get; set; }

    [JsonPropertyName("VentopayUsername")]
    public string? VentopayUsername { get; set; }

    [JsonPropertyName("VentopayPassword")]
    public string? VentopayPassword { get; set; }

    public UserSettings ToUserSettings()
    {
        var userSettings = new UserSettings
        {
            GourmetLoginUsername = GourmetLoginUsername ?? string.Empty,
            GourmetLoginPassword = Decrypt(GourmetLoginPassword),
            VentopayUsername = VentopayUsername ?? string.Empty,
            VentopayPassword = Decrypt(VentopayPassword)
        };

        if (CacheValidityMinutes is > 0)
        {
            userSettings.CacheValidity = TimeSpan.FromMinutes(CacheValidityMinutes.Value);
        }

        return userSettings;
    }

    private static string Encrypt(string value)
    {
        return EncryptionHelper.Encrypt(value, GetEncryptionKey());
    }

    private static string Decrypt(string? encryptedText)
    {
        if (!string.IsNullOrEmpty(encryptedText))
        {
            try
            {
                return EncryptionHelper.Decrypt(encryptedText, GetEncryptionKey());
            }
            catch (Exception)
            {
            }
        }

        return string.Empty;
    }

    private static string GetEncryptionKey()
    {
        var machineName = Environment.MachineName;
        var userName = Environment.UserName;

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes($"{userName}@{machineName}"));

        return Encoding.UTF8.GetString(hash);
    }
}