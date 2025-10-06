using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
///   iOS-optimized About view with touch-friendly layout
/// </summary>
public static class AboutViewIOS {
  private static SolidColorBrush GetCardBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#2E2E2E")
      : Colors.White);

  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

  private static SolidColorBrush GetLinkBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#4A9EFF")
      : Color.Parse("#007AFF"));

  public static Control Create(MainViewModel viewModel) {
    var contentPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 14,
      Margin = new Thickness(12)
    };

    // Version section
    var versionStack = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 6
    };

    versionStack.Children.Add(new TextBlock {
      Text = "Version: 1.0.0",
      FontSize = 16,
      FontWeight = FontWeight.Medium,
      Foreground = GetTextBrush()
    });

    var releaseNotesBtn = MakeLinkButton("Versionsinformationen");
    versionStack.Children.Add(releaseNotesBtn);
    contentPanel.Children.Add(versionStack);

    // Description
    contentPanel.Children.Add(new TextBlock {
      Text = "Dieses Programm wird ohne Garantie ausgeliefert. Verwendung auf eigene Verantwortung.",
      FontSize = 14,
      Foreground = GetTextBrush(),
      TextWrapping = TextWrapping.Wrap
    });

    // TODO: add used nugets for credits

    var border = new Border {
      Padding = new Thickness(16),
      MaxWidth = 500,
    };

    var scroll = new ScrollViewer { Content = contentPanel };
    border.Child = scroll;

    return border;
  }

  private static Button MakeLinkButton(string text, double fontSize = 14) =>
    new() {
      Content = text,
      FontSize = fontSize,
      Foreground = GetLinkBrush(),
      Background = Brushes.Transparent,
      BorderBrush = Brushes.Transparent,
      Padding = new Thickness(0),
      Cursor = new Cursor(StandardCursorType.Hand)
    };
}