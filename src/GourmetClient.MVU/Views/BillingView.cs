using System.Linq;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Shapes;
using Avalonia.Collections;
using Avalonia.Styling;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

public static class BillingView {
  private static SolidColorBrush GetTextBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Colors.White 
      : Colors.Black);

  private static SolidColorBrush GetCardBackgroundBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#3C3C3C") 
      : Colors.White);

  private static SolidColorBrush GetHeaderBackgroundBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#2D2D30") 
      : Color.Parse("#F2F2F2"));

  private static SolidColorBrush GetBorderBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#464647") 
      : Colors.LightGray);

  private static SolidColorBrush GetPositiveBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#4CAF50") 
      : Colors.Green);

  private static SolidColorBrush GetNegativeBrush() => 
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark 
      ? Color.Parse("#F44336") 
      : Colors.Red);

  public static Control Create(AppState state, Action<Msg> dispatch) {
    var mainPanel = new DockPanel {
      Background = GetCardBackgroundBrush(),
      MinWidth = 500,
      MinHeight = 400
      // Removed MaxHeight to prevent bottom cutoff
    };

    // Header with title and close button
    var header = CreateHeader(state, dispatch);
    DockPanel.SetDock(header, Dock.Top);
    mainPanel.Children.Add(header);

    // Content area with improved scrolling
    var contentScrollViewer = new ScrollViewer {
      Padding = new Thickness(15),
      VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
      HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled
    };

    if (state.IsLoadingBilling) {
      contentScrollViewer.Content = CreateLoadingPanel();
    }
    else {
      contentScrollViewer.Content = CreateBillingContent(state, dispatch);
    }

    mainPanel.Children.Add(contentScrollViewer);
    return mainPanel;
  }

  private static Control CreateLoadingPanel() {
    var loadingPanel = new StackPanel {
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Spacing = 15,
      MinHeight = 200
    };

    // Create a simple spinning loading indicator using emoji
    var spinner = new TextBlock {
      Text = "⟳",
      FontSize = 40,
      Foreground = new SolidColorBrush(Color.Parse("#007ACC")),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center
    };

    loadingPanel.Children.Add(spinner);

    var loadingText = new TextBlock {
      Text = "Lade Abrechnungsdaten...",
      FontSize = 16,
      Foreground = GetTextBrush(),
      HorizontalAlignment = HorizontalAlignment.Center,
      Margin = new Thickness(0, 10, 0, 0)
    };
    loadingPanel.Children.Add(loadingText);

    return loadingPanel;
  }

  private static Control CreateHeader(AppState state, Action<Msg> dispatch) {
    var headerBorder = new Border {
      Background = GetHeaderBackgroundBrush(),
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(0, 0, 0, 1),
      Padding = new Thickness(15, 10)
    };

    var headerPanel = new DockPanel();

    var titleText = new TextBlock {
      Text = "Transaktionen",
      FontSize = 18,
      FontWeight = FontWeight.Bold,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(titleText, Dock.Left);
    headerPanel.Children.Add(titleText);

    var refreshButton = new Button {
      Content = "🔄",
      FontSize = 16,
      Width = 30,
      Height = 30,
      Margin = new Thickness(10, 0, 0, 0)
    };
    refreshButton.Click += (_, _) => dispatch(new LoadBilling());
    DockPanel.SetDock(refreshButton, Dock.Right);
    headerPanel.Children.Add(refreshButton);

    // Month selector
    var monthSelector = CreateMonthSelector(state, dispatch);
    DockPanel.SetDock(monthSelector, Dock.Right);
    headerPanel.Children.Add(monthSelector);

    headerBorder.Child = headerPanel;
    return headerBorder;
  }

  private static Control CreateBillingContent(AppState state, Action<Msg> dispatch) {
    var mainPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 15
    };

    // Summary section
    var summarySection = CreateSummarySection(state);
    mainPanel.Children.Add(summarySection);

    // Menu transactions section
    if (state.MenuBillingPositions?.Count > 0) {
      var menuSection = CreateTransactionSection("Menü-Transaktionen", state.MenuBillingPositions, state.SumCostMenuBillingPositions);
      mainPanel.Children.Add(menuSection);
    }

    // Drink transactions section
    if (state.DrinkBillingPositions?.Count > 0) {
      var drinkSection = CreateTransactionSection("Getränke-Transaktionen", state.DrinkBillingPositions, state.SumCostDrinkBillingPositions);
      mainPanel.Children.Add(drinkSection);
    }

    // Unknown transactions (if any)
    if (state.SumCostUnknownBillingPositions > 0) {
      var unknownSection = CreateUnknownSection(state.SumCostUnknownBillingPositions);
      mainPanel.Children.Add(unknownSection);
    }

    if (mainPanel.Children.Count == 1) {
      var noDataText = new TextBlock {
        Text = "Keine Transaktionsdaten verfügbar",
        FontSize = 14,
        Foreground = GetTextBrush(),
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Thickness(0, 20)
      };
      mainPanel.Children.Add(noDataText);
    }

    return mainPanel;
  }

  private static Control CreateSummarySection(AppState state) {
    var border = new Border {
      Background = GetHeaderBackgroundBrush(),
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(5),
      Padding = new Thickness(15),
      Margin = new Thickness(0, 0, 0, 10)
    };

    var panel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 8
    };

    var titleText = new TextBlock {
      Text = "Zusammenfassung",
      FontSize = 16,
      FontWeight = FontWeight.Bold,
      Foreground = GetTextBrush(),
      Margin = new Thickness(0, 0, 0, 5)
    };
    panel.Children.Add(titleText);

    var totalCost = state.SumCostMenuBillingPositions + state.SumCostDrinkBillingPositions + state.SumCostUnknownBillingPositions;

    var summaryItems = new[] {
      ($"Menüs:", state.SumCostMenuBillingPositions),
      ($"Getränke:", state.SumCostDrinkBillingPositions),
      ($"Unbekannt:", state.SumCostUnknownBillingPositions),
      ($"Gesamt:", totalCost)
    };

    foreach (var (label, amount) in summaryItems) {
      var itemPanel = new DockPanel();

      var labelText = new TextBlock {
        Text = label,
        Foreground = GetTextBrush(),
        VerticalAlignment = VerticalAlignment.Center
      };
      DockPanel.SetDock(labelText, Dock.Left);
      itemPanel.Children.Add(labelText);

      var amountText = new TextBlock {
        Text = $"{amount:C2}",
        Foreground = amount >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
        FontWeight = label == "Gesamt:" ? FontWeight.Bold : FontWeight.Normal,
        VerticalAlignment = VerticalAlignment.Center
      };
      DockPanel.SetDock(amountText, Dock.Right);
      itemPanel.Children.Add(amountText);

      panel.Children.Add(itemPanel);
    }

    border.Child = panel;
    return border;
  }

  private static Control CreateTransactionSection(string title, System.Collections.Immutable.ImmutableList<GroupedBillingPositionsViewModel> positions, decimal totalAmount) {
    var border = new Border {
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(5),
      Margin = new Thickness(0, 5)
    };

    var panel = new StackPanel {
      Orientation = Orientation.Vertical
    };

    // Section header
    var headerBorder = new Border {
      Background = GetHeaderBackgroundBrush(),
      Padding = new Thickness(15, 10)
    };

    var headerPanel = new DockPanel();

    var titleText = new TextBlock {
      Text = title,
      FontSize = 14,
      FontWeight = FontWeight.SemiBold,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(titleText, Dock.Left);
    headerPanel.Children.Add(titleText);

    var totalText = new TextBlock {
      Text = $"Summe: {totalAmount:C2}",
      FontWeight = FontWeight.Bold,
      Foreground = totalAmount >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(totalText, Dock.Right);
    headerPanel.Children.Add(totalText);

    headerBorder.Child = headerPanel;
    panel.Children.Add(headerBorder);

    // Transaction items
    var itemsPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Margin = new Thickness(15, 5, 15, 15)
    };

    foreach (var position in positions) {
      var itemBorder = new Border {
        BorderBrush = GetBorderBrush(),
        BorderThickness = new Thickness(0, 0, 0, 1),
        Padding = new Thickness(0, 8),
        Margin = new Thickness(0, 2)
      };

      var itemPanel = new DockPanel();

      var descriptionPanel = new StackPanel {
        Orientation = Orientation.Vertical
      };

      var nameText = new TextBlock {
        Text = position.Description ?? "Unbekannt",
        FontWeight = FontWeight.Medium,
        Foreground = GetTextBrush()
      };
      descriptionPanel.Children.Add(nameText);

      var quantityText = new TextBlock {
        Text = $"Menge: {position.Quantity}",
        FontSize = 11,
        Foreground = GetTextBrush(),
        Opacity = 0.8
      };
      descriptionPanel.Children.Add(quantityText);

      DockPanel.SetDock(descriptionPanel, Dock.Left);
      itemPanel.Children.Add(descriptionPanel);

      var priceText = new TextBlock {
        Text = $"{position.TotalCost:C2}",
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
    border.Child = panel;
    return border;
  }

  private static Control CreateUnknownSection(decimal unknownAmount) {
    var border = new Border {
      BorderBrush = GetBorderBrush(),
      BorderThickness = new Thickness(1),
      CornerRadius = new CornerRadius(5),
      Padding = new Thickness(15),
      Margin = new Thickness(0, 5)
    };

    var panel = new DockPanel();

    var titleText = new TextBlock {
      Text = "Unbekannte Transaktionen",
      FontSize = 14,
      FontWeight = FontWeight.SemiBold,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(titleText, Dock.Left);
    panel.Children.Add(titleText);

    var amountText = new TextBlock {
      Text = $"{unknownAmount:C2}",
      FontWeight = FontWeight.Bold,
      Foreground = unknownAmount >= 0 ? GetPositiveBrush() : GetNegativeBrush(),
      VerticalAlignment = VerticalAlignment.Center
    };
    DockPanel.SetDock(amountText, Dock.Right);
    panel.Children.Add(amountText);

    border.Child = panel;
    return border;
  }

  private static Control CreateMonthSelector(AppState state, Action<Msg> dispatch) {
    var comboBox = new ComboBox {
      Width = 150,
      Margin = new Thickness(10, 0),
      VerticalAlignment = VerticalAlignment.Center
    };

    // Use available months from state, fallback to current month if not yet initialized
    var availableMonths = state.AvailableMonths?.ToList() ?? new List<DateTime> { DateTime.Now };

    // Set items and display format
    comboBox.ItemsSource = availableMonths;
    comboBox.DisplayMemberBinding = new Avalonia.Data.Binding(".")
    {
      StringFormat = "MMMM yyyy"
    };

    // Set selected item to current selected month or current month
    var selectedMonth = state.SelectedMonth ?? DateTime.Now;
    var matchingMonth = availableMonths.FirstOrDefault(m => 
      m.Year == selectedMonth.Year && m.Month == selectedMonth.Month);
    
    if (matchingMonth != default(DateTime)) {
      comboBox.SelectedItem = matchingMonth;
    } else if (availableMonths.Count > 0) {
      comboBox.SelectedItem = availableMonths[0];
    }

    // Handle selection changed - automatically trigger loading
    comboBox.SelectionChanged += (sender, e) => {
      if (comboBox.SelectedItem is DateTime selectedDate) {
        // Only dispatch if the billing view is visible and the month actually changed
        if (state.IsBillingVisible && selectedDate != state.SelectedMonth) {
          dispatch(new SelectMonth(selectedDate));
        }
      }
    };

    return comboBox;
  }
}
