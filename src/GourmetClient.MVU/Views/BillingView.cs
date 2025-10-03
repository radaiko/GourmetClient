using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

public static class BillingView {
  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  private static SolidColorBrush GetMinimalistBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#0d1117")
      : Color.Parse("#ffffff"));

  private static SolidColorBrush GetMinimalistBorderBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#21262d")
      : Color.Parse("#d0d7de"));

  private static SolidColorBrush GetPositiveBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#28a745")
      : Color.Parse("#22863a"));

  private static SolidColorBrush GetNegativeBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#d73a49")
      : Color.Parse("#cb2431"));

  public static Control Create(AppState state, Action<Msg> dispatch) {
    var scrollViewer = new ScrollViewer
    {
        HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
        VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
        //Padding = new Thickness(60, 40),
        Background = GetMinimalistBackgroundBrush()
    };

    var mainPanel = new StackPanel
    {
        Orientation = Orientation.Vertical,
        Spacing = 32,
        MinWidth = 800,
        Margin = new Thickness(10, 10, 10, 10)
    };

    // Minimal header
    var header = CreateMinimalistHeader(state, dispatch);
    mainPanel.Children.Add(header);

    // Content
    if (state.IsLoadingBilling) {
      var loadingView = CreateMinimalistLoadingView();
      mainPanel.Children.Add(loadingView);
    }
    else {
      var content = CreateMinimalistBillingContent(state, dispatch);
      mainPanel.Children.Add(content);
    }

    scrollViewer.Content = mainPanel;
    return scrollViewer;
  }

  private static Control CreateMinimalistHeader(AppState state, Action<Msg> dispatch) {
    var headerPanel = new StackPanel
    {
        Orientation = Orientation.Vertical,
        Spacing = 16,
        Margin = new Thickness(0, 0, 0, 32)
    };

    var titlePanel = new DockPanel();

    var titleText = new TextBlock {
      Text = "Transaktionen",
      FontSize = 24,
      FontWeight = FontWeight.Light,
      Foreground = GetTextBrush()
    };
    DockPanel.SetDock(titleText, Dock.Left);
    titlePanel.Children.Add(titleText);

    var controlsPanel = new StackPanel
    {
        Orientation = Orientation.Horizontal,
        Spacing = 16,
        VerticalAlignment = VerticalAlignment.Center
    };

    // Month selector
    var monthSelector = CreateMinimalistMonthSelector(state, dispatch);
    controlsPanel.Children.Add(monthSelector);

    // Refresh button
    var refreshButton = new Button {
      Content = "Aktualisieren",
      FontSize = 12,
      FontWeight = FontWeight.Normal,
      Padding = new Thickness(16, 8),
      Background = Brushes.Transparent,
      BorderBrush = GetMinimalistBorderBrush(),
      BorderThickness = new Thickness(1),
      Foreground = GetTextBrush(),
      CornerRadius = new CornerRadius(4)
    };
    refreshButton.Click += (_, _) => dispatch(new LoadBilling());
    controlsPanel.Children.Add(refreshButton);

    DockPanel.SetDock(controlsPanel, Dock.Right);
    titlePanel.Children.Add(controlsPanel);

    headerPanel.Children.Add(titlePanel);
    return headerPanel;
  }

  private static Control CreateMinimalistLoadingView() {
    var loadingPanel = new StackPanel {
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Spacing = 16,
      Margin = new Thickness(0, 80)
    };

    var spinner = new TextBlock {
      Text = "⟳",
      FontSize = 32,
      Foreground = new SolidColorBrush(Color.Parse("#007ACC")),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      RenderTransformOrigin = RelativePoint.Center
    };

    // Create rotation animation using a simple timer-based approach
    var rotateTransform = new RotateTransform();
    spinner.RenderTransform = rotateTransform;

    // Use a timer for smooth rotation animation
    var timer = new System.Timers.Timer(16); // ~60 FPS
    var angle = 0.0;
    timer.Elapsed += (sender, e) =>
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            angle += 6; // 360 degrees in 1 second (6 degrees per 16ms)
            if (angle >= 360) angle = 0;
            rotateTransform.Angle = angle;
        });
    };
    timer.Start();

    loadingPanel.Children.Add(spinner);

    var loadingText = new TextBlock {
      Text = "Lade Abrechnungsdaten...",
      FontSize = 14,
      FontWeight = FontWeight.Light,
      Foreground = GetTextBrush(),
      Opacity = 0.7,
      HorizontalAlignment = HorizontalAlignment.Center
    };
    loadingPanel.Children.Add(loadingText);

    return loadingPanel;
  }

  private static Control CreateMinimalistBillingContent(AppState state, Action<Msg> dispatch) {
    var mainPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 40
    };

    // Summary section
    var summarySection = CreateMinimalistSummarySection(state);
    mainPanel.Children.Add(summarySection);

    // Transaction sections
    if (state.MenuBillingPositions?.Count > 0) {
      var menuSection = CreateMinimalistTransactionSection("Menüs", state.MenuBillingPositions, state.SumCostMenuBillingPositions);
      mainPanel.Children.Add(menuSection);
    }

    if (state.DrinkBillingPositions?.Count > 0) {
      var drinkSection = CreateMinimalistTransactionSection("Getränke", state.DrinkBillingPositions, state.SumCostDrinkBillingPositions);
      mainPanel.Children.Add(drinkSection);
    }

    if (state.SumCostUnknownBillingPositions > 0) {
      var unknownSection = CreateMinimalistUnknownSection(state.SumCostUnknownBillingPositions);
      mainPanel.Children.Add(unknownSection);
    }

    if (mainPanel.Children.Count == 1) {
      var noDataPanel = new StackPanel
      {
          HorizontalAlignment = HorizontalAlignment.Center,
          Spacing = 12,
          Margin = new Thickness(0, 60)
      };

      var noDataText = new TextBlock {
        Text = "Keine Transaktionen",
        FontSize = 16,
        FontWeight = FontWeight.Light,
        Foreground = GetTextBrush(),
        HorizontalAlignment = HorizontalAlignment.Center
      };
      noDataPanel.Children.Add(noDataText);

      var noDataSubText = new TextBlock {
        Text = "Für den ausgewählten Zeitraum sind keine Transaktionen verfügbar",
        FontSize = 12,
        FontWeight = FontWeight.Light,
        Foreground = GetTextBrush(),
        Opacity = 0.6,
        HorizontalAlignment = HorizontalAlignment.Center
      };
      noDataPanel.Children.Add(noDataSubText);

      mainPanel.Children.Add(noDataPanel);
    }

    return mainPanel;
  }

  private static Control CreateMinimalistSummarySection(AppState state) {
    var panel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 16
    };

    var titleText = new TextBlock {
      Text = "\u00DCbersicht",
      FontSize = 18,
      FontWeight = FontWeight.Normal,
      Foreground = GetTextBrush(),
      Margin = new Thickness(0, 0, 0, 8)
    };
    panel.Children.Add(titleText);

    var totalCost = state.SumCostMenuBillingPositions + state.SumCostDrinkBillingPositions + state.SumCostUnknownBillingPositions;

    var summaryItems = new[] {
      ($"Menüs", state.SumCostMenuBillingPositions),
      ($"Getränke", state.SumCostDrinkBillingPositions),
      ($"Sonstiges", state.SumCostUnknownBillingPositions)
    };

    foreach (var (label, amount) in summaryItems) {
      if (amount == 0) continue;

      var itemBorder = new Border
      {
          BorderBrush = GetMinimalistBorderBrush(),
          BorderThickness = new Thickness(0, 0, 0, 1),
          Padding = new Thickness(0, 12)
      };

      var itemPanel = new DockPanel();

      var labelText = new TextBlock {
        Text = label,
        FontSize = 14,
        FontWeight = FontWeight.Normal,
        Foreground = GetTextBrush(),
        VerticalAlignment = VerticalAlignment.Center
      };
      DockPanel.SetDock(labelText, Dock.Left);
      itemPanel.Children.Add(labelText);

      var amountText = new TextBlock {
        Text = $"{amount:C2}",
        FontSize = 14,
        FontWeight = FontWeight.Medium,
        Foreground = amount >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
        VerticalAlignment = VerticalAlignment.Center
      };
      DockPanel.SetDock(amountText, Dock.Right);
      itemPanel.Children.Add(amountText);

      itemBorder.Child = itemPanel;
      panel.Children.Add(itemBorder);
    }

    // Total with emphasis
    var totalBorder = new Border
    {
        BorderBrush = GetMinimalistBorderBrush(),
        BorderThickness = new Thickness(0, 2, 0, 0),
        Padding = new Thickness(0, 16, 0, 0),
        Margin = new Thickness(0, 16, 0, 0)
    };

    var totalPanel = new DockPanel();

    var totalLabelText = new TextBlock {
      Text = "Gesamt",
      FontSize = 16,
      FontWeight = FontWeight.Medium,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(totalLabelText, Dock.Left);
    totalPanel.Children.Add(totalLabelText);

    var totalAmountText = new TextBlock {
      Text = $"{totalCost:C2}",
      FontSize = 16,
      FontWeight = FontWeight.Bold,
      Foreground = totalCost >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(totalAmountText, Dock.Right);
    totalPanel.Children.Add(totalAmountText);

    totalBorder.Child = totalPanel;
    panel.Children.Add(totalBorder);

    return panel;
  }

  private static Control CreateMinimalistTransactionSection(string title, System.Collections.Immutable.ImmutableList<GroupedBillingPositionsViewModel> positions, decimal totalAmount) {
    var panel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 20
    };

    // Section header
    var headerPanel = new DockPanel
    {
        Margin = new Thickness(0, 0, 0, 16)
    };

    var titleText = new TextBlock {
      Text = title,
      FontSize = 18,
      FontWeight = FontWeight.Normal,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(titleText, Dock.Left);
    headerPanel.Children.Add(titleText);

    var totalText = new TextBlock {
      Text = $"{totalAmount:C2}",
      FontSize = 16,
      FontWeight = FontWeight.Medium,
      Foreground = totalAmount >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(totalText, Dock.Right);
    headerPanel.Children.Add(totalText);

    panel.Children.Add(headerPanel);

    // Transaction items
    var itemsPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 8
    };

    foreach (var position in positions) {
      var itemBorder = new Border
      {
          BorderBrush = GetMinimalistBorderBrush(),
          BorderThickness = new Thickness(0, 0, 0, 1),
          Padding = new Thickness(0, 12)
      };

      var itemPanel = new DockPanel();

      var descriptionPanel = new StackPanel {
        Orientation = Orientation.Vertical,
        Spacing = 4
      };

      var nameText = new TextBlock {
        Text = position.Description ?? "Unbekannt",
        FontSize = 14,
        FontWeight = FontWeight.Normal,
        Foreground = GetTextBrush()
      };
      descriptionPanel.Children.Add(nameText);

      var quantityText = new TextBlock {
        Text = $"{position.Quantity}x",
        FontSize = 12,
        FontWeight = FontWeight.Light,
        Foreground = GetTextBrush(),
        Opacity = 0.6
      };
      descriptionPanel.Children.Add(quantityText);

      DockPanel.SetDock(descriptionPanel, Dock.Left);
      itemPanel.Children.Add(descriptionPanel);

      var priceText = new TextBlock {
        Text = $"{position.TotalCost:C2}",
        FontSize = 14,
        FontWeight = FontWeight.Medium,
        Foreground = position.TotalCost >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
        VerticalAlignment = VerticalAlignment.Center
      };
      DockPanel.SetDock(priceText, Dock.Right);
      itemPanel.Children.Add(priceText);

      itemBorder.Child = itemPanel;
      itemsPanel.Children.Add(itemBorder);
    }

    panel.Children.Add(itemsPanel);
    return panel;
  }

  private static Control CreateMinimalistUnknownSection(decimal unknownAmount) {
    var panel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 16
    };

    var titleText = new TextBlock {
      Text = "Sonstige Transaktionen",
      FontSize = 18,
      FontWeight = FontWeight.Normal,
      Foreground = GetTextBrush(),
      Margin = new Thickness(0, 0, 0, 8)
    };
    panel.Children.Add(titleText);

    var itemBorder = new Border
    {
        BorderBrush = GetMinimalistBorderBrush(),
        BorderThickness = new Thickness(0, 0, 0, 1),
        Padding = new Thickness(0, 12)
    };

    var itemPanel = new DockPanel();

    var descriptionText = new TextBlock {
      Text = "Nicht kategorisierte Transaktionen",
      FontSize = 14,
      FontWeight = FontWeight.Normal,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(descriptionText, Dock.Left);
    itemPanel.Children.Add(descriptionText);

    var amountText = new TextBlock {
      Text = $"{unknownAmount:C2}",
      FontSize = 14,
      FontWeight = FontWeight.Medium,
      Foreground = unknownAmount >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(amountText, Dock.Right);
    itemPanel.Children.Add(amountText);

    itemBorder.Child = itemPanel;
    panel.Children.Add(itemBorder);

    return panel;
  }

  private static Control CreateMinimalistMonthSelector(AppState state, Action<Msg> dispatch) {
    var comboBox = new ComboBox {
      MinWidth = 140,
      FontSize = 12,
      Padding = new Thickness(12, 8),
      Background = Brushes.Transparent,
      BorderBrush = GetMinimalistBorderBrush(),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(4)
    };

    var availableMonths = state.AvailableMonths?.ToList() ?? new List<DateTime> { DateTime.Now };

    comboBox.ItemsSource = availableMonths;
    comboBox.DisplayMemberBinding = new Avalonia.Data.Binding(".")
    {
      StringFormat = "MMMM yyyy"
    };

    var selectedMonth = state.SelectedMonth ?? DateTime.Now;
    var matchingMonth = availableMonths.FirstOrDefault(m =>
      m.Year == selectedMonth.Year && m.Month == selectedMonth.Month);

    if (matchingMonth != default(DateTime)) {
      comboBox.SelectedItem = matchingMonth;
    } else if (availableMonths.Count > 0) {
      comboBox.SelectedItem = availableMonths[0];
    }

    comboBox.SelectionChanged += (sender, e) => {
      if (comboBox.SelectedItem is DateTime selectedDate) {
        if (state.IsBillingVisible && selectedDate != state.SelectedMonth) {
          dispatch(new SelectMonth(selectedDate));
        }
      }
    };

    return comboBox;
  }
}
