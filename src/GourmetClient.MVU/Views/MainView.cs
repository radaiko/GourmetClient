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
      Background = GetActionBarBackgroundBrush()
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
      Margin = new Thickness(2)
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
      Margin = new Thickness(2)
    };
    ToolTip.SetTip(refreshButton, "Lokale Daten aktualisieren");
    refreshButton.Click += (_, _) => dispatch(new LoadMenus());
    leftPanel.Children.Add(refreshButton);

    // Execute order button
    var executeButton = new Button {
      Content = CreateIconButton("ExecuteOrder.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(5, 2)
    };
    ToolTip.SetTip(executeButton, "Bestellung ausführen");
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
      Margin = new Thickness(5, 2)
    };
    ToolTip.SetTip(aboutButton, "Über");
    aboutButton.Click += (_, _) => dispatch(new ToggleAbout());
    rightPanel.Children.Add(aboutButton);

    // Settings button
    var settingsButton = new ToggleButton {
      Content = CreateIconButton("Settings.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(2)
    };
    ToolTip.SetTip(settingsButton, "Einstellungen");
    settingsButton.Click += (_, _) => dispatch(new ToggleSettings());
    rightPanel.Children.Add(settingsButton);

    DockPanel.SetDock(rightPanel, Dock.Right);
    dockPanel.Children.Add(rightPanel);

    border.Child = dockPanel;
    return border;
  }

  private static Control CreateIconButton(string iconName) {
    // Create a styled button that looks like the WPF icon buttons
    var border = new Border {
      Width = 32,
      Height = 32,
      Background = GetIconBackgroundBrush(),
      CornerRadius = new CornerRadius(3),
      BorderBrush = GetIconBorderBrush(),
      BorderThickness = new Thickness(1)
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
    var scrollViewer = new ScrollViewer {
      HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
      VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto
    };

    if (state.IsLoading) {
      var loadingPanel = new StackPanel {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
      };

      var loadingText = new TextBlock {
        Text = "Loading...",
        FontSize = 16,
        Foreground = GetTextBrush(),
        HorizontalAlignment = HorizontalAlignment.Center
      };
      loadingPanel.Children.Add(loadingText);

      scrollViewer.Content = loadingPanel;
    }
    else {
      // Create menu content similar to WPF MenuOrderView
      scrollViewer.Content = CreateMenuContent(state, dispatch);
    }

    return scrollViewer;
  }

  private static Control CreateMenuContent(AppState state, Action<Msg> dispatch) {
    var mainPanel = new StackPanel {
      Margin = new Thickness(10)
    };

    // Welcome header
    var headerText = new TextBlock {
      Text = "Gourmet Client",
      FontSize = 24,
      FontWeight = FontWeight.Bold,
      Foreground = GetTextBrush(),
      HorizontalAlignment = HorizontalAlignment.Center,
      Margin = new Thickness(0, 10, 0, 20)
    };
    mainPanel.Children.Add(headerText);

    // Menu categories placeholder (to be implemented with actual menu data)
    var categoriesPanel = CreateMenuCategoriesPanel(state, dispatch);
    mainPanel.Children.Add(categoriesPanel);

    return mainPanel;
  }

  private static Control CreateMenuCategoriesPanel(AppState state, Action<Msg> dispatch) {
    var panel = new StackPanel();

    // Menu categories (matching WPF structure)
    var categories = new[] { "Menü 1", "Menü 2", "Menü 3", "Suppe & Salat" };

    foreach (var category in categories) {
      var categoryHeader = CreateCategoryHeader(category);
      panel.Children.Add(categoryHeader);

      // Placeholder for menu items
      var menuItemsPanel = CreateMenuItemsPanel(category, state, dispatch);
      panel.Children.Add(menuItemsPanel);
    }

    return panel;
  }

  private static Control CreateCategoryHeader(string categoryName) {
    var border = new Border {
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(2, 0, 0, 0),
      Margin = new Thickness(0, 10, 0, 5)
    };

    var textBlock = new TextBlock {
      Text = categoryName,
      FontSize = 20,
      Foreground = GetTextBrush(),
      Margin = new Thickness(12, 0, 0, 0),
      VerticalAlignment = VerticalAlignment.Center
    };

    border.Child = textBlock;
    return border;
  }

  private static Control CreateMenuItemsPanel(string category, AppState state, Action<Msg> dispatch) {
    var panel = new StackPanel();

    // Placeholder menu item (to be replaced with actual data)
    var menuItem = CreateMenuItemCard($"Sample menu from {category}", "Sample description for the menu item", state, dispatch);
    panel.Children.Add(menuItem);

    return panel;
  }

  private static Control CreateMenuItemCard(string title, string description, AppState state, Action<Msg> dispatch) {
    var dockPanel = new DockPanel {
      LastChildFill = true,
      Margin = new Thickness(0, 2)
    };

    // Left border (matching WPF design)
    var leftBorder = new Border {
      Width = 2,
      Background = GetBorderBrush(),
      Margin = new Thickness(0, -2)
    };
    DockPanel.SetDock(leftBorder, Dock.Left);
    dockPanel.Children.Add(leftBorder);

    // Menu card
    var menuBorder = new Border {
      Height = 120,
      Background = GetCardBackgroundBrush(),
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(0, 2)
    };

    var menuButton = new Button {
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch,
      HorizontalContentAlignment = HorizontalAlignment.Stretch,
      VerticalContentAlignment = VerticalAlignment.Top,
      Margin = new Thickness(10, 0),
      Background = Brushes.Transparent,
      BorderThickness = new Thickness(0)
    };

    var contentGrid = new Grid {
      Margin = new Thickness(0, 10, 0, 0)
    };
    contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
    contentGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
    contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
    contentGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

    // Menu description
    var descriptionText = new TextBlock {
      Text = description,
      FontSize = 14,
      Foreground = GetTextBrush(),
      TextWrapping = TextWrapping.Wrap,
      TextTrimming = TextTrimming.CharacterEllipsis
    };
    Grid.SetRow(descriptionText, 0);
    Grid.SetColumn(descriptionText, 0);
    contentGrid.Children.Add(descriptionText);

    // Status icons panel
    var iconsPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Margin = new Thickness(5, 0)
    };
    Grid.SetRow(iconsPanel, 0);
    Grid.SetRowSpan(iconsPanel, 2);
    Grid.SetColumn(iconsPanel, 1);

    // Add status icons (placeholder)
    var statusIcon = CreateIconButton("MenuNotAvailable.png");
    iconsPanel.Children.Add(statusIcon);

    contentGrid.Children.Add(iconsPanel);

    menuButton.Content = contentGrid;
    menuButton.Click += (_, _) => dispatch(new ToggleMenuOrder(title));

    menuBorder.Child = menuButton;
    dockPanel.Children.Add(menuBorder);

    return dockPanel;
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
      CornerRadius = new CornerRadius(15)
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