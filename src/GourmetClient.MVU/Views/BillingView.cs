using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
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
    var loadingPanel = new StackPanel {
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Spacing = 16,
      Margin = new Thickness(0, 80)
    };

    var spinner = new TextBlock {
      Text = "⟳",
      FontSize = 32,
      Foreground = new SolidColorBrush(Color.Parse("#007ACC")),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      RenderTransformOrigin = RelativePoint.Center
    };

    // Create rotation animation using a simple timer-based approach
    var rotateTransform = new RotateTransform();
    spinner.RenderTransform = rotateTransform;

    // Use a timer for smooth rotation animation
    var timer = new System.Timers.Timer(16); // ~60 FPS
    var angle = 0.0;
    timer.Elapsed += (sender, e) =>
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            angle += 6; // 360 degrees in 1 second (6 degrees per 16ms)
            if (angle >= 360) angle = 0;
            rotateTransform.Angle = angle;
        });
    };
    timer.Start();

    loadingPanel.Children.Add(spinner);

    var loadingText = new TextBlock {
      Text = "Lade Abrechnungsdaten...",
      FontSize = 14,
      FontWeight = FontWeight.Light,
      Foreground = GetTextBrush(),
      Opacity = 0.7,
      HorizontalAlignment = HorizontalAlignment.Center
    };
    loadingPanel.Children.Add(loadingText);

    return loadingPanel;
  }
}