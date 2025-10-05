using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Utils;

namespace GourmetClient.MVU.Views;

/// <summary>
/// Main About view that routes to platform specific implementations.
/// </summary>
public static class AboutView {
  public static Control Create(AppState state, Action<Msg> dispatch) {
    if (PlatformDetector.IsIOS) {
      return AboutViewIOS.Create(state, dispatch);
    }
    return AboutViewDesktop.Create(state, dispatch);
  }
}

/// <summary>
/// Shared helpers for About view styling.
/// </summary>
public static class AboutViewShared {
  public static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Colors.White : Colors.Black);

  public static SolidColorBrush GetLinkBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Color.Parse("#4A9EFF") : Colors.Blue);
}
