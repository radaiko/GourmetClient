using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace GC.Database;

public class SqliteService {
  private readonly ILogger<SqliteService> _logger;
  private readonly string _dbPath;

  public SqliteService(ILogger<SqliteService> logger) {
    _logger = logger;
    _dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GourmetClient.db");
    Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
    InitializeDatabase();
  }

  private void InitializeDatabase() {
    // Initialize billing debug database
    using var connection = new SqliteConnection($"Data Source={_dbPath}");
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS BillingCache (
                Month TEXT PRIMARY KEY,
                Data TEXT,
                LastWrite TEXT
            );
        ";
    command.ExecuteNonQuery();

    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS DebugBillingCache (
                Month TEXT PRIMARY KEY,
                Data TEXT,
                LastWrite TEXT
            );
        ";
    command.ExecuteNonQuery();

    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS MenuCache (
                Key TEXT PRIMARY KEY,
                Data TEXT,
                LastWrite TEXT
            );
        ";
    command.ExecuteNonQuery();

    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS DebugMenuCache (
                Key TEXT PRIMARY KEY,
                Data TEXT,
                LastWrite TEXT
            );
        ";
    command.ExecuteNonQuery();
  }

  public async Task SaveBillingDataAsync(DateTime month, string data) {
    // Write billing data to database
    _logger.LogInformation("Saving billing data for month: {Month}", month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            INSERT OR REPLACE INTO BillingCache (Month, Data, LastWrite)
            VALUES ($month, $data, $lastWrite);
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("$data", data);
    command.Parameters.AddWithValue("$lastWrite", DateTime.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
  }

  public async Task<string?> LoadBillingDataAsync(DateTime month) {
    // Read billing data from database
    _logger.LogInformation("Loading billing data for month: {Month}", month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT Data FROM BillingCache WHERE Month = $month;
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));

    var result = await command.ExecuteScalarAsync();
    return result as string;
  }

  public async Task<DateTime?> GetBillingLastWriteAsync(DateTime month) {
    // Get timestamp of last write
    _logger.LogInformation("Getting billing last write timestamp for month: {Month}", month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT LastWrite FROM BillingCache WHERE Month = $month;
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));

    var result = await command.ExecuteScalarAsync();
    if (result is string lastWriteStr && DateTime.TryParse(lastWriteStr, out var lastWrite)) {
      return lastWrite;
    }
    return null;
  }

  public async Task SaveDebugBillingDataAsync(DateTime month, string data) {
    // Write debug billing data to database
    _logger.LogInformation("Saving debug billing data for month: {Month}", month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            INSERT OR REPLACE INTO DebugBillingCache (Month, Data, LastWrite)
            VALUES ($month, $data, $lastWrite);
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("$data", data);
    command.Parameters.AddWithValue("$lastWrite", DateTime.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
  }

  public async Task<string?> LoadDebugBillingDataAsync(DateTime month) {
    // Read debug billing data from database
    _logger.LogInformation("Loading debug billing data for month: {Month}", month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT Data FROM DebugBillingCache WHERE Month = $month;
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));

    var result = await command.ExecuteScalarAsync();
    return result as string;
  }

  public async Task<DateTime?> GetDebugBillingLastWriteAsync(DateTime month) {
    // Get timestamp of last write for debug
    _logger.LogInformation("Getting debug billing last write timestamp for month: {Month}", month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT LastWrite FROM DebugBillingCache WHERE Month = $month;
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));

    var result = await command.ExecuteScalarAsync();
    if (result is string lastWriteStr && DateTime.TryParse(lastWriteStr, out var lastWrite)) {
      return lastWrite;
    }
    return null;
  }

  public async Task SaveDataAsync(string table, DateTime month, string data) {
    // Write data to the specified table
    _logger.LogInformation("Saving {Table} data for month: {Month}", table, month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
            INSERT OR REPLACE INTO {table} (Month, Data, LastWrite)
            VALUES ($month, $data, $lastWrite);
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));
    command.Parameters.AddWithValue("$data", data);
    command.Parameters.AddWithValue("$lastWrite", DateTime.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
  }

  public async Task<string?> LoadDataAsync(string table, DateTime month) {
    // Read data from the specified table
    _logger.LogInformation("Loading {Table} data for month: {Month}", table, month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
            SELECT Data FROM {table} WHERE Month = $month;
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));

    var result = await command.ExecuteScalarAsync();
    return result as string;
  }

  public async Task<DateTime?> GetLastWriteAsync(string table, DateTime month) {
    // Get timestamp of last write for the specified table
    _logger.LogInformation("Getting {Table} last write timestamp for month: {Month}", table, month);

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = $@"
            SELECT LastWrite FROM {table} WHERE Month = $month;
        ";
    command.Parameters.AddWithValue("$month", month.ToString("yyyy-MM-dd"));

    var result = await command.ExecuteScalarAsync();
    if (result is string lastWriteStr && DateTime.TryParse(lastWriteStr, out var lastWrite)) {
      return lastWrite;
    }
    return null;
  }

  public async Task SaveMenuDataAsync(string data) {
    // Write menu data to database
    _logger.LogInformation("Saving menu data");

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            INSERT OR REPLACE INTO MenuCache (Key, Data, LastWrite)
            VALUES ($key, $data, $lastWrite);
        ";
    command.Parameters.AddWithValue("$key", "current");
    command.Parameters.AddWithValue("$data", data);
    command.Parameters.AddWithValue("$lastWrite", DateTime.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
  }

  public async Task<string?> LoadMenuDataAsync() {
    // Read menu data from database
    _logger.LogInformation("Loading menu data");

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT Data FROM MenuCache WHERE Key = $key;
        ";
    command.Parameters.AddWithValue("$key", "current");

    var result = await command.ExecuteScalarAsync();
    return result as string;
  }

  public async Task<DateTime?> GetMenuLastWriteAsync() {
    // Get timestamp of last write for menu
    _logger.LogInformation("Getting menu last write timestamp");

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT LastWrite FROM MenuCache WHERE Key = $key;
        ";
    command.Parameters.AddWithValue("$key", "current");

    var result = await command.ExecuteScalarAsync();
    if (result is string lastWriteStr && DateTime.TryParse(lastWriteStr, out var lastWrite)) {
      return lastWrite;
    }
    return null;
  }

  public async Task SaveDebugMenuDataAsync(string data) {
    // Write debug menu data to database
    _logger.LogInformation("Saving debug menu data");

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            INSERT OR REPLACE INTO DebugMenuCache (Key, Data, LastWrite)
            VALUES ($key, $data, $lastWrite);
        ";
    command.Parameters.AddWithValue("$key", "current");
    command.Parameters.AddWithValue("$data", data);
    command.Parameters.AddWithValue("$lastWrite", DateTime.UtcNow.ToString("O"));

    await command.ExecuteNonQueryAsync();
  }

  public async Task<string?> LoadDebugMenuDataAsync() {
    // Read debug menu data from database
    _logger.LogInformation("Loading debug menu data");

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT Data FROM DebugMenuCache WHERE Key = $key;
        ";
    command.Parameters.AddWithValue("$key", "current");

    var result = await command.ExecuteScalarAsync();
    return result as string;
  }

  public async Task<DateTime?> GetDebugMenuLastWriteAsync() {
    // Get timestamp of last write for debug menu
    _logger.LogInformation("Getting debug menu last write timestamp");

    await using var connection = new SqliteConnection($"Data Source={_dbPath}");
    await connection.OpenAsync();

    var command = connection.CreateCommand();
    command.CommandText = @"
            SELECT LastWrite FROM DebugMenuCache WHERE Key = $key;
        ";
    command.Parameters.AddWithValue("$key", "current");

    var result = await command.ExecuteScalarAsync();
    if (result is string lastWriteStr && DateTime.TryParse(lastWriteStr, out var lastWrite)) {
      return lastWrite;
    }
    return null;
  }
}