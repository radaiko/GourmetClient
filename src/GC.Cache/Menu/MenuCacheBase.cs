using GC.Database;

namespace GC.Cache.Menu;

public abstract class MenuCacheBase(SqliteService sqliteService) {
    protected readonly SqliteService _sqliteService = sqliteService;

    protected abstract Task<string?> LoadDataAsync();
    protected abstract Task SaveDataAsync(string data);
    protected abstract Task<DateTime?> GetLastWriteTimeAsync();

    public async Task<string?> GetCachedDataAsync()
    {
        return await LoadDataAsync();
    }

    public async Task SetCachedDataAsync(string data)
    {
        await SaveDataAsync(data);
    }

    public async Task<DateTime?> GetLastWriteAsync()
    {
        return await GetLastWriteTimeAsync();
    }
}
