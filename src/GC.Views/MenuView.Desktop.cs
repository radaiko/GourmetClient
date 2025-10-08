using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GC.ViewModels;
using Avalonia.Controls.Primitives;
using GC.Core.Model; // Added for ScrollBarVisibility

namespace GC.Views;

/// <summary>
/// Desktop variant of the Menu view rendered as a table: Day | Menu1 | Menu2 | Menu3 | Soup & Salad.
/// Clickable cells toggle ordering, with visual state indicators.
/// </summary>
public static class MenuViewDesktop {
  // Palette helpers
  private static bool IsDark() => Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
  private static IBrush HeaderBackground() => IsDark() ? new SolidColorBrush(Color.Parse("#314457")) : new SolidColorBrush(Color.Parse("#E0E6ED"));
  private static IBrush HeaderBorder() => IsDark() ? new SolidColorBrush(Color.Parse("#465869")) : new SolidColorBrush(Color.Parse("#C2CBD3"));
  private static IBrush NeutralBorder() => IsDark() ? new SolidColorBrush(Color.Parse("#4B5563")) : new SolidColorBrush(Color.Parse("#D0D7DE"));
  private static IBrush DefaultCellBackground(int rowIndex) {
    if (!IsDark()) return Brushes.Transparent;
    // Subtle zebra striping in dark mode
    return rowIndex % 2 == 0 ? new SolidColorBrush(Color.Parse("#1F252B")) : new SolidColorBrush(Color.Parse("#252C33"));
  }
  private static IBrush BaseText() => IsDark() ? new SolidColorBrush(Color.Parse("#F1F5F9")) : new SolidColorBrush(Color.Parse("#222222"));
  private static IBrush MutedText() => IsDark() ? new SolidColorBrush(Color.Parse("#94A3B8")) : new SolidColorBrush(Color.Parse("#555555"));
  private static IBrush AccentBlue() => IsDark() ? new SolidColorBrush(Color.Parse("#5FA8FF")) : new SolidColorBrush(Color.Parse("#2F6FAB"));

  public static Control Create(MainViewModel mainVm) {
    var vm = mainVm.MenuViewModel;
    if (vm == null) return CenterMessage("Keine Menüdaten – Gourmet Anmeldedaten in den Einstellungen eintragen.");

    if (vm.IsLoading) return LoadingView(vm.LoadingProgress);

    if (vm.MenuDays.Count == 0) {
      // Auto-trigger load if credentials present
      if (!string.IsNullOrEmpty(mainVm.GourmetUsername) && !string.IsNullOrEmpty(mainVm.GourmetPassword)) {
        Dispatcher.UIThread.Post(async () => await vm.LoadMenusCommand.ExecuteAsync(null));
        return LoadingView(vm.LoadingProgress);
      }
      return CenterMessage("Noch keine Daten geladen. Zugangsdaten prüfen.");
    }

    // Root layout
    var root = new DockPanel { LastChildFill = true };
    var headerBar = BuildHeaderBar(vm);
    DockPanel.SetDock(headerBar, Dock.Top);
    root.Children.Add(headerBar);

    var scroll = new ScrollViewer { HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    root.Children.Add(scroll);

    // Table grid (will be wrapped for centering + max width)
    var grid = new Grid { Margin = new Thickness(8, 12) };
    // Columns: Day + 4 categories
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
    for (int i = 0; i < 4; i++) grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

    int row = 0;
    // Header row
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
    AddHeaderCell(grid, row, 0, "Tag");
    AddHeaderCell(grid, row, 1, "Menü I");
    AddHeaderCell(grid, row, 2, "Menü II");
    AddHeaderCell(grid, row, 3, "Menü III");
    AddHeaderCell(grid, row, 4, "Suppe/Salat");
    row++;

    var categories = new[] { GourmetMenuCategory.Menu1, GourmetMenuCategory.Menu2, GourmetMenuCategory.Menu3, GourmetMenuCategory.SoupAndSalad };

    foreach (var day in vm.MenuDays.OrderBy(d => d.Date)) {
      grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
      int rowIndexForStriping = row - 1; // exclude header

      // Day cell
      var dayText = new TextBlock {
        Text = day.Date.ToString("ddd, dd.MM."),
        FontSize = 14,
        FontWeight = FontWeight.SemiBold,
        Margin = new Thickness(4,4,8,4),
        VerticalAlignment = VerticalAlignment.Top,
        Foreground = BaseText()
      };
      Grid.SetRow(dayText, row);
      Grid.SetColumn(dayText, 0);
      grid.Children.Add(dayText);

      for (int c = 0; c < categories.Length; c++) {
        var category = categories[c];
        var menu = day.Menus.FirstOrDefault(m => m.Category == category);
        var cell = BuildMenuCell(vm, day.Date, menu, rowIndexForStriping);
        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, c + 1);
        grid.Children.Add(cell);
      }
      row++;
    }

    // Wrap table + legend in a vertical stack for proper order inside scroll
    var tableContainer = new Border {
      HorizontalAlignment = HorizontalAlignment.Center,
      MaxWidth = DesktopLayout.WideTableMaxWidth,
      Padding = new Thickness(4,0),
      Child = grid
    };

    var legend = BuildLegend();
    var legendContainer = new Border {
      HorizontalAlignment = HorizontalAlignment.Center,
      MaxWidth = DesktopLayout.WideTableMaxWidth,
      Margin = new Thickness(4,8,4,16),
      Child = legend
    };

    var contentStack = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 4,
      Children = { tableContainer, legendContainer }
    };

