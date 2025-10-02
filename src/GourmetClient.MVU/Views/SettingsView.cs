using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

public static class SettingsView {
  private static int GetThemeIndex(string theme) => theme switch {
    "Hell" => 1,
    "Dunkel" => 2,
    _ => 0 // "System" or default
  };

  private static SolidColorBrush GetTextBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Colors.White 
      : Colors.Black);

  private static SolidColorBrush GetCardBackgroundBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#3C3C3C") 
      : Colors.White);

  private static SolidColorBrush GetGroupBoxBackgroundBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#2D2D30") 
      : Color.Parse("#F8F8F8"));

  private static SolidColorBrush GetBorderBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#464647") 
      : Colors.LightGray);

  public static Control Create(AppState state, Action<Msg> dispatch) {
    var border = new Border {
      Background = GetCardBackgroundBrush(),
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(8),
      Padding = new Thickness(15),
      MinWidth = 350,
      MaxWidth = 500,
      MaxHeight = 600
    };

    var scrollViewer = new ScrollViewer {
      HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
      VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
    };

    var mainPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 15
    };

    // Declare all form controls at this scope so they're accessible to the save button
    TextBox usernameTextBox = null!;
    TextBox passwordBox = null!;
    TextBox ventoUsernameTextBox = null!;
    TextBox ventoPasswordBox = null!;
    CheckBox autoUpdateCheckBox = null!;
    CheckBox startWithWindowsCheckBox = null!;
    ComboBox themeComboBox = null!;

    // Gourmet Settings Group
    var gourmetGroup = CreateSettingsGroup("Gourmet", CreateGourmetSettings(state, dispatch, out usernameTextBox, out passwordBox));
    mainPanel.Children.Add(gourmetGroup);

    // VentoPay Settings Group
    var ventoPayGroup = CreateSettingsGroup("VentoPay", CreateVentoPaySettings(state, dispatch, out ventoUsernameTextBox, out ventoPasswordBox));
    mainPanel.Children.Add(ventoPayGroup);

    // Application Settings Group
    var appGroup = CreateSettingsGroup("Anwendung", CreateApplicationSettings(state, dispatch, out autoUpdateCheckBox, out startWithWindowsCheckBox, out themeComboBox));
    mainPanel.Children.Add(appGroup);

    // Save button section
    var saveButtonPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Right,
      Spacing = 10,
      Margin = new Thickness(0, 20, 0, 10)
    };

    var saveButton = new Button {
      Content = "Einstellungen speichern",
      FontWeight = FontWeight.Medium,
      Padding = new Thickness(20, 10),
      Background = new SolidColorBrush(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
        ? Color.Parse("#0078D4") 
        : Color.Parse("#0066CC")),
      Foreground = new SolidColorBrush(Colors.White),
      CornerRadius = new CornerRadius(5)
    };
    saveButton.Click += (_, _) => {
      // Collect current form values when saving
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
    saveButtonPanel.Children.Add(saveButton);

    var cancelButton = new Button {
      Content = "Abbrechen",
      Padding = new Thickness(20, 10),
      Background = Brushes.Transparent,
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(1),
      Foreground = GetTextBrush(),
      CornerRadius = new CornerRadius(5)
    };
    cancelButton.Click += (_, _) => dispatch(new ToggleSettings());
    saveButtonPanel.Children.Add(cancelButton);

    mainPanel.Children.Add(saveButtonPanel);

    scrollViewer.Content = mainPanel;
    border.Child = scrollViewer;
    return border;
  }

  private static Control CreateSettingsGroup(string title, Control content) {
    var border = new Border {
      Background = GetGroupBoxBackgroundBrush(),
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(3),
      Padding = new Thickness(10),
      Margin = new Thickness(0, 5)
    };

    var panel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 5
    };

    // Group header
    var header = new TextBlock {
      Text = title,
      FontWeight = FontWeight.SemiBold,
      FontSize = 14,
      Foreground = GetTextBrush(),
      Margin = new Thickness(0, 0, 0, 10)
    };
    panel.Children.Add(header);

    panel.Children.Add(content);
    border.Child = panel;
    return border;
  }

  private static Control CreateGourmetSettings(AppState state, Action<Msg> dispatch, out TextBox usernameTextBox, out TextBox passwordBox) {
    var grid = new Grid {
      RowDefinitions = {
        new RowDefinition(GridLength.Auto),
        new RowDefinition(GridLength.Auto),
        new RowDefinition(GridLength.Auto)
      },
      ColumnDefinitions = {
        new ColumnDefinition(GridLength.Auto),
        new ColumnDefinition(GridLength.Star)
      }
    };

    // Username setting
    var usernameLabel = new TextBlock {
      Text = "Benutzername:",
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(0, 0, 10, 5)
    };
    Grid.SetRow(usernameLabel, 0);
    Grid.SetColumn(usernameLabel, 0);
    grid.Children.Add(usernameLabel);

    usernameTextBox = new TextBox {
      Text = state.Settings?.Username ?? "",
      Margin = new Thickness(0, 0, 0, 5),
      MinWidth = 200
    };
    // Remove TextChanged event to prevent feedback loop - settings will be saved on Save button click
    Grid.SetRow(usernameTextBox, 0);
    Grid.SetColumn(usernameTextBox, 1);
    grid.Children.Add(usernameTextBox);

    // Password setting
    var passwordLabel = new TextBlock {
      Text = "Passwort:",
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(0, 0, 10, 5)
    };
    Grid.SetRow(passwordLabel, 1);
    Grid.SetColumn(passwordLabel, 0);
    grid.Children.Add(passwordLabel);

    passwordBox = new TextBox {
      Text = state.Settings?.Password ?? "",
      PasswordChar = '*',
      Margin = new Thickness(0, 0, 0, 5),
      MinWidth = 200
    };
    // Remove TextChanged event to prevent feedback loop - settings will be saved on Save button click
    Grid.SetRow(passwordBox, 1);
    Grid.SetColumn(passwordBox, 1);
    grid.Children.Add(passwordBox);



    return grid;
  }

  private static Control CreateApplicationSettings(AppState state, Action<Msg> dispatch, out CheckBox autoUpdateCheckBox, out CheckBox startWithWindowsCheckBox, out ComboBox themeComboBox) {
    var panel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 10
    };

    // Auto-update checkbox
    autoUpdateCheckBox = new CheckBox {
      Content = "Automatische Updates",
      IsChecked = state.Settings?.AutoUpdate ?? true,
      Foreground = GetTextBrush()
    };
    // Remove immediate event handlers to prevent feedback loops - values will be collected on save
    panel.Children.Add(autoUpdateCheckBox);

    // Start with Windows checkbox
    startWithWindowsCheckBox = new CheckBox {
      Content = "Mit Windows starten",
      IsChecked = state.Settings?.StartWithWindows ?? false,
      Foreground = GetTextBrush()
    };
    // Remove immediate event handlers to prevent feedback loops - values will be collected on save
    panel.Children.Add(startWithWindowsCheckBox);

    // Theme selection
    var themePanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Spacing = 10
    };

    var themeLabel = new TextBlock {
      Text = "Design:",
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    themePanel.Children.Add(themeLabel);

    themeComboBox = new ComboBox {
      Items = { "System", "Hell", "Dunkel" },
      SelectedIndex = GetThemeIndex(state.Settings?.Theme ?? "System"),
      MinWidth = 120
    };
    // Remove immediate event handlers to prevent feedback loops - values will be collected on save
    themePanel.Children.Add(themeComboBox);

    panel.Children.Add(themePanel);

    return panel;
  }

  private static Control CreateVentoPaySettings(AppState state, Action<Msg> dispatch, out TextBox ventoUsernameTextBox, out TextBox ventoPasswordBox) {
    var grid = new Grid {
      RowDefinitions = {
        new RowDefinition(GridLength.Auto),
        new RowDefinition(GridLength.Auto),
        new RowDefinition(GridLength.Auto)
      },
      ColumnDefinitions = {
        new ColumnDefinition(GridLength.Auto),
        new ColumnDefinition(GridLength.Star)
      }
    };

    // VentoPay Username setting
    var ventoUsernameLabel = new TextBlock {
      Text = "VentoPay Benutzername:",
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(0, 0, 10, 5)
    };
    Grid.SetRow(ventoUsernameLabel, 0);
    Grid.SetColumn(ventoUsernameLabel, 0);
    grid.Children.Add(ventoUsernameLabel);

    ventoUsernameTextBox = new TextBox {
      Text = state.Settings?.VentoPayUsername ?? "",
      Margin = new Thickness(0, 0, 0, 5),
      MinWidth = 200
    };
    // Remove TextChanged event to prevent feedback loop - settings will be saved on Save button click
    Grid.SetRow(ventoUsernameTextBox, 0);
    Grid.SetColumn(ventoUsernameTextBox, 1);
    grid.Children.Add(ventoUsernameTextBox);

    // VentoPay Password setting
    var ventoPasswordLabel = new TextBlock {
      Text = "VentoPay Passwort:",
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(0, 0, 10, 5)
    };
    Grid.SetRow(ventoPasswordLabel, 1);
    Grid.SetColumn(ventoPasswordLabel, 0);
    grid.Children.Add(ventoPasswordLabel);

    ventoPasswordBox = new TextBox {
      Text = state.Settings?.VentoPayPassword ?? "",
      PasswordChar = '*',
      Margin = new Thickness(0, 0, 0, 5),
      MinWidth = 200
    };
    // Remove TextChanged event to prevent feedback loop - settings will be saved on Save button click
    Grid.SetRow(ventoPasswordBox, 1);
    Grid.SetColumn(ventoPasswordBox, 1);
    grid.Children.Add(ventoPasswordBox);



    return grid;
  }
}
