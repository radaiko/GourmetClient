using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// Desktop settings view: two-column responsive form reusing binding properties from MainViewModel.
/// </summary>
public static class SettingsViewDesktop
{
    private static IBrush Bg() => new SolidColorBrush(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Color.Parse("#1E1E1E") : Color.Parse("#F5F5F5"));
    private static IBrush Card() => new SolidColorBrush(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Color.Parse("#2B2B2B") : Colors.White);
    private static IBrush Txt() => new SolidColorBrush(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Colors.White : Colors.Black);
    private static IBrush Sub() => new SolidColorBrush(Color.Parse("#6E6E73"));

    public static Control Create(MainViewModel vm)
    {
        var scroll = new ScrollViewer { Background = Bg() };
        var outer = new StackPanel { Spacing = 22, Margin = new Thickness(24), MaxWidth = DesktopLayout.ContentMaxWidth };
        scroll.Content = new Grid
        {
            ColumnDefinitions = { new ColumnDefinition(GridLength.Star) },
            Children = { new StackPanel { Children = { outer }, HorizontalAlignment = HorizontalAlignment.Center } }
        };

        outer.Children.Add(new TextBlock
        {
            Text = "Anmeldedaten & Einstellungen",
            FontSize = 24,
            FontWeight = FontWeight.SemiBold,
            Foreground = Txt()
        });
        outer.Children.Add(new TextBlock
        {
            Text = "Benutzername und Passwort für Gourmet und VentoPay Dienste verwalten.",
            FontSize = 13,
            Foreground = Sub(),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 560
        });

        outer.Children.Add(Section("Gourmet", vm, nameof(MainViewModel.GourmetUsername), nameof(MainViewModel.GourmetPassword)));
        outer.Children.Add(Section("VentoPay", vm, nameof(MainViewModel.VentoPayUsername), nameof(MainViewModel.VentoPayPassword)));

        return scroll;
    }

    private static Control Section(string title, MainViewModel vm, string userProp, string passProp)
    {
        var border = new Border
        {
            Background = Card(),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(18)
        };
        var stack = new StackPanel { Spacing = 14 };
        border.Child = stack;
        stack.Children.Add(new TextBlock
        {
            Text = title + " Zugangsdaten",
            FontSize = 18,
            FontWeight = FontWeight.SemiBold,
            Foreground = Txt()
        });

        // Simplified grid (ColumnSpacing/RowSpacing not available in Avalonia 11.1.3)
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            }
        };

        var userBox = LabeledBox("Benutzername", userProp, false);
        var passBox = LabeledBox("Passwort", passProp, true);
        // Add a right margin to the first column content to simulate column spacing
        if (userBox is Control uc) uc.Margin = new Thickness(0, 0, 16, 0);
        if (passBox is Control pc) pc.Margin = new Thickness(0);
        grid.Children.Add(userBox);
        Grid.SetColumn(passBox, 1);
        grid.Children.Add(passBox);

        stack.Children.Add(grid);
        return border;
    }

    private static Control LabeledBox(string label, string prop, bool isPassword)
    {
        var stack = new StackPanel { Spacing = 4 };
        stack.Children.Add(new TextBlock { Text = label, FontSize = 12, Foreground = Sub() });
        var tb = new TextBox
        {
            PasswordChar = isPassword ? '•' : (char)0,
            FontSize = 14,
            Padding = new Thickness(10),
            BorderBrush = new SolidColorBrush(Color.Parse("#B0B0B0")),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4)
        };
        tb.Bind(TextBox.TextProperty, new Binding(prop) { Mode = BindingMode.TwoWay });
        stack.Children.Add(tb);
        return stack;
    }
}
