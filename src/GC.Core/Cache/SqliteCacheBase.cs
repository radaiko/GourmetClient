using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;

namespace GC.Core.Cache;

public static partial class SqliteCacheBase {
  private static bool _isInitialized;
  private static string _dbPath = string.Empty;
  private static readonly object _initLock = new();

  public static void SetDbPathForTests(string path) {
    // Allow overriding DB path in tests
    _dbPath = path ?? string.Empty;
    _isInitialized = true;
  }

  private static void Initialize() {
    if (_isInitialized) return;
    lock (_initLock) {
      if (_isInitialized) return;
      _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "cache.db");
      // ensure directory exists
      try {
        var dir = Path.GetDirectoryName(_dbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
      }
      catch {
        // ignore
      }
      _isInitialized = true;
    }
  }


  // allow simple safe names (letters, digits, underscore) to avoid SQL injection via table name
  [GeneratedRegex("^[A-Za-z0-9_]+$")]
  private static partial Regex SafeNames();

  private static void ValidateTableName(string tableName) {
    if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("Table name cannot be null or empty", nameof(tableName));
    if (!SafeNames().IsMatch(tableName)) throw new ArgumentException("Invalid table name", nameof(tableName));
  }

  // Open a connection to the cache DB and ensure it's ready
  public static SqliteConnection OpenConnection() {
    Initialize();
    var cs = new SqliteConnectionStringBuilder { DataSource = _dbPath }.ToString();
    var conn = new SqliteConnection(cs);
    conn.Open();
    return conn;
  }

  // Ensure a table exists by running the provided CREATE statement. Table name will be validated.
  public static void EnsureTable(string tableName, string createStatement) {
    ValidateTableName(tableName);
    using var conn = OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = createStatement;
    cmd.ExecuteNonQuery();
  }

  // Execute a non-query with named parameters. Parameter tuples are (name, value).
  public static int ExecuteNonQuery(string sql, params (string name, object? value)[] parameters) {
    using var conn = OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    foreach (var (name, value) in parameters) {
      cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }
    return cmd.ExecuteNonQuery();
  }

  // Read a single string column (e.g., Payload) from a table using the given WHERE clause and parameters.
  // whereClause should include the WHERE keyword if needed (e.g. "WHERE Date = $date").
  public static List<string> ReadStringColumn(string tableName, string columnName, string whereClause, params (string name, object? value)[] parameters) {
    ValidateTableName(tableName);
    var result = new List<string>();
    using var conn = OpenConnection();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = $"SELECT {columnName} FROM {tableName} {whereClause}";
    foreach (var (name, value) in parameters) {
      cmd.Parameters.AddWithValue(name, value ?? DBNull.Value);
    }
    using var rdr = cmd.ExecuteReader();
    while (rdr.Read()) {
      if (!rdr.IsDBNull(0)) result.Add(rdr.GetString(0));
    }
    return result;
  }
}
