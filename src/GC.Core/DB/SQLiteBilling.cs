using System;
using System.Collections.Generic;
using GC.Models;

namespace GC.Core.DB;

// ReSharper disable once InconsistentNaming
public static class SQLiteBilling {
  private static void Init() {
    SQLiteBase.EnsureInitialized();
    if (SQLiteBase.Connection == null)
      throw new InvalidOperationException("SQLite connection is not initialized.");
  }
  
  /// <summary>
  /// Gets the date of the last fetched billing transaction.
  /// </summary>
  /// <returns></returns>
  public static DateOnly GetLastFetchDate() {
    Init();
    
    using var cmd = SQLiteBase.Connection!.CreateCommand();
    cmd.CommandText = "SELECT MAX(date) FROM BillingTransactions;";
    
    var result = cmd.ExecuteScalar();
    if (result == DBNull.Value || result == null) {
      return DateOnly.MinValue;
    }
    
    var lastDate = (DateTime)result;
    return DateOnly.FromDateTime(lastDate.ToLocalTime());
  }
  
  /// <summary>
  /// Writes the billing month data into the SQLite database.
  /// </summary>
  /// <param name="month"></param>
  /// <exception cref="InvalidOperationException"></exception>
  public static void Insert(BillingMonth month) {
    Init();
    
    var transactions = month.Transactions;
    List<Transaction> newTransactions = [];
    
    // Validate to have only transactions added where the same has is not already present
    foreach (var transaction in transactions) {
      using var cmdCheck = SQLiteBase.Connection!.CreateCommand();
      cmdCheck.CommandText = "SELECT COUNT(*) FROM BillingTransactions WHERE hash = @hash;";
      cmdCheck.Parameters.AddWithValue("@hash", transaction.Hash);
      var count = (long)cmdCheck.ExecuteScalar()!;
      if (count == 0) {
        newTransactions.Add(transaction);
      }
    }
    
    // Write new transactions and positions
    using var dbTransaction = SQLiteBase.Connection!.BeginTransaction();
    using var cmd = SQLiteBase.Connection.CreateCommand();
    cmd.Transaction = dbTransaction;
    
    foreach (var transaction in newTransactions) {
      // Insert or replace each transaction
      cmd.CommandText = "INSERT OR REPLACE INTO BillingTransactions (type, date, hash) VALUES (@type, @date, @hash);";
      cmd.Parameters.Clear();
      cmd.Parameters.AddWithValue("@type", (int)transaction.Type);
      cmd.Parameters.AddWithValue("@date", transaction.Date.ToUniversalTime());
      cmd.Parameters.AddWithValue("@hash", transaction.Hash);
      cmd.ExecuteNonQuery();
      
      // Get the inserted transaction id
      cmd.CommandText = "SELECT last_insert_rowid();";
      var transactionId = (long)cmd.ExecuteScalar()!;
      
      // Insert or replace each position
      foreach (var position in transaction.Positions) {
        cmd.CommandText = "INSERT OR REPLACE INTO BillingPositions (name, quantity, unit_price, support, transaction_id) VALUES (@name, @quantity, @unitPrice, @support, @transactionId);";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@name", position.Name);
        cmd.Parameters.AddWithValue("@quantity", position.Quantity);
        cmd.Parameters.AddWithValue("@unitPrice", position.UnitPrice);
        cmd.Parameters.AddWithValue("@support", position.Support);
        cmd.Parameters.AddWithValue("@transactionId", transactionId);
        cmd.ExecuteNonQuery();
      }
    }
    
    dbTransaction.Commit();
  }
  
  
  /// <summary>
  /// Reads the billing month data from the SQLite database for the specified month.
  /// </summary>
  /// <param name="month"></param>
  /// <returns></returns>
  /// <exception cref="InvalidOperationException"></exception>
  public static BillingMonth? Read(DateTime month)
  {
    Init();
    
    DateTime startOfMonth = new DateTime(month.Year, month.Month, 1);
    DateTime startOfNextMonth = startOfMonth.AddMonths(1);
    
    // Read all transactions for the specified month
    using var cmd = SQLiteBase.Connection!.CreateCommand();
    cmd.CommandText = "SELECT id, type, date, hash FROM BillingTransactions WHERE date >= @start AND date < @end ORDER BY date;";
    cmd.Parameters.AddWithValue("@start", startOfMonth);
    cmd.Parameters.AddWithValue("@end", startOfNextMonth);
    
    using var reader = cmd.ExecuteReader();
    List<Transaction> transactions = new();
    
    while (reader.Read())
    {
      var id = reader.GetInt64(0);
      var typeInt = reader.GetInt32(1);
      var date = reader.GetDateTime(2).ToLocalTime();
      var type = (Transaction.TransactionType)typeInt;
      
      // Read associated positions
      using var cmdPos = SQLiteBase.Connection.CreateCommand();
      cmdPos.CommandText = "SELECT name, quantity, unit_price, support FROM BillingPositions WHERE transaction_id = @id;";
      cmdPos.Parameters.AddWithValue("@id", id);
      
      using var readerPos = cmdPos.ExecuteReader();
      List<Position> positions = new();
      
      while (readerPos.Read())
      {
        var name = readerPos.GetString(0);
        var quantity = readerPos.GetInt32(1);
        var unitPrice = readerPos.GetDecimal(2);
        var support = readerPos.GetDecimal(3);
        positions.Add(new Position(name, quantity, unitPrice, support, 0));
      }
      
      transactions.Add(new Transaction
      {
        Type = type,
        Date = date,
        Positions = positions.ToArray()
      });
    }
    
    if (transactions.Count == 0) return null;
    
    return new BillingMonth { Transactions = transactions.ToArray() };
  }
  

  
  
  /// <summary>
  /// Reads all billing months from the SQLite database.
  /// </summary>
  /// <returns></returns>
  public static IEnumerable<BillingMonth> Read() {
    Init();
    
    List<BillingMonth> months = new();
    
    using var cmdMonths = SQLiteBase.Connection!.CreateCommand();
    cmdMonths.CommandText = "SELECT DISTINCT strftime('%Y-%m', date) AS month FROM BillingTransactions ORDER BY month DESC;";
    
    using var readerMonths = cmdMonths.ExecuteReader();
    while (readerMonths.Read()) {
      var monthStr = readerMonths.GetString(0);
      var monthDate = DateTime.ParseExact(monthStr + "-01", "yyyy-MM-dd", null);
      var billingMonth = Read(monthDate);
      if (billingMonth != null) {
        months.Add(billingMonth);
      }
    }
    
    return months;
  }
}