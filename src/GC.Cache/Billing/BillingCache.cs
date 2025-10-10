using GC.Database;

namespace GC.Cache.Billing;

public class BillingCache(SqliteService sqliteService)
  : BillingCacheBase(sqliteService) {
  protected override async Task<string?> LoadDataAsync(DateTime month)
  {
    return await _sqliteService.LoadBillingDataAsync(month);
  }

  protected override async Task SaveDataAsync(DateTime month, string data)
  {
    await _sqliteService.SaveBillingDataAsync(month, data);
  }

  protected override async Task<DateTime?> GetLastWriteTimeAsync(DateTime month)
  {
    return await _sqliteService.GetBillingLastWriteAsync(month);
  }
}
