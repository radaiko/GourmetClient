using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

public static class MenuView
{
    private static SolidColorBrush GetTextBrush() =>
      new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
        ? Colors.White
        : Colors.Black);

    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        if (state.IsLoading)
        {
            return CreateLoadingView();
        }

        if (state.MenuDays == null || state.MenuDays.Count == 0)
        {
            // Check if Gourmet credentials are configured
            if (state.Settings != null && 
                !string.IsNullOrEmpty(state.Settings.Username) && 
                !string.IsNullOrEmpty(state.Settings.Password))
            {
                // Auto-refresh if credentials are available
                dispatch(new LoadMenus());
                return CreateLoadingView();
            }
            
            return CreateWelcomeView(state, dispatch);
        }

        return CreateMenuGridView(state, dispatch);
    }

    private static Control CreateLoadingView()
    {
        var loadingPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 15
        };

        var spinner = new TextBlock
        {
            Text = "⟳",
            FontSize = 40,
            Foreground = new SolidColorBrush(Color.Parse("#007ACC")),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransformOrigin = RelativePoint.Center
        };

        // Create rotation animation using a simple timer-based approach
        var rotateTransform = new RotateTransform();
        spinner.RenderTransform = rotateTransform;

        // Use a timer for smooth rotation animation
        var timer = new System.Timers.Timer(16); // ~60 FPS
        var angle = 0.0;
        timer.Elapsed += (sender, e) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                angle += 6; // 360 degrees in 1 second (6 degrees per 16ms)
                if (angle >= 360) angle = 0;
                rotateTransform.Angle = angle;
            });
        };
        timer.Start();

        loadingPanel.Children.Add(spinner);

        var loadingText = new TextBlock
        {
            Text = "Lade Menüdaten...",
            FontSize = 16,
            Foreground = GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 0)
        };
        loadingPanel.Children.Add(loadingText);

        return loadingPanel;
    }

    private static Control CreateWelcomeView(AppState state, Action<Msg> dispatch)
    {
        var welcomePanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 32,
            Margin = new Thickness(60)
        };

        var welcomeTitle = new TextBlock
        {
            Text = "Gourmet Client",
            FontSize = 32,
            FontWeight = FontWeight.Light,
            Foreground = GetTextBrush(),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        welcomePanel.Children.Add(welcomeTitle);

        var welcomeMessage = new TextBlock
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
        };
        welcomePanel.Children.Add(welcomeMessage);

        var settingsButton = new Button
        {
            Content = "Einstellungen",
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(32, 12),
            FontSize = 14,
            FontWeight = FontWeight.Normal,
            Background = Brushes.Transparent,
            BorderBrush = GetTextBrush(),
            BorderThickness = new Thickness(1),
            Foreground = GetTextBrush()
        };
        settingsButton.Click += (_, _) => dispatch(new ToggleSettings());
        welcomePanel.Children.Add(settingsButton);

        return welcomePanel;
    }

    private static Control CreateMenuGridView(AppState state, Action<Msg> dispatch)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Padding = new Thickness(40, 20),
            Background = new SolidColorBrush(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
                ? Color.Parse("#0d1117") : Color.Parse("#ffffff"))
        };

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 32
        };

        // Create menu days with minimal cards
        foreach (var menuDay in state.MenuDays!)
        {
            var dayCard = CreateMinimalistMenuDayCard(menuDay, dispatch);
            mainPanel.Children.Add(dayCard);
        }

        scrollViewer.Content = mainPanel;
        return scrollViewer;
    }

    private static Control CreateMinimalistMenuDayCard(GourmetMenuDayViewModel menuDay, Action<Msg> dispatch)
    {
        var dayPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(0, 0, 0, 40)
        };

        // Simple date header
        var dateHeader = new TextBlock
        {
            Text = menuDay.Date.ToString("dddd, dd. MMMM yyyy"),
            FontWeight = FontWeight.Normal,
            FontSize = 18,
            Foreground = GetTextBrush(),
            Margin = new Thickness(0, 0, 0, 16)
        };
        dayPanel.Children.Add(dateHeader);

        // Horizontal menu layout
        var menusPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 24
        };

        // Date spacer to align with header
        var dateSpacer = new Border
        {
            Width = 120,
            Height = 1
        };
        menusPanel.Children.Add(dateSpacer);

        // Menu cards
        foreach (var menu in menuDay.Menus.Take(4))
        {
            if (menu != null)
            {
                var menuCard = CreateMinimalistMenuCard(menu, dispatch);
                menusPanel.Children.Add(menuCard);
            }
            else
            {
                var emptySpace = new Border { Width = 200, Height = 1 };
                menusPanel.Children.Add(emptySpace);
            }
        }

        dayPanel.Children.Add(menusPanel);
        return dayPanel;
    }

    private static Control CreateMinimalistMenuCard(GourmetMenuViewModel menu, Action<Msg> dispatch)
    {
        var cardBorder = new Border
        {
            Background = GetMinimalistBackgroundBrush(menu.MenuState),
            BorderBrush = GetMinimalistBorderBrush(menu.MenuState),
            BorderThickness = new Thickness(0, 0, 0, 2), // Only bottom border for minimal look
            Padding = new Thickness(0, 16, 0, 16),
            Width = 200,
            MinHeight = 100,
            Cursor = new Avalonia.Input.Cursor(menu.MenuState != GourmetMenuState.NotAvailable ?
                Avalonia.Input.StandardCursorType.Hand : Avalonia.Input.StandardCursorType.Arrow)
        };

        var contentStack = new StackPanel
        {
            Spacing = 12
        };

        // Menu description - clean typography
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

        // Status and allergens - minimal bottom info
        var bottomPanel = new DockPanel();

        // Simple status indicator
        var statusText = new TextBlock
        {
            Text = GetMinimalistStatusText(menu.MenuState),
            FontSize = 11,
            FontWeight = FontWeight.Light,
            Foreground = GetMinimalistStatusBrush(menu.MenuState),
            Opacity = 0.8
        };
        DockPanel.SetDock(statusText, Dock.Left);
        bottomPanel.Children.Add(statusText);

        // Allergens - simple text
        if (menu.Allergens != null && menu.Allergens.Length > 0)
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

        // Interaction
        var canInteract = menu.MenuState != GourmetMenuState.NotAvailable &&
                         (menu.MenuState != GourmetMenuState.Ordered || menu.IsOrderCancelable);

        if (!canInteract)
        {
            cardBorder.Opacity = 0.4;
        }
        else
        {
            // Subtle hover effect
            cardBorder.PointerEntered += (s, e) =>
            {
                cardBorder.Background = new SolidColorBrush(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
                    ? Color.Parse("#161b22") : Color.Parse("#f6f8fa"));
            };

            cardBorder.PointerExited += (s, e) =>
            {
                cardBorder.Background = GetMinimalistBackgroundBrush(menu.MenuState);
            };

            cardBorder.PointerPressed += (s, e) => dispatch(new ToggleMenuOrder(menu.MenuDescription));
        }

        // Simple tooltip
        var tooltipText = menu.MenuState switch
        {
            GourmetMenuState.MarkedForOrder => "Entfernen",
            GourmetMenuState.MarkedForCancel => "Behalten",
            GourmetMenuState.Ordered when menu.IsOrderCancelable => "Stornieren",
            GourmetMenuState.Ordered when !menu.IsOrderCancelable => "Bestellt",
            GourmetMenuState.NotAvailable => "Nicht verfügbar",
            _ => "Bestellen"
        };
        ToolTip.SetTip(cardBorder, tooltipText);

        return cardBorder;
    }

    // Minimalist color schemes
    private static SolidColorBrush GetMinimalistBackgroundBrush(GourmetMenuState state)
    {
        var isDark = Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark;

        return state switch
        {
            GourmetMenuState.MarkedForOrder => new SolidColorBrush(isDark
                ? Color.Parse("#0d1117")
                : Color.Parse("#ffffff")),
            GourmetMenuState.MarkedForCancel => new SolidColorBrush(isDark
                ? Color.Parse("#0d1117")
                : Color.Parse("#ffffff")),
            GourmetMenuState.Ordered => new SolidColorBrush(isDark
                ? Color.Parse("#0d1117")
                : Color.Parse("#ffffff")),
            GourmetMenuState.NotAvailable => new SolidColorBrush(isDark
                ? Color.Parse("#0d1117")
                : Color.Parse("#ffffff")),
            _ => new SolidColorBrush(isDark ? Color.Parse("#0d1117") : Color.Parse("#ffffff"))
        };
    }

    private static SolidColorBrush GetMinimalistBorderBrush(GourmetMenuState state)
    {
        return state switch
        {
            GourmetMenuState.MarkedForOrder => new SolidColorBrush(Color.Parse("#28a745")),
            GourmetMenuState.MarkedForCancel => new SolidColorBrush(Color.Parse("#d73a49")),
            GourmetMenuState.Ordered => new SolidColorBrush(Color.Parse("#fb8500")),
            GourmetMenuState.NotAvailable => new SolidColorBrush(Color.Parse("#586069")),
            _ => new SolidColorBrush(Color.Parse("#d0d7de"))
        };
    }

    private static SolidColorBrush GetMinimalistStatusBrush(GourmetMenuState state)
    {
        return state switch
        {
            GourmetMenuState.MarkedForOrder => new SolidColorBrush(Color.Parse("#28a745")),
            GourmetMenuState.MarkedForCancel => new SolidColorBrush(Color.Parse("#d73a49")),
            GourmetMenuState.Ordered => new SolidColorBrush(Color.Parse("#fb8500")),
            GourmetMenuState.NotAvailable => new SolidColorBrush(Color.Parse("#586069")),
            _ => GetTextBrush()
        };
    }

    private static string GetMinimalistStatusText(GourmetMenuState state)
    {
        return state switch
        {
            GourmetMenuState.MarkedForOrder => "Ausgewählt",
            GourmetMenuState.MarkedForCancel => "Stornieren",
            GourmetMenuState.Ordered => "Bestellt",
            GourmetMenuState.NotAvailable => "Nicht verfügbar",
            _ => ""
        };
    }
}