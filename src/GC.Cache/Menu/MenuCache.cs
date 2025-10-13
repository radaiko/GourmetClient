using GC.Database;

namespace GC.Cache.Menu;

public class MenuCache(SqliteService sqliteService)
  : MenuCacheBase(sqliteService) {
  protected override async Task<string?> LoadDataAsync() {
    return await _sqliteService.LoadMenuDataAsync();
  }

  protected override async Task SaveDataAsync(string data) {
    await _sqliteService.SaveMenuDataAsync(data);
  }

  protected override async Task<DateTime?> GetLastWriteTimeAsync() {
    return await _sqliteService.GetMenuLastWriteAsync();
  }
}