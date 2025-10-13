using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
///   iOS-optimized About view with touch-friendly layout
/// </summary>
public static class AboutViewIOS {
  public static Control Create(MainViewModel viewModel) {
    var contentPanel = AboutViewShared.CreateContentPanel();
    contentPanel.Margin = new Thickness(12);

    var scroll = new ScrollViewer {
      Content = contentPanel,
      HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
    };

    return scroll;
  }
}