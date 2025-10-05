using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// iOS-optimized menu view with vertical paged layout
/// </summary>
public static class MenuViewIOS
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
        // Placeholder for menu view
        return new ScrollViewer
        {
            Background = GetBackgroundBrush(),
            Content = new StackPanel
            {
                Margin = new Thickness(12),
                Spacing = 12,
                Children =
                {
                    CreateWelcomeCard()
                }
            }
        };
    }

    private static Control CreateWelcomeCard()
    {
        return new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(20),
            Child = new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    new TextBlock
                    {
                        Text = "🍽️ Willkommen",
                        FontSize = 28,
                        FontWeight = FontWeight.Bold,
                        Foreground = GetTextBrush(),
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = "Bitte konfigurieren Sie Ihre Anmeldedaten in den Einstellungen, um Menüs anzuzeigen.",
                        FontSize = 16,
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
