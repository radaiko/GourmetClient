using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

public static class AboutView {
  private static SolidColorBrush GetTextBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Colors.White 
      : Colors.Black);
      
  private static SolidColorBrush GetLinkBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#4A9EFF") 
      : Colors.Blue);

  private static SolidColorBrush GetCardBackgroundBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#3C3C3C") 
      : Colors.White);

  public static Control Create(AppState state, Action<Msg> dispatch) {
    var border = new Border {
      Background = GetCardBackgroundBrush(),
      BorderBrush = new SolidColorBrush(Colors.Gray),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(5),
      Padding = new Thickness(15),
      MinWidth = 300,
      MaxWidth = 400
    };

    var mainPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 10
    };

    // Version info section
    var versionPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Spacing = 10
    };

    var versionText = new TextBlock {
      Text = "Version: 1.0.0",
      FontSize = 14,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    versionPanel.Children.Add(versionText);

    var releaseNotesButton = new Button {
      Content = "Versionsinformationen",
      Background = Brushes.Transparent,
      BorderThickness = new Thickness(0),
      Foreground = GetLinkBrush(),
      Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
    };
    releaseNotesButton.Click += (_, _) => dispatch(new ShowReleaseNotes());
    versionPanel.Children.Add(releaseNotesButton);

    mainPanel.Children.Add(versionPanel);

    // Disclaimer text
    var disclaimerText = new TextBlock {
      Text = "Dieses Programm wird ohne Garantie ausgeliefert. Verwendung auf eigene Verantwortung.",
      FontSize = 12,
      Foreground = GetTextBrush(),
      TextWrapping = TextWrapping.Wrap,
      Margin = new Thickness(0, 5)
    };
    mainPanel.Children.Add(disclaimerText);

    // Icon attribution section
    var attributionPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Spacing = 5
    };

    var iconText = new TextBlock {
      Text = "Icons made by ",
      FontSize = 11,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    attributionPanel.Children.Add(iconText);

    var iconAuthorLink = new Button {
      Content = "Smashicons",
      Background = Brushes.Transparent,
      BorderThickness = new Thickness(0),
      Foreground = GetLinkBrush(),
      FontSize = 11,
      Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
    };
    iconAuthorLink.Click += (_, _) => dispatch(new OpenIconAuthorWebPage());
    attributionPanel.Children.Add(iconAuthorLink);

    var fromText = new TextBlock {
      Text = " from ",
      FontSize = 11,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    attributionPanel.Children.Add(fromText);

    var flatIconLink = new Button {
      Content = "www.flaticon.com",
      Background = Brushes.Transparent,
      BorderThickness = new Thickness(0),
      Foreground = GetLinkBrush(),
      FontSize = 11,
      Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
    };
    flatIconLink.Click += (_, _) => dispatch(new OpenFlatIconWebPage());
    attributionPanel.Children.Add(flatIconLink);

    mainPanel.Children.Add(attributionPanel);

    // License info
    var licenseText = new TextBlock {
      Text = "is licensed by Creative Commons BY 3.0",
      FontSize = 11,
      Foreground = GetTextBrush(),
      Margin = new Thickness(0, 2)
    };
    mainPanel.Children.Add(licenseText);

    border.Child = mainPanel;
    return border;
  }
}
