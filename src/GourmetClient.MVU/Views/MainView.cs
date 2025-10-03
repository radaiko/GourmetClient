using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Utils;

namespace GourmetClient.MVU.Views;

/// <summary>
/// Main view that routes to platform-specific implementations
/// </summary>
public static class MainView {
  public static Control Create(AppState state, Action<Msg> dispatch) {
    // Use iOS-specific layout on iOS devices
    if (PlatformDetector.IsIOS) {
      return MainViewIOS.Create(state, dispatch);
    }
    
    // Use desktop layout for all other platforms
    return MainViewDesktop.Create(state, dispatch);
  }
}

/// <summary>
/// Shared utilities and styling for main views across all platforms
/// </summary>
public static class MainViewShared {
  // Theme-aware color brushes that work in both light and dark modes
  public static SolidColorBrush GetActionBarBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#2D2D30")
      : Color.Parse("#F2F2F2"));

  public static SolidColorBrush GetCardBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#3C3C3C")
      : Colors.White);

  public static SolidColorBrush GetBorderBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#464647")
      : Color.Parse("#F2F2F2"));

  public static SolidColorBrush GetTransparentBrush() => new(Colors.Transparent);

  public static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  public static (int orderCount, int cancelCount) CountMarkedMenus(AppState state) {
    if (state.MenuDays == null) return (0, 0);

    var orderCount = 0;
    var cancelCount = 0;

    foreach (var day in state.MenuDays) {
      foreach (var menu in day.Menus) {
        if (menu.MenuState == GourmetMenuState.MarkedForOrder) orderCount++;
        else if (menu.MenuState == GourmetMenuState.MarkedForCancel) cancelCount++;
      }
    }

    return (orderCount, cancelCount);
  }

  public static Control CreateIconButton(string iconName) {
    var border = new Border {
      Width = 32,
      Height = 32,
      Background = GetTransparentBrush(),
      CornerRadius = new CornerRadius(3),
      BorderBrush = GetTransparentBrush(),
      BorderThickness = new Thickness(0)
    };

    var isDark = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
    var iconText = iconName switch {
      "Bill.png" => isDark ? "💳" : "💰",
      "RefreshLocalData.png" => isDark ? "⟳" : "🔄",
      "ExecuteOrder.png" => isDark ? "☑" : "✓",
      "Information.png" => isDark ? "🛈" : "ℹ️",
      "Settings.png" => isDark ? "⚙" : "⚙️",
      "Error.png" => isDark ? "⚠" : "❌",
      _ => isDark ? "📋" : "📄"
    };

    var textBlock = new TextBlock {
      Text = iconText,
      FontSize = 16,
      Foreground = GetTextBrush(),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center
    };

    border.Child = textBlock;
    return border;
  }

  public static Panel CreateErrorPanel(AppState state, Action<Msg> dispatch) {
    var isDark = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;

    var panel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Background = new SolidColorBrush(isDark ? Color.Parse("#4D1F1F") : Colors.LightCoral),
      Margin = new Thickness(5)
    };

    var errorIcon = CreateIconButton("Error.png");
    panel.Children.Add(errorIcon);

    var errorText = new TextBlock {
      Text = state.ErrorMessage,
      Foreground = new SolidColorBrush(isDark ? Color.Parse("#FF9999") : Colors.DarkRed),
      Margin = new Thickness(10, 5),
      VerticalAlignment = VerticalAlignment.Center
    };
    panel.Children.Add(errorText);

    var clearButton = new Button {
      Content = "Clear",
      Margin = new Thickness(10, 5)
    };
    clearButton.Click += (_, _) => dispatch(new ClearError());
    panel.Children.Add(clearButton);

    return panel;
  }
}