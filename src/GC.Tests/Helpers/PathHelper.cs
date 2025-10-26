namespace GC.Tests.Helpers;

public static class PathHelper {
  public static string GetTempPath() => Path.GetFullPath(System.IO.Path.Combine("..", "..", "..", "Temp"));
  public static string GetTempDbPath() => Path.GetFullPath(System.IO.Path.Combine(GetTempPath(), "test.db"));
}