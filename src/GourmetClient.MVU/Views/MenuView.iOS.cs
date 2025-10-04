using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Controls.Shapes;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

/// <summary>
/// iOS-optimized menu view with vertical paged layout: one day per screen, scroll down for next day.
/// The initially shown day is today if available; otherwise the next available future day.
/// </summary>
public static class MenuViewIOS
{
    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        if (state.IsLoading)
        {
            return MenuViewShared.CreateLoadingView();
        }

        if (state.MenuDays == null || state.MenuDays.Count == 0)
        {
            if (state.Settings != null && 
                !string.IsNullOrEmpty(state.Settings.Username) && 
                !string.IsNullOrEmpty(state.Settings.Password))
            {
                dispatch(new LoadMenus());
                return MenuViewShared.CreateLoadingView();
            }
            return MenuViewShared.CreateWelcomeView(state, dispatch);
        }
        return CreateMobileMenuView(state, dispatch);
    }

    private static Control CreateMobileMenuView(AppState state, Action<Msg> dispatch)
    {
        var today = DateTime.Today;
        // Determine initial page: prefer persisted CurrentMenuDayIndex if valid.
        int targetIndex;
        if (state.CurrentMenuDayIndex >= 0 && state.MenuDays != null && state.CurrentMenuDayIndex < state.MenuDays.Count)
        {
            targetIndex = state.CurrentMenuDayIndex;
        }
        else
        {
            targetIndex = state.MenuDays!.FindIndex(d => d.Date.Date == today && d.Menus.Any(m => m != null));
            if (targetIndex < 0)
                targetIndex = state.MenuDays.FindIndex(d => d.Date.Date > today && d.Menus.Any(m => m != null));
            if (targetIndex < 0)
                targetIndex = 0;
        }

        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden,
            Padding = new Thickness(0),
            Background = MenuViewShared.GetBackgroundBrush()
        };

        var daysStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 0 };
        for (int i = 0; i < state.MenuDays!.Count; i++)
        {
            var dayCard = CreateMinimalistMenuDayCard(state.MenuDays[i], dispatch);
            if (dayCard is Panel p) p.Tag = "DayPanel";
            daysStack.Children.Add(dayCard);
        }
        scrollViewer.Content = daysStack;

        // Indicator UI
        var indicatorDotsPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Spacing = 6
        };
        var total = state.MenuDays.Count;
        for (int i = 0; i < total; i++) indicatorDotsPanel.Children.Add(CreateIndicatorDot(false));

        var progressText = new TextBlock
        {
            FontSize = 12,
            Foreground = MenuViewShared.GetTextBrush(),
            Margin = new Thickness(8,0,0,0)
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
        int lastPageIndex = targetIndex; // track locally to reduce redundant dispatches

        var snapTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
        snapTimer.Tick += (_, __) =>
        {
            snapTimer.Stop();
            var snapped = SnapToPage(scrollViewer);
            if (snapped != lastPageIndex)
            {
                lastPageIndex = snapped;
                dispatch(new SetCurrentMenuDayIndex(snapped));
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
                    el.Fill = active ? new SolidColorBrush(Color.FromRgb(255,255,255)) : new SolidColorBrush(Color.FromArgb(90,255,255,255));
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
                // Persist chosen initial index if state didn't already have one
                if (state.CurrentMenuDayIndex != targetIndex)
                {
                    lastPageIndex = targetIndex;
                    dispatch(new SetCurrentMenuDayIndex(targetIndex));
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
                dispatch(new SetCurrentMenuDayIndex(snapped));
            }
            UpdateIndicators();
        };

        return grid;
    }

    private static Ellipse CreateIndicatorDot(bool active) => new Ellipse
    {
        Width = active ? 10 : 8,
        Height = active ? 10 : 8,
        Fill = active ? new SolidColorBrush(Color.FromRgb(255,255,255)) : new SolidColorBrush(Color.FromArgb(90,255,255,255)),
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

    private static Control CreateMinimalistMenuDayCard(GourmetMenuDayViewModel menuDay, Action<Msg> dispatch)
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
            Foreground = MenuViewShared.GetTextBrush(),
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
            if (menu != null)
            {
                var menuCard = MenuViewShared.CreateMinimalistMenuCard(menuDay.Date, menu, dispatch, null);
                menusPanel.Children.Add(menuCard);
            }
        }

        dayPanel.Children.Add(menusPanel);
        return dayPanel;
    }
}
