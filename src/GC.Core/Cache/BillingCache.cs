using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GC.Common;
using GC.Core.WebApis;
using GC.Models;

namespace GC.Core.Cache;

// TODO: fix that on every month are all transactions added
// TODO: fix that only 2 months are fetched even if more are missing

public class BillingCache {
  private static readonly ConcurrentDictionary<DateTime, BillingMonth> _cache = new();
  private static readonly object _tableLock = new();
  private static bool _tableEnsured;

  private static DateTime? _latestFetchDateUtc;

  // Key normalization: store months as first-of-month UTC dates
  private static DateTime NormalizeMonth(DateTime d) => new DateTime(d.Year, d.Month, 1);

  private static void EnsureTable() {
    if (_tableEnsured) return;
    lock (_tableLock) {
      if (_tableEnsured) return;
      SqliteCacheBase.EnsureTable("BillingTransactions", "CREATE TABLE IF NOT EXISTS BillingTransactions (Id INTEGER PRIMARY KEY AUTOINCREMENT, Type TEXT, Date TEXT);");
      SqliteCacheBase.EnsureTable("BillingPositions", "CREATE TABLE IF NOT EXISTS BillingPositions (TransactionId INTEGER, Name TEXT, Quantity INTEGER, UnitPrice REAL, Support REAL, TotalPrice REAL);");
      _tableEnsured = true;
    }
  }

  private static void LoadAllFromDb() {
    EnsureTable();
    var transRows = SqliteCacheBase.ReadRows("BillingTransactions", new[] { "Id", "Type", "Date" }, "");
    var transactions = new List<Transaction>();
    foreach (var row in transRows) {
      var id = (long)row["Id"];
      var type = Enum.Parse<Transaction.TransactionType>((string)row["Type"]);
      var date = DateTime.Parse((string)row["Date"]);
      var posRows = SqliteCacheBase.ReadRows("BillingPositions", new[] { "Name", "Quantity", "UnitPrice", "Support", "TotalPrice" }, "WHERE TransactionId = $tid", ("$tid", id));
      var positions = posRows.Select(r => new Position(
        (string)r["Name"],
        (int)(long)r["Quantity"],
        (decimal)(double)r["UnitPrice"],
        (decimal)(double)r["Support"],
        (decimal)(double)r["TotalPrice"]
      )).ToArray();
      var transaction = new Transaction { Type = type, Date = date, Positions = positions };
      transactions.Add(transaction);
    }
    var grouped = transactions.GroupBy(t => NormalizeMonth(t.Date));
    foreach (var group in grouped) {
      if (group.Any()) {
        var month = group.Key;
        var billingMonth = new BillingMonth { Month = month, Transactions = group.ToArray() };
        _cache.AddOrUpdate(month, billingMonth, (_, __) => billingMonth);
      }
    }
  }

  public static IEnumerable<DateTime> GetAvailableMonths() {
    if (_cache.IsEmpty) LoadAllFromDb();
    return _cache.Keys.OrderByDescending(d => d);
  }

  public static BillingMonth? Get(DateTime key) {
    if (_cache.IsEmpty) LoadAllFromDb();
    var k = NormalizeMonth(key);
    return _cache.TryGetValue(k, out var item) ? item : null;
  }

