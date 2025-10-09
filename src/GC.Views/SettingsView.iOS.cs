using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
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
            Focusable = false,
            Content = new StackPanel
            {
                Margin = new Thickness(12),
                Spacing = 20,
                Children =
                {
                    CreateHeader(),
                    CreateGourmetSection(),
                    CreateVentoPaySection(),
                    CreateInfoSection(viewModel)
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
                    CreateBoundTextBox("Benutzername", nameof(MainViewModel.GourmetUsername)),
                    CreateBoundPasswordBox("Passwort", nameof(MainViewModel.GourmetPassword))
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
                    CreateBoundTextBox("Benutzername", nameof(MainViewModel.VentoPayUsername)),
                    CreateBoundPasswordBox("Passwort", nameof(MainViewModel.VentoPayPassword))
                }
            }
        };
    }

    private static Control CreateInfoSection(MainViewModel vm)
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
                        Text = "Information",
                        FontSize = 18,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = GetTextBrush()
                    },
                    new Button
                    {
                        Content = "Über die App",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        Command = vm.ShowAboutCommand
                    },
                    new Button
                    {
                        Content = "Changelog",
                        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                        Command = vm.ShowChangelogCommand
                    }
                }
            }
        };
    }

    private static Control CreateBoundTextBox(string watermark, string propertyName)
    {
        var textBox = new TextBox
        {
            Watermark = watermark,
            FontSize = 17,
            Padding = new Thickness(12),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            CornerRadius = new CornerRadius(8),
            Background = Brushes.Transparent
        };
        
        // Use proper Avalonia data binding with two-way mode
        textBox.Bind(TextBox.TextProperty, new Binding(propertyName)
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
        });
        
        return textBox;
    }

    private static Control CreateBoundPasswordBox(string watermark, string propertyName)
    {
        var textBox = new TextBox
        {
            Watermark = watermark,
            PasswordChar = '•',
            FontSize = 17,
            Padding = new Thickness(12),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            CornerRadius = new CornerRadius(8),
            Background = Brushes.Transparent
        };
        
        // Use proper Avalonia data binding with two-way mode
        textBox.Bind(TextBox.TextProperty, new Binding(propertyName)
        {
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus
        });
        
        return textBox;
    }
}
