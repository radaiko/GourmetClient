using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using System.Linq;

namespace GourmetClient.MVU.Views;

/// <summary>
/// iOS-optimized billing view with mobile-friendly list layout
/// </summary>
public static class BillingViewIOS
{
    private static SolidColorBrush GetSecondaryTextBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#8E8E93")
        : Color.Parse("#6E6E73"));

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
            Spacing = 0,
            Background = GetBackgroundBrush()
        };

        // Header with month selector
        var header = CreateMobileHeader(state, dispatch);
        mainPanel.Children.Add(header);

        // Content
        if (state.IsLoadingBilling)
        {
            // Use shared loading view factory (customized for iOS look)
            var loadingView = LoadingViewFactory.Create(
                message: "Lade Transaktionen...",
                spinnerFontSize: 48,
                textFontSize: 17,
                spacing: 20,
                margin: new Thickness(20, 60, 20, 20),
                spinnerColor: Color.Parse("#007AFF"),
                textBrush: BillingViewShared.GetTextBrush());
            mainPanel.Children.Add(loadingView);
        }
        else
        {
            var content = CreateBillingContent(state);
            mainPanel.Children.Add(content);
        }

        return mainPanel;
    }

    private static Control CreateMobileHeader(AppState state, Action<Msg> dispatch)
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Background = GetCardBackgroundBrush(),
            Margin = new Thickness(16, 12, 16, 16)
        };

        // Month selector
        if (state.AvailableMonths != null && state.AvailableMonths.Count > 0)
        {
            // Replaced DockPanel with Grid to push the ComboBox fully to the right side
            var monthPanel = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star)
                }
            };

            var label = new TextBlock
            {
                Text = "Monat",
                FontSize = 17,
                Foreground = BillingViewShared.GetTextBrush(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0,0,12,0) // space between label and picker
            };
            Grid.SetColumn(label, 0);
            monthPanel.Children.Add(label);

            var monthPicker = new ComboBox
            {
                ItemsSource = state.AvailableMonths.Select(m => m.ToString("MMMM yyyy")).ToList(),
                SelectedIndex = state.SelectedMonth.HasValue
                    ? state.AvailableMonths.IndexOf(state.SelectedMonth.Value)
                    : 0,
                MinWidth = 180,
                FontSize = 17,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            monthPicker.SelectionChanged += (_, _) =>
            {
                if (monthPicker.SelectedIndex >= 0 && monthPicker.SelectedIndex < state.AvailableMonths.Count)
                {
                    var selectedMonth = state.AvailableMonths[monthPicker.SelectedIndex];
                    dispatch(new SelectMonth(selectedMonth));
                }
            };
            Grid.SetColumn(monthPicker, 1);
            monthPanel.Children.Add(monthPicker);

            headerPanel.Children.Add(monthPanel);

            // Refresh button
            var refreshButton = new Button
            {
                Content = "↻ Aktualisieren",
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16, 12),
                FontSize = 17,
                Background = new SolidColorBrush(Color.Parse("#007AFF")),
                Foreground = Brushes.White,
                CornerRadius = new CornerRadius(10)
            };
            refreshButton.Click += (_, _) => dispatch(new LoadBilling());
            headerPanel.Children.Add(refreshButton);
        }

        return headerPanel;
    }

    private static Control CreateBillingContent(AppState state)
    {
        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 24,
            Margin = new Thickness(0, 16, 0, 16)
        };

        // Summary card
        var summaryCard = CreateSummaryCard(state);
        contentPanel.Children.Add(summaryCard);

        // Menu positions section
        if (state.MenuBillingPositions != null && state.MenuBillingPositions.Count > 0)
        {
            var menuSection = CreateBillingSection("Menüs", state.MenuBillingPositions);
            contentPanel.Children.Add(menuSection);
        }

        // Drink positions section
        if (state.DrinkBillingPositions != null && state.DrinkBillingPositions.Count > 0)
        {
            var drinkSection = CreateBillingSection("Getränke", state.DrinkBillingPositions);
            contentPanel.Children.Add(drinkSection);
        }

        // Empty state
        if ((state.MenuBillingPositions == null || state.MenuBillingPositions.Count == 0) &&
            (state.DrinkBillingPositions == null || state.DrinkBillingPositions.Count == 0))
        {
            var emptyText = new TextBlock
            {
                Text = "Keine Transaktionen für diesen Monat",
                FontSize = 17,
                Foreground = GetSecondaryTextBrush(),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20, 40)
            };
            contentPanel.Children.Add(emptyText);
        }

        return contentPanel;
    }

    private static Control CreateSummaryCard(AppState state)
    {
        var card = new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(16, 0),
            Padding = new Thickness(16)
        };

        var summaryStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 12
        };

        var titleText = new TextBlock
        {
            Text = "Zusammenfassung",
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            Foreground = BillingViewShared.GetTextBrush()
        };
        summaryStack.Children.Add(titleText);

        var totalAmount = state.SumCostMenuBillingPositions + 
                         state.SumCostDrinkBillingPositions + 
                         state.SumCostUnknownBillingPositions;

        var totalText = new TextBlock
        {
            Text = $"Gesamt: {totalAmount:C}",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#007AFF"))
        };
        summaryStack.Children.Add(totalText);

        // Breakdown
        if (state.SumCostMenuBillingPositions > 0)
        {
            var menuText = new TextBlock
            {
                Text = $"Menüs: {state.SumCostMenuBillingPositions:C}",
                FontSize = 15,
                Foreground = GetSecondaryTextBrush()
            };
            summaryStack.Children.Add(menuText);
        }

        if (state.SumCostDrinkBillingPositions > 0)
        {
            var drinkText = new TextBlock
            {
                Text = $"Getränke: {state.SumCostDrinkBillingPositions:C}",
                FontSize = 15,
                Foreground = GetSecondaryTextBrush()
            };
            summaryStack.Children.Add(drinkText);
        }

        card.Child = summaryStack;
        return card;
    }

    private static Control CreateBillingSection(string title, System.Collections.Immutable.ImmutableList<GroupedBillingPositionsViewModel> positions)
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
            Margin = new Thickness(16, 0)
        };
        section.Children.Add(headerText);

        // Items card
        var itemsCard = new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(16, 0)
        };

        var itemsStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0
        };

        for (int i = 0; i < positions.Count; i++)
        {
            var position = positions[i];
            var itemRow = CreateBillingItem(position, i < positions.Count - 1);
            itemsStack.Children.Add(itemRow);
        }

        itemsCard.Child = itemsStack;
        section.Children.Add(itemsCard);

        return section;
    }

    private static Control CreateBillingItem(GroupedBillingPositionsViewModel position, bool showBorder) {
        var container = new Border {
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = showBorder ? new Thickness(0, 0, 0, 0.5) : new Thickness(0),
            Padding = new Thickness(16, 12)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var leftStack = new StackPanel {
            Orientation = Orientation.Vertical,
            Spacing = 4
        };

        var descriptionText = new TextBlock {
            Text = position.Description,
            FontSize = 17,
            Foreground = BillingViewShared.GetTextBrush(),
            TextWrapping = TextWrapping.Wrap
        };
        leftStack.Children.Add(descriptionText);

        var quantityText = new TextBlock {
            Text = $"{position.Quantity}x",
            FontSize = 13,
            Foreground = GetSecondaryTextBrush()
        };
        leftStack.Children.Add(quantityText);

        Grid.SetColumn(leftStack, 0);
        grid.Children.Add(leftStack);

        var costText = new TextBlock {
            Text = position.TotalCost.ToString("C"),
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            Foreground = BillingViewShared.GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(costText, 1);
        grid.Children.Add(costText);

        container.Child = grid;
        return container;
    }
}
