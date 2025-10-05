using Avalonia.Controls;
using GC.ViewModels;
using GC.Views.Utils;

namespace GC.Views;

/// <summary>
/// Main view host for iOS single-view lifetime.
/// </summary>
public class MainViewHostControl : UserControl {
  public MainViewHostControl() {
    var viewModel = new MainViewModel();
    DataContext = viewModel;
    
    // Use iOS-specific UI on iOS platform
    if (PlatformDetector.IsIOS) {
      Content = MainViewIOS.Create(viewModel);
    } else {
      // For desktop/other platforms, use the standard XAML view
      Content = new MainView();
    }
  }
}

