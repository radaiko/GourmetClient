using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Bottom navigation

        // Top bar with title and actions
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

        // Main content
        var mainContent = MenuViewIOS.Create(state, dispatch);
        Grid.SetRow(mainContent, 2);
        mainGrid.Children.Add(mainContent);

        // Bottom navigation bar (iOS-style)
        var bottomNav = CreateBottomNavigation(state, dispatch);
        Grid.SetRow(bottomNav, 3);
        mainGrid.Children.Add(bottomNav);

        // Overlay modals
        if (state.IsAboutVisible)
        {
            var aboutOverlay = CreateMobileOverlay(AboutView.Create(state, dispatch), dispatch, new ToggleAbout(), "Über");
            Grid.SetRowSpan(aboutOverlay, 4);
            mainGrid.Children.Add(aboutOverlay);
        }

        if (state.IsSettingsVisible)
        {
            var settingsOverlay = CreateMobileOverlay(SettingsView.Create(state, dispatch), dispatch, new ToggleSettings(), "Einstellungen");
            Grid.SetRowSpan(settingsOverlay, 4);
            mainGrid.Children.Add(settingsOverlay);
        }

        if (state.IsBillingVisible)
        {
            var billingOverlay = CreateMobileOverlay(BillingView.Create(state, dispatch), dispatch, new ToggleBilling(), "Transaktionen");
            Grid.SetRowSpan(billingOverlay, 4);
            mainGrid.Children.Add(billingOverlay);
        }

        return mainGrid;
    }

    private static Border CreateTopBar(AppState state, Action<Msg> dispatch)
    {
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

        // Left: User info or logo
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center
        };

        var titleText = new TextBlock
        {
            Text = "Gourmet",
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = MainViewShared.GetTextBrush()
        };
        leftPanel.Children.Add(titleText);

        if (!string.IsNullOrEmpty(state.UserName))
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

        // Right: Refresh button
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
        Grid.SetColumn(refreshButton, 2);
        grid.Children.Add(refreshButton);

        border.Child = grid;
        return border;
    }

    private static Control CreateBottomNavigation(AppState state, Action<Msg> dispatch)
    {
        var border = new Border
        {
            Background = GetCardBackgroundBrush(),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0.5, 0, 0)
        };

        var navGrid = new Grid();
        navGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        navGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        navGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        navGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        // Order button
        var (orderCount, cancelCount) = MainViewShared.CountMarkedMenus(state);
        var orderButton = CreateNavButton(
            "☑",
            orderCount > 0 || cancelCount > 0 ? $"Bestellen ({orderCount + cancelCount})" : "Bestellen",
            () => dispatch(new ExecuteOrder()),
            orderCount > 0 || cancelCount > 0
        );
        Grid.SetColumn(orderButton, 0);
        navGrid.Children.Add(orderButton);

        // Billing button
        var billingButton = CreateNavButton(
            "💳",
            "Rechnung",
            () => dispatch(new ToggleBilling()),
            state.IsBillingVisible
        );
        Grid.SetColumn(billingButton, 1);
        navGrid.Children.Add(billingButton);

        // About button
        var aboutButton = CreateNavButton(
            "ℹ",
            "Info",
            () => dispatch(new ToggleAbout()),
            state.IsAboutVisible
        );
        Grid.SetColumn(aboutButton, 2);
        navGrid.Children.Add(aboutButton);

        // Settings button
        var settingsButton = CreateNavButton(
            "⚙",
            "Einst.",
            () => dispatch(new ToggleSettings()),
            state.IsSettingsVisible
        );
        Grid.SetColumn(settingsButton, 3);
        navGrid.Children.Add(settingsButton);

        border.Child = navGrid;
        return border;
    }

    private static Control CreateNavButton(string icon, string label, Action onTap, bool isActive)
    {
        var button = new Button
        {
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Padding = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center
        };

        var stack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 4,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        var iconText = new TextBlock
        {
            Text = icon,
            FontSize = 24,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = isActive 
                ? new SolidColorBrush(Color.Parse("#007AFF"))
                : GetSecondaryTextBrush()
        };
        stack.Children.Add(iconText);

        var labelText = new TextBlock
        {
            Text = label,
            FontSize = 11,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = isActive 
                ? new SolidColorBrush(Color.Parse("#007AFF"))
                : GetSecondaryTextBrush()
        };
        stack.Children.Add(labelText);

        button.Content = stack;
        button.Click += (_, _) => onTap();

        return button;
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

    private static Control CreateMobileOverlay(Control content, Action<Msg> dispatch, Msg closeMessage, string title)
    {
        var overlayGrid = new Grid
        {
            Background = GetBackgroundBrush()
        };

        overlayGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        overlayGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

        // Navigation bar
        var navBar = new Border
        {
            Background = GetCardBackgroundBrush(),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5),
        };

        var navGrid = new Grid();
        navGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));
        navGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        navGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        // Close button (left)
        var closeButton = new Button
        {
            Content = "✕",
            FontSize = 20,
            Width = 44,
            Height = 44,
            Background = Brushes.Transparent,
            BorderBrush = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.Parse("#007AFF"))
        };
        closeButton.Click += (_, _) => dispatch(closeMessage);
        Grid.SetColumn(closeButton, 0);
        navGrid.Children.Add(closeButton);

        // Title (center)
        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 17,
            FontWeight = FontWeight.SemiBold,
            Foreground = MainViewShared.GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(titleText, 1);
        navGrid.Children.Add(titleText);

        navBar.Child = navGrid;
        Grid.SetRow(navBar, 0);
        overlayGrid.Children.Add(navBar);

        // Content area with scroll
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            Background = GetBackgroundBrush()
        };
        scrollViewer.Content = content;
        Grid.SetRow(scrollViewer, 1);
        overlayGrid.Children.Add(scrollViewer);

        return overlayGrid;
    }
}
