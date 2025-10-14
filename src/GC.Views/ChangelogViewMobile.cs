// filepath: /Users/aikoradlingmayr/Git/PRIVAT/GourmetClient/src/GC.Views/ChangelogView.iOS.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Controls.Primitives; // for ScrollBarVisibility
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// Simple iOS styled changelog view displayed inside an overlay card.
/// </summary>
public static class ChangelogViewMobile {
  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark ? Colors.White : Colors.Black);

  private static SolidColorBrush GetSecondaryBrush() => new(Color.Parse("#8E8E93"));

  public static Control Create(MainViewModel vm) {
    var stack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 14, Margin = new Thickness(16, 8) };

    stack.Children.Add(new TextBlock {
      Text = "Changelog",
      FontSize = 22,
      FontWeight = FontWeight.SemiBold,
      Foreground = GetTextBrush(),
      Margin = new Thickness(0, 0, 0, 4)
    });

    stack.Children.Add(new TextBlock {
      Text = "Eine Übersicht der wichtigsten Änderungen.",
      FontSize = 14,
      Foreground = GetSecondaryBrush(),
      TextWrapping = TextWrapping.Wrap
    });

    // Static sample entries; later could be loaded from resource / file.
    stack.Children.Add(MakeEntry("1.0.0", "Erste veröffentlichte Version mit Menüanzeige, Abrechnung, Einstellungen, Über-Ansicht und Changelog."));
    stack.Children.Add(MakeEntry("0.9.0", "Interne Testversion, Grundfunktionen integriert."));

    return new ScrollViewer {
      Content = stack,
      HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
    };
  }

  private static Control MakeEntry(string version, string description) {
    var panel = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
    panel.Children.Add(new TextBlock {
      Text = version,
      FontSize = 16,
      FontWeight = FontWeight.Medium,
      Foreground = GetTextBrush()
    });
    panel.Children.Add(new TextBlock {
      Text = description,
      FontSize = 14,
      Foreground = GetSecondaryBrush(),
      TextWrapping = TextWrapping.Wrap
    });
    return panel;
  }
}