using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GC.Common;
using GC.Core.DB;
using GC.Core.WebApis;
using GC.Models;

namespace GC.Core.Cache;

public static class MenuCache {
  
  public static bool IsLoading { get; private set; }
  
  public static async Task<IEnumerable<Day>> GetAsync() {
    IsLoading = true;
    IEnumerable<Day> cache = SQLiteMenus.Read();
    
    var today = DateTime.Now.ToDateOnly();
    var fridayIn2Weeks = today.AddDays(14 - (int)today.DayOfWeek + 5);
    if (cache.Any() && cache.Max(d => d.Date) >= fridayIn2Weeks) { // cache is up to date
      Log.Info("MenuCache is up to date, using cached menus.");
    }
    else { // cache is out of date, load from webapi (GourmetApi)
      Log.Info("MenuCache is out of date, fetching new menus from WebApi.");
      var days = await GourmetApi.GetOrderDaysAsync();
      SQLiteMenus.Insert(days);
    }
    
    cache = SQLiteMenus.Read();
    IsLoading = false;
    return cache;
  }
}