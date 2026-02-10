using GourmetClient.Maui.Core.Settings;
using System;
using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Serialization;

internal class SerializableGourmetClientSettings
{
    public static SerializableGourmetClientSettings FromGourmetClientSettings(GourmetClientSettings clientSettings)
    {
        return new SerializableGourmetClientSettings
        {
            Version = 1,
            UserSettings = SerializableUserSettings.FromUserSettings(clientSettings.UserSettings),
            UpdateSettings = SerializableUpdateSettings.FromUpdateSettings(clientSettings.UpdateSettings),
            WindowSettings = clientSettings.WindowSettings is not null 
                ? SerializableWindowSettings.FromWindowSettings(clientSettings.WindowSettings)
                : null
        };
    }

    [JsonPropertyName("Version")]
    public required int Version { get; set; }

    [JsonPropertyName("UserSettings")]
    public SerializableUserSettings? UserSettings { get; set; }

    [JsonPropertyName("WindowSettings")]
    public SerializableWindowSettings? WindowSettings { get; set; }

    [JsonPropertyName("UpdateSettings")]
    public SerializableUpdateSettings? UpdateSettings { get; set; }

    public GourmetClientSettings ToGourmetSettings()
    {
        if (Version is not 1)
        {
            throw new InvalidOperationException($"Unsupported version of serialized data: {Version}");
        }

        var settings = new GourmetClientSettings
        {
            WindowSettings = WindowSettings?.ToWindowSettings()
        };

        UserSettings? userSettings = UserSettings?.ToUserSettings();
        if (userSettings is not null)
        {
            settings.UserSettings = userSettings;
        }

        UpdateSettings? updateSettings = UpdateSettings?.ToUpdateSettings();
        if (updateSettings is not null)
        {
            settings.UpdateSettings = updateSettings;
        }

        return settings;
    }
}