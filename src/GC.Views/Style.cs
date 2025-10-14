using Avalonia.Media;

namespace GC.Views;

/// <summary>
///   Style class to hold common styles, brushes and resources for the application.
/// </summary>
public static class Style {
  public static bool IsDarkMode { get; set; } = false;

  public static SolidColorBrush GetBackgroundBrush() => new(IsDarkMode ? Color.Parse("#000000") : Color.Parse("#ffffff"));

  public static SolidColorBrush GetTextBrush() => new(IsDarkMode ? Colors.White : Colors.Black);

  public static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));
}