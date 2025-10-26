using System.Diagnostics;

namespace GC.Tests.Helpers;

public static class PathHelper {
  public static string GetTempPath() => Path.GetFullPath(Path.Combine("..", "..", "..", "Temp"));
  public static string GetTempDbPath() {
    var filename = $"{Guid.NewGuid()}.db";
    if (Debugger.IsAttached) {
      filename = "gc_test_debug.db";
    }
    return Path.GetFullPath(Path.Combine(GetTempPath(), filename));
  }
}