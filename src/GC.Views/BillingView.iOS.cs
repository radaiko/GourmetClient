using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GC.ViewModels;
using System.ComponentModel; // For PropertyChangedEventHandler

namespace GC.Views;

/// <summary>
/// iOS billing view: constructs UI once, updates content & visibility via simple switch on property changes.
/// Kept imperative style (no XAML duplication) but removed indirection to stay readable.
/// NOTE: Class name kept as BillingViewIOS for compatibility even if style analyzer suggests BillingViewIos.
/// </summary>
public static class BillingViewIOS // Keeping existing public name to avoid breaking references.
{
    // --- Theme helpers ----------------------------------------------------
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

    // --- Public entry -----------------------------------------------------
    public static Control Create(MainViewModel mainViewModel)
    {
        if (mainViewModel.BillingViewModel is null)
            return CreatePlaceholderContent();

        var vm = mainViewModel.BillingViewModel;

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Background = GetBackgroundBrush()
        };

        var (headerPanel, monthPicker) = CreateHeader();
        mainPanel.Children.Add(headerPanel);

        // --- create loading panel & keep direct reference to progress text to avoid FindControl before namescope exists
        var (loadingPanel, loadingProgressText) = CreateLoadingPanel();
        var placeholderPanel = CreatePlaceholderContent();
        var billingContentStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 24, Margin = new Thickness(0, 16, 0, 16) };
        RebuildBillingContent(billingContentStack, vm);

        var contentGrid = new Grid();
        contentGrid.Children.Add(loadingPanel);
        contentGrid.Children.Add(placeholderPanel);
        contentGrid.Children.Add(billingContentStack);

        var refreshContainer = new RefreshContainer
        {
            Background = GetBackgroundBrush(),
            Content = new ScrollViewer { Content = contentGrid }
        };
        refreshContainer.RefreshRequested += async (_, _) =>
        {
            if (vm.RefreshBillingCommand.CanExecute(null))
                await vm.RefreshBillingCommand.ExecuteAsync(null);
        };
        mainPanel.Children.Add(refreshContainer);

        // --- Local helpers (tight + inline) --------------------------------
        void UpdateMonthPicker()
        {
            if (monthPicker == null) return;
            var months = vm.AvailableMonths;
            if (months.Count == 0)
            {
                monthPicker.ItemsSource = null;
                monthPicker.IsVisible = false;
                return;
            }
            var monthItems = new System.Collections.Generic.List<string>(months.Count);
            for (int i = 0; i < months.Count; i++)
                monthItems.Add(months[i].ToString("MMMM yyyy"));
            monthPicker.ItemsSource = monthItems;
            monthPicker.SelectedIndex = vm.SelectedMonth.HasValue ? months.IndexOf(vm.SelectedMonth.Value) : 0;
            monthPicker.IsVisible = true;
        }

        void UpdateLoadingProgress()
        {
            var show = vm.LoadingProgress > 0;
            loadingProgressText.Text = show ? vm.LoadingProgress + "%" : string.Empty;
            loadingProgressText.IsVisible = show;
        }

        void UpdateVisibility()
        {
            var hasMonths = vm.AvailableMonths.Count > 0;
            loadingPanel.IsVisible = vm.IsLoading;
            placeholderPanel.IsVisible = !vm.IsLoading && !hasMonths;
            billingContentStack.IsVisible = !vm.IsLoading && hasMonths;
        }

        // Initial state
        UpdateMonthPicker();
        UpdateLoadingProgress();
        UpdateVisibility();

        PropertyChangedEventHandler handler = (_, e) => Dispatcher.UIThread.Post(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(BillingViewModel.AvailableMonths):
                    UpdateMonthPicker();
                    UpdateVisibility();
                    break;
                case nameof(BillingViewModel.LoadingProgress):
                case nameof(BillingViewModel.IsLoading):
                    UpdateLoadingProgress();
                    UpdateVisibility();
                    break;
                case nameof(BillingViewModel.MenuBillingPositions):
                case nameof(BillingViewModel.DrinkBillingPositions):
                case nameof(BillingViewModel.SumCostMenuBillingPositions):
                case nameof(BillingViewModel.SumCostDrinkBillingPositions):
                case nameof(BillingViewModel.SumCostUnknownBillingPositions):
                    RebuildBillingContent(billingContentStack, vm);
                    UpdateVisibility();
                    break;
                default:
                    UpdateVisibility();
                    break;
            }
        });
        vm.PropertyChanged += handler;
        mainPanel.DetachedFromVisualTree += (_, _) => vm.PropertyChanged -= handler;

        if (monthPicker != null)
        {
            monthPicker.SelectionChanged += (_, _) =>
            {
                if (monthPicker.SelectedIndex >= 0 && monthPicker.SelectedIndex < vm.AvailableMonths.Count)
                    vm.SelectedMonth = vm.AvailableMonths[monthPicker.SelectedIndex];
            };
        }

        // Auto-load when credentials exist & no data yet.
        if (vm.AvailableMonths.Count == 0 &&
            !string.IsNullOrEmpty(mainViewModel.VentoPayUsername) &&
            !string.IsNullOrEmpty(mainViewModel.VentoPayPassword) &&
            vm.LoadBillingCommand.CanExecute(null))
        {
            Dispatcher.UIThread.Post(() => _ = vm.LoadBillingCommand.ExecuteAsync(null)); // fire & forget
        }

        return mainPanel;
    }

    // --- Header ------------------------------------------------------------
    private static (Control Header, ComboBox? MonthPicker) CreateHeader()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Background = GetCardBackgroundBrush(),
            Margin = new Thickness(16, 12, 16, 16)
        };

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
            MinWidth = 180,
            FontSize = 17,
            HorizontalAlignment = HorizontalAlignment.Right,
            IsVisible = false
        };
        Grid.SetColumn(monthPicker, 1);
        monthPanel.Children.Add(monthPicker);

        headerPanel.Children.Add(monthPanel);
        return (headerPanel, monthPicker);
    }

    // --- Panels ------------------------------------------------------------
    private static (Control Panel, TextBlock ProgressText) CreateLoadingPanel()
    {
        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 20,
            Margin = new Thickness(20, 60)
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = "Lade Transaktionen...",
            FontSize = 17,
            Foreground = GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        var progress = new TextBlock
        {
            Name = "ProgressText",
            Text = "0%",
            FontSize = 15,
            Foreground = GetSecondaryTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            IsVisible = false
        };
        stackPanel.Children.Add(progress);

        return (stackPanel, progress);
    }

    private static void RebuildBillingContent(StackPanel target, BillingViewModel vm)
    {
        target.Children.Clear();
        target.Children.Add(CreateSummaryCard(vm));

        if (vm.MenuBillingPositions.Count > 0)
            target.Children.Add(CreateBillingSection("Menüs", vm.MenuBillingPositions));
        if (vm.DrinkBillingPositions.Count > 0)
            target.Children.Add(CreateBillingSection("Getränke", vm.DrinkBillingPositions));

        if (vm.MenuBillingPositions.Count == 0 && vm.DrinkBillingPositions.Count == 0)
        {
            target.Children.Add(new TextBlock
            {
                Text = "Keine Transaktionen für diesen Monat",
                FontSize = 17,
                Foreground = GetSecondaryTextBrush(),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(20, 40)
            });
        }
    }

    // --- Cards & Sections --------------------------------------------------
    private static Control CreateSummaryCard(BillingViewModel vm)
    {
        var card = new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(16, 0),
            Padding = new Thickness(16)
        };

        var stack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12 };
        stack.Children.Add(new TextBlock
        {
            Text = "Zusammenfassung",
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            Foreground = GetTextBrush()
        });

        var totalAmount = vm.SumCostMenuBillingPositions + vm.SumCostDrinkBillingPositions + vm.SumCostUnknownBillingPositions;
        stack.Children.Add(new TextBlock
        {
            Text = $"Gesamt: {totalAmount:C}",
            FontSize = 28,
            FontWeight = FontWeight.Bold,
            Foreground = new SolidColorBrush(Color.Parse("#007AFF"))
        });

        if (vm.SumCostMenuBillingPositions > 0)
            stack.Children.Add(new TextBlock { Text = $"Menüs: {vm.SumCostMenuBillingPositions:C}", FontSize = 15, Foreground = GetSecondaryTextBrush() });
        if (vm.SumCostDrinkBillingPositions > 0)
            stack.Children.Add(new TextBlock { Text = $"Getränke: {vm.SumCostDrinkBillingPositions:C}", FontSize = 15, Foreground = GetSecondaryTextBrush() });

        card.Child = stack;
        return card;
    }

    private static Control CreateBillingSection(string title, System.Collections.ObjectModel.ObservableCollection<GroupedBillingPosition> positions)
    {
        var section = new StackPanel { Orientation = Orientation.Vertical, Spacing = 12 };

        section.Children.Add(new TextBlock
        {
            Text = title.ToUpper(),
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            Foreground = GetSecondaryTextBrush(),
            Margin = new Thickness(16, 0)
        });

        var itemsCard = new Border
        {
            Background = GetCardBackgroundBrush(),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(16, 0)
        };

        var itemsStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 0 };
        for (int i = 0; i < positions.Count; i++)
            itemsStack.Children.Add(CreateBillingItem(positions[i], i < positions.Count - 1));

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

        var leftStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 4 };
        leftStack.Children.Add(new TextBlock { Text = position.Description, FontSize = 17, Foreground = GetTextBrush(), TextWrapping = TextWrapping.Wrap });
        leftStack.Children.Add(new TextBlock { Text = position.Quantity + "x", FontSize = 13, Foreground = GetSecondaryTextBrush() });
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

    private static Control CreatePlaceholderContent() => new StackPanel
    {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Spacing = 12,
        Margin = new Thickness(20),
        Children =
        {
            new TextBlock { Text = "💳", FontSize = 48, HorizontalAlignment = HorizontalAlignment.Center },
            new TextBlock { Text = "Keine Abrechnungsdaten verfügbar", FontSize = 16, Foreground = GetTextBrush(), HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center },
            new TextBlock { Text = "Bitte konfigurieren Sie Ihre VentoPay-Anmeldedaten in den Einstellungen.", FontSize = 14, Foreground = GetSecondaryTextBrush(), TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 400 }
        }
    };
}
