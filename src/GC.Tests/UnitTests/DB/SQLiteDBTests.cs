using System.Diagnostics;
using GC.Core.DB;
using GC.Models;
using Microsoft.Data.Sqlite;

namespace GC.Tests.UnitTests.DB;

// ReSharper disable once InconsistentNaming
// ReSharper disable once ClassNeverInstantiated.Global
public class SQLiteDBTestFixture : IDisposable {
  public string TempDb { get; }

  public SQLiteDBTestFixture() {
    TempDb = Helpers.PathHelper.GetTempDbPath();
    try {
      File.Delete(TempDb);
    }
    catch {
      // ignored
    }
    SQLiteBase.DbPath = TempDb;
  }

  public void Dispose() {
    SQLiteBase.Close();
    if (!Debugger.IsAttached) {
      try {
        File.Delete(TempDb);
      }
      catch {
        // ignored
      }
    }
  }
}

// ReSharper disable once InconsistentNaming
public class SqLiteBaseTests(SQLiteDBTestFixture fixture) : IClassFixture<SQLiteDBTestFixture> {
  [Fact]
  public void SQLiteDB_Can_Be_Initialized() {
    SQLiteBase.EnsureInitialized();

    Assert.True(File.Exists(fixture.TempDb));
  }

  [Fact]
  public void SQLiteDB_Initialization_Is_Valid() {
    SQLiteBase.EnsureInitialized();

    // Validate if the created schema is correct
    using var connection = new SqliteConnection($"Data Source={fixture.TempDb}");
    connection.Open();

    // Check BillingTransactions table
    using (var cmd = connection.CreateCommand()) {
      cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='BillingTransactions';";
      using var reader = cmd.ExecuteReader();
      Assert.True(reader.Read(), "BillingTransactions table should exist");
    }

    using (var cmd = connection.CreateCommand()) {
      cmd.CommandText = "PRAGMA table_info(BillingTransactions);";
      using var reader = cmd.ExecuteReader();
      var columns = new List<string>();
      while (reader.Read()) {
        columns.Add(reader.GetString(1)); // name is at index 1
      }
      var expected = new List<string> { "id", "type", "date", "hash" };
      Assert.Equal(expected, columns);
    }

    // Check BillingPosition table
    using (var cmd = connection.CreateCommand()) {
      cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='BillingPositions';";
      using var reader = cmd.ExecuteReader();
      Assert.True(reader.Read(), "BillingPosition tables should exist");
    }

    using (var cmd = connection.CreateCommand()) {
      cmd.CommandText = "PRAGMA table_info(BillingPositions);";
      using var reader = cmd.ExecuteReader();
      var columns = new List<string>();
      while (reader.Read()) {
        columns.Add(reader.GetString(1));
      }
      var expected = new List<string> { "id", "name", "quantity", "unit_price", "support", "transaction_id" };
      Assert.Equal(expected, columns);
    }
  }

  [Fact]
  public void SQLiteDB_Can_Insert_And_Read_BillingMonth() {
    var billingMonth = BillingMonth.GetDummyData().First();
    SQLiteBilling.Insert(billingMonth);

    var month = billingMonth.Month;
    var readBack = SQLiteBilling.Read(month);

    Assert.NotNull(readBack);
    Assert.Equal(billingMonth.TotalAmount, readBack.TotalAmount);
    Assert.Equal(billingMonth.TotalCafeAndCo, readBack.TotalCafeAndCo);
    Assert.Equal(billingMonth.TotalGourmet, readBack.TotalGourmet);
    Assert.Equal(billingMonth.CountGourmet, readBack.CountGourmet);
    Assert.Equal(billingMonth.CountCafeAndCo, readBack.CountCafeAndCo);
    Assert.Equal(billingMonth.Month, readBack.Month);
  }
  
  [Fact]
  public void SQLiteDB_Can_Insert_And_Read_Menu() {
    var days = Day.GetDummyData();
    SQLiteMenus.Insert(days);
    
    var readBack = SQLiteMenus.Read(days.First().Date, days.Last().Date.AddDays(1));
    
    for (var i = 0; i < days.Count; i++) {
      var originalDay = days[i];
      var readDay = readBack.ElementAt(i);
      
      // Assert.Equal(originalDay.Date, readDay.Date);
      
      for (var j = 0; j < 4; j++) {
        var originalMenu = j switch {
          0 => originalDay.Menu1,
          1 => originalDay.Menu2,
          2 => originalDay.Menu3,
          3 => originalDay.SoupAndSalad,
          _ => throw new InvalidOperationException("Invalid menu index")
        };
        var readMenu = j switch {
          0 => readDay.Menu1,
          1 => readDay.Menu2,
          2 => readDay.Menu3,
          3 => readDay.SoupAndSalad,
          _ => throw new InvalidOperationException("Invalid menu index")
        };
        

        Assert.Equal(originalMenu.Type, readMenu.Type);
        Assert.Equal(originalMenu.Title, readMenu.Title);
        Assert.Equal(originalMenu.Allergens, readMenu.Allergens);
        Assert.Equal(originalMenu.Price, readMenu.Price);
        Assert.Equal(originalMenu.Date, readMenu.Date);
      }
    }
  }
}