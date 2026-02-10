namespace GourmetClient.Maui.Services;

public interface IUpdateService
{
    bool IsSupported { get; }
    Task<bool> CheckForUpdateAsync();
    Task DownloadAndApplyAsync();
}
