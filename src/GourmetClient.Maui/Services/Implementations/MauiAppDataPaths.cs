namespace GourmetClient.Maui.Services.Implementations;

public class MauiAppDataPaths : IAppDataPaths
{
    public string AppDataDirectory => FileSystem.AppDataDirectory;
    public string CacheDirectory => FileSystem.CacheDirectory;
    public string SettingsFilePath => Path.Combine(AppDataDirectory, "settings.json");
}
