using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

/// <summary>
/// iOS-optimized main view with bottom navigation and mobile-friendly layout
/// </summary>
public static class MainViewIOS
{
    private static SolidColorBrush GetBackgroundBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#000000")
        : Color.Parse("#F2F2F7"));

    private static SolidColorBrush GetCardBackgroundBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Color.Parse("#1C1C1E")
        : Colors.White);

    private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        var mainGrid = new Grid
        {
            Background = GetBackgroundBrush()
        };

        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Top bar
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Error display
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Main content
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Page indicators

        // Gesture support variables
        double swipeStartX = 0;
        bool swipeActive = false;

        mainGrid.PointerPressed += (s, e) =>
        {
            var point = e.GetPosition(mainGrid);
            swipeStartX = point.X;
            swipeActive = true;
        };
        mainGrid.PointerReleased += (s, e) =>
        {
            if (!swipeActive) return;
            swipeActive = false;
            var point = e.GetPosition(mainGrid);
            var deltaX = point.X - swipeStartX;
            const double threshold = 80; // pixels
            if (deltaX <= -threshold && state.CurrentPageIndex < 3)
            {
                dispatch(new NavigateToPage(state.CurrentPageIndex + 1));
            }
            else if (deltaX >= threshold && state.CurrentPageIndex > 0)
            {
                dispatch(new NavigateToPage(state.CurrentPageIndex - 1));
            }
        };

        // Top bar with dynamic title and actions
        var topBar = CreateTopBar(state, dispatch);
        Grid.SetRow(topBar, 0);
        mainGrid.Children.Add(topBar);

        // Error display
        if (!string.IsNullOrEmpty(state.ErrorMessage))
        {
            var errorPanel = CreateErrorPanel(state, dispatch);
            Grid.SetRow(errorPanel, 1);
            mainGrid.Children.Add(errorPanel);
        }

        // Main content page based on CurrentPageIndex
        var pageContent = CreatePageContent(state, dispatch);
        Grid.SetRow(pageContent, 2);
        mainGrid.Children.Add(pageContent);

        // Page indicators
        var indicators = CreatePageIndicators(state, dispatch);
        Grid.SetRow(indicators, 3);
        mainGrid.Children.Add(indicators);

        return mainGrid;
    }

    private static Border CreateTopBar(AppState state, Action<Msg> dispatch)
    {
        var titles = new[] { "Gourmet", "Rechnung", "Einstellungen", "Über" };
        var title = titles[Math.Clamp(state.CurrentPageIndex, 0, titles.Length - 1)];

        var border = new Border
        {
            Background = GetCardBackgroundBrush(),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        // Left: App/User info
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = MainViewShared.GetTextBrush()
        };
        leftPanel.Children.Add(titleText);

        if (!string.IsNullOrEmpty(state.UserName) && state.CurrentPageIndex == 0)
        {
            var userText = new TextBlock
            {
                Text = state.UserName,
                FontSize = 13,
                Foreground = GetSecondaryTextBrush()
            };
            leftPanel.Children.Add(userText);
        }

        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        // Middle: (spacer)
        // Right buttons: Order (only on menu page when there are marked menus) and Refresh
        var (orderCount, cancelCount) = MainViewShared.CountMarkedMenus(state);
        if (state.CurrentPageIndex == 0 && (orderCount > 0 || cancelCount > 0))
        {
            var orderButton = new Button
            {
                Content = orderCount + cancelCount > 0 ? $"✓ {orderCount + cancelCount}" : "☑",
                FontSize = 16,
                Padding = new Thickness(12, 6),
                Background = new SolidColorBrush(Color.Parse("#007AFF")),
                Foreground = Brushes.White,
                BorderBrush = Brushes.Transparent,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 4, 8, 4)
            };
            orderButton.Click += (_, _) => dispatch(new ExecuteOrder());
            Grid.SetColumn(orderButton, 2);
            grid.Children.Add(orderButton);
        }

        var refreshButton = new Button
        {
            Content = "⟳",
            FontSize = 24,
            Width = 44,
            Height = 44,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.Parse("#007AFF")),
            VerticalAlignment = VerticalAlignment.Center
        };
        refreshButton.Click += (_, _) => dispatch(new LoadMenus());
        Grid.SetColumn(refreshButton, 3);
        grid.Children.Add(refreshButton);

        border.Child = grid;
        return border;
    }

    private static Control CreatePageContent(AppState state, Action<Msg> dispatch)
    {
        return state.CurrentPageIndex switch
        {
            0 => MenuViewIOS.Create(state, dispatch),
            1 => BillingView.Create(state, dispatch),
            2 => SettingsView.Create(state, dispatch),
            3 => AboutView.Create(state, dispatch),
            _ => new TextBlock { Text = "Unbekannte Seite", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
        };
    }

    private static Control CreatePageIndicators(AppState state, Action<Msg> dispatch)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Spacing = 8,
            Margin = new Thickness(0, 8, 0, 12)
        };

        for (int i = 0; i < 4; i++)
        {
            var ellipse = new Border
            {
                Width = state.CurrentPageIndex == i ? 18 : 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = state.CurrentPageIndex == i
                    ? new SolidColorBrush(Color.Parse("#007AFF"))
                    : GetSecondaryTextBrush(),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
            int pageIndex = i;
            ellipse.PointerPressed += (_, _) => dispatch(new NavigateToPage(pageIndex));
            panel.Children.Add(ellipse);
        }

        return panel;
    }

    private static Panel CreateErrorPanel(AppState state, Action<Msg> dispatch)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FF3B30"), 0.15),
            BorderBrush = new SolidColorBrush(Color.Parse("#FF3B30")),
            BorderThickness = new Thickness(0, 0, 0, 2),
            Padding = new Thickness(16, 12),
            Margin = new Thickness(0)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        var errorText = new TextBlock
        {
            Text = state.ErrorMessage,
            FontSize = 15,
            Foreground = new SolidColorBrush(Color.Parse("#FF3B30")),
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(errorText, 0);
        grid.Children.Add(errorText);

        var closeButton = new Button
        {
            Content = "✕",
            FontSize = 18,
            Width = 32,
            Height = 32,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.Parse("#FF3B30"))
        };
        closeButton.Click += (_, _) => dispatch(new ClearError());
        Grid.SetColumn(closeButton, 1);
        grid.Children.Add(closeButton);

        border.Child = grid;

        var panel = new StackPanel();
        panel.Children.Add(border);
        return panel;
    }
}
