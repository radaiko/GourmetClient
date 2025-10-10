using GC.Database;

namespace GC.Cache.Billing;

public abstract class BillingCacheBase(SqliteService sqliteService) {
    protected readonly SqliteService _sqliteService = sqliteService;

    protected abstract Task<string?> LoadDataAsync(DateTime month);
    protected abstract Task SaveDataAsync(DateTime month, string data);
    protected abstract Task<DateTime?> GetLastWriteTimeAsync(DateTime month);

    public async Task<string?> GetCachedDataAsync(DateTime month)
    {
        return await LoadDataAsync(month);
    }

    public async Task SetCachedDataAsync(DateTime month, string data)
    {
        await SaveDataAsync(month, data);
    }

    public async Task<DateTime?> GetLastWriteAsync(DateTime month)
    {
        return await GetLastWriteTimeAsync(month);
    }
}
