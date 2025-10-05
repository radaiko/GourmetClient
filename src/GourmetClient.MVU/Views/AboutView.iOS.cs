using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

// iOS specific About view (touch friendly, simplified layout widths)
public static class AboutViewIOS {
  private static SolidColorBrush GetCardBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#2E2E2E") // Slightly darker on iOS for contrast behind nav bars
      : Colors.White);

  public static Control Create(AppState state, Action<Msg> dispatch) {
    // Use ScrollViewer so smaller screens can scroll content
    var contentPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 14,
      Margin = new Thickness(12)
    };

    // Version row (vertical on iPhone for better wrapping)
    var versionStack = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 6
    };

    versionStack.Children.Add(new TextBlock {
      Text = "Version: 1.0.0",
      FontSize = 16,
      FontWeight = FontWeight.Medium,
      Foreground = AboutViewShared.GetTextBrush()
    });

    var releaseNotesBtn = MakeLinkButton("Versionsinformationen", () => dispatch(new ShowReleaseNotes()));
    versionStack.Children.Add(releaseNotesBtn);
    contentPanel.Children.Add(versionStack);

    contentPanel.Children.Add(new TextBlock {
      Text = "Dieses Programm wird ohne Garantie ausgeliefert. Verwendung auf eigene Verantwortung.",
      FontSize = 14,
      Foreground = AboutViewShared.GetTextBrush(),
      TextWrapping = TextWrapping.Wrap
    });

    var border = new Border {
      Background = GetCardBackgroundBrush(),
      BorderBrush = new SolidColorBrush(Colors.Gray),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(10),
      Padding = new Thickness(0),
      MaxWidth = 500,
    };

    var scroll = new ScrollViewer { Content = contentPanel };

    border.Child = scroll;
    return border;
  }

  private static Button MakeLinkButton(string text, Action onTap, double fontSize = 14) {
    var btn = new Button {
      Content = text,
      Background = Brushes.Transparent,
      BorderThickness = new Thickness(0),
      Foreground = AboutViewShared.GetLinkBrush(),
      FontSize = fontSize,
      Padding = new Thickness(2, 4)
    };
    btn.Click += (_, _) => onTap();
    return btn;
  }
}
