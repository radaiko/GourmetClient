using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// Desktop variant of the About view. Uses a centered, scrollable content card.
/// </summary>
public static class AboutViewDesktop
{
    private static SolidColorBrush Card() => new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Color.Parse("#2B2B2B") : Colors.White);
    private static SolidColorBrush Txt() => new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Colors.White : Colors.Black);
    private static SolidColorBrush Sub() => new(Color.Parse("#6E6E73"));
    private static SolidColorBrush Link() => new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Color.Parse("#4A9EFF") : Color.Parse("#0066CC"));

    public static Control Create(MainViewModel vm)
    {
        var scroll = new ScrollViewer();
        var wrapper = new Grid();
        scroll.Content = wrapper;

        var panel = new StackPanel
        {
            Spacing = 18,
            Width = 760,
            Margin = new Thickness(30)
        };
        wrapper.Children.Add(new StackPanel { Children = { panel }, HorizontalAlignment = HorizontalAlignment.Center });

        panel.Children.Add(Header("Über diese Anwendung"));
        panel.Children.Add(new TextBlock
        {
            Text = "Version: 1.0.0",
            FontSize = 16,
            FontWeight = FontWeight.Medium,
            Foreground = Txt()
        });
        panel.Children.Add(new TextBlock
        {
            Text = "Dieses Programm wird ohne Garantie ausgeliefert. Verwendung auf eigene Verantwortung.",
            FontSize = 14,
            Foreground = Txt(),
            TextWrapping = TextWrapping.Wrap
        });
        panel.Children.Add(Header("Icons"));
        panel.Children.Add(new TextBlock
        {
            Text = "Icons erstellt von Freepik auf flaticon.com",
            FontSize = 13,
            Foreground = Sub(),
            TextWrapping = TextWrapping.Wrap
        });

        var card = new Border
        {
            Background = Card(),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(24),
            Child = scroll
        };

        return new Grid
        {
            Children = { card }
        };
    }

    private static Control Header(string text) => new TextBlock
    {
        Text = text,
        FontSize = 20,
        FontWeight = FontWeight.SemiBold,
        Foreground = Txt(),
        Margin = new Thickness(0, 12, 0, 4)
    };
}

