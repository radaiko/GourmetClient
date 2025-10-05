using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GC.ViewModels;
using System;
using System.Linq;

namespace GC.Views;

/// <summary>
/// iOS-optimized billing view with mobile-friendly layout
/// </summary>
public static class BillingViewIOS
{
    private static SolidColorBrush GetBackgroundBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#000000")
        : Color.Parse("#F2F2F7"));

    private static SolidColorBrush GetCardBackgroundBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#1C1C1E")
        : Colors.White);

    private static SolidColorBrush GetTextBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Colors.White
            : Colors.Black);

    private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

    public static Control Create(MainViewModel viewModel)
    {
        if (viewModel.BillingViewModel == null)
        {
            return CreatePlaceholderCard();
        }

        var billingViewModel = viewModel.BillingViewModel;

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 0,
            Background = GetBackgroundBrush()
        };

        // Header with month selector
        var header = CreateMobileHeader(billingViewModel);
        mainPanel.Children.Add(header);

        // Content
        if (billingViewModel.IsLoading)
        {
            var loadingView = CreateLoadingView(billingViewModel.LoadingProgress);
            mainPanel.Children.Add(loadingView);
        }
        else if (billingViewModel.AvailableMonths.Count == 0)
        {
            // Trigger load if not already loaded
            if (!string.IsNullOrEmpty(viewModel.VentoPayUsername) && !string.IsNullOrEmpty(viewModel.VentoPayPassword))
            {
                Dispatcher.UIThread.Post(async () => await billingViewModel.LoadBillingCommand.ExecuteAsync(null));
                mainPanel.Children.Add(CreateLoadingView(billingViewModel.LoadingProgress));
            }
            else
            {
                mainPanel.Children.Add(CreatePlaceholderCard());
            }
        }
        else
        {
            var content = CreateBillingContent(billingViewModel);
            mainPanel.Children.Add(content);
        }

        return mainPanel;
    }

    private static Control CreateLoadingView(int progress = 0)
    {
        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 20,
            Margin = new Thickness(20, 60),
            Children =
            {
                new TextBlock
                {
                    Text = "Lade Transaktionen...",
                    FontSize = 17,
                    Foreground = GetTextBrush(),
                    HorizontalAlignment = HorizontalAlignment.Center
                }
            }
        };

        // Show progress percentage if loading has started (progress > 0)
        if (progress > 0)
        {
            stackPanel.Children.Add(new TextBlock
            {
                Text = $"{progress}%",
                FontSize = 15,
                Foreground = GetSecondaryTextBrush(),
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }

        return stackPanel;
    }

    private static Control CreateMobileHeader(BillingViewModel billingViewModel)
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Background = GetCardBackgroundBrush(),
            Margin = new Thickness(16, 12, 16, 16)
        };

        // Month selector
        if (billingViewModel.AvailableMonths.Count > 0)
        {
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
                Foreground = GetTextBrush(),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 12, 0)
            };
            Grid.SetColumn(label, 0);
            monthPanel.Children.Add(label);

            var monthPicker = new ComboBox
            {
                ItemsSource = billingViewModel.AvailableMonths.Select(m => m.ToString("MMMM yyyy")).ToList(),
                SelectedIndex = billingViewModel.SelectedMonth.HasValue
                    ? billingViewModel.AvailableMonths.ToList().IndexOf(billingViewModel.SelectedMonth.Value)
                    : 0,
                MinWidth = 180,
                FontSize = 17,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            monthPicker.SelectionChanged += (_, _) =>
            {
                if (monthPicker.SelectedIndex >= 0 && monthPicker.SelectedIndex < billingViewModel.AvailableMonths.Count)
                {
                    var selectedMonth = billingViewModel.AvailableMonths[monthPicker.SelectedIndex];
                    billingViewModel.SelectedMonth = selectedMonth;
                }
            };
            Grid.SetColumn(monthPicker, 1);
            monthPanel.Children.Add(monthPicker);

            headerPanel.Children.Add(monthPanel);
        }

        return headerPanel;
    }

    private static Control CreateBillingContent(BillingViewModel billingViewModel)
    {
        var contentPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 24,
            Margin = new Thickness(0, 16, 0, 16)
        };

        // Summary card
        var summaryCard = CreateSummaryCard(billingViewModel);
        contentPanel.Children.Add(summaryCard);

        // Menu positions section
        if (billingViewModel.MenuBillingPositions.Count > 0)
        {
            var menuSection = CreateBillingSection("Menüs", billingViewModel.MenuBillingPositions);
            contentPanel.Children.Add(menuSection);
        }

        // Drink positions section
        if (billingViewModel.DrinkBillingPositions.Count > 0)
        {
            var drinkSection = CreateBillingSection("Getränke", billingViewModel.DrinkBillingPositions);
            contentPanel.Children.Add(drinkSection);
        }

        // Empty state
        if (billingViewModel.MenuBillingPositions.Count == 0 && billingViewModel.DrinkBillingPositions.Count == 0)
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

        return new ScrollViewer
        {
            Content = contentPanel
        };
    }

    private static Control CreateSummaryCard(BillingViewModel billingViewModel)
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
            Foreground = GetTextBrush()
        };
        summaryStack.Children.Add(titleText);

        var totalAmount = billingViewModel.SumCostMenuBillingPositions +
                         billingViewModel.SumCostDrinkBillingPositions +
                         billingViewModel.SumCostUnknownBillingPositions;

        var totalText = new TextBlock
        {
            Text = $"Gesamt: {totalAmount:C}",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#007AFF"))
        };
        summaryStack.Children.Add(totalText);

        // Breakdown
        if (billingViewModel.SumCostMenuBillingPositions > 0)
        {
            var menuText = new TextBlock
            {
                Text = $"Menüs: {billingViewModel.SumCostMenuBillingPositions:C}",
                FontSize = 15,
                Foreground = GetSecondaryTextBrush()
            };
            summaryStack.Children.Add(menuText);
        }

        if (billingViewModel.SumCostDrinkBillingPositions > 0)
        {
            var drinkText = new TextBlock
            {
                Text = $"Getränke: {billingViewModel.SumCostDrinkBillingPositions:C}",
                FontSize = 15,
                Foreground = GetSecondaryTextBrush()
            };
            summaryStack.Children.Add(drinkText);
        }

        card.Child = summaryStack;
        return card;
    }

    private static Control CreateBillingSection(string title, System.Collections.ObjectModel.ObservableCollection<GroupedBillingPosition> positions)
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

    private static Control CreateBillingItem(GroupedBillingPosition position, bool showBorder)
    {
        var container = new Border
        {
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = showBorder ? new Thickness(0, 0, 0, 0.5) : new Thickness(0),
            Padding = new Thickness(16, 12)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var leftStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4
        };

        var descriptionText = new TextBlock
        {
            Text = position.Description,
            FontSize = 17,
            Foreground = GetTextBrush(),
            TextWrapping = TextWrapping.Wrap
        };
        leftStack.Children.Add(descriptionText);

        var quantityText = new TextBlock
        {
            Text = $"{position.Quantity}x",
            FontSize = 13,
            Foreground = GetSecondaryTextBrush()
        };
        leftStack.Children.Add(quantityText);

        Grid.SetColumn(leftStack, 0);
        grid.Children.Add(leftStack);

        var costText = new TextBlock
        {
            Text = position.TotalCost.ToString("C"),
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            Foreground = GetTextBrush(),
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(costText, 1);
        grid.Children.Add(costText);

        container.Child = grid;
        return container;
    }

    private static Control CreatePlaceholderCard()
    {
        return new ScrollViewer
        {
            Background = GetBackgroundBrush(),
            Content = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 12,
                Margin = new Thickness(20),
                Children =
                {
                    new TextBlock
                    {
                        Text = "💳",
                        FontSize = 48,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = "Keine Abrechnungsdaten verfügbar",
                        FontSize = 16,
                        Foreground = GetTextBrush(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    },
                    new TextBlock
                    {
                        Text = "Bitte konfigurieren Sie Ihre VentoPay-Anmeldedaten in den Einstellungen.",
                        FontSize = 14,
                        Foreground = GetSecondaryTextBrush(),
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        MaxWidth = 400
                    }
                }
            }
        };
    }
}
