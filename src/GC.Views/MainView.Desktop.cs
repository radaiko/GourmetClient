using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// Desktop oriented main view builder sharing logic with iOS but using a left side navigation panel
/// and a larger content region.
/// </summary>
public static class MainViewDesktop
{
    private static SolidColorBrush GetBackgroundBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Color.Parse("#1E1E1E")
            : Color.Parse("#F3F3F3"));

    private static SolidColorBrush GetPanelBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Color.Parse("#2B2B2B")
            : Colors.White);

    private static SolidColorBrush GetTextBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Colors.White
            : Colors.Black);

    public static Control Create(MainViewModel viewModel)
    {
        var rootGrid = new Grid
        {
            Background = GetBackgroundBrush(),
            DataContext = viewModel
        };
        rootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // nav
        rootGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star)); // content
        rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // error row
        rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // main row

        // Navigation panel
        var navPanel = CreateNavigationPanel(viewModel);
        Grid.SetColumn(navPanel, 0);
        Grid.SetRow(navPanel, 1);
        rootGrid.Children.Add(navPanel);

        // Error panel (spanning columns)
        if (!string.IsNullOrEmpty(viewModel.ErrorMessage))
        {
            var error = CreateErrorPanel(viewModel);
            Grid.SetColumn(error, 0);
            Grid.SetRow(error, 0);
            Grid.SetColumnSpan(error, 2);
            rootGrid.Children.Add(error);
        }

        // Main content
        var content = CreatePageContent(viewModel);
        Grid.SetColumn(content, 1);
        Grid.SetRow(content, 1);
        rootGrid.Children.Add(content);

        return rootGrid;
    }

    private static Control CreateNavigationPanel(MainViewModel vm)
    {
        var panel = new StackPanel
        {
            Width = 210,
            Background = GetPanelBrush(),
            Spacing = 4,
            Margin = new Thickness(0)
        };

        panel.Children.Add(new TextBlock
        {
            Text = "Gourmet Client",
            FontSize = 20,
            FontWeight = FontWeight.SemiBold,
            Foreground = GetTextBrush(),
            Margin = new Thickness(12, 0, 12, 12)
        });

        // Navigation buttons with uniform horizontal margin
        panel.Children.Add(MakeNavButton("Menüs", 0, vm));
        panel.Children.Add(MakeNavButton("Abrechnung", 1, vm));
        panel.Children.Add(MakeNavButton("Einstellungen", 2, vm));
        panel.Children.Add(MakeNavButton("Über", 3, vm));

        panel.Children.Add(new Separator { Margin = new Thickness(0, 10) });

        var saveBtn = new Button
        {
            Content = "Speichern",
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(12, 8, 12, 0)
        };
        saveBtn.Click += (_, _) => vm.SaveSettingsCommand.Execute(null);
        panel.Children.Add(saveBtn);

        return panel;
    }

    private static Button MakeNavButton(string text, int index, MainViewModel vm)
    {
        var btn = new Button
        {
            Content = text,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Tag = index,
            Padding = new Thickness(10,6),
            Margin = new Thickness(12,2,12,2)
        };
        if (vm.CurrentPageIndex == index)
        {
            btn.FontWeight = FontWeight.Bold;
        }
        btn.Click += (_, _) => vm.NavigateToPageCommand.Execute(index);
        ToolTip.SetTip(btn, text);
        return btn;
    }

    private static Control CreatePageContent(MainViewModel vm)
    {
        return vm.CurrentPageIndex switch
        {
            0 => MenuViewDesktop.Create(vm),
            1 => BillingViewDesktop.Create(vm),
            2 => SettingsViewDesktop.Create(vm),
            3 => AboutViewDesktop.Create(vm),
            _ => new TextBlock { Text = "Unbekannte Seite", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center }
        };
    }

    private static Control CreateErrorPanel(MainViewModel vm)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse("#FFCDD2")),
            BorderBrush = new SolidColorBrush(Color.Parse("#B71C1C")),
            BorderThickness = new Thickness(0,0,0,1),
            Padding = new Thickness(12,6),
        };
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
        grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

        grid.Children.Add(new TextBlock
        {
            Text = vm.ErrorMessage,
            Foreground = new SolidColorBrush(Color.Parse("#B71C1C")),
            TextWrapping = TextWrapping.Wrap
        });

        var close = new Button { Content = "✕", Background = Brushes.Transparent, BorderBrush = Brushes.Transparent, Padding = new Thickness(4,0) };
        close.Click += (_, _) => vm.ClearErrorCommand.Execute(null);
        Grid.SetColumn(close, 1);
        grid.Children.Add(close);
        border.Child = grid;
        return border;
    }
}
