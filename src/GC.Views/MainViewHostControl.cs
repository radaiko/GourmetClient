using Avalonia.Controls;
using GC.ViewModels;

namespace GC.Views;

/// <summary>
/// Main view host for iOS single-view lifetime.
/// </summary>
public class MainViewHostControl : UserControl {
  public MainViewHostControl() {
    // For iOS, we use the same content as the desktop window
    DataContext = new MainViewModel();
    Content = new MainView();
  }
}

