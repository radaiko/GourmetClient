using GC.Database;

namespace GC.Cache.Billing;

public class BillingCacheDebug(SqliteService sqliteService)
  : BillingCacheBase(sqliteService) {
  protected override async Task<string?> LoadDataAsync(DateTime month) {
    return await _sqliteService.LoadDebugBillingDataAsync(month);
  }

  protected override async Task SaveDataAsync(DateTime month, string data) {
    await _sqliteService.SaveDebugBillingDataAsync(month, data);
  }

  protected override async Task<DateTime?> GetLastWriteTimeAsync(DateTime month) {
    return await _sqliteService.GetDebugBillingLastWriteAsync(month);
  }
}