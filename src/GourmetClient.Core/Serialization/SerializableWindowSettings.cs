using GourmetClient.Settings;
using System.Text.Json.Serialization;

namespace GourmetClient.Serialization;

internal class SerializableWindowSettings
{
    public static SerializableWindowSettings FromWindowSettings(WindowSettings windowSettings)
    {
        return new SerializableWindowSettings
        {
            WindowPositionTop = windowSettings.WindowPositionTop,
            WindowPositionLeft = windowSettings.WindowPositionLeft,
            WindowWidth = windowSettings.WindowWidth,
            WindowHeight = windowSettings.WindowHeight
        };
    }

    [JsonPropertyName("WindowPositionTop")]
    public required int WindowPositionTop { get; set; }

    [JsonPropertyName("WindowPositionLeft")]
    public required int WindowPositionLeft { get; set; }

    [JsonPropertyName("WindowWidth")]
    public required int WindowWidth { get; set; }

    [JsonPropertyName("WindowHeight")]
    public required int WindowHeight { get; set; }

    public WindowSettings? ToWindowSettings()
    {
        if (WindowWidth < 1 || WindowHeight < 1)
        {
            // Settings are not valid
            return null;
        }

        return new WindowSettings(WindowPositionTop, WindowPositionLeft, WindowWidth, WindowHeight);
    }
}