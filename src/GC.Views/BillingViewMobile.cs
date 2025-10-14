using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GC.ViewModels;
using System.ComponentModel;

namespace GC.Views;

/// <summary>
/// iOS billing view: simple display of error message if present.
/// </summary>
public static class BillingViewMobile {
  private static SolidColorBrush GetBackgroundBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Color.Parse("#000000")
      : Color.Parse("#F2F2F7"));

  private static SolidColorBrush GetTextBrush() =>
    new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
      ? Colors.White
      : Colors.Black);

  private static SolidColorBrush GetSecondaryTextBrush() => new(Color.Parse("#8E8E93"));

  public static Control Create(MainViewModel mainViewModel) {
    if (mainViewModel.BillingViewModel is null) {
      return CreatePlaceholderContent();
    }

    var vm = mainViewModel.BillingViewModel;

    var mainPanel = new StackPanel {
      Orientation = Orientation.Vertical,
      Background = GetBackgroundBrush(),
      HorizontalAlignment = HorizontalAlignment.Stretch,
      VerticalAlignment = VerticalAlignment.Stretch
    };

    var errorTextBlock = new TextBlock {
      FontSize = 16,
      Foreground = GetTextBrush(),
      HorizontalAlignment = HorizontalAlignment.Center,
      VerticalAlignment = VerticalAlignment.Center,
      TextAlignment = TextAlignment.Center,
      TextWrapping = TextWrapping.Wrap,
      MaxWidth = 400,
      Text = vm.ErrorMessage ?? "Keine Abrechnungsdaten verfügbar"
    };

    mainPanel.Children.Add(errorTextBlock);

    PropertyChangedEventHandler? handler = null;
    handler = (_, e) => {
      if (e.PropertyName == nameof(BillingViewModel.ErrorMessage)) {
        errorTextBlock.Text = vm.ErrorMessage ?? "Keine Abrechnungsdaten verfügbar";
      }
    };
    vm.PropertyChanged += handler;

    mainPanel.DetachedFromVisualTree += (_, _) => { vm.PropertyChanged -= handler; };

    return mainPanel;
  }

  private static Control CreatePlaceholderContent() => new StackPanel {
    HorizontalAlignment = HorizontalAlignment.Center,
    VerticalAlignment = VerticalAlignment.Center,
    Spacing = 12,
    Margin = new Thickness(20),
    Children = {
      new TextBlock { Text = "💳", FontSize = 48, HorizontalAlignment = HorizontalAlignment.Center },
      new TextBlock { Text = "Keine Abrechnungsdaten verfügbar", FontSize = 16, Foreground = GetTextBrush(), HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center },
      new TextBlock { Text = "Bitte konfigurieren Sie Ihre VentoPay-Anmeldedaten in den Einstellungen.", FontSize = 14, Foreground = GetSecondaryTextBrush(), TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, MaxWidth = 300 }
    }
  };
}