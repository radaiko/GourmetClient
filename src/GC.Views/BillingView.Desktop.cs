using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;
using System.ComponentModel; // added
using Avalonia.Threading; // added for auto load

namespace GC.Views;

/// <summary>
/// Desktop variant of the Billing view. Reuses core logic from iOS implementation but
/// arranges summary and detail sections in a scrollable vertical stack with wider content.
/// </summary>
public static class BillingViewDesktop
{
    private static SolidColorBrush Bg() => new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Color.Parse("#1E1E1E") : Color.Parse("#FAFAFA"));
    private static SolidColorBrush Card() => new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Color.Parse("#2C2C2C") : Colors.White);
    private static SolidColorBrush Txt() => new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Colors.White : Colors.Black);
    private static SolidColorBrush SubTxt() => new(Color.Parse("#6E6E73"));

    public static Control Create(MainViewModel mainVm)
    {
        var vm = mainVm.BillingViewModel;
        if (vm == null)
        {
            return Placeholder("Keine Abrechnungsdaten verfügbar – VentoPay Anmeldedaten in den Einstellungen eintragen.");
        }

        // Auto-trigger initial load like iOS if no months yet and credentials are present
        if (vm.AvailableMonths.Count == 0 && !vm.IsLoading &&
            !string.IsNullOrWhiteSpace(mainVm.VentoPayUsername) &&
            !string.IsNullOrWhiteSpace(mainVm.VentoPayPassword))
        {
            Dispatcher.UIThread.Post(async () => await vm.LoadBillingCommand.ExecuteAsync(null), DispatcherPriority.Background);
        }

        var root = new Grid { Background = Bg() };
        root.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        root.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        // Content area stack reused on updates
        var contentScroll = new ScrollViewer();
        var contentHost = new StackPanel {
          Spacing = 18,
          Margin = new Thickness(18, 16, 18, 30),
          HorizontalAlignment = HorizontalAlignment.Stretch
        };
        contentScroll.Content = contentHost;
        Grid.SetRow(contentScroll, 1);
        root.Children.Add(contentScroll);

        // Month selector (dynamic)
        var topBar = CreateDynamicTopBar(vm, () => UpdateContent(vm, contentHost));
        Grid.SetRow(topBar, 0);
        root.Children.Add(topBar);

        // Initial content population
        UpdateContent(vm, contentHost);

        // Subscribe to VM changes to update UI in-place
        PropertyChangedEventHandler? handler = null;
        handler = (_, e) => {
          if (e.PropertyName == nameof(BillingViewModel.AvailableMonths) ||
              e.PropertyName == nameof(BillingViewModel.SelectedMonth) ||
              e.PropertyName == nameof(BillingViewModel.IsLoading) ||
              e.PropertyName == nameof(BillingViewModel.MenuBillingPositions) ||
              e.PropertyName == nameof(BillingViewModel.DrinkBillingPositions) ||
              e.PropertyName == nameof(BillingViewModel.SumCostDrinkBillingPositions) ||
              e.PropertyName == nameof(BillingViewModel.SumCostMenuBillingPositions) ||
              e.PropertyName == nameof(BillingViewModel.SumCostUnknownBillingPositions)) {
            UpdateContent(vm, contentHost);
            UpdateTopBarMonthSelector(vm, topBar);
          }
        };
        vm.PropertyChanged += handler;

        // Clean up when control detached
        root.DetachedFromVisualTree += (_, _) => { vm.PropertyChanged -= handler; };

        return root;
    }

    // New dynamic top bar with month selector placeholder
    private static Border CreateDynamicTopBar(BillingViewModel vm, Action refreshContent)
    {
        var bar = new Border
        {
            Background = Card(),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.15),
            BorderThickness = new Thickness(0,0,0,1),
            Padding = new Thickness(16,8)
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var title = new TextBlock
        {
            Text = "Abrechnung",
            FontSize = 22,
            FontWeight = FontWeight.SemiBold,
            Foreground = Txt(),
            VerticalAlignment = VerticalAlignment.Center
        };
        grid.Children.Add(title);

        // Month selector combo (initially empty/disabled)
        var monthBox = new ComboBox
        {
            Name = "MonthSelector",
            Width = 200,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            IsEnabled = false
        };
        monthBox.SelectionChanged += (_, _) => {
          if (monthBox.SelectedIndex >= 0 && monthBox.SelectedIndex < vm.AvailableMonths.Count) {
            vm.SelectedMonth = vm.AvailableMonths[monthBox.SelectedIndex];
            refreshContent();
          }
        };
        Grid.SetColumn(monthBox, 2);
        grid.Children.Add(monthBox);

        // Refresh button
        var refreshBtn = new Button {
          Content = "Aktualisieren",
          Margin = new Thickness(12,0,0,0),
          VerticalAlignment = VerticalAlignment.Center
        };
        refreshBtn.Click += async (_, _) => await vm.LoadBillingCommand.ExecuteAsync(null);
        Grid.SetColumn(refreshBtn, 3);
        grid.Children.Add(refreshBtn);

        bar.Child = grid;
        UpdateTopBarMonthSelector(vm, bar);
        return bar;
    }

    // Update month selector items & selected index
    private static void UpdateTopBarMonthSelector(BillingViewModel vm, Border topBar)
    {
        if (topBar.Child is not Grid grid) return;
        var monthBox = grid.Children.OfType<ComboBox>().FirstOrDefault(cb => cb.Name == "MonthSelector");
        if (monthBox == null) return;

        var months = vm.AvailableMonths.ToList();
        monthBox.ItemsSource = months.Select(m => m.ToString("MMMM yyyy")).ToList();
        monthBox.IsEnabled = months.Count > 0 && !vm.IsLoading;

        if (vm.SelectedMonth.HasValue)
        {
            var idx = months.IndexOf(vm.SelectedMonth.Value);
            if (idx >= 0) monthBox.SelectedIndex = idx; else monthBox.SelectedIndex = 0;
        }
        else if (months.Count > 0)
        {
            // Select the current month (list is generated starting with current month first)
            monthBox.SelectedIndex = 0;
            vm.SelectedMonth = months[0];
        }
    }

    // Rebuilds the content stack according to current VM state
    private static void UpdateContent(BillingViewModel vm, StackPanel host)
    {
        host.Children.Clear();

        if (vm.IsLoading)
        {
            host.Children.Add(CenteredMessage(vm.LoadingProgress > 0 ? $"Lade Transaktionen... {vm.LoadingProgress}%" : "Lade Transaktionen..."));
            return;
        }

        if (vm.AvailableMonths.Count == 0)
        {
            host.Children.Add(Placeholder("Noch keine Monate geladen. Anmeldedaten prüfen und aktualisieren."));
            return;
        }

        host.Children.Add(CreateSummaryCard(vm));

        if (vm.MenuBillingPositions.Count > 0)
          host.Children.Add(CreateGroupedSection("Menüs", vm.MenuBillingPositions));
        if (vm.DrinkBillingPositions.Count > 0)
          host.Children.Add(CreateGroupedSection("Getränke", vm.DrinkBillingPositions));

        if (vm.MenuBillingPositions.Count == 0 && vm.DrinkBillingPositions.Count == 0)
          host.Children.Add(CenteredMessage("Keine Transaktionen für den gewählten Monat"));
    }

    private static Control CreateSummaryCard(BillingViewModel vm)
    {
        var total = vm.SumCostMenuBillingPositions + vm.SumCostDrinkBillingPositions + vm.SumCostUnknownBillingPositions;
        var border = new Border { Background = Card(), CornerRadius = new CornerRadius(8), Padding = new Thickness(18), HorizontalAlignment = HorizontalAlignment.Stretch };
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(new TextBlock { Text = "Zusammenfassung", FontSize = 20, FontWeight = FontWeight.SemiBold, Foreground = Txt() });
        stack.Children.Add(new TextBlock { Text = $"Gesamt: {total:C}", FontSize = 28, FontWeight = FontWeight.Bold, Foreground = new SolidColorBrush(Color.Parse("#007AFF")) });
        if (vm.SumCostMenuBillingPositions > 0)
            stack.Children.Add(SubLine($"Menüs: {vm.SumCostMenuBillingPositions:C}"));
        if (vm.SumCostDrinkBillingPositions > 0)
            stack.Children.Add(SubLine($"Getränke: {vm.SumCostDrinkBillingPositions:C}"));
        border.Child = stack;
        return border;
    }

    private static TextBlock SubLine(string text) => new() { Text = text, FontSize = 14, Foreground = SubTxt() };

    private static Control CreateGroupedSection(string title, System.Collections.ObjectModel.ObservableCollection<GroupedBillingPosition> positions)
    {
        var section = new StackPanel { Spacing = 6, HorizontalAlignment = HorizontalAlignment.Stretch };
        section.Children.Add(new TextBlock { Text = title.ToUpper(), FontSize = 12, FontWeight = FontWeight.SemiBold, Foreground = SubTxt(), Margin = new Thickness(4, 12, 4, 0) });
        var card = new Border { Background = Card(), CornerRadius = new CornerRadius(6), HorizontalAlignment = HorizontalAlignment.Stretch };
        var list = new StackPanel { Spacing = 0 };
        for (int i = 0; i < positions.Count; i++)
        {
            var p = positions[i];
            list.Children.Add(CreateBillingRow(p, showDivider: i < positions.Count - 1));
        }
        card.Child = list;
        section.Children.Add(card);
        return section;
    }

    private static Control CreateBillingRow(GroupedBillingPosition p, bool showDivider)
    {
        var border = new Border
        {
            Padding = new Thickness(14, 10),
            BorderBrush = showDivider ? new SolidColorBrush(Color.Parse("#E0E0E0")) : null,
            BorderThickness = showDivider ? new Thickness(0,0,0,1) : new Thickness(0)
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.Children.Add(new StackPanel
        {
            Spacing = 2,
            Children =
            {
                new TextBlock { Text = p.Description, FontSize = 14, FontWeight = FontWeight.Medium, Foreground = Txt(), TextWrapping = TextWrapping.Wrap },
                new TextBlock { Text = $"{p.Quantity}x", FontSize = 11, Foreground = SubTxt() }
            }
        });
        var cost = new TextBlock { Text = p.TotalCost.ToString("C"), FontSize = 14, FontWeight = FontWeight.SemiBold, Foreground = Txt(), VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(cost, 1);
        grid.Children.Add(cost);
        border.Child = grid;
        return border;
    }

    private static Control CenteredMessage(string text)
    {
        return new Border
        {
            Background = Bg(),
            Child = new TextBlock
            {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 16,
                Foreground = Txt(),
                Margin = new Thickness(40),
                TextWrapping = TextWrapping.Wrap
            }
        };
    }

    private static Control Placeholder(string text) => CenteredMessage(text);
}
