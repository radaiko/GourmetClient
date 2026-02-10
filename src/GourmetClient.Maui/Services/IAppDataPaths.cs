namespace GourmetClient.Maui.Services;

public interface IAppDataPaths
{
    string AppDataDirectory { get; }
    string CacheDirectory { get; }
    string SettingsFilePath { get; }
}
