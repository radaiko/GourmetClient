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

    private static SolidColorBrush GetSecondaryTextBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#8E8E93")
        : Color.Parse("#8E8E93"));

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

        // Declare form controls
        TextBox usernameTextBox = null!;
        TextBox passwordBox = null!;
        TextBox ventoUsernameTextBox = null!;
        TextBox ventoPasswordBox = null!;
        CheckBox autoUpdateCheckBox = null!;
        CheckBox startWithWindowsCheckBox = null!;
        ComboBox themeComboBox = null!;

        // Gourmet credentials section
        var gourmetSection = CreateFormSection(
            "Gourmet Anmeldedaten",
            CreateGourmetFields(state, out usernameTextBox, out passwordBox)
        );
        mainPanel.Children.Add(gourmetSection);

        // VentoPay credentials section
        var ventoSection = CreateFormSection(
            "VentoPay Anmeldedaten",
            CreateVentoPayFields(state, out ventoUsernameTextBox, out ventoPasswordBox)
        );
        mainPanel.Children.Add(ventoSection);

        // App settings section
        var appSection = CreateFormSection(
            "App Einstellungen",
            CreateAppSettingsFields(state, out autoUpdateCheckBox, out startWithWindowsCheckBox, out themeComboBox)
        );
        mainPanel.Children.Add(appSection);

        // Save button
        var saveButton = new Button
        {
            Content = "Einstellungen speichern",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(16, 14),
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            Background = new SolidColorBrush(Color.Parse("#007AFF")),
            Foreground = Brushes.White,
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(16, 8, 16, 32)
        };
        saveButton.Click += (_, _) =>
        {
            dispatch(new SaveFormSettings(
                usernameTextBox.Text ?? "",
                passwordBox.Text ?? "",
                ventoUsernameTextBox.Text ?? "",
                ventoPasswordBox.Text ?? "",
                autoUpdateCheckBox.IsChecked ?? true,
                startWithWindowsCheckBox.IsChecked ?? false,
                themeComboBox.SelectedIndex switch { 1 => "Hell", 2 => "Dunkel", _ => "System" }
            ));
        };
        mainPanel.Children.Add(saveButton);

        return mainPanel;
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

    private static Control CreateGourmetFields(AppState state, out TextBox usernameBox, out TextBox passwordBox)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0
        };

        usernameBox = new TextBox
        {
            Text = state.Settings?.Username ?? "",
            Watermark = "Benutzername",
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        var usernameContainer = new Border
        {
            Child = usernameBox,
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };
        stack.Children.Add(usernameContainer);

        passwordBox = new TextBox
        {
            Text = state.Settings?.Password ?? "",
            Watermark = "Passwort",
            PasswordChar = '•',
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        stack.Children.Add(passwordBox);

        return stack;
    }

    private static Control CreateVentoPayFields(AppState state, out TextBox usernameBox, out TextBox passwordBox)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0
        };

        usernameBox = new TextBox
        {
            Text = state.Settings?.VentoPayUsername ?? "",
            Watermark = "VentoPay Benutzername",
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        var usernameContainer = new Border
        {
            Child = usernameBox,
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };
        stack.Children.Add(usernameContainer);

        passwordBox = new TextBox
        {
            Text = state.Settings?.VentoPayPassword ?? "",
            Watermark = "VentoPay Passwort",
            PasswordChar = '•',
            FontSize = 17,
            Padding = new Thickness(16, 12),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent
        };
        stack.Children.Add(passwordBox);

        return stack;
    }

    private static Control CreateAppSettingsFields(AppState state, out CheckBox autoUpdateBox, out CheckBox startWithWindowsBox, out ComboBox themeBox)
    {
        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0
        };

        // Auto update toggle
        autoUpdateBox = new CheckBox
        {
            Content = "Automatische Updates",
            IsChecked = state.Settings?.AutoUpdate ?? true,
            FontSize = 17,
            Padding = new Thickness(16, 12)
        };
        var autoUpdateContainer = new Border
        {
            Child = autoUpdateBox,
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };
        stack.Children.Add(autoUpdateContainer);

        // Start with Windows (hidden on iOS but kept for compatibility)
        startWithWindowsBox = new CheckBox
        {
            Content = "Mit System starten",
            IsChecked = state.Settings?.StartWithWindows ?? false,
            FontSize = 17,
            Padding = new Thickness(16, 12),
            IsVisible = false // Not applicable on iOS
        };

        // Theme selector
        var themePanel = new DockPanel
        {
            Margin = new Thickness(16, 12)
        };
        var themeLabel = new TextBlock
        {
            Text = "Erscheinungsbild",
            FontSize = 17,
            Foreground = SettingsViewShared.GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center
        };
        DockPanel.SetDock(themeLabel, Dock.Left);
        themePanel.Children.Add(themeLabel);

        themeBox = new ComboBox
        {
            ItemsSource = new[] { "System", "Hell", "Dunkel" },
            SelectedIndex = SettingsViewShared.GetThemeIndex(state.Settings?.Theme ?? "System"),
            MinWidth = 120,
            FontSize = 17
        };
        DockPanel.SetDock(themeBox, Dock.Right);
        themePanel.Children.Add(themeBox);

        stack.Children.Add(themePanel);

        return stack;
    }
}
