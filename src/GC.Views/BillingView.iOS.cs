using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// iOS-optimized billing view with mobile-friendly layout
/// </summary>
public static class BillingViewIOS
{
    private static SolidColorBrush GetBackgroundBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#000000")
        : Color.Parse("#F2F2F7"));

    private static SolidColorBrush GetCardBackgroundBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#1C1C1E")
        : Colors.White);

    private static SolidColorBrush GetTextBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Colors.White
            : Colors.Black);

    private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

    public static Control Create(MainViewModel viewModel)
    {
        return new ScrollViewer
        {
            Background = GetBackgroundBrush(),
            Content = new StackPanel
            {
                Margin = new Thickness(12),
                Spacing = 12,
                Children =
                {
                    CreateHeader(),
                    CreatePlaceholderCard()
                }
            }
        };
    }

    private static Control CreateHeader()
    {
        return new StackPanel
        {
            Spacing = 8,
            Children =
            {
                new TextBlock
                {
                    Text = "Ihre Abrechnungsinformationen",
                    FontSize = 14,
                    Foreground = GetSecondaryTextBrush()
                }
            }
        };
    }

    private static Control CreatePlaceholderCard()
    {
        return new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "💳",
                        FontSize = 48,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = "Keine Abrechnungsdaten verfügbar",
                        FontSize = 16,
                        Foreground = GetTextBrush(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = "Bitte konfigurieren Sie Ihre VentoPay-Anmeldedaten in den Einstellungen.",
                        FontSize = 14,
                        Foreground = GetSecondaryTextBrush(),
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    }
                }
            }
        };
    }
}
