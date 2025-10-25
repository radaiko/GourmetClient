using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GC.Common;
using GC.Core.WebApis;
using GC.Models;

namespace GC.Core.Cache;

// TODO: fix that on every month are all transactions added
// TODO: fix that only 2 months are fetched even if more are missing

public class BillingCache {
  // Delegate persistence/in-memory store to generic base repository
  private static readonly CacheRepositoryBase<BillingMonth> _repo = new(
    tableName: "BillingMonths",
    keyColumn: "Month",
    getKey: bm => new DateTime(bm.Month.Year, bm.Month.Month, 1)
  );

  private static DateTime? _latestFetchDateUtc;

  // Key normalization: store months as first-of-month UTC dates
  private static DateTime NormalizeMonth(DateTime d) => new DateTime(d.Year, d.Month, 1);

  public static IEnumerable<DateTime> GetAvailableMonths() {
    return _repo.GetAvailableKeys();
  }

  public static void UpsertBillingMonth(BillingMonth month, DateTime fetchDateUtc) {
    // BillingMonth is non-nullable; no need to check for null here
    _repo.Upsert(month);
    _latestFetchDateUtc = fetchDateUtc;
  }

  public static DateTime? GetLatestFetchDate() => _latestFetchDateUtc ?? _repo.LatestFetchDate;

  // Get billing months as an async stream: yield cached months immediately so the UI can show something,
  // then fetch missing months and yield them as they arrive.
  public static async IAsyncEnumerable<BillingMonth> GetAsync() {
    var yielded = new HashSet<DateTime>();

    // 1) Yield cached months (most recent first)
    var keys = _repo.GetAvailableKeys().Select(k => NormalizeMonth(k)).Distinct().OrderByDescending(d => d);
    foreach (var k in keys) {
      var item = _repo.Get(k);
      if (item != null) {
        var norm = NormalizeMonth(item.Month);
        if (yielded.Add(norm)) {
          yield return item;
        }
      }
    }

    // Determine which months we need to fetch
    var now = DateTime.UtcNow.Date;
    var thisMonth = new DateTime(now.Year, now.Month, 1);
    var previousMonth = thisMonth.AddMonths(-1);

    var needFetchThis = _repo.Get(thisMonth) == null;
    var needFetchPrevious = false;

    if (_latestFetchDateUtc.HasValue) {
      var last = _latestFetchDateUtc.Value.Date;
      var lastMonth = new DateTime(last.Year, last.Month, 1);
      if (lastMonth != thisMonth) {
        needFetchPrevious = true;
      }
    } else {
      needFetchPrevious = _repo.Get(previousMonth) == null;
    }
    

    var toFetch = new List<DateTime>();
    if (needFetchThis) toFetch.Add(thisMonth);
    if (needFetchPrevious && _repo.Get(previousMonth) == null) toFetch.Add(previousMonth);

    // 2) Fetch missing months and yield them as they become available
    if (toFetch.Count > 0) {
      await foreach (var fetched in FetchMonthsFromVentoAsync(toFetch)) {
        // Fire-and-forget upsert so we don't need a try/catch inside the iterator method
        _ = Task.Run(() => UpsertBillingMonth(fetched, DateTime.UtcNow));

        var norm = NormalizeMonth(fetched.Month);
        if (yielded.Add(norm)) {
          yield return fetched;
        }
      }
    }

    // Done
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