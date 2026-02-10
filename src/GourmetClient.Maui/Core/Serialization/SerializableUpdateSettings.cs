using GourmetClient.Maui.Core.Settings;
using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Serialization;

internal class SerializableUpdateSettings
{
    public static SerializableUpdateSettings FromUpdateSettings(UpdateSettings updateSettings)
    {
        return new SerializableUpdateSettings
        {
            CheckForUpdates = updateSettings.CheckForUpdates
        };
    }

    [JsonPropertyName("CheckForUpdates")]
    public bool? CheckForUpdates { get; set; }

    public UpdateSettings ToUpdateSettings()
    {
        var updateSettings = new UpdateSettings();

        if (CheckForUpdates.HasValue)
        {
            updateSettings.CheckForUpdates = CheckForUpdates.Value;
        }

        return updateSettings;
    }
}