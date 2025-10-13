using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// Desktop variant of the About view. Uses a centered, scrollable content card.
/// </summary>
public static class AboutViewDesktop {
  private static SolidColorBrush Card() => new(Application.Current?.ActualThemeVariant == ThemeVariant.Dark ? Color.Parse("#2B2B2B") : Colors.White);

  public static Control Create(MainViewModel vm) {
    var contentPanel = AboutViewShared.CreateContentPanel();
    contentPanel.Spacing = 18;
    contentPanel.Margin = new Thickness(30);

    var scroll = new ScrollViewer();
    var wrapper = new Grid();
    scroll.Content = wrapper;

    var centerWrapper = new StackPanel {
      Children = { contentPanel },
      HorizontalAlignment = HorizontalAlignment.Center,
      Width = 760
    };
    wrapper.Children.Add(centerWrapper);

    var card = new Border {
      Background = Card(),
      CornerRadius = new CornerRadius(10),
      Padding = new Thickness(24),
      Child = scroll
    };

    return new Grid {
      Children = { card }
    };
  }
}