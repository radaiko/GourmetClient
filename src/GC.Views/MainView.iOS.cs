using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;
using GC.Views.Utils;

namespace GC.Views;

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

    private static SolidColorBrush GetTextBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Colors.White
            : Colors.Black);

    public static Control Create(MainViewModel viewModel)
    {
        var mainGrid = new Grid
        {
            Background = GetBackgroundBrush(),
            DataContext = viewModel
        };

        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Top bar
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Error display
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Main content
        mainGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Page indicators

        // Gesture support variables
        double swipeStartX = 0;
        bool swipeActive = false;

        mainGrid.PointerPressed += (_, e) =>
        {
            var point = e.GetPosition(mainGrid);
            swipeStartX = point.X;
            swipeActive = true;
        };
        mainGrid.PointerReleased += (_, e) =>
        {
            if (!swipeActive) return;
            swipeActive = false;
            var point = e.GetPosition(mainGrid);
            var deltaX = point.X - swipeStartX;
            const double threshold = 80; // pixels
            if (deltaX <= -threshold && viewModel.CurrentPageIndex < 3)
            {
                viewModel.NavigateToPageCommand.Execute(viewModel.CurrentPageIndex + 1);
            }
            else if (deltaX >= threshold && viewModel.CurrentPageIndex > 0)
            {
                viewModel.NavigateToPageCommand.Execute(viewModel.CurrentPageIndex - 1);
            }
        };

        // Top bar with dynamic title and actions
        var topBar = CreateTopBar(viewModel);
        Grid.SetRow(topBar, 0);
        mainGrid.Children.Add(topBar);

        // Error display
        if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
        {
            var errorPanel = CreateErrorPanel(viewModel);
            Grid.SetRow(errorPanel, 1);
            mainGrid.Children.Add(errorPanel);
        }

        // Main content page based on CurrentPageIndex
        var pageContent = CreatePageContent(viewModel);
        Grid.SetRow(pageContent, 2);
        mainGrid.Children.Add(pageContent);

        // Page indicators
        var indicators = CreatePageIndicators(viewModel);
        Grid.SetRow(indicators, 3);
        mainGrid.Children.Add(indicators);

        return mainGrid;
    }

    private static Border CreateTopBar(MainViewModel viewModel)
    {
        var titles = new[] { "Gourmet", "Rechnung", "Einstellungen", "Über" };
        var title = titles[Math.Clamp(viewModel.CurrentPageIndex, 0, titles.Length - 1)];

        var border = new Border
        {
            Background = GetCardBackgroundBrush(),
            BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
            BorderThickness = new Thickness(0, 0, 0, 0.5)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // title/user
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star)); // spacer
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // action button

        // Title & optional username
        var leftPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 8, 0, 8)
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 20,
            FontWeight = FontWeight.Bold,
            Foreground = GetTextBrush()
        };
        leftPanel.Children.Add(titleText);

        if (!string.IsNullOrEmpty(viewModel.UserName) && viewModel.CurrentPageIndex == 0)
        {
            var userText = new TextBlock
            {
                Text = viewModel.UserName,
                FontSize = 13,
                Foreground = GetSecondaryTextBrush()
            };
            leftPanel.Children.Add(userText);
        }
        Grid.SetColumn(leftPanel, 0);
        grid.Children.Add(leftPanel);

        border.Child = grid;
        return border;
    }

    private static Control CreatePageContent(MainViewModel viewModel)
    {
        // Create content for each page using the iOS-specific views
        var content = viewModel.CurrentPageIndex switch
        {
            0 => MenuViewIOS.Create(viewModel),
            1 => BillingViewIOS.Create(viewModel),
            2 => SettingsViewIOS.Create(viewModel),
            3 => AboutViewIOS.Create(viewModel),
            _ => new TextBlock { Text = "Unbekannte Seite", HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }
        };

        return content;
    }

    private static Control CreatePageIndicators(MainViewModel viewModel)
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
                Width = viewModel.CurrentPageIndex == i ? 18 : 8,
                Height = 8,
                CornerRadius = new CornerRadius(4),
                Background = viewModel.CurrentPageIndex == i
                    ? new SolidColorBrush(Color.Parse("#007AFF"))
                    : GetSecondaryTextBrush(),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
            };
            int pageIndex = i;
            ellipse.PointerPressed += (_, _) => viewModel.NavigateToPageCommand.Execute(pageIndex);
            panel.Children.Add(ellipse);
        }

        return panel;
    }

    private static Panel CreateErrorPanel(MainViewModel viewModel)
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
            Text = viewModel.ErrorMessage,
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
        closeButton.Click += (_, _) => viewModel.ClearErrorCommand.Execute(null);
        Grid.SetColumn(closeButton, 1);
        grid.Children.Add(closeButton);

        border.Child = grid;

        var panel = new StackPanel();
        panel.Children.Add(border);
        return panel;
    }
}
