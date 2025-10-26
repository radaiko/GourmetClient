namespace GC.Common;

public static class StringExtensions {
  public static bool IsBlank(this string? str) {
    return string.IsNullOrWhiteSpace(str);
  }
  
  public static DateOnly ToDateOnly(this DateTime time) {
    return DateOnly.FromDateTime(time);
  }
}