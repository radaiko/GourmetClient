using System;
using System.Linq;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
///   iOS-optimized main view with bottom tab bar navigation (Menu, Billing, Settings) and overlay modals (About, Changelog)
/// </summary>
// ReSharper disable once InconsistentNaming
public static class MainViewMobile {
  private static SolidColorBrush GetBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#000000")
      : Color.Parse("#F2F2F7"));

  private static SolidColorBrush GetCardBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Color.Parse("#000000")
      : Colors.White);

  private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  public static Control Create(MainViewModel viewModel) {
    var rootGrid = new Grid {
      Background = GetBackgroundBrush(),
      DataContext = viewModel
    };

    // Layout rows: TopBar, Error(optional), Content, TabBar
    rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Top bar
    rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Error
    rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // Content
    rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Tab bar

    // Top bar
    var topBar = CreateTopBar(viewModel);
    Grid.SetRow(topBar, 0);
    rootGrid.Children.Add(topBar);

    // Error display
    if (!string.IsNullOrEmpty(viewModel.ErrorMessage)) {
      var errorPanel = CreateErrorPanel(viewModel);
      Grid.SetRow(errorPanel, 1);
      rootGrid.Children.Add(errorPanel);
    }

    // Content (switch by CurrentPageIndex)
    var content = CreateContentForPage(viewModel);
    Grid.SetRow(content, 2);
    rootGrid.Children.Add(content);

    // Bottom tab bar
    var tabBar = CreateTabBar(viewModel);
    Grid.SetRow(tabBar, 3);
    rootGrid.Children.Add(tabBar);

    // Overlays (About / Changelog)
    if (viewModel.ShowAboutOverlay) {
      var aboutOverlay = CreateOverlay(viewModel, AboutViewMobile.Create(viewModel));
      rootGrid.Children.Add(aboutOverlay);
    }
    else if (viewModel.ShowChangelogOverlay) {
      var changelogOverlay = CreateOverlay(viewModel, ChangelogViewMobile.Create(viewModel));
      rootGrid.Children.Add(changelogOverlay);
    }

    return rootGrid;
  }

  private static Control CreateContentForPage(MainViewModel vm) {
    return vm.CurrentPageIndex switch {
      0 => MenuViewMobile.Create(vm),
      1 => BillingViewMobile.Create(vm),
      2 => SettingsViewMobile.Create(vm),
      _ => new TextBlock { Text = "Unbekannte Seite", Foreground = GetTextBrush(), Margin = new Thickness(16) }
    };
  }

  private static Border CreateTopBar(MainViewModel viewModel) {
    var titles = new[] { "Bestellen", "Rechnung", "Einstellungen" };
    var title = titles[Math.Clamp(viewModel.CurrentPageIndex, 0, titles.Length - 1)];

    var border = new Border {
      Background = GetCardBackgroundBrush(),
      BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
      BorderThickness = new Thickness(0, 0, 0, 0.5)
    };

    var grid = new Grid();
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // title/user
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star)); // spacer
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto)); // action button

    // Title & optional username
    var leftPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      VerticalAlignment = VerticalAlignment.Center,
      Margin = new Thickness(10, 8, 0, 8)
    };

    var titleText = new TextBlock {
      Text = title,
      FontSize = 20,
      FontWeight = FontWeight.Bold,
      Foreground = GetTextBrush()
    };
    leftPanel.Children.Add(titleText);

    if (!string.IsNullOrEmpty(viewModel.UserName) && viewModel.CurrentPageIndex == 0) {
      var userText = new TextBlock {
        Text = viewModel.UserName,
        FontSize = 13,
        Foreground = GetSecondaryTextBrush()
      };
      leftPanel.Children.Add(userText);
    }
    Grid.SetColumn(leftPanel, 0);
    grid.Children.Add(leftPanel);

    // TODO: add action button for menuview to save on changes

    border.Child = grid;
    return border;
  }

  private static Button CreateIconButton(string glyph, Color color, Action onClick) {
    var btn = new Button {
      Content = glyph,
      FontSize = 24,
      Width = 44,
      Height = 44,
      Background = Brushes.Transparent,
      BorderBrush = Brushes.Transparent,
      Foreground = new SolidColorBrush(color),
      Margin = new Thickness(0, 0, 4, 0)
    };
    btn.Click += (_, _) => onClick();
    return btn;
  }

  private static Border CreateTabBar(MainViewModel vm) {
    var border = new Border {
      Background = GetCardBackgroundBrush(),
      BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.25)
    };

    var grid = new UniformGrid {
      Rows = 1,
      Columns = 3,
      Height = 60
    };

    // Use base icon names that map to Assets/Icons/{name}-light.svg / {name}-dark.svg
    grid.Children.Add(CreateTabBarItem("menu", 0, vm));
    grid.Children.Add(CreateTabBarItem("billing", 1, vm));
    grid.Children.Add(CreateTabBarItem("settings", 2, vm));

    border.Child = grid;
    return border;
  }

  private static string GetIconUri(string baseName) {
    var isDarkTheme = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;
    var suffix = isDarkTheme ? "dark" : "light";
    return $"avares://GC.Views/Assets/Icons/{baseName}-{suffix}.png";
  }

  private static Control CreatePngIcon(string baseName, bool selected) {
    var uriString = GetIconUri(baseName);
    var uri = new Uri(uriString);

    if (!AssetLoader.Exists(uri)) {
      Console.WriteLine($"[PNG] NOT FOUND: {uriString}");
      return CreateMissingIconPlaceholder(uriString);
    }

    try {
      using var stream = AssetLoader.Open(uri);
      if (stream.Length == 0) {
        Console.WriteLine($"[PNG] EMPTY STREAM: {uriString}");
        return CreateMissingIconPlaceholder(uriString, 'E');
      }

      var image = new Image {
        Source = new Bitmap(stream),
        Width = 26,
        Height = 26,
        Opacity = selected ? 1.0 : 0.85,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
      };
      return image;
    }
    catch (Exception ex) {
      Console.WriteLine($"[PNG] ERROR loading {uriString}: {ex.GetType().Name} {ex.Message}");
      return CreateMissingIconPlaceholder(uriString, '!');
    }
  }

  private static Control CreateMissingIconPlaceholder(string uriString, char symbol = '?') => new Border {
    Width = 26,
    Height = 26,
    Background = new SolidColorBrush(Color.Parse("#FF3B30"), 0.15),
    BorderBrush = new SolidColorBrush(Color.Parse("#FF3B30")),
    BorderThickness = new Thickness(1),
    Child = new TextBlock {
      Text = symbol.ToString(),
      Foreground = new SolidColorBrush(Color.Parse("#FF3B30")),
      FontWeight = FontWeight.Bold,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      FontSize = 16
    }
  };

  private static Button CreateTabBarItem(string baseIconName, int index, MainViewModel vm) {
    var isSelected = vm.CurrentPageIndex == index;

    var stack = new StackPanel {
      Orientation = Orientation.Vertical,
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      Spacing = 2
    };

    stack.Children.Add(CreatePngIcon(baseIconName, isSelected));

    var button = new Button {
      Background = Brushes.Transparent,
      BorderBrush = Brushes.Transparent,
      Content = stack,
      Cursor = new Cursor(StandardCursorType.Hand),
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch,
      HorizontalContentAlignment = HorizontalAlignment.Center,
      VerticalContentAlignment = VerticalAlignment.Center,
      Padding = new Thickness(0, 8, 0, 8)
    };
    button.Click += (_, _) => vm.NavigateToPageCommand.Execute(index);

    Grid.SetColumn(button, index);
    return button;
  }

  private static Grid CreateOverlay(MainViewModel vm, Control innerContent) {
    var overlayGrid = new Grid {
      Background = new SolidColorBrush(Colors.Black, 0.4)
    };

    // Full-screen layout
    overlayGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));

    var card = new Border {
      Background = GetCardBackgroundBrush(),
      CornerRadius = new CornerRadius(16),
      Padding = new Thickness(0, 8, 0, 16),
      Margin = new Thickness(20, 60, 20, 80),
      BorderBrush = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
      BorderThickness = new Thickness(1)
    };

    var layout = new Grid();
    layout.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // header
    layout.RowDefinitions.Add(new RowDefinition(GridLength.Star)); // content

    var header = new Grid { Height = 44 };
    header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
    header.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

    var dragHandle = new Border {
      Width = 40,
      Height = 4,
      CornerRadius = new CornerRadius(2),
      Background = new SolidColorBrush(Color.Parse("#3C3C43"), 0.3),
      HorizontalAlignment = HorizontalAlignment.Center,
      Margin = new Thickness(0, 4, 0, 4)
    };
    Grid.SetColumnSpan(dragHandle, 2);
    header.Children.Add(dragHandle);

    var closeBtn = CreateIconButton("✕", Color.Parse("#FF3B30"), () => vm.CloseOverlayCommand.Execute(null));
    closeBtn.FontSize = 20;
    closeBtn.Width = 40;
    closeBtn.Height = 40;
    closeBtn.Margin = new Thickness(0, 0, 4, 0);
    Grid.SetColumn(closeBtn, 1);
    header.Children.Add(closeBtn);

    Grid.SetRow(header, 0);
    layout.Children.Add(header);

    innerContent.Margin = new Thickness(0, 0, 0, 0);
    Grid.SetRow(innerContent, 1);
    layout.Children.Add(innerContent);

    card.Child = layout;
    overlayGrid.Children.Add(card);

    Grid.SetRow(overlayGrid, 0);
    Grid.SetColumnSpan(overlayGrid, 1);

    return overlayGrid;
  }

  private static StackPanel CreateErrorPanel(MainViewModel viewModel) {
    var border = new Border {
      Background = new SolidColorBrush(Color.Parse("#FF3B30"), 0.15),
      BorderBrush = new SolidColorBrush(Color.Parse("#FF3B30")),
      BorderThickness = new Thickness(0, 0, 0, 2),
      Padding = new Thickness(16, 12),
      Margin = new Thickness(0)
    };

    var grid = new Grid();
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
    grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

    var errorText = new TextBlock {
      Text = viewModel.ErrorMessage,
      FontSize = 15,
      Foreground = new SolidColorBrush(Color.Parse("#FF3B30")),
      TextWrapping = TextWrapping.Wrap,
      VerticalAlignment = VerticalAlignment.Center
    };
    Grid.SetColumn(errorText, 0);
    grid.Children.Add(errorText);

    var closeButton = new Button {
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