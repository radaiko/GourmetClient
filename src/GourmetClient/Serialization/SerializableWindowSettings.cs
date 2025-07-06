using GourmetClient.Settings;

namespace GourmetClient.Serialization;

internal class SerializableWindowSettings
{
    public SerializableWindowSettings()
    {
        // Used for deserialization
    }

    public SerializableWindowSettings(WindowSettings windowSettings)
    {
        WindowPositionTop = windowSettings.WindowPositionTop;
        WindowPositionLeft = windowSettings.WindowPositionLeft;
        WindowWidth = windowSettings.WindowWidth;
        WindowHeight = windowSettings.WindowHeight;
    }

    public int? WindowPositionTop { get; set; }

    public int? WindowPositionLeft { get; set; }

    public int? WindowWidth { get; set; }

    public int? WindowHeight { get; set; }

    public WindowSettings? ToWindowSettings()
    {
        if (WindowPositionTop is null || WindowPositionLeft is null || WindowWidth is null || WindowHeight is null)
        {
            return null;
        }

        if (WindowWidth < 1 || WindowHeight < 1)
        {
            // Settings are not valid
            return null;
        }

        return new WindowSettings(WindowPositionTop.Value, WindowPositionLeft.Value, WindowWidth.Value, WindowHeight.Value);
    }
}