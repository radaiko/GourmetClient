namespace GC.Tests.Helpers;

public static class PathHelper {
  public static string GetTempPath() => Path.GetFullPath(System.IO.Path.Combine("..", "..", "..", "Temp"));
  public static string GetTempDbPath(string dbName) => Path.GetFullPath(System.IO.Path.Combine(GetTempPath(), dbName));
}