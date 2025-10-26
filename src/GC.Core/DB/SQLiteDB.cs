using System;
using System.IO;
using System.Reflection;
using Microsoft.Data.Sqlite;


namespace GC.Core.DB;

// ReSharper disable once InconsistentNaming
public static class SQLiteBase
{
  private static SqliteConnection? _connection;
  private static bool _initialized = false;
  
  public static string DbPath { get; set; } = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "GC",
    "cache.db"
  );
  
  // Open connection to the SQLite DB, initializing if necessary
  internal static void EnsureInitialized()
  {
    if (_initialized) return;
    
    // Ensure file exists
    if (!File.Exists(DbPath)) {
      using var fs = File.Create(DbPath);
    }
    
    // Open connection
    _connection = new SqliteConnection($"Data Source={DbPath}");
    _connection.Open();
    
    // Check if schema exists, create if not
    using var cmd = _connection.CreateCommand();
    using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("GC.Core.DB.sql.billing.sql");
    if (stream == null)
      throw new InvalidOperationException("Embedded resource 'GC.Core.DB.sql.billing.sql' not found.");
    using var reader = new StreamReader(stream);
    cmd.CommandText = reader.ReadToEnd();
    cmd.ExecuteNonQuery();
    
    // Load menu schema
    using var cmdMenu = _connection.CreateCommand();
    using var streamMenu = Assembly.GetExecutingAssembly().GetManifestResourceStream("GC.Core.DB.sql.menu.sql");
    if (streamMenu == null)
      throw new InvalidOperationException("Embedded resource 'GC.Core.DB.sql.menu.sql' not found.");
    using var readerMenu = new StreamReader(streamMenu);
    cmdMenu.CommandText = readerMenu.ReadToEnd();
    cmdMenu.ExecuteNonQuery();
    
    _initialized = true;
  }
  
  internal static SqliteConnection? Connection => _connection;
  
  // Close the database connection and reset initialization
  internal static void Close()
  {
    if (_connection != null)
    {
      _connection.Close();
      _connection = null;
    }
    _initialized = false;
  }
}
