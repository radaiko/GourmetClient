using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GC.Common;
using GC.Core.DB;
using GC.Core.WebApis;
using GC.Models;

namespace GC.Core.Cache;

public static class BillingCache {
  
  /// <summary>
  /// Indicates whether billing months are currently being loaded.
  /// </summary>
  public static bool IsLoading { get; private set; }
  public static bool IsValid { get; private set; }

  /// <summary>
  /// Gets billing months from cache, fetching missing months from WebApi if needed.
  /// </summary>
  /// <returns></returns>
  public static async Task<IEnumerable<BillingMonth>> GetAsync() {
    IsLoading = true;
    IEnumerable<BillingMonth> cache;
    
    var today = DateTime.Now.ToDateOnly();
    var endOfMonth = new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
    var lastFetch = SQLiteBilling.GetLastFetchDate();
    int monthsPassed = 0;
    
    if (lastFetch == DateOnly.MinValue) { // First time fetch, get last 6 months from WebApi and store in sqlite
      monthsPassed = 6;
      lastFetch = DateTime.Now.ToDateOnly().AddMonths(-6);
    }
    else // Not first time, fetch only missing months
      monthsPassed = (today.Year - lastFetch.Year) * 12 + today.Month - lastFetch.Month;

    // Get Data
    for (var i = 1; i <= monthsPassed; i++) {
      var monthToFetch = lastFetch.AddMonths(i);
      try {
        await GetAndWrite(monthToFetch);
      }
      catch {
        IsValid = false;
        return default;
      }
    }
    
    // Read from cache
    cache = SQLiteBilling.Read();
    IsLoading = false;
    IsValid = true;
    return cache;
  }

  public static async Task GetAndWrite(DateOnly monthToFetch) {
    SQLiteBilling.Insert(await VentoApi.GetBillingMonthAsync(monthToFetch.Year, monthToFetch.Month));
  }
}