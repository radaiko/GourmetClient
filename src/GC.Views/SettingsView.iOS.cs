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
                    CreateGourmetSection(viewModel),
                    CreateVentoPaySection(viewModel)
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

    private static Control CreateGourmetSection(MainViewModel viewModel)
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
                    CreateBoundTextBox("Benutzername", viewModel.GourmetUsername, text => viewModel.GourmetUsername = text),
                    CreateBoundPasswordBox("Passwort", viewModel.GourmetPassword, text => viewModel.GourmetPassword = text)
                }
            }
        };
    }

    private static Control CreateVentoPaySection(MainViewModel viewModel)
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
                    CreateBoundTextBox("Benutzername", viewModel.VentoPayUsername, text => viewModel.VentoPayUsername = text),
                    CreateBoundPasswordBox("Passwort", viewModel.VentoPayPassword, text => viewModel.VentoPayPassword = text)
                }
            }
        };
    }

    private static Control CreateBoundTextBox(string watermark, string text, Action<string> onChanged)
    {
        var textBox = new TextBox
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
        textBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                onChanged(textBox.Text ?? "");
        };
        return textBox;
    }

    private static Control CreateBoundPasswordBox(string watermark, string text, Action<string> onChanged)
    {
        var textBox = new TextBox
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
        textBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                onChanged(textBox.Text ?? "");
        };
        return textBox;
    }
}
