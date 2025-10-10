using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
///   iOS-optimized menu view with vertical paged layout
/// </summary>
public static class MenuViewIOS {
  private static SolidColorBrush GetBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#0d1117")
      : Color.Parse("#ffffff"));

  private static SolidColorBrush GetCardBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#1C1C1E")
      : Colors.White);

  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

  public static Control Create(MainViewModel viewModel) {
    if (viewModel.MenuViewModel == null) {
      return CreateWelcomeCard(viewModel);
    }

    var menuViewModel = viewModel.MenuViewModel;

    if (menuViewModel.IsLoading) {
      return CreateLoadingView(menuViewModel.LoadingProgress);
    }

    if (menuViewModel.MenuDays.Count == 0) {
      // Trigger load if not already loaded
      if (!string.IsNullOrEmpty(viewModel.GourmetUsername) && !string.IsNullOrEmpty(viewModel.GourmetPassword)) {
        Dispatcher.UIThread.Post(async () => await menuViewModel.LoadMenusCommand.ExecuteAsync(null));
        return CreateLoadingView(menuViewModel.LoadingProgress);
      }
      return CreateWelcomeCard(viewModel);
    }

    return CreateMobileMenuView(viewModel, menuViewModel);
  }

  private static Control CreateLoadingView(int progress = 0) {
    var stackPanel = new StackPanel {
      Background = GetBackgroundBrush(),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Spacing = 20,
      Margin = new Thickness(20, 60),
      Children = {
        new TextBlock {
          Text = "Lade Menüdaten...",
          FontSize = 17,
          Foreground = GetTextBrush(),
          HorizontalAlignment = HorizontalAlignment.Center
        }
      }
    };

    // Show progress percentage if loading has started (progress > 0)
    if (progress > 0) {
      stackPanel.Children.Add(new TextBlock {
        Text = $"{progress}%",
        FontSize = 15,
        Foreground = GetSecondaryTextBrush(),
        HorizontalAlignment = HorizontalAlignment.Center
      });
    }

    return stackPanel;
  }

  private static Control CreateWelcomeCard(MainViewModel viewModel) =>
    new ScrollViewer {
      Background = GetBackgroundBrush(),
      Content = new StackPanel {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Spacing = 32,
        Margin = new Thickness(60),
        Children = {
          new TextBlock {
            Text = "Gourmet Client",
            FontSize = 32,
            FontWeight = FontWeight.Light,
            Foreground = GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
          },
          new TextBlock {
            Text = "Anmeldedaten in den Einstellungen konfigurieren",
            FontSize = 16,
            FontWeight = FontWeight.Normal,
            Foreground = GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400,
            LineHeight = 24,
            Opacity = 0.7
          }
        }
      }
    };

  private static Control CreateMobileMenuView(MainViewModel mainViewModel, MenuViewModel menuViewModel) {
    var today = DateTime.Today;
    var targetIndex = menuViewModel.CurrentMenuDayIndex >= 0 && menuViewModel.CurrentMenuDayIndex < menuViewModel.MenuDays.Count
      ? menuViewModel.CurrentMenuDayIndex
      : menuViewModel.MenuDays.ToList().FindIndex(d => d.Date.Date == today);

    if (targetIndex < 0)
      targetIndex = menuViewModel.MenuDays.ToList().FindIndex(d => d.Date.Date > today);
    if (targetIndex < 0)
      targetIndex = 0;

    var carousel = new Carousel {
      Background = GetBackgroundBrush(),
      PageTransition = new PageSlide(TimeSpan.FromMilliseconds(300), PageSlide.SlideAxis.Vertical),
      IsHitTestVisible = true
    };
    
    // Prevent multiple page changes from a single continuous swipe
    var isGestureHandled = false;
    
    // Swipe to change page
    carousel.GestureRecognizers.Add(new ScrollGestureRecognizer { CanHorizontallyScroll = false });
    carousel.AddHandler(Gestures.ScrollGestureEvent, (s, e) => {
      // If a page change was already triggered recently, ignore further scroll events until reset
      if (isGestureHandled) return;

      const double requiredDelta = 10;
      if (e.Delta.Y < -requiredDelta) {
        isGestureHandled = true;
        carousel.Previous();
      } else if (e.Delta.Y > requiredDelta) {
        isGestureHandled = true;
        carousel.Next();
      } else {
        return;
      }

      // Reset the guard after the transition duration + small buffer to avoid multiple advances
      var resetTimer = new System.Timers.Timer(350) { AutoReset = false };
      resetTimer.Elapsed += (_, __) => {
        Dispatcher.UIThread.Post(() => { isGestureHandled = false; });
        resetTimer.Dispose();
      };
      resetTimer.Start();
    });
    
    PopulateCarousel();

    carousel.SelectedIndex = targetIndex;

    // Indicator UI
    var indicatorDotsPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Left,
      VerticalAlignment = VerticalAlignment.Center,
      Spacing = 6
    };
    var total = menuViewModel.MenuDays.Count;
    for (var i = 0; i < total; i++) indicatorDotsPanel.Children.Add(CreateIndicatorDot(false));

    var progressText = new TextBlock {
      FontSize = 12,
      Foreground = GetTextBrush(),
      Margin = new Thickness(8, 0, 0, 0)
    };

    var indicatorRow = new StackPanel {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Left,
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(12, 8, 12, 8)
    };
    indicatorRow.Children.Add(indicatorDotsPanel);
    indicatorRow.Children.Add(progressText);

    var grid = new Grid();
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
    grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
    grid.Children.Add(indicatorRow);
    Grid.SetRow(carousel, 1);
    grid.Children.Add(carousel);

    carousel.SelectionChanged += (_, __) => {
      menuViewModel.CurrentMenuDayIndex = carousel.SelectedIndex;
      UpdateIndicators();
    };

    carousel.AttachedToVisualTree += (_, __) => UpdateIndicators();

    return grid;

    // Helpers
    void UpdateIndicators() {
      var page = carousel.SelectedIndex;
      for (var i = 0; i < indicatorDotsPanel.Children.Count; i++) {
        if (indicatorDotsPanel.Children[i] is Ellipse el) {
          var active = i == page;
          el.Fill = active ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush(Color.FromArgb(90, 255, 255, 255));
          el.Width = el.Height = active ? 10 : 8;
        }
      }
      var remaining = Math.Max(0, total - page - 1);
      progressText.Text = $"Day {page + 1} of {total} ({remaining} left)";
    }

    void PopulateCarousel() {
      carousel.Items.Clear();
      foreach (var day in menuViewModel.MenuDays) {
        var dayCard = CreateMenuDayCard(day, menuViewModel, false);
        carousel.Items.Add(dayCard);
      }
    }
  }

  private static Ellipse CreateIndicatorDot(bool active) => new() {
    Width = active ? 10 : 8,
    Height = active ? 10 : 8,
    Fill = active ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
    StrokeThickness = 0
  };

  private static Control CreateMenuDayCard(MenuDayViewModel menuDay, MenuViewModel menuViewModel, bool showReserved) {
    var dayPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 16,
      Margin = new Thickness(0)
    };

    var dateHeader = new TextBlock {
      Text = menuDay.Date.ToString("dddd, dd. MMMM yyyy"),
      FontWeight = FontWeight.Normal,
      FontSize = 18,
      Foreground = GetTextBrush(),
      Margin = new Thickness(20, 28, 20, 16)
    };
    dayPanel.Children.Add(dateHeader);

    var menusPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Spacing = 16,
      Margin = new Thickness(20, 0, 20, 40)
    };

    foreach (var menu in menuDay.Menus.Where(m => !showReserved || m.State == MenuState.Ordered).Take(4)) {
      var menuCard = CreateMenuCard(menuDay.Date, menu, menuViewModel);
      menusPanel.Children.Add(menuCard);
    }

    dayPanel.Children.Add(menusPanel);
    return dayPanel;
  }

  private static Control CreateMenuCard(DateTime day, MenuItemViewModel menu, MenuViewModel menuViewModel) {
    var canInteract = menu.State != MenuState.NotAvailable &&
                      (menu.State != MenuState.Ordered || menu.IsOrderCancelable);

    var cardBorder = new Border {
      Background = GetMinimalistBackgroundBrush(menu.State),
      BorderBrush = GetMinimalistBorderBrush(menu.State),
      BorderThickness = new Thickness(0, 0, 0, 2),
      Padding = new Thickness(0, 20, 0, 20),
      MinHeight = 120,
      Cursor = new Cursor(menu.State != MenuState.NotAvailable ? StandardCursorType.Hand : StandardCursorType.Arrow)
    };

    var descriptionText = new TextBlock {
      Text = menu.MenuDescription,
      FontSize = 14,
      FontWeight = FontWeight.Normal,
      Foreground = GetTextBrush(),
      TextWrapping = TextWrapping.Wrap,
      LineHeight = 20,
      MaxLines = 4
    };

    // Left column: description, allergens, status. Right column: checkbox centered vertically.
    var leftStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 6 };
    leftStack.Children.Add(descriptionText);

    if (menu.Allergens?.Length > 0) {
      var allergensText = new TextBlock {
        Text = string.Join(" ", menu.Allergens),
        FontSize = 10,
        FontWeight = FontWeight.Light,
        Foreground = GetTextBrush(),
        Opacity = 0.5
      };
      leftStack.Children.Add(allergensText);
    }

    var statusText = new TextBlock {
      Text = GetMinimalistStatusText(menu.State),
      FontSize = 11,
      FontWeight = FontWeight.Light,
      Foreground = GetMinimalistStatusBrush(menu.State),
      Opacity = 0.8
    };
    leftStack.Children.Add(statusText);

    var mainGrid = new Grid {
      ColumnDefinitions = new ColumnDefinitions {
        new ColumnDefinition(GridLength.Star),
        new ColumnDefinition(GridLength.Auto)
      }
    };

    mainGrid.Children.Add(leftStack);
    Grid.SetColumn(leftStack, 0);

    var checkBox = new CheckBox {
      IsChecked = menu.State == MenuState.Ordered || menu.State == MenuState.MarkedForOrder,
      IsEnabled = canInteract,
      Margin = new Thickness(10, 0, 0, 0),
      VerticalAlignment = VerticalAlignment.Top,
      HorizontalAlignment = HorizontalAlignment.Right
    };
    checkBox.Click += async (s, e) => { await menuViewModel.ToggleMenuOrderCommand.ExecuteAsync(new ToggleMenuOrderParameter(day, menu)); };
    mainGrid.Children.Add(checkBox);
    Grid.SetColumn(checkBox, 1);

    cardBorder.Child = mainGrid;


    if (!canInteract) {
      cardBorder.Opacity = 0.4;
    }

    var tooltipText = menu.State switch {
      MenuState.MarkedForOrder => "Entfernen",
      MenuState.MarkedForCancel => "Behalten",
      MenuState.Ordered when menu.IsOrderCancelable => "Stornieren",
      MenuState.Ordered when !menu.IsOrderCancelable => "Bestellt",
      MenuState.NotAvailable => "Nicht verfügbar",
      _ => "Bestellen"
    };
    ToolTip.SetTip(cardBorder, tooltipText);

    return cardBorder;
  }

  private static SolidColorBrush GetMinimalistBackgroundBrush(MenuState state) {
    var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    return new SolidColorBrush(isDark ? Color.Parse("#0d1117") : Color.Parse("#ffffff"));
  }

  private static SolidColorBrush GetMinimalistBorderBrush(MenuState state) {
    return state switch {
      MenuState.MarkedForOrder => new SolidColorBrush(Color.Parse("#28a745")),
      MenuState.MarkedForCancel => new SolidColorBrush(Color.Parse("#d73a49")),
      MenuState.Ordered => new SolidColorBrush(Color.Parse("#fb8500")),
      MenuState.NotAvailable => new SolidColorBrush(Color.Parse("#586069")),
      _ => new SolidColorBrush(Color.Parse("#d0d7de"))
    };
  }

  private static SolidColorBrush GetMinimalistStatusBrush(MenuState state) {
    return state switch {
      MenuState.MarkedForOrder => new SolidColorBrush(Color.Parse("#28a745")),
      MenuState.MarkedForCancel => new SolidColorBrush(Color.Parse("#d73a49")),
      MenuState.Ordered => new SolidColorBrush(Color.Parse("#fb8500")),
      MenuState.NotAvailable => new SolidColorBrush(Color.Parse("#586069")),
      _ => GetTextBrush()
    };
  }

  private static string GetMinimalistStatusText(MenuState state) {
    return state switch {
      MenuState.MarkedForOrder => "Ausgewählt",
      MenuState.MarkedForCancel => "Stornieren",
      MenuState.Ordered => "Bestellt",
      MenuState.NotAvailable => "Nicht verfügbar",
      _ => ""
    };
  }
}