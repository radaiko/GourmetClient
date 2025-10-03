using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

/// <summary>
/// Desktop-optimized main view with top action bar and toolbar layout
/// </summary>
public static class MainViewDesktop {
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
      var errorPanel = MainViewShared.CreateErrorPanel(state, dispatch);
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
      Background = MainViewShared.GetTransparentBrush()
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
      Content = MainViewShared.CreateIconButton("Bill.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(2),
      Background = MainViewShared.GetTransparentBrush(),
      BorderBrush = MainViewShared.GetTransparentBrush(),
      BorderThickness = new Thickness(0)
    };
    ToolTip.SetTip(billButton, "Transaktionen anzeigen");
    billButton.Click += (_, _) => dispatch(new ToggleBilling());
    leftPanel.Children.Add(billButton);

    // Separator
    var separator = new Rectangle {
      Width = 1,
      Height = 30,
      Fill = MainViewShared.GetBorderBrush(),
      Margin = new Thickness(5, 2)
    };
    leftPanel.Children.Add(separator);

    // Refresh button
    var refreshButton = new Button {
      Content = MainViewShared.CreateIconButton("RefreshLocalData.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(2),
      Background = MainViewShared.GetTransparentBrush(),
      BorderBrush = MainViewShared.GetTransparentBrush(),
      BorderThickness = new Thickness(0)
    };
    ToolTip.SetTip(refreshButton, "Lokale Daten aktualisieren");
    refreshButton.Click += (_, _) => dispatch(new LoadMenus());
    leftPanel.Children.Add(refreshButton);

    // Execute order button
    var executeButton = new Button {
      Content = MainViewShared.CreateIconButton("ExecuteOrder.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(5, 2),
      Background = MainViewShared.GetTransparentBrush(),
      BorderBrush = MainViewShared.GetTransparentBrush(),
      BorderThickness = new Thickness(0)
    };

    var (orderCount, cancelCount) = MainViewShared.CountMarkedMenus(state);
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
      Content = MainViewShared.CreateIconButton("Information.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(5, 2),
      Background = MainViewShared.GetTransparentBrush(),
      BorderBrush = MainViewShared.GetTransparentBrush(),
      BorderThickness = new Thickness(0)
    };
    ToolTip.SetTip(aboutButton, "Über");
    aboutButton.Click += (_, _) => dispatch(new ToggleAbout());
    rightPanel.Children.Add(aboutButton);

    // Settings button
    var settingsButton = new ToggleButton {
      Content = MainViewShared.CreateIconButton("Settings.png"),
      Width = 40,
      Height = 40,
      Margin = new Thickness(2),
      Background = MainViewShared.GetTransparentBrush(),
      BorderBrush = MainViewShared.GetTransparentBrush(),
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

  private static Control CreateMainContent(AppState state, Action<Msg> dispatch) {
    return MenuView.Create(state, dispatch);
  }

  private static Control CreateStatusBar(AppState state) {
    var border = new Border {
      Background = MainViewShared.GetActionBarBackgroundBrush(),
      BorderBrush = MainViewShared.GetBorderBrush(),
      BorderThickness = new Thickness(0, 1, 0, 0),
      Padding = new Thickness(10, 5)
    };

    var dockPanel = new DockPanel();

    if (!string.IsNullOrEmpty(state.UserName)) {
      var userText = new TextBlock {
        Text = $"Angemeldet als: {state.UserName}",
        Foreground = MainViewShared.GetTextBrush(),
        VerticalAlignment = VerticalAlignment.Center,
        FontSize = 12
      };
      DockPanel.SetDock(userText, Dock.Left);
      dockPanel.Children.Add(userText);
    }

    if (state.LastMenuUpdate.HasValue) {
      var updateText = new TextBlock {
        Text = $"Letzte Aktualisierung: {state.LastMenuUpdate.Value:dd.MM.yyyy HH:mm}",
        Foreground = MainViewShared.GetTextBrush(),
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
    var overlayGrid = new Grid {
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch,
      Background = new SolidColorBrush(Colors.Black, 0.3)
    };

    var closeButton = new Button {
      Content = "✕",
      FontSize = 16,
      Width = 30,
      Height = 30,
      HorizontalAlignment = HorizontalAlignment.Right,
      VerticalAlignment = VerticalAlignment.Top,
      Margin = new Thickness(0, 10, 10, 0),
      Background = new SolidColorBrush(Colors.Red),
      Foreground = new SolidColorBrush(Colors.White)
    };
    closeButton.Click += (_, _) => dispatch(closeMessage);

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

    overlayGrid.Children.Add(contentContainer);
    overlayGrid.Children.Add(closeButton);

    overlayGrid.PointerPressed += (_, e) => {
      if (e.Source == overlayGrid) {
        dispatch(closeMessage);
      }
    };

    return overlayGrid;
  }
}

