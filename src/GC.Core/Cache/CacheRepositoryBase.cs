using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GC.Common;

namespace GC.Core.Cache;

public class CacheRepositoryBase<TItem> where TItem : class {
  private readonly string _tableName;
  private readonly string _keyColumn;
  private readonly Func<TItem, DateTime> _getKey;
  private readonly object _tableLock = new();
  private bool _tableEnsured;

  protected readonly ConcurrentDictionary<DateTime, TItem> Cache = new();
  public DateTime? LatestFetchDate { get; protected set; }

  public CacheRepositoryBase(string tableName, string keyColumn, Func<TItem, DateTime> getKey) {
    _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
    _keyColumn = keyColumn ?? throw new ArgumentNullException(nameof(keyColumn));
    _getKey = getKey ?? throw new ArgumentNullException(nameof(getKey));
  }

  protected void EnsureTable(string createStatement) {
    if (_tableEnsured) return;
    lock (_tableLock) {
      if (_tableEnsured) return;
      SqliteCacheBase.EnsureTable(_tableName, createStatement);
      _tableEnsured = true;
    }
  }

  // Load all payloads from the table into the in-memory cache
  public void LoadAllFromDb() {
    try {
      EnsureTable($"CREATE TABLE IF NOT EXISTS {_tableName} ({_keyColumn} TEXT PRIMARY KEY, Payload TEXT);");
      var rows = SqliteCacheBase.ReadStringColumn(_tableName, "Payload", "");
      foreach (var payload in rows) {
        try {
          var item = JsonSerializer.Deserialize<TItem>(payload);
          if (item == null) continue;
          var key = NormalizeKey(_getKey(item));
          Cache.AddOrUpdate(key, item, (_, __) => item);
        }
        catch (Exception ex) {
          Log.Debug(ex.ToString());
        }
      }
    }
    catch (Exception ex) {
      Log.Debug(ex.ToString());
    }
  }

  // Upsert a single item (delete by key, insert payload)
  public void Upsert(TItem item) {
    if (item == null) return;
    EnsureTable($"CREATE TABLE IF NOT EXISTS {_tableName} ({_keyColumn} TEXT PRIMARY KEY, Payload TEXT);");
    var keyDt = _getKey(item);
    var key = NormalizeKey(keyDt);
    try {
      var payload = JsonSerializer.Serialize(item);
      SqliteCacheBase.ExecuteNonQuery($"DELETE FROM {_tableName} WHERE {_keyColumn} = $key", ("$key", key.ToString("o")));
      SqliteCacheBase.ExecuteNonQuery($"INSERT INTO {_tableName} ({_keyColumn}, Payload) VALUES ($key, $payload)", ("$key", key.ToString("o")), ("$payload", payload));
      Cache.AddOrUpdate(key, item, (_, __) => item);
      // Update latest fetch timestamp to now (caller may override with its own tracking if desired)
      LatestFetchDate = DateTime.UtcNow;
    }
    catch (Exception ex) {
      Log.Debug(ex.ToString());
    }
  }

  // Get item by date key (returns null if not present)
  public TItem? Get(DateTime key) {
    if (Cache.IsEmpty) LoadAllFromDb();
    var k = NormalizeKey(key);
    return Cache.TryGetValue(k, out var item) ? item : null;
  }

  // Get all available keys
  public IEnumerable<DateTime> GetAvailableKeys() {
    if (Cache.IsEmpty) LoadAllFromDb();
    return Cache.Keys.OrderByDescending(d => d);
  }

  private static DateTime NormalizeKey(DateTime d) => new DateTime(d.Year, d.Month, d.Day);
}
