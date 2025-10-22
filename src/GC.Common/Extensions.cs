namespace GC.Common;

public static class StringExtensions {
  public static bool IsBlank(this string? str) {
    return string.IsNullOrWhiteSpace(str);
  }
}