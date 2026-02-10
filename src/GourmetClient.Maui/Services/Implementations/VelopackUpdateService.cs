#if WINDOWS || MACCATALYST
using Velopack;

namespace GourmetClient.Maui.Services.Implementations;

/// <summary>
/// Velopack-based update service for desktop platforms (Windows and Mac).
/// </summary>
public class VelopackUpdateService : IUpdateService
{
    private const string ReleasesUrl = "https://github.com/patrickl92/GourmetClient/releases";

    private readonly UpdateManager _manager;
    private UpdateInfo? _pendingUpdate;

    public VelopackUpdateService()
    {
        _manager = new UpdateManager(ReleasesUrl);
    }

    public bool IsSupported => true;

    public async Task<bool> CheckForUpdateAsync()
    {
        try
        {
            _pendingUpdate = await _manager.CheckForUpdatesAsync();
            return _pendingUpdate != null;
        }
        catch
        {
            // Update check failed - network issue or releases not available
            _pendingUpdate = null;
            return false;
        }
    }

    public async Task DownloadAndApplyAsync()
    {
        if (_pendingUpdate == null)
        {
            // Re-check if no pending update
            _pendingUpdate = await _manager.CheckForUpdatesAsync();
        }

        if (_pendingUpdate == null)
        {
            return;
        }

        await _manager.DownloadUpdatesAsync(_pendingUpdate);
        _manager.ApplyUpdatesAndRestart(_pendingUpdate);
    }
}
#endif
