namespace GourmetClient.Maui.Core.Settings;

public record UpdateSettings
{
    public bool CheckForUpdates { get; set; } = true;
}