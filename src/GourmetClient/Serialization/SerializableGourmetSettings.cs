using GourmetClient.Settings;
using System;
using System.Text.Json.Serialization;

namespace GourmetClient.Serialization;

internal class SerializableGourmetSettings
{
    public static SerializableGourmetSettings FromGourmetSettings(GourmetSettings settings)
    {
        return new SerializableGourmetSettings
        {
            Version = 1,
            UserSettings = SerializableUserSettings.FromUserSettings(settings.UserSettings),
            UpdateSettings = SerializableUpdateSettings.FromUpdateSettings(settings.UpdateSettings),
            WindowSettings = settings.WindowSettings != null ? SerializableWindowSettings.FromWindowSettings(settings.WindowSettings) : null
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

    public GourmetSettings ToGourmetSettings()
    {
        if (Version is not 1)
        {
            throw new InvalidOperationException($"Unsupported version of serialized data: {Version}");
        }

        var settings = new GourmetSettings
        {
            WindowSettings = WindowSettings?.ToWindowSettings()
        };

        var userSettings = UserSettings?.ToUserSettings();
        if (userSettings is not null)
        {
            settings.UserSettings = userSettings;
        }

        var updateSettings = UpdateSettings?.ToUpdateSettings();
        if (updateSettings is not null)
        {
            settings.UpdateSettings = updateSettings;
        }

        return settings;
    }
}