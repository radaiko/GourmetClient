using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

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

    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 32,
            Margin = new Thickness(0),
            Background = GetBackgroundBrush()
        };

        // Gourmet credentials section
        var gourmetSection = CreateFormSection(
            "Gourmet Anmeldedaten",
            CreateGourmetFields(state, dispatch)
        );
        mainPanel.Children.Add(gourmetSection);

        // VentoPay credentials section
        var ventoSection = CreateFormSection(
            "VentoPay Anmeldedaten",
            CreateVentoPayFields(state, dispatch)
        );
        mainPanel.Children.Add(ventoSection);

        // Removed inline save button; save now handled by top bar when dirty

        return new ScrollViewer { Content = mainPanel };
    }

    private static Control CreateFormSection(string title, Control content)
    {
        var section = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12
        };

        // Section header
        var headerText = new TextBlock
        {
            Text = title.ToUpper(),
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = GetSecondaryTextBrush(),
            Margin = new Thickness(16, 0, 16, 0)
        };
        section.Children.Add(headerText);

        // Content card
        var card = new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(16, 0),
            Child = content
        };
        section.Children.Add(card);

        return section;
    }

    private static Control CreateGourmetFields(AppState state, Action<Msg> dispatch)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0
        };

        var usernameBox = new TextBox
        {
            Text = state.Settings?.Username ?? "",
            Watermark = "Benutzername",
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        usernameBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                dispatch(new UpdateUsername(usernameBox.Text ?? ""));
        };
        var usernameContainer = new Border
        {
            Child = usernameBox,
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };
        stack.Children.Add(usernameContainer);

        var passwordBox = new TextBox
        {
            Text = state.Settings?.Password ?? "",
            Watermark = "Passwort",
            PasswordChar = '•',
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        passwordBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                dispatch(new UpdatePassword(passwordBox.Text ?? ""));
        };
        stack.Children.Add(passwordBox);

        return stack;
    }

    private static Control CreateVentoPayFields(AppState state, Action<Msg> dispatch)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0
        };

        var usernameBox = new TextBox
        {
            Text = state.Settings?.VentoPayUsername ?? "",
            Watermark = "VentoPay Benutzername",
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        usernameBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                dispatch(new UpdateVentoPayUsername(usernameBox.Text ?? ""));
        };
        var usernameContainer = new Border
        {
            Child = usernameBox,
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };
        stack.Children.Add(usernameContainer);

        var passwordBox = new TextBox
        {
            Text = state.Settings?.VentoPayPassword ?? "",
            Watermark = "VentoPay Passwort",
            PasswordChar = '•',
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        passwordBox.PropertyChanged += (_, e) =>
        {
            if (e.Property == TextBox.TextProperty)
                dispatch(new UpdateVentoPayPassword(passwordBox.Text ?? ""));
        };
        stack.Children.Add(passwordBox);

        return stack;
    }
}
