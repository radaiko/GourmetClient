using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace GC.Views;

/// <summary>
/// Shared content builder for About view across platforms
/// </summary>
internal static class AboutViewShared {
  internal static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  internal static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

  internal static SolidColorBrush GetLinkBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#4A9EFF")
      : Color.Parse("#007AFF"));

  /// <summary>
  /// Creates the main content panel with version, description, and credits
  /// </summary>
  internal static StackPanel CreateContentPanel() {
    var contentPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 14
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

    // Credits section
    contentPanel.Children.Add(CreateCreditsSection());

    return contentPanel;
  }

  private static StackPanel CreateCreditsSection() {
    var creditsStack = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 8,
      Margin = new Thickness(0, 12, 0, 0)
    };

    creditsStack.Children.Add(new TextBlock {
      Text = "Credits",
      FontSize = 18,
      FontWeight = FontWeight.SemiBold,
      Foreground = GetTextBrush()
    });

    creditsStack.Children.Add(new TextBlock {
      Text = "Dieses Programm verwendet folgende Open-Source-Bibliotheken:",
      FontSize = 14,
      Foreground = GetSecondaryTextBrush(),
      TextWrapping = TextWrapping.Wrap,
      Margin = new Thickness(0, 0, 0, 8)
    });

    // List of used NuGet packages
    var packages = new[] {
      ("Avalonia", "Cross-platform UI framework"),
      ("CommunityToolkit.Mvvm", "MVVM helpers and utilities"),
      ("HtmlAgilityPack", "HTML parsing library"),
      ("Microsoft.Extensions.*", "Dependency injection and logging"),
      ("Semver", "Semantic versioning library")
    };

    foreach (var (name, description) in packages) {
      var packageStack = new StackPanel {
        Orientation = Orientation.Vertical,
        Spacing = 2,
        Margin = new Thickness(0, 0, 0, 8)
      };

      packageStack.Children.Add(new TextBlock {
        Text = $"• {name}",
        FontSize = 14,
        FontWeight = FontWeight.Medium,
        Foreground = GetTextBrush()
      });

      packageStack.Children.Add(new TextBlock {
        Text = description,
        FontSize = 13,
        Foreground = GetSecondaryTextBrush(),
        Margin = new Thickness(12, 0, 0, 0)
      });

      creditsStack.Children.Add(packageStack);
    }

    return creditsStack;
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

