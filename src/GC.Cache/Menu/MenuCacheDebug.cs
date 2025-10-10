using GC.Database;

namespace GC.Cache.Menu;

public class MenuCacheDebug(SqliteService sqliteService)
    : MenuCacheBase(sqliteService) {
    protected override async Task<string?> LoadDataAsync()
    {
        return await _sqliteService.LoadDebugMenuDataAsync();
    }

    protected override async Task SaveDataAsync(string data)
    {
        await _sqliteService.SaveDebugMenuDataAsync(data);
    }

    protected override async Task<DateTime?> GetLastWriteTimeAsync()
    {
        return await _sqliteService.GetDebugMenuLastWriteAsync();
    }
}
