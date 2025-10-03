using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

/// <summary>
/// Desktop-optimized settings view with form grid layout
/// </summary>
public static class SettingsViewDesktop
{
    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Background = SettingsViewShared.GetMinimalistBackgroundBrush()
        };

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 40,
            MinWidth = 700,
            Margin = new Thickness(10, 0, 10, 10)
        };

        // Header
        var header = SettingsViewShared.CreateMinimalistHeader();
        mainPanel.Children.Add(header);

        // Declare all form controls at this scope so they're accessible to the save button
        TextBox usernameTextBox = null!;
        TextBox passwordBox = null!;
        TextBox ventoUsernameTextBox = null!;
        TextBox ventoPasswordBox = null!;
        CheckBox autoUpdateCheckBox = null!;
        CheckBox startWithWindowsCheckBox = null!;
        ComboBox themeComboBox = null!;

        // Settings sections
        var gourmetSection = CreateMinimalistGourmetSettings(state, out usernameTextBox, out passwordBox);
        mainPanel.Children.Add(gourmetSection);

        var ventoPaySection = CreateMinimalistVentoPaySettings(state, out ventoUsernameTextBox, out ventoPasswordBox);
        mainPanel.Children.Add(ventoPaySection);

        var appSection = CreateMinimalistApplicationSettings(state, out autoUpdateCheckBox, out startWithWindowsCheckBox, out themeComboBox);
        mainPanel.Children.Add(appSection);

        // Action buttons
        var actionsSection = CreateMinimalistActions(dispatch, usernameTextBox, passwordBox, ventoUsernameTextBox, ventoPasswordBox, autoUpdateCheckBox, startWithWindowsCheckBox, themeComboBox);
        mainPanel.Children.Add(actionsSection);

        scrollViewer.Content = mainPanel;
        return scrollViewer;
    }

    private static Control CreateMinimalistGourmetSettings(AppState state, out TextBox usernameTextBox, out TextBox passwordBox)
    {
        var section = SettingsViewShared.CreateMinimalistSection("Gourmet");

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(180)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // Username
        var usernameLabel = new TextBlock
        {
            Text = "Benutzername",
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = SettingsViewShared.GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 20, 8)
        };
        Grid.SetRow(usernameLabel, 0);
        Grid.SetColumn(usernameLabel, 0);
        grid.Children.Add(usernameLabel);

        usernameTextBox = new TextBox
        {
            Text = state.Settings?.Username ?? "",
            FontSize = 14,
            Padding = new Thickness(12, 10),
            Background = Brushes.Transparent,
            BorderBrush = SettingsViewShared.GetMinimalistBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 0, 8)
        };
        Grid.SetRow(usernameTextBox, 0);
        Grid.SetColumn(usernameTextBox, 1);
        grid.Children.Add(usernameTextBox);

        // Password
        var passwordLabel = new TextBlock
        {
            Text = "Passwort",
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = SettingsViewShared.GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 20, 0)
        };
        Grid.SetRow(passwordLabel, 1);
        Grid.SetColumn(passwordLabel, 0);
        grid.Children.Add(passwordLabel);

        passwordBox = new TextBox
        {
            Text = state.Settings?.Password ?? "",
            PasswordChar = '•',
            FontSize = 14,
            Padding = new Thickness(12, 10),
            Background = Brushes.Transparent,
            BorderBrush = SettingsViewShared.GetMinimalistBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4)
        };
        Grid.SetRow(passwordBox, 1);
        Grid.SetColumn(passwordBox, 1);
        grid.Children.Add(passwordBox);

        section.Children.Add(grid);
        return section;
    }

    private static Control CreateMinimalistVentoPaySettings(AppState state, out TextBox ventoUsernameTextBox, out TextBox ventoPasswordBox)
    {
        var section = SettingsViewShared.CreateMinimalistSection("VentoPay");

        var grid = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Auto)
            },
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(180)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        // VentoPay Username
        var ventoUsernameLabel = new TextBlock
        {
            Text = "Benutzername",
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = SettingsViewShared.GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 20, 8)
        };
        Grid.SetRow(ventoUsernameLabel, 0);
        Grid.SetColumn(ventoUsernameLabel, 0);
        grid.Children.Add(ventoUsernameLabel);

        ventoUsernameTextBox = new TextBox
        {
            Text = state.Settings?.VentoPayUsername ?? "",
            FontSize = 14,
            Padding = new Thickness(12, 10),
            Background = Brushes.Transparent,
            BorderBrush = SettingsViewShared.GetMinimalistBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 0, 8)
        };
        Grid.SetRow(ventoUsernameTextBox, 0);
        Grid.SetColumn(ventoUsernameTextBox, 1);
        grid.Children.Add(ventoUsernameTextBox);

        // VentoPay Password
        var ventoPasswordLabel = new TextBlock
        {
            Text = "Passwort",
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = SettingsViewShared.GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 20, 0)
        };
        Grid.SetRow(ventoPasswordLabel, 1);
        Grid.SetColumn(ventoPasswordLabel, 0);
        grid.Children.Add(ventoPasswordLabel);

        ventoPasswordBox = new TextBox
        {
            Text = state.Settings?.VentoPayPassword ?? "",
            PasswordChar = '•',
            FontSize = 14,
            Padding = new Thickness(12, 10),
            Background = Brushes.Transparent,
            BorderBrush = SettingsViewShared.GetMinimalistBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4)
        };
        Grid.SetRow(ventoPasswordBox, 1);
        Grid.SetColumn(ventoPasswordBox, 1);
        grid.Children.Add(ventoPasswordBox);

        section.Children.Add(grid);
        return section;
    }

    private static Control CreateMinimalistApplicationSettings(AppState state, out CheckBox autoUpdateCheckBox, out CheckBox startWithWindowsCheckBox, out ComboBox themeComboBox)
    {
        var section = SettingsViewShared.CreateMinimalistSection("Anwendung");

        var panel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 20
        };

        // Checkboxes
        autoUpdateCheckBox = new CheckBox
        {
            Content = "Automatische Updates",
            IsChecked = state.Settings?.AutoUpdate ?? true,
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = SettingsViewShared.GetTextBrush()
        };
        panel.Children.Add(autoUpdateCheckBox);

        startWithWindowsCheckBox = new CheckBox
        {
            Content = "Mit Windows starten",
            IsChecked = state.Settings?.StartWithWindows ?? false,
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = SettingsViewShared.GetTextBrush()
        };
        panel.Children.Add(startWithWindowsCheckBox);

        // Theme selection
        var themePanel = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(180)),
                new ColumnDefinition(GridLength.Star)
            }
        };

        var themeLabel = new TextBlock
        {
            Text = "Design",
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = SettingsViewShared.GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 20, 0)
        };
        Grid.SetColumn(themeLabel, 0);
        themePanel.Children.Add(themeLabel);

        themeComboBox = new ComboBox
        {
            Items = { "System", "Hell", "Dunkel" },
            SelectedIndex = SettingsViewShared.GetThemeIndex(state.Settings?.Theme ?? "System"),
            FontSize = 14,
            Padding = new Thickness(12, 10),
            Background = Brushes.Transparent,
            BorderBrush = SettingsViewShared.GetMinimalistBorderBrush(),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            MinWidth = 150
        };
        Grid.SetColumn(themeComboBox, 1);
        themePanel.Children.Add(themeComboBox);

        panel.Children.Add(themePanel);
        section.Children.Add(panel);
        return section;
    }

    private static Control CreateMinimalistActions(Action<Msg> dispatch, TextBox usernameTextBox, TextBox passwordBox, TextBox ventoUsernameTextBox, TextBox ventoPasswordBox, CheckBox autoUpdateCheckBox, CheckBox startWithWindowsCheckBox, ComboBox themeComboBox)
    {
        var actionsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 20, 0, 0)
        };

        var saveButton = new Button
        {
            Content = "Speichern",
            FontSize = 14,
            FontWeight = FontWeight.Medium,
            Padding = new Thickness(20, 12),
            Background = SettingsViewShared.GetAccentBrush(),
            Foreground = new SolidColorBrush(Colors.White),
            BorderThickness = new Thickness(0),
            CornerRadius = new CornerRadius(4)
        };
        saveButton.Click += (_, _) =>
        {
            dispatch(new SaveFormSettings(
                usernameTextBox.Text ?? "",
                passwordBox.Text ?? "",
                ventoUsernameTextBox.Text ?? "",
                ventoPasswordBox.Text ?? "",
                autoUpdateCheckBox.IsChecked == true,
                startWithWindowsCheckBox.IsChecked == true,
                themeComboBox.SelectedItem?.ToString() ?? "System"
            ));
        };
        actionsPanel.Children.Add(saveButton);

        var cancelButton = new Button
        {
            Content = "Abbrechen",
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Padding = new Thickness(20, 12),
            Background = Brushes.Transparent,
            BorderBrush = SettingsViewShared.GetMinimalistBorderBrush(),
            BorderThickness = new Thickness(1),
            Foreground = SettingsViewShared.GetTextBrush(),
            CornerRadius = new CornerRadius(4)
        };
        cancelButton.Click += (_, _) => dispatch(new ToggleSettings());
        actionsPanel.Children.Add(cancelButton);

        return actionsPanel;
    }
}

