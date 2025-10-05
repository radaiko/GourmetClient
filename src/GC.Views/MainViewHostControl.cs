using Avalonia.Controls;
using GC.ViewModels;
using GC.Views.Utils;
using System.ComponentModel;

namespace GC.Views;

/// <summary>
/// Main view host for iOS single-view lifetime.
/// </summary>
public class MainViewHostControl : UserControl {
  private readonly MainViewModel _viewModel;

  public MainViewHostControl() {
    _viewModel = new MainViewModel();
    DataContext = _viewModel;
    
    // Use iOS-specific UI on iOS platform
    if (PlatformDetector.IsIOS) {
      // Subscribe to property changes to rebuild the view
      _viewModel.PropertyChanged += OnViewModelPropertyChanged;
      UpdateContent();
    } else {
      // For desktop/other platforms, use the standard XAML view
      Content = new MainView();
    }
  }

  private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    // Rebuild the entire view when properties change
    // This ensures the UI is updated when navigation or other state changes
    if (e.PropertyName == nameof(MainViewModel.CurrentPageIndex) ||
        e.PropertyName == nameof(MainViewModel.ErrorMessage) ||
        e.PropertyName == nameof(MainViewModel.UserName)) {
      UpdateContent();
    }
  }

  private void UpdateContent() {
    Content = MainViewIOS.Create(_viewModel);
  }
}

