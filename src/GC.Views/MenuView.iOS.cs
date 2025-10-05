using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GC.ViewModels;
using System;
using System.Linq;

namespace GC.Views;

/// <summary>
/// iOS-optimized menu view with vertical paged layout
/// </summary>
public static class MenuViewIOS
{
    private static SolidColorBrush GetBackgroundBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#0d1117")
        : Color.Parse("#ffffff"));

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
        if (viewModel.MenuViewModel == null)
        {
            return CreateWelcomeCard(viewModel);
        }

        var menuViewModel = viewModel.MenuViewModel;

        if (menuViewModel.IsLoading)
        {
            return CreateLoadingView(menuViewModel.LoadingProgress);
        }

        if (menuViewModel.MenuDays.Count == 0)
        {
            // Trigger load if not already loaded
            if (!string.IsNullOrEmpty(viewModel.GourmetUsername) && !string.IsNullOrEmpty(viewModel.GourmetPassword))
            {
                Dispatcher.UIThread.Post(async () => await menuViewModel.LoadMenusCommand.ExecuteAsync(null));
                return CreateLoadingView(menuViewModel.LoadingProgress);
            }
            return CreateWelcomeCard(viewModel);
        }

        return CreateMobileMenuView(viewModel, menuViewModel);
    }

    private static Control CreateLoadingView(int progress = 0)
    {
        var stackPanel = new StackPanel
        {
            Background = GetBackgroundBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 20,
            Margin = new Thickness(20, 60),
            Children =
            {
                new TextBlock
                {
                    Text = "Lade Menüdaten...",
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

    private static Control CreateWelcomeCard(MainViewModel viewModel)
    {
        return new ScrollViewer
        {
            Background = GetBackgroundBrush(),
            Content = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = 32,
                Margin = new Thickness(60),
                Children =
                {
                    new TextBlock
                    {
                        Text = "Gourmet Client",
                        FontSize = 32,
                        FontWeight = FontWeight.Light,
                        Foreground = GetTextBrush(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    },
                    new TextBlock
                    {
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
    }

    private static Control CreateMobileMenuView(MainViewModel mainViewModel, MenuViewModel menuViewModel)
    {
        var today = DateTime.Today;
        int targetIndex = menuViewModel.CurrentMenuDayIndex >= 0 && menuViewModel.CurrentMenuDayIndex < menuViewModel.MenuDays.Count
            ? menuViewModel.CurrentMenuDayIndex
            : menuViewModel.MenuDays.ToList().FindIndex(d => d.Date.Date == today);

        if (targetIndex < 0)
            targetIndex = menuViewModel.MenuDays.ToList().FindIndex(d => d.Date.Date > today);
        if (targetIndex < 0)
            targetIndex = 0;

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
            Padding = new Thickness(0),
            Background = GetBackgroundBrush()
        };

        var daysStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 0 };
        foreach (var day in menuViewModel.MenuDays)
        {
            var dayCard = CreateMenuDayCard(day, menuViewModel);
            if (dayCard is Panel p) p.Tag = "DayPanel";
            daysStack.Children.Add(dayCard);
        }
        scrollViewer.Content = daysStack;

        // Indicator UI
        var indicatorDotsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 6
        };
        var total = menuViewModel.MenuDays.Count;
        for (int i = 0; i < total; i++) indicatorDotsPanel.Children.Add(CreateIndicatorDot(false));

        var progressText = new TextBlock
        {
            FontSize = 12,
            Foreground = GetTextBrush(),
            Margin = new Thickness(8, 0, 0, 0)
        };

        var indicatorRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 8, 12, 8)
        };
        indicatorRow.Children.Add(indicatorDotsPanel);
        indicatorRow.Children.Add(progressText);

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        grid.Children.Add(indicatorRow);
        Grid.SetRow(scrollViewer, 1);
        grid.Children.Add(scrollViewer);

        bool initialScrollDone = false;
        int lastPageIndex = targetIndex;

        var snapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
        snapTimer.Tick += (_, __) =>
        {
            snapTimer.Stop();
            var snapped = SnapToPage(scrollViewer);
            if (snapped != lastPageIndex)
            {
                lastPageIndex = snapped;
                menuViewModel.CurrentMenuDayIndex = snapped;
            }
            UpdateIndicators();
        };

        int CurrentPageFromOffset()
        {
            var vp = scrollViewer.Viewport;
            if (vp.Height <= 0) return targetIndex;
            return Math.Clamp((int)Math.Round(scrollViewer.Offset.Y / vp.Height), 0, total - 1);
        }

        void UpdateIndicators()
        {
            int page = CurrentPageFromOffset();
            for (int i = 0; i < indicatorDotsPanel.Children.Count; i++)
            {
                if (indicatorDotsPanel.Children[i] is Ellipse el)
                {
                    bool active = (i == page);
                    el.Fill = active ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush(Color.FromArgb(90, 255, 255, 255));
                    el.Width = el.Height = active ? 10 : 8;
                }
            }
            int remaining = Math.Max(0, total - page - 1);
            progressText.Text = $"Day {page + 1} of {total} ({remaining} left)";
        }

        void UpdateDayPanelHeights(Size viewport)
        {
            if (viewport.Height <= 0) return;
            foreach (var child in daysStack.Children.OfType<Panel>().Where(c => Equals(c.Tag, "DayPanel")))
            {
                child.MinHeight = viewport.Height;
            }
            if (!initialScrollDone)
            {
                if (targetIndex > 0)
                    scrollViewer.Offset = new Vector(0, targetIndex * viewport.Height);
                initialScrollDone = true;
                UpdateIndicators();
                if (menuViewModel.CurrentMenuDayIndex != targetIndex)
                {
                    lastPageIndex = targetIndex;
                    menuViewModel.CurrentMenuDayIndex = targetIndex;
                }
            }
        }

        scrollViewer.PropertyChanged += (_, e) =>
        {
            if (e.Property == ScrollViewer.ViewportProperty && e.NewValue is Size sz)
            {
                UpdateDayPanelHeights(sz);
            }
            else if (e.Property == ScrollViewer.OffsetProperty)
            {
                if (!initialScrollDone) return;
                if (!snapTimer.IsEnabled) snapTimer.Start(); else { snapTimer.Stop(); snapTimer.Start(); }
                UpdateIndicators();
            }
        };

        scrollViewer.AttachedToVisualTree += (_, __) =>
            Dispatcher.UIThread.Post(() => UpdateDayPanelHeights(scrollViewer.Viewport), DispatcherPriority.Background);

        scrollViewer.PointerReleased += (_, __) =>
        {
            if (snapTimer.IsEnabled) snapTimer.Stop();
            var snapped = SnapToPage(scrollViewer);
            if (snapped != lastPageIndex)
            {
                lastPageIndex = snapped;
                menuViewModel.CurrentMenuDayIndex = snapped;
            }
            UpdateIndicators();
        };

        return grid;
    }

    private static Ellipse CreateIndicatorDot(bool active) => new Ellipse
    {
        Width = active ? 10 : 8,
        Height = active ? 10 : 8,
        Fill = active ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
        StrokeThickness = 0
    };

    private static int SnapToPage(ScrollViewer scrollViewer)
    {
        var viewport = scrollViewer.Viewport;
        if (viewport.Height <= 0) return 0;
        double current = scrollViewer.Offset.Y;
        int page = (int)Math.Floor((current + viewport.Height / 2.0) / viewport.Height);
        if (page < 0) page = 0;
        int maxPage = (int)Math.Max(0, Math.Floor((scrollViewer.Extent.Height - viewport.Height) / viewport.Height));
        if (page > maxPage) page = maxPage;
        double targetOffset = page * viewport.Height;
        if (Math.Abs(targetOffset - current) >= 0.5)
        {
            scrollViewer.Offset = new Vector(0, targetOffset);
        }
        return page;
    }

    private static Control CreateMenuDayCard(MenuDayViewModel menuDay, MenuViewModel menuViewModel)
    {
        var dayPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(0)
        };

        var dateHeader = new TextBlock
        {
            Text = menuDay.Date.ToString("dddd, dd. MMMM yyyy"),
            FontWeight = FontWeight.Normal,
            FontSize = 18,
            Foreground = GetTextBrush(),
            Margin = new Thickness(20, 28, 20, 16)
        };
        dayPanel.Children.Add(dateHeader);

        var menusPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(20, 0, 20, 40)
        };

        foreach (var menu in menuDay.Menus.Take(4))
        {
            var menuCard = CreateMenuCard(menuDay.Date, menu, menuViewModel);
            menusPanel.Children.Add(menuCard);
        }

        dayPanel.Children.Add(menusPanel);
        return dayPanel;
    }

    private static Control CreateMenuCard(DateTime day, MenuItemViewModel menu, MenuViewModel menuViewModel)
    {
        var cardBorder = new Border
        {
            Background = GetMinimalistBackgroundBrush(menu.State),
            BorderBrush = GetMinimalistBorderBrush(menu.State),
            BorderThickness = new Thickness(0, 0, 0, 2),
            Padding = new Thickness(0, 16, 0, 16),
            MinHeight = 100,
            Cursor = new Avalonia.Input.Cursor(menu.State != MenuState.NotAvailable ?
                Avalonia.Input.StandardCursorType.Hand : Avalonia.Input.StandardCursorType.Arrow)
        };

        var contentStack = new StackPanel { Spacing = 12 };

        var descriptionText = new TextBlock
        {
            Text = menu.MenuDescription,
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Foreground = GetTextBrush(),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20,
            MaxLines = 4
        };
        contentStack.Children.Add(descriptionText);

        var bottomPanel = new DockPanel();

        var statusText = new TextBlock
        {
            Text = GetMinimalistStatusText(menu.State),
            FontSize = 11,
            FontWeight = FontWeight.Light,
            Foreground = GetMinimalistStatusBrush(menu.State),
            Opacity = 0.8
        };
        DockPanel.SetDock(statusText, Dock.Left);
        bottomPanel.Children.Add(statusText);

        if (menu.Allergens?.Length > 0)
        {
            var allergensText = new TextBlock
            {
                Text = string.Join(" ", menu.Allergens),
                FontSize = 10,
                FontWeight = FontWeight.Light,
                Foreground = GetTextBrush(),
                Opacity = 0.5,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            DockPanel.SetDock(allergensText, Dock.Right);
            bottomPanel.Children.Add(allergensText);
        }

        contentStack.Children.Add(bottomPanel);
        cardBorder.Child = contentStack;

        var canInteract = menu.State != MenuState.NotAvailable &&
                         (menu.State != MenuState.Ordered || menu.IsOrderCancelable);

        if (!canInteract)
        {
            cardBorder.Opacity = 0.4;
        }
        else
        {
            cardBorder.PointerPressed += async (_, _) =>
            {
                await menuViewModel.ToggleMenuOrderCommand.ExecuteAsync(new ToggleMenuOrderParameter(day, menu));
            };
        }

        var tooltipText = menu.State switch
        {
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

    private static SolidColorBrush GetMinimalistBackgroundBrush(MenuState state)
    {
        var isDark = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;
        return new SolidColorBrush(isDark ? Color.Parse("#0d1117") : Color.Parse("#ffffff"));
    }

    private static SolidColorBrush GetMinimalistBorderBrush(MenuState state)
    {
        return state switch
        {
            MenuState.MarkedForOrder => new SolidColorBrush(Color.Parse("#28a745")),
            MenuState.MarkedForCancel => new SolidColorBrush(Color.Parse("#d73a49")),
            MenuState.Ordered => new SolidColorBrush(Color.Parse("#fb8500")),
            MenuState.NotAvailable => new SolidColorBrush(Color.Parse("#586069")),
            _ => new SolidColorBrush(Color.Parse("#d0d7de"))
        };
    }

    private static SolidColorBrush GetMinimalistStatusBrush(MenuState state)
    {
        return state switch
        {
            MenuState.MarkedForOrder => new SolidColorBrush(Color.Parse("#28a745")),
            MenuState.MarkedForCancel => new SolidColorBrush(Color.Parse("#d73a49")),
            MenuState.Ordered => new SolidColorBrush(Color.Parse("#fb8500")),
            MenuState.NotAvailable => new SolidColorBrush(Color.Parse("#586069")),
            _ => GetTextBrush()
        };
    }

    private static string GetMinimalistStatusText(MenuState state)
    {
        return state switch
        {
            MenuState.MarkedForOrder => "Ausgewählt",
            MenuState.MarkedForCancel => "Stornieren",
            MenuState.Ordered => "Bestellt",
            MenuState.NotAvailable => "Nicht verfügbar",
            _ => ""
        };
    }
}