  public static void UpsertBillingMonth(BillingMonth month, DateTime fetchDateUtc, DateTime? deleteFrom = null) {
    if (month == null) return;
    EnsureTable();
    var normMonth = NormalizeMonth(month.Month);
    // Delete old data
    if (deleteFrom.HasValue) {
      SqliteCacheBase.ExecuteNonQuery("DELETE FROM BillingPositions WHERE TransactionId IN (SELECT Id FROM BillingTransactions WHERE Date >= $start)", ("$start", deleteFrom.Value.ToString("o")));
      SqliteCacheBase.ExecuteNonQuery("DELETE FROM BillingTransactions WHERE Date >= $start", ("$start", deleteFrom.Value.ToString("o")));
    } else {
      var start = normMonth;
      var end = normMonth.AddMonths(1);
      SqliteCacheBase.ExecuteNonQuery("DELETE FROM BillingPositions WHERE TransactionId IN (SELECT Id FROM BillingTransactions WHERE Date >= $start AND Date < $end)", ("$start", start.ToString("o")), ("$end", end.ToString("o")));
      SqliteCacheBase.ExecuteNonQuery("DELETE FROM BillingTransactions WHERE Date >= $start AND Date < $end", ("$start", start.ToString("o")), ("$end", end.ToString("o")));
    }
    // Insert new
    var transactionsToInsert = deleteFrom.HasValue ? month.Transactions.Where(t => t.Date >= deleteFrom.Value).ToArray() : month.Transactions;
    foreach (var transaction in transactionsToInsert) {
      SqliteCacheBase.ExecuteNonQuery("INSERT INTO BillingTransactions (Type, Date) VALUES ($type, $date)", ("$type", transaction.Type.ToString()), ("$date", transaction.Date.ToString("o")));
      var transId = (long)SqliteCacheBase.ExecuteScalar("SELECT last_insert_rowid()")!;
      foreach (var position in transaction.Positions) {
        SqliteCacheBase.ExecuteNonQuery("INSERT INTO BillingPositions (TransactionId, Name, Quantity, UnitPrice, Support, TotalPrice) VALUES ($tid, $name, $qty, $up, $sup, $tp)",
          ("$tid", transId),
          ("$name", position.Name),
          ("$qty", position.Quantity),
          ("$up", position.UnitPrice),
          ("$sup", position.Support),
          ("$tp", position.TotalPrice)
        );
      }
    }
    // Load the full month from DB
    var startLoad = normMonth;
    var endLoad = normMonth.AddMonths(1);
    var transRows = SqliteCacheBase.ReadRows("BillingTransactions", new[] { "Id", "Type", "Date" }, "WHERE Date >= $start AND Date < $end", ("$start", startLoad.ToString("o")), ("$end", endLoad.ToString("o")));
    var transactions = new List<Transaction>();
    foreach (var row in transRows) {
      var id = (long)row["Id"];
      var type = Enum.Parse<Transaction.TransactionType>((string)row["Type"]);
      var date = DateTime.Parse((string)row["Date"]);
      var posRows = SqliteCacheBase.ReadRows("BillingPositions", new[] { "Name", "Quantity", "UnitPrice", "Support", "TotalPrice" }, "WHERE TransactionId = $tid", ("$tid", id));
      var positions = posRows.Select(r => new Position(
        (string)r["Name"],
        (int)(long)r["Quantity"],
        (decimal)(double)r["UnitPrice"],
        (decimal)(double)r["Support"],
        (decimal)(double)r["TotalPrice"]
      )).ToArray();
      var transaction = new Transaction { Type = type, Date = date, Positions = positions };
      transactions.Add(transaction);
    }
    var fullMonth = new BillingMonth { Month = normMonth, Transactions = transactions.ToArray() };
    // Update cache
    if (fullMonth.Transactions.Length == 0) {
      _cache.TryRemove(normMonth, out _);
    } else {
      _cache.AddOrUpdate(normMonth, fullMonth, (_, __) => fullMonth);
    }
    _latestFetchDateUtc = fetchDateUtc;
  }

  public static DateTime? GetLatestFetchDate() => _latestFetchDateUtc;

  // Get billing months as an async stream: yield all months after updating from last fetch.
  public static async IAsyncEnumerable<BillingMonth> GetAsync() {
    // Load all from DB
    GetAvailableMonths();

    var hasData = !_cache.IsEmpty;
    var lastFetch = GetLatestFetchDate();
    DateTime startMonth;
    if (hasData && lastFetch.HasValue) {
      startMonth = NormalizeMonth(lastFetch.Value);
    } else if (hasData) {
      var now = DateTime.UtcNow;
      startMonth = NormalizeMonth(now.AddMonths(-1));
    } else {
      // No data in DB, fetch last 6 months
      var now = DateTime.UtcNow;
      startMonth = NormalizeMonth(now.AddMonths(-5));
    }
    var currentMonth = NormalizeMonth(DateTime.UtcNow);
    var monthsToFetch = new List<DateTime>();
    for (var m = startMonth; m <= currentMonth; m = m.AddMonths(1)) {
      monthsToFetch.Add(m);
    }

    // Fetch and upsert
    await foreach (var month in FetchMonthsFromVentoAsync(monthsToFetch)) {
      var norm = NormalizeMonth(month.Month);
      DateTime? deleteFrom = (norm == startMonth && lastFetch.HasValue) ? lastFetch.Value : null;
      UpsertBillingMonth(month, DateTime.UtcNow, deleteFrom);
    }

    // Yield all
    foreach (var m in GetAvailableMonths().OrderByDescending(d => d)) {
      var item = Get(m);
      if (item != null) {
        yield return item;
      }
    }
  }

  // Fetch months one-by-one and yield them as they arrive. If API doesn't provide the requested month,
  // fabricate an empty BillingMonth so consumers always receive an item for requested months.
  private static async IAsyncEnumerable<BillingMonth> FetchMonthsFromVentoAsync(IEnumerable<DateTime> months) {
    var requested = months.Select(m => NormalizeMonth(m)).ToList();

    foreach (var m in requested) {
      // Start with an empty month as a safe default the UI can show immediately
      var result = new BillingMonth { Month = m, Transactions = Array.Empty<Transaction>() };

      try {
        // VentoApi.GetBillingMonthAsync returns a non-null BillingMonth; normalize and use it
        var apiResult = await VentoApi.GetBillingMonthAsync(m.Year, m.Month);
        apiResult.Month = NormalizeMonth(apiResult.Month);
        result = apiResult;
      }
      catch (Exception ex) {
        // Log and fallthrough to return the empty default so UI always gets something
        Log.Debug($"Failed to fetch billing month {m:yyyy-MM}: {ex.Message}");
      }

      // Yield the resolved result (either API data or the empty default)
      yield return result;
    }
  }
}
