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
    
    if (lastFetch == DateOnly.MinValue) { // First time fetch, get last 6 months from WebApi and store in sqlite
      for (var i = 0; i < 6; i++) {
        var monthToFetch = endOfMonth.AddMonths(-i);
        var billingMonth = await VentoApi.GetBillingMonthAsync(monthToFetch.Year, monthToFetch.Month);
        SQLiteBilling.Insert(billingMonth);
      }
    }
    else { // Not first time, fetch only missing months
      var monthsPassed = (today.Year - lastFetch.Year) * 12 + today.Month - lastFetch.Month;
      for (var i = 1; i <= monthsPassed; i++) {
        var monthToFetch = lastFetch.AddMonths(i);
        var billingMonth = await VentoApi.GetBillingMonthAsync(monthToFetch.Year, monthToFetch.Month);
        SQLiteBilling.Insert(billingMonth);
      }
    }
    cache = SQLiteBilling.Read();
    IsLoading = false;
    return cache;
  }
}