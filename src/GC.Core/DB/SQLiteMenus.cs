using System;
using System.Collections.Generic;
using GC.Common;
using GC.Models;

namespace GC.Core.DB;

// ReSharper disable once InconsistentNaming
public static class SQLiteMenus {
  private static void Init() {
    SQLiteBase.EnsureInitialized();
    if (SQLiteBase.Connection == null)
      throw new InvalidOperationException("SQLite connection is not initialized.");
  }
  
  /// <summary>
  /// Inserts the menu data into the SQLite database.
  /// </summary>
  /// <param name="days"></param>
  /// <exception cref="InvalidOperationException"></exception>
  public static void Insert(IEnumerable<Day> days) {
    Init();

    var menus = days.ToMenu();
    List<Menu> newMenus = [];
    
    // Validate to have only menus added where the same hash is not already present
    foreach (var menu in menus) {
      using var cmdCheck = SQLiteBase.Connection!.CreateCommand();
      cmdCheck.CommandText = "SELECT COUNT(*) FROM Menus WHERE hash = @hash;";
      cmdCheck.Parameters.AddWithValue("@hash", menu.Hash);
      var count = (long)cmdCheck.ExecuteScalar()!;
      if (count == 0) {
        newMenus.Add(menu);
      }
    }
    
    // Write new menus
    using var dbTransaction = SQLiteBase.Connection!.BeginTransaction();
    using var cmd = SQLiteBase.Connection.CreateCommand();
    cmd.Transaction = dbTransaction;
    
    foreach (var menu in newMenus) {
      // Insert or replace each menu
      cmd.CommandText = "INSERT OR REPLACE INTO Menus (hash, type, title, date, allergens, price) VALUES (@hash, @type, @title, @date, @allergens, @price);";
      cmd.Parameters.Clear();
      cmd.Parameters.AddWithValue("@hash", menu.Hash);
      cmd.Parameters.AddWithValue("@type", (int)menu.Type);
      cmd.Parameters.AddWithValue("@title", menu.Title);
      cmd.Parameters.AddWithValue("@date", menu.Date);
      cmd.Parameters.AddWithValue("@allergens", new string(menu.Allergens));
      cmd.Parameters.AddWithValue("@price", menu.Price);
      cmd.ExecuteNonQuery();
    }
    
    dbTransaction.Commit();
  }


  /// <summary>
  /// Reads the menu data from the SQLite database for a specified timeframe.
  /// </summary>
  /// <param name="start"></param>
  /// <param name="end"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static IEnumerable<Day> Read(DateOnly start, DateOnly end) {
    Init();
    
    // Read all menus for the specified month
    using var cmd = SQLiteBase.Connection!.CreateCommand();
    cmd.CommandText = "SELECT id, type, title, date, allergens, price FROM Menus WHERE date >= @start AND date < @end ORDER BY date, type;";
    cmd.Parameters.AddWithValue("@start", start);
    cmd.Parameters.AddWithValue("@end", end);
    
    using var reader = cmd.ExecuteReader();
    List<Menu> menus = [];
    
    while (reader.Read())
    {
      var id = reader.GetInt32(0);
      var typeInt = reader.GetInt32(1);
      var title = reader.GetString(2);
      var date = reader.GetDateTime(3).ToDateOnly();
      var allergensStr = reader.GetString(4);
      var price = reader.GetDecimal(5);
      var type = (MenuType)typeInt;
      var allergens = allergensStr.ToCharArray();
      
      var menu = new Menu(type, title, allergens, price, date) { Id = id };
      menus.Add(menu);
    }

    return menus.ToDays();
  }
  
  /// <summary>
  /// Reads the menu data from the SQLite database for the next two weeks starting from today.
  /// </summary>
  /// <returns></returns>
  public static IEnumerable<Day> Read() {
    var start = DateOnly.FromDateTime(DateTime.Now);
    // Read menus until friday in 2 weeks
    var end = start.AddDays(14 + (5 - (int)start.DayOfWeek + 7) % 7);
    return Read(start, end);
  }
}