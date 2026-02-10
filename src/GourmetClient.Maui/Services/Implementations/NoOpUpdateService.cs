namespace GourmetClient.Maui.Services.Implementations;

/// <summary>
/// No-op update service for mobile platforms where updates are handled via app stores.
/// </summary>
public class NoOpUpdateService : IUpdateService
{
    public bool IsSupported => false;

    public Task<bool> CheckForUpdateAsync() => Task.FromResult(false);

    public Task DownloadAndApplyAsync() => Task.CompletedTask;
}
