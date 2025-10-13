using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using GC.ViewModels;
using GC.Core.Utils;

namespace GC.Views;

/// <summary>
///   iOS-optimized menu view with vertical paged layout
/// </summary>
// ReSharper disable once InconsistentNaming
public static class MenuViewIOS {
  private static SolidColorBrush GetBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#000000")
      : Color.Parse("#ffffff"));

  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

  public static Control Create(MainViewModel viewModel) {
    if (viewModel.MenuViewModel == null) {
      return CreateWelcomeCard();
    }

    var menuViewModel = viewModel.MenuViewModel;

    if (menuViewModel.LoginFailed) {
      return CreateErrorView(menuViewModel.ErrorMessage ?? "Anmeldung fehlgeschlagen. Bitte Einstellungen überprüfen.");
    }

    if (menuViewModel.IsLoading) {
      return CreateLoadingView(menuViewModel.LoadingProgress);
    }

    if (menuViewModel.MenuDays.Count == 0) {
      // Trigger load if not already loaded
      if (!string.IsNullOrEmpty(viewModel.GourmetUsername) && !string.IsNullOrEmpty(viewModel.GourmetPassword)) {
        // Avoid 'async void' lambda - assign the returned Task to a discard so exceptions can be observed by the task scheduler
        Dispatcher.UIThread.Post(() => _ = menuViewModel.LoadMenusCommand.ExecuteAsync(null));
        return CreateLoadingView(menuViewModel.LoadingProgress);
      }
      return CreateWelcomeCard();
    }

    return CreateMobileMenuView(menuViewModel);
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

  private static Control CreateWelcomeCard() =>
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

  private static Control CreateErrorView(string message) =>
    new ScrollViewer {
      Background = GetBackgroundBrush(),
      Content = new StackPanel {
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Spacing = 32,
        Margin = new Thickness(60),
        Children = {
          new TextBlock {
            Text = "Fehler",
            FontSize = 32,
            FontWeight = FontWeight.Light,
            Foreground = GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
          },
          new TextBlock {
            Text = message,
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

  private static Control CreateMobileMenuView(MenuViewModel menuViewModel) {
    var contentControl = new ContentControl();

    void UpdateContent() {
      contentControl.Content = menuViewModel.IsLoading 
        ? CreateLoadingView(menuViewModel.LoadingProgress) 
        : CreateMobileMenuViewInternal(menuViewModel);
    }

    menuViewModel.PropertyChanged += (_, e) => {
      if (e.PropertyName == nameof(MenuViewModel.IsLoading)) {
        UpdateContent();
      }
    };

    UpdateContent();
    return contentControl;
  }

  private static Control CreateMobileMenuViewInternal(MenuViewModel menuViewModel) {
    var today = DateTime.Today;
    var targetIndex = menuViewModel.CurrentMenuDayIndex >= 0 && menuViewModel.CurrentMenuDayIndex < menuViewModel.MenuDays.Count
      ? menuViewModel.CurrentMenuDayIndex
      : menuViewModel.MenuDays.ToList().FindIndex(d => d.Date.Date == today);

    if (targetIndex < 0)
      targetIndex = menuViewModel.MenuDays.ToList().FindIndex(d => d.Date.Date > today);
    if (targetIndex < 0)
      targetIndex = 0;

    // Setup carousel
    Debug.WriteLine($"[MenuViewIOS] Menu has {menuViewModel.MenuDays.Count} days, setting targetIndex to {targetIndex}");
    Log.Write($"Creating carousel with targetIndex={targetIndex}");
    var carousel = new GC.Views.Controls.Carousel();
    foreach (var day in menuViewModel.MenuDays) {
      var dayCard = CreateMenuDayCard(day, menuViewModel, false);
      carousel.Items.Add(dayCard);
    }
    carousel.SetupSwipe();
    
    // Setup indicator dots
    Log.Write($"Creating indicator dots for {carousel.Items.Count} items");
    var indicatorDots = new Controls.IndicatorDots {
      Total = carousel.Items.Count,
      CurrentIndex = targetIndex
    };
    Log.Write($"Indicator dots created with Total={indicatorDots.Total}, CurrentIndex={indicatorDots.CurrentIndex}");
    indicatorDots.DotClicked += (_, index) => {
      Log.Write($"Indicator dot clicked: {index}");
      carousel.GoToIndex(index);
    };
    
    // Setup progress text
    Log.Write($"Creating progress text for item {targetIndex + 1} of {carousel.Items.Count}");
    var progressText = new TextBlock {
      Text = $"{targetIndex + 1} / {carousel.Items.Count}",
      FontSize = 14,
      FontWeight = FontWeight.Normal,
      Foreground = GetTextBrush(),
      VerticalAlignment = VerticalAlignment.Center,
      Opacity = 0.7
    };
    Log.Write($"Progress text created: {progressText.Text}");
    
    // Attach index changed handler to update indicator and progress text
    carousel.OnIndexChanged += (_, currentIndex) => {
      Log.Write($"Carousel index changed to {currentIndex}");
      indicatorDots.CurrentIndex = currentIndex;
      progressText.Text = $"{currentIndex+ 1} / {carousel.Items.Count}";
      menuViewModel.CurrentMenuDayIndex = currentIndex;
    };
    Log.Write($"OnIndexChanged handler attached");
    
    carousel.OnPullDown += (_, _) => {
      Log.Write("Pull-down refresh triggered");
      // Trigger refresh
      if (!menuViewModel.IsLoading) {
        Log.Write("Executing RefreshMenusCommand");
        menuViewModel.RefreshMenusCommand.Execute(null);
      }
    };
    Log.Write($"OnPullDown handler attached");

    // Layout: indicator row on top, scrollViewer below
    var layoutGrid = new Grid {
      RowDefinitions = [
        new RowDefinition(GridLength.Auto),
        new RowDefinition(GridLength.Star)
      ]
    };
    Log.Write($"Layout grid created with {layoutGrid.RowDefinitions.Count} rows");

    var topIndicatorPanel = new StackPanel {
      Orientation = Orientation.Horizontal,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(12, 8, 8, 8),
      Spacing = 6
    };
    topIndicatorPanel.Children.Add(indicatorDots);
    topIndicatorPanel.Children.Add(progressText);
    Log.Write($"Top indicator panel created with {topIndicatorPanel.Children.Count} children");

    layoutGrid.Children.Add(topIndicatorPanel);
    Grid.SetRow(topIndicatorPanel, 0);
    Log.Write($"Top indicator panel added to grid at row 0");

    layoutGrid.Children.Add(carousel);
    Grid.SetRow(carousel, 1);
    Log.Write($"Carousel added to grid at row 1");

    carousel.AttachedToVisualTree += (_, _) => carousel.GoToIndex(targetIndex);

    return layoutGrid;
  }

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
      Background = GetMinimalistBackgroundBrush(),
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

    if (menu.Allergens.Length > 0) {
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
      ColumnDefinitions = [
        new ColumnDefinition(GridLength.Star),
        new ColumnDefinition(GridLength.Auto)
      ]
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
    checkBox.Click += async (_, _) => { await menuViewModel.ToggleMenuOrderCommand.ExecuteAsync(new ToggleMenuOrderParameter(day, menu)); };
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

    // Update UI when menu state changes
    menu.PropertyChanged += (_, e) => {
      if (e.PropertyName == nameof(MenuItemViewModel.State)) {
        checkBox.IsChecked = menu.State == MenuState.Ordered || menu.State == MenuState.MarkedForOrder;
        var newCanInteract = menu.State != MenuState.NotAvailable &&
                             (menu.State != MenuState.Ordered || menu.IsOrderCancelable);
        checkBox.IsEnabled = newCanInteract;
        cardBorder.Opacity = newCanInteract ? 1.0 : 0.4;
        cardBorder.Background = GetMinimalistBackgroundBrush();
        cardBorder.BorderBrush = GetMinimalistBorderBrush(menu.State);
        statusText.Text = GetMinimalistStatusText(menu.State);
        statusText.Foreground = GetMinimalistStatusBrush(menu.State);
        var newTooltip = menu.State switch {
          MenuState.MarkedForOrder => "Entfernen",
          MenuState.MarkedForCancel => "Behalten",
          MenuState.Ordered when menu.IsOrderCancelable => "Stornieren",
          MenuState.Ordered when !menu.IsOrderCancelable => "Bestellt",
          MenuState.NotAvailable => "Nicht verfügbar",
          _ => "Bestellen"
        };
        ToolTip.SetTip(cardBorder, newTooltip);
      }
    };

    return cardBorder;
  }

  private static SolidColorBrush GetMinimalistBackgroundBrush() {
    var isDark = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    return new SolidColorBrush(isDark ? Color.Parse("#000000") : Color.Parse("#ffffff"));
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
