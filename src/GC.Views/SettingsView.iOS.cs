using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// iOS-optimized settings view with mobile-friendly form layout
/// </summary>
public static class SettingsViewIOS
{
    private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

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

    public static Control Create(MainViewModel viewModel)
    {
        return new ScrollViewer
        {
            Background = GetBackgroundBrush(),
            Content = new StackPanel
            {
                Margin = new Thickness(12),
                Spacing = 20,
                Children =
                {
                    CreateHeader(),
                    CreateGourmetSection(),
                    CreateVentoPaySection(),
                    CreateAppSettingsSection()
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
                    Text = "Einstellungen",
                    FontSize = 28,
                    FontWeight = FontWeight.Bold,
                    Foreground = GetTextBrush()
                },
                new TextBlock
                {
                    Text = "Konfiguration der Anmeldedaten und Anwendungseinstellungen",
                    FontSize = 14,
                    Foreground = GetSecondaryTextBrush(),
                    TextWrapping = TextWrapping.Wrap
                }
            }
        };
    }

    private static Control CreateGourmetSection()
    {
        return new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Gourmet Anmeldedaten",
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = GetTextBrush()
                    },
                    CreateTextBox("Benutzername", ""),
                    CreatePasswordBox("Passwort", "")
                }
            }
        };
    }

    private static Control CreateVentoPaySection()
    {
        return new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "VentoPay Anmeldedaten",
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = GetTextBrush()
                    },
                    CreateTextBox("Benutzername", ""),
                    CreatePasswordBox("Passwort", "")
                }
            }
        };
    }

    private static Control CreateAppSettingsSection()
    {
        return new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(16),
            Child = new StackPanel
            {
                Spacing = 12,
                Children =
                {
                    new TextBlock
                    {
                        Text = "Anwendungseinstellungen",
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = GetTextBrush()
                    },
                    CreateCheckBox("Automatische Updates"),
                    CreateCheckBox("Mit Windows starten")
                }
            }
        };
    }

    private static Control CreateTextBox(string watermark, string text)
    {
        return new TextBox
        {
            Text = text,
            Watermark = watermark,
            FontSize = 17,
            Padding = new Thickness(12),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            CornerRadius = new CornerRadius(8),
            Background = Brushes.Transparent
        };
    }

    private static Control CreatePasswordBox(string watermark, string text)
    {
        return new TextBox
        {
            Text = text,
            Watermark = watermark,
            PasswordChar = '•',
            FontSize = 17,
            Padding = new Thickness(12),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            CornerRadius = new CornerRadius(8),
            Background = Brushes.Transparent
        };
    }

    private static Control CreateCheckBox(string label)
    {
        return new CheckBox
        {
            Content = label,
            FontSize = 16,
            Foreground = GetTextBrush(),
            Padding = new Thickness(8, 4)
        };
    }
}