    scroll.Content = contentStack;

    return root;
  }

  private static Control BuildHeaderBar(MenuViewModel vm) {
    var bar = new DockPanel {
      Background = AccentBlue(),
      Height = 42,
      DataContext = vm
    };

    var title = new TextBlock {
      Text = "Menü Übersicht",
      Foreground = Brushes.White,
      FontSize = 16,
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(12,0,0,0)
    };
    DockPanel.SetDock(title, Dock.Left);
    bar.Children.Add(title);

    var applyBtn = new Button {
      Content = "✔ Anwenden",
      Margin = new Thickness(8,4,0,4),
      VerticalAlignment = VerticalAlignment.Center,
      Background = new SolidColorBrush(Color.Parse("#34C759")),
      Foreground = Brushes.White,
      Padding = new Thickness(10,2)
    };
    applyBtn.Bind(Button.IsVisibleProperty, new Binding(nameof(MenuViewModel.HasPendingChanges)));
    // Disabled when applying changes
    applyBtn.Bind(Button.IsEnabledProperty, new Binding(nameof(MenuViewModel.IsApplyingChanges)) { Converter = new InverseBoolConverter() });
    applyBtn.Click += async (_, _) => await vm.ApplyOrderChangesCommand.ExecuteAsync(null);
    DockPanel.SetDock(applyBtn, Dock.Right);
    bar.Children.Add(applyBtn);

    var refreshBtn = new Button {
      Content = "⟳",
      Margin = new Thickness(8,4,12,4),
      VerticalAlignment = VerticalAlignment.Center,
      HorizontalAlignment = HorizontalAlignment.Right,
      Width = 40
    };
    // Hide refresh when there are pending changes
    refreshBtn.Bind(Button.IsVisibleProperty, new Binding(nameof(MenuViewModel.HasPendingChanges)) { Converter = new InverseBoolConverter() });
    refreshBtn.Bind(Button.IsEnabledProperty, new Binding(nameof(MenuViewModel.IsApplyingChanges)) { Converter = new InverseBoolConverter() });
    refreshBtn.Click += async (_, _) => await vm.RefreshMenusCommand.ExecuteAsync(null);
    DockPanel.SetDock(refreshBtn, Dock.Right);
    bar.Children.Add(refreshBtn);

    return bar;
  }

  private static void AddHeaderCell(Grid grid, int row, int col, string text) {
    var border = new Border {
      Background = HeaderBackground(),
      BorderBrush = HeaderBorder(),
      BorderThickness = new Thickness(0,0,1,1),
      Padding = new Thickness(8,6),
      Child = new TextBlock { Text = text, FontSize = 13, FontWeight = FontWeight.SemiBold, Foreground = BaseText() }
    };
    Grid.SetRow(border, row); Grid.SetColumn(border, col);
    grid.Children.Add(border);
  }

  private static Control BuildMenuCell(MenuViewModel vm, DateTime day, MenuItemViewModel? menu, int rowIndex) {
    if (menu == null) {
      return new Border {
        BorderBrush = NeutralBorder(),
        BorderThickness = new Thickness(0,0,1,1),
        MinWidth = 160,
        Padding = new Thickness(8,6),
        Background = DefaultCellBackground(rowIndex),
        Child = new TextBlock { Text = "-", FontSize = 12, Opacity = 0.5, HorizontalAlignment = HorizontalAlignment.Center, Foreground = BaseText() }
      };
    }

    var state = menu.State;
    var visuals = StateVisuals(state, menu);

    var desc = new TextBlock {
      Text = menu.MenuDescription,
      FontSize = 12,
      TextWrapping = TextWrapping.Wrap,
      MaxWidth = 180,
      MaxLines = 6,
      Foreground = visuals.accent
    };

    var bottom = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 6 };

    if (menu.Allergens?.Length > 0) {
      bottom.Children.Add(new TextBlock {
        Text = string.Join(" ", menu.Allergens),
        FontSize = 10,
        Foreground = MutedText(),
        Opacity = 0.7
      });
    }

    bottom.Children.Add(new TextBlock {
      Text = StateLabel(state, menu),
      FontSize = 10,
      FontWeight = FontWeight.Medium,
      Foreground = visuals.accent,
      Opacity = 0.95
    });

    var stack = new StackPanel { Spacing = 4 };
    stack.Children.Add(desc);
    stack.Children.Add(bottom);

    var cell = new Border {
      Background = visuals.bg ?? DefaultCellBackground(rowIndex),
      BorderBrush = visuals.border ?? NeutralBorder(),
      BorderThickness = new Thickness(0,0,1,1),
      Padding = new Thickness(6,4),
      MinWidth = 140,
      Child = stack,
      Cursor = menu.IsAvailable ? new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand) : Avalonia.Input.Cursor.Default,
      Opacity = menu.IsAvailable ? 1.0 : 0.5
    };

    ToolTip.SetTip(cell, visuals.tooltip);

    if (menu.IsAvailable && (state != MenuState.Ordered || menu.IsOrderCancelable)) {
      cell.PointerReleased += async (_, _) => {
        await vm.ToggleMenuOrderCommand.ExecuteAsync(new ToggleMenuOrderParameter(day, menu));
      };
    }

    return cell;
  }

  private static (IBrush? bg, IBrush? border, IBrush accent, string tooltip) StateVisuals(MenuState state, MenuItemViewModel menu) {
    // Distinct palettes for dark / light
    if (IsDark()) {
      return state switch {
        MenuState.MarkedForOrder => (new SolidColorBrush(Color.Parse("#153F2B")), new SolidColorBrush(Color.Parse("#2F6F47")), new SolidColorBrush(Color.Parse("#34D058")), "Zur Bestellung entfernen"),
        MenuState.MarkedForCancel => (new SolidColorBrush(Color.Parse("#4A1F23")), new SolidColorBrush(Color.Parse("#B92533")), new SolidColorBrush(Color.Parse("#FF7B85")), "Stornierung zurücknehmen"),
        MenuState.Ordered when menu.IsOrderCancelable => (new SolidColorBrush(Color.Parse("#4A3215")), new SolidColorBrush(Color.Parse("#C96A00")), new SolidColorBrush(Color.Parse("#FFB347")), "Klicken zum Stornieren"),
        MenuState.Ordered when !menu.IsOrderCancelable => (new SolidColorBrush(Color.Parse("#3A3A3A")), new SolidColorBrush(Color.Parse("#B25E00")), new SolidColorBrush(Color.Parse("#FF9F1C")), "Bestellung fixiert"),
        MenuState.NotAvailable => (new SolidColorBrush(Color.Parse("#2E2E2E")), new SolidColorBrush(Color.Parse("#586069")), MutedText(), "Nicht verfügbar"),
        _ => (null, null, BaseText(), "Klicken zum Bestellen")
      };
    }
    // Light theme
    return state switch {
      MenuState.MarkedForOrder => (new SolidColorBrush(Color.Parse("#E9F7EF")), new SolidColorBrush(Color.Parse("#28a745")), new SolidColorBrush(Color.Parse("#1E7E34")), "Zur Bestellung entfernen"),
      MenuState.MarkedForCancel => (new SolidColorBrush(Color.Parse("#FDECEA")), new SolidColorBrush(Color.Parse("#d73a49")), new SolidColorBrush(Color.Parse("#B92533")), "Stornierung zurücknehmen"),
      MenuState.Ordered when menu.IsOrderCancelable => (new SolidColorBrush(Color.Parse("#FFF4E5")), new SolidColorBrush(Color.Parse("#fb8500")), new SolidColorBrush(Color.Parse("#C96A00")), "Klicken zum Stornieren"),
      MenuState.Ordered when !menu.IsOrderCancelable => (new SolidColorBrush(Color.Parse("#F5F5F5")), new SolidColorBrush(Color.Parse("#fb8500")), new SolidColorBrush(Color.Parse("#B25E00")), "Bestellung fixiert"),
      MenuState.NotAvailable => (new SolidColorBrush(Color.Parse("#F2F2F2")), new SolidColorBrush(Color.Parse("#586069")), MutedText(), "Nicht verfügbar"),
      _ => (null, null, new SolidColorBrush(Color.Parse("#333333")), "Klicken zum Bestellen")
    };
  }

  private static string StateLabel(MenuState state, MenuItemViewModel menu) => state switch {
    MenuState.MarkedForOrder => "Ausgewählt",
    MenuState.MarkedForCancel => "Stornieren",
    MenuState.Ordered => menu.IsOrderCancelable ? "Bestellt (stornierbar)" : "Bestellt",
    MenuState.NotAvailable => "Nicht verfügbar",
    _ => string.Empty
  };

  private static Control LoadingView(int progress) => CenterMessage(progress > 0 ? $"Lade Menüs... {progress}%" : "Lade Menüs...");

  private static Control CenterMessage(string text) => new Border {
    Padding = new Thickness(40),
    Child = new TextBlock {
      Text = text,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      FontSize = 16,
      Opacity = 0.85,
      TextWrapping = TextWrapping.Wrap,
      MaxWidth = 420,
      Foreground = BaseText()
    }
  };

  private static Control BuildLegend() {
    StackPanel LegendItem(string label, Color c, IBrush? stroke = null) => new() {
      Orientation = Orientation.Horizontal,
      Spacing = 6,
      Children = {
        new Border { Width = 14, Height = 14, Background = new SolidColorBrush(c), BorderBrush = stroke ?? NeutralBorder(), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(3) },
        new TextBlock { Text = label, FontSize = 11, Foreground = BaseText() }
      }
    };

    var panel = new StackPanel {
      Orientation = Orientation.Horizontal,
      Spacing = 18,
      Margin = new Thickness(16,4,16,8)
    };

    if (IsDark()) {
      panel.Children.Add(LegendItem("Bestellt", Color.Parse("#4A3215")));
      panel.Children.Add(LegendItem("Markiert", Color.Parse("#153F2B")));
      panel.Children.Add(LegendItem("Storno", Color.Parse("#4A1F23")));
      panel.Children.Add(LegendItem("Nicht verfügbar", Color.Parse("#2E2E2E")));
    } else {
      panel.Children.Add(LegendItem("Bestellt", Color.Parse("#FFF4E5")));
      panel.Children.Add(LegendItem("Markiert", Color.Parse("#E9F7EF")));
      panel.Children.Add(LegendItem("Storno", Color.Parse("#FDECEA")));
      panel.Children.Add(LegendItem("Nicht verfügbar", Color.Parse("#F2F2F2")));
    }

    return panel;
  }
}
