using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Utils;

namespace GourmetClient.MVU.Views;

/// <summary>
/// Main billing view that routes to platform-specific implementations
/// </summary>
public static class BillingView {
  public static Control Create(AppState state, Action<Msg> dispatch) {
    // Use iOS-specific layout on iOS devices
    if (PlatformDetector.IsIOS) {
      return BillingViewIOS.Create(state, dispatch);
    }
    
    // Use desktop layout for all other platforms
    return BillingViewDesktop.Create(state, dispatch);
  }
}

/// <summary>
/// Shared utilities and styling for billing views across all platforms
/// </summary>
public static class BillingViewShared {
  public static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  public static SolidColorBrush GetMinimalistBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#0d1117")
      : Color.Parse("#ffffff"));

  public static SolidColorBrush GetMinimalistBorderBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#21262d")
      : Color.Parse("#d0d7de"));

  public static SolidColorBrush GetPositiveBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#28a745")
      : Color.Parse("#22863a"));

  public static SolidColorBrush GetNegativeBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#d73a49")
      : Color.Parse("#cb2431"));

  public static Control CreateLoadingView() {
    // Delegates to shared factory to avoid duplication; spinner glyph unified in factory
    return LoadingViewFactory.Create(
      message: "Lade Abrechnungsdaten...",
      spinnerFontSize: 32,
      textFontSize: 14,
      spacing: 16,
      margin: new Thickness(0, 80),
      spinnerColor: Color.Parse("#007ACC"),
      textBrush: GetTextBrush(),
      textFontWeight: FontWeight.Light,
      textOpacity: 0.7
    );
  }
}