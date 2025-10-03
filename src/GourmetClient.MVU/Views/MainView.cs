using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using System.IO;

namespace GourmetClient.MVU.Views;

public static class MainView {
  // Theme-aware color brushes that work in both light and dark modes
  private static SolidColorBrush GetActionBarBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#2D2D30")
      : Color.Parse("#F2F2F2"));

  private static SolidColorBrush GetCardBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#3C3C3C")
      : Colors.White);

  private static SolidColorBrush GetBorderBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#464647")
      : Color.Parse("#F2F2F2"));

  private static SolidColorBrush GetIconBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#404040")
      : Colors.LightGray);

    private static SolidColorBrush GetTransparentBrush() => new(Colors.Transparent);

    private static SolidColorBrush GetIconBorderBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#555555")
      : Colors.Gray);

  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  public static Control Create(AppState state, Action<Msg> dispatch) {
    var mainGrid = new Grid();

    // Define rows like in WPF version
    mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Action bar
    mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Error display
    mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Main content
    mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Status bar

    // Action bar (matching WPF design)
    var actionBar = CreateActionBar(state, dispatch);
    Grid.SetRow(actionBar, 0);
    mainGrid.Children.Add(actionBar);

    // Error display
    if (!string.IsNullOrEmpty(state.ErrorMessage)) {
      var errorPanel = CreateErrorPanel(state, dispatch);
      Grid.SetRow(errorPanel, 1);
      mainGrid.Children.Add(errorPanel);
    }

    // Main content area
    var mainContent = CreateMainContent(state, dispatch);
    Grid.SetRow(mainContent, 2);
    mainGrid.Children.Add(mainContent);

    // Status bar
    var statusBar = CreateStatusBar(state);
    Grid.SetRow(statusBar, 3);
    mainGrid.Children.Add(statusBar);

    // Overlay panels for About, Settings, and Billing views
    if (state.IsAboutVisible) {
      var aboutOverlay = CreateOverlay(AboutView.Create(state, dispatch), dispatch, new ToggleAbout());
      Grid.SetRowSpan(aboutOverlay, 4);
      mainGrid.Children.Add(aboutOverlay);
    }

    if (state.IsSettingsVisible) {
      var settingsOverlay = CreateOverlay(SettingsView.Create(state, dispatch), dispatch, new ToggleSettings());
      Grid.SetRowSpan(settingsOverlay, 4);
      mainGrid.Children.Add(settingsOverlay);
    }

    if (state.IsBillingVisible) {
      var billingOverlay = CreateOverlay(BillingView.Create(state, dispatch), dispatch, new ToggleBilling());
      Grid.SetRowSpan(billingOverlay, 4);
      mainGrid.Children.Add(billingOverlay);
    }

    return mainGrid;
  }

  private static Border CreateActionBar(AppState state, Action<Msg> dispatch) {
    var border = new Border {
      Background = GetTransparentBrush()
    };

    var dockPanel = new DockPanel {
      LastChildFill = false
    };

    // Left side buttons
    var leftPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Margin = new Thickness(2)
    };

    // Bill toggle button with icon
    var billButton = new ToggleButton {
      Content = CreateIconButton("Bill.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(2),
      Background = GetTransparentBrush(),
      BorderBrush = GetTransparentBrush(),
      BorderThickness = new Thickness(0)
    };
    ToolTip.SetTip(billButton, "Transaktionen anzeigen");
    billButton.Click += (_, _) => dispatch(new ToggleBilling());
    leftPanel.Children.Add(billButton);

    // Separator
    var separator = new Rectangle {
      Width = 1,
      Height = 30,
      Fill = GetBorderBrush(),
      Margin = new Thickness(5, 2)
    };
    leftPanel.Children.Add(separator);

    // Refresh button
    var refreshButton = new Button {
      Content = CreateIconButton("RefreshLocalData.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(2),
      Background = GetTransparentBrush(),
        BorderBrush = GetTransparentBrush(),
        BorderThickness = new Thickness(0)
    };
    ToolTip.SetTip(refreshButton, "Lokale Daten aktualisieren");
    refreshButton.Click += (_, _) => dispatch(new LoadMenus());
    leftPanel.Children.Add(refreshButton);

    // Execute order button
    var executeButton = new Button {
      Content = CreateIconButton("ExecuteOrder.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(5, 2),
        Background = GetTransparentBrush(),
        BorderBrush = GetTransparentBrush(),
        BorderThickness = new Thickness(0)
    };

    var (orderCount, cancelCount) = CountMarkedMenus(state);
    var executeTooltip = orderCount > 0 || cancelCount > 0
      ? $"Bestellung ausführen ({orderCount} bestellen, {cancelCount} stornieren)"
      : "Bestellung ausführen (keine Änderungen)";

    ToolTip.SetTip(executeButton, executeTooltip);
    executeButton.Click += (_, _) => dispatch(new ExecuteOrder());
    leftPanel.Children.Add(executeButton);

    DockPanel.SetDock(leftPanel, Dock.Left);
    dockPanel.Children.Add(leftPanel);

    // Right side buttons
    var rightPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Margin = new Thickness(2)
    };

    // About button
    var aboutButton = new ToggleButton {
      Content = CreateIconButton("Information.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(5, 2),
        Background = GetTransparentBrush(),
        BorderBrush = GetTransparentBrush(),
        BorderThickness = new Thickness(0)
    };
    ToolTip.SetTip(aboutButton, "Über");
    aboutButton.Click += (_, _) => dispatch(new ToggleAbout());
    rightPanel.Children.Add(aboutButton);

    // Settings button
    var settingsButton = new ToggleButton {
      Content = CreateIconButton("Settings.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(2),
        Background = GetTransparentBrush(),
        BorderBrush = GetTransparentBrush(),
        BorderThickness = new Thickness(0)
    };
    ToolTip.SetTip(settingsButton, "Einstellungen");
    settingsButton.Click += (_, _) => dispatch(new ToggleSettings());
    rightPanel.Children.Add(settingsButton);

    DockPanel.SetDock(rightPanel, Dock.Right);
    dockPanel.Children.Add(rightPanel);

    border.Child = dockPanel;
    return border;
  }

  private static (int orderCount, int cancelCount) CountMarkedMenus(AppState state) {
    if (state.MenuDays == null) return (0, 0);

    var orderCount = 0;
    var cancelCount = 0;

    foreach (var day in state.MenuDays) {
      foreach (var menu in day.Menus) {
        if (menu.MenuState == GourmetMenuState.MarkedForOrder) orderCount++;
        else if (menu.MenuState == GourmetMenuState.MarkedForCancel) cancelCount++;
      }
    }

    return (orderCount, cancelCount);
  }

  private static Control CreateIconButton(string iconName) {
    // Create a styled button that looks like the WPF icon buttons
    var border = new Border {
      Width = 32,
      Height = 32,
      Background = GetTransparentBrush(),
      CornerRadius = new CornerRadius(3),
      BorderBrush = GetTransparentBrush(),
      BorderThickness = new Thickness(0)
    };

    // Use text symbols as placeholders for icons - theme-aware colors
    var isDark = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
    var iconText = iconName switch {
      "Bill.png" => isDark ? "💳" : "💰",
      "RefreshLocalData.png" => isDark ? "⟳" : "🔄",
      "ExecuteOrder.png" => isDark ? "☑" : "✓",
      "Information.png" => isDark ? "🛈" : "ℹ️",
      "Settings.png" => isDark ? "⚙" : "⚙️",
      "Error.png" => isDark ? "⚠" : "❌",
      "MenuNotAvailable.png" => isDark ? "⊘" : "🚫",
      "MenuMarkedForOrder.png" => isDark ? "📋" : "📝",
      "MenuOrdered.png" => isDark ? "✓" : "✅",
      _ => isDark ? "📋" : "📄"
    };

    var textBlock = new TextBlock {
      Text = iconText,
      FontSize = 16,
      Foreground = GetTextBrush(),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center
    };

    border.Child = textBlock;
    return border;
  }

  private static Panel CreateErrorPanel(AppState state, Action<Msg> dispatch) {
    var isDark = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;

    var panel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Background = new SolidColorBrush(isDark ? Color.Parse("#4D1F1F") : Colors.LightCoral),
      Margin = new Thickness(5)
    };

    var errorIcon = CreateIconButton("Error.png");
    panel.Children.Add(errorIcon);

    var errorText = new TextBlock {
      Text = state.ErrorMessage,
      Foreground = new SolidColorBrush(isDark ? Color.Parse("#FF9999") : Colors.DarkRed),
      Margin = new Thickness(10, 5),
      VerticalAlignment = VerticalAlignment.Center
    };
    panel.Children.Add(errorText);

    var clearButton = new Button {
      Content = "Clear",
      Margin = new Thickness(10, 5)
    };
    clearButton.Click += (_, _) => dispatch(new ClearError());
    panel.Children.Add(clearButton);

    return panel;
  }

  private static Control CreateMainContent(AppState state, Action<Msg> dispatch) {
    // Use the dedicated MenuView for the main content
    return MenuView.Create(state, dispatch);
  }

  private static Control CreateStatusBar(AppState state) {
    var border = new Border {
      Background = GetActionBarBackgroundBrush(),
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(0, 1, 0, 0),
      Padding = new Thickness(10, 5)
    };

    var dockPanel = new DockPanel();

    // User information on the left
    if (!string.IsNullOrEmpty(state.UserName)) {
      var userText = new TextBlock {
        Text = $"Angemeldet als: {state.UserName}",
        Foreground = GetTextBrush(),
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 12
      };
      DockPanel.SetDock(userText, Dock.Left);
      dockPanel.Children.Add(userText);
    }

    // Last update time on the right
    if (state.LastMenuUpdate.HasValue) {
      var updateText = new TextBlock {
        Text = $"Letzte Aktualisierung: {state.LastMenuUpdate.Value:dd.MM.yyyy HH:mm}",
        Foreground = GetTextBrush(),
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 12
      };
      DockPanel.SetDock(updateText, Dock.Right);
      dockPanel.Children.Add(updateText);
    }

    border.Child = dockPanel;
    return border;
  }

  private static Control CreateOverlay(Control content, Action<Msg> dispatch, Msg closeMessage) {
    // Create a Grid for better layout control
    var overlayGrid = new Grid {
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch,
      Background = new SolidColorBrush(Colors.Black, 0.3) // Lighter background to be less intrusive
    };

    // Add close button in top-right corner
    var closeButton = new Button {
      Content = "✕",
      FontSize = 16,
      Width = 30,
      Height = 30,
      HorizontalAlignment = HorizontalAlignment.Right,
      VerticalAlignment = VerticalAlignment.Top,
      Margin = new Thickness(0, 10, 10, 0),
      Background = new SolidColorBrush(Colors.Red),
      Foreground = new SolidColorBrush(Colors.White),
      //CornerRadius = new CornerRadius(15)
    };
    closeButton.Click += (_, _) => dispatch(closeMessage);

    // Content container
    var contentContainer = new Border {
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      CornerRadius = new CornerRadius(8),
      Margin = new Thickness(20),
      BoxShadow = new BoxShadows(new BoxShadow {
        Color = Colors.Black,
        Blur = 10,
        OffsetX = 0,
        OffsetY = 3,
        Spread = 0
      }),
      Child = content
    };

    // Add both elements to the grid
    overlayGrid.Children.Add(contentContainer);
    overlayGrid.Children.Add(closeButton);

    // Only close on background click, not anywhere
    overlayGrid.PointerPressed += (_, e) => {
      // Only close if clicking directly on the overlay background, not on child controls
      if (e.Source == overlayGrid) {
        dispatch(closeMessage);
      }
    };

    return overlayGrid;
  }
}