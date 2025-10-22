using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GC.Common;
using GC.Core.WebApis;
using GC.Models;

namespace GC.Core.Cache;

public static class MenuCache {
  // Single lock for table creation
  private static readonly object _tableLock = new();
  private static bool _tableEnsured;

  private static void EnsureMenuTable() {
    if (_tableEnsured) return;
    lock (_tableLock) {
      if (_tableEnsured) return;
      var create = @"CREATE TABLE IF NOT EXISTS Menus (
      Id INTEGER PRIMARY KEY AUTOINCREMENT,
      Date TEXT NOT NULL,
      Type INTEGER NOT NULL,
      Title TEXT NOT NULL,
      Allergens TEXT,
      Price REAL,
      Payload TEXT
    );";
      SqliteCacheBase.EnsureTable("Menus", create);
      _tableEnsured = true;
    }
  }

  // Read distinct dates available in the sqlite cache (date-only)
  public static IEnumerable<DateTime> GetAvailableOrderDays() {
    try {
      EnsureMenuTable();
      var rows = SqliteCacheBase.ReadStringColumn("Menus", "Date", "GROUP BY Date");
      var list = new List<DateTime>();
      foreach (var s in rows) {
        if (DateTime.TryParse(s, out var dt)) list.Add(dt.Date);
      }
      return list.OrderBy(d => d);
    }
    catch (Exception ex) {
      Log.Debug(ex.ToString());
      return Array.Empty<DateTime>();
    }
  }

  // Persist menus to sqlite for the given menus list. Simple upsert: delete matching rows and insert new ones.
  public static void UpsertMenus(IEnumerable<Menu> menus, DateTime fetchDateUtc) {
    if (menus == null) return;
    var list = menus.ToList();
    EnsureMenuTable();
    foreach (var m in list) {
      try {
        SqliteCacheBase.ExecuteNonQuery(
          "DELETE FROM Menus WHERE Date = $date AND Type = $type AND Title = $title",
          ("$date", m.Date.ToString("o")), ("$type", (int)m.Type), ("$title", m.Title)
        );

        SqliteCacheBase.ExecuteNonQuery(
          "INSERT INTO Menus (Date, Type, Title, Allergens, Price, Payload) VALUES ($date,$type,$title,$allergens,$price,$payload)",
          ("$date", m.Date.ToString("o")), ("$type", (int)m.Type), ("$title", m.Title), ("$allergens", JsonSerializer.Serialize(m.Allergens)), ("$price", m.Price), ("$payload", JsonSerializer.Serialize(m))
        );
      }
      catch (Exception ex) {
        Log.Debug(ex.ToString());
      }
    }

    // We store fetchDateUtc as a simple log marker - not persisted here. Keep method signature for compatibility.
  }

  // Async enumerable: yield cached days first (from sqlite), then fetch from web api and yield updated days.
  public static async IAsyncEnumerable<Day> GetAsync() {
    // Yield cached days from sqlite so UI can display them immediately
    var cachedDays = GetAvailableOrderDays().OrderBy(d => d);
    var yieldedDates = new HashSet<DateTime>();

    foreach (var d in cachedDays) {
      var menus = ReadMenusFromDb(d);
      var day = BuildOrderDayFromMenus(d, menus);
      yieldedDates.Add(day.Date.Date);
      yield return day;
    }

    // Determine if we should fetch from web API
   var today = DateTime.UtcNow.Date;
    var nextWeekDate = today.AddDays(7);
    var maxYielded = yieldedDates.Any() ? yieldedDates.Max(d => d.Date) : DateTime.MinValue;
    var shouldFetch = maxYielded < nextWeekDate;
    if (!shouldFetch) yield break;

    // Fetch from web API for today and onward
    List<Day> webDays;
    try {
      webDays = await FetchMenusFromWebAsync(today).ConfigureAwait(false);
    }
    catch (Exception ex) {
      Log.Debug(ex.ToString());
      yield break;
    }

    // Store results to sqlite and yield each day. If we already yielded a cached day for the same date,
    // yield the updated web result as well so consumers can refresh the UI.
    foreach (var day in webDays.OrderBy(d => d.Date)) {
      try {
        UpsertMenus(day.Menus, DateTime.UtcNow);
      }
      catch (Exception ex) {
        Log.Debug(ex.ToString());
      }

      // If we already yielded this date from cache and we're not forcing refresh, still yield the web day
      // so the UI can update to the freshest data.
      yield return day;
    }
  }

  private static List<Menu> ReadMenusFromDb(DateTime date) {
    try {
      EnsureMenuTable();
      var rows = SqliteCacheBase.ReadStringColumn("Menus", "Payload", "WHERE Date = $date", ("$date", date.ToString("o")));
      var res = new List<Menu>();
      foreach (var payload in rows) {
        try {
          var m = JsonSerializer.Deserialize<Menu>(payload);
          if (m != null) res.Add(m);
        }
        catch (Exception ex) {
          Log.Debug(ex.ToString());
        }
      }
      return res;
    }
    catch (Exception ex) {
      Log.Debug(ex.ToString());
      return new List<Menu>();
    }
  }

  // Build an Day from a list of menus (will create empty menus for missing types)
  private static Day BuildOrderDayFromMenus(DateTime date, IEnumerable<Menu> menus) {
    Menu? m1 = null; Menu? m2 = null; Menu? m3 = null; Menu? s = null;
    foreach (var m in menus) {
      switch (m.Type) {
        case MenuType.Menu1: m1 = m; break;
        case MenuType.Menu2: m2 = m; break;
        case MenuType.Menu3: m3 = m; break;
        case MenuType.SoupAndSalad: s = m; break;
      }
    }

    // Fill missing menus with placeholders so the UI can bind safely
    var placeholder = new Menu(MenuType.Menu1, "(no menu)", Array.Empty<char>(), 0m, date);
    return new Day(date, m1 ?? placeholder, m2 ?? placeholder, m3 ?? placeholder, s ?? placeholder);
  }

  // Delegate to the existing web API client
  private static async Task<List<Day>> FetchMenusFromWebAsync(DateTime startDate) {
    var all = await GourmetApi.GetOrderDaysAsync();
    var filtered = all.Where(d => d.Date.Date >= startDate.Date).ToList();
    return filtered;
  }
}