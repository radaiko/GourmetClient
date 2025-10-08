using System.ComponentModel;
using Avalonia.Controls;
using GC.ViewModels;
using GC.ViewModels.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GC.Views;

/// <summary>
/// Main view host for iOS single-view lifetime and Desktop dynamic layout.
/// </summary>
public class MainViewHostControl : UserControl {
  private readonly MainViewModel _viewModel;

  public MainViewHostControl() {
    // Get MainViewModel from DI container (which includes MenuViewModel and BillingViewModel)
    _viewModel = ServiceProviderHolder.Services.GetRequiredService<MainViewModel>();
    DataContext = _viewModel;
    
#if IOS
    // Use iOS-specific UI on iOS platform
    // Subscribe to property changes to rebuild the view
    _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    UpdateContentIOS();
#else
    // Desktop: use desktop-specific dynamic builder for shared navigation-based UI
    _viewModel.PropertyChanged += OnViewModelPropertyChangedDesktop;
    UpdateContentDesktop();
#endif
  }

#if IOS
  private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    // Rebuild the entire view when properties change
    // This ensures the UI is updated when navigation or other state changes
    if (e.PropertyName == nameof(MainViewModel.CurrentPageIndex) ||
        e.PropertyName == nameof(MainViewModel.ErrorMessage) ||
        e.PropertyName == nameof(MainViewModel.UserName) ||
        e.PropertyName == nameof(MainViewModel.MenuViewModel) ||
        e.PropertyName == nameof(MainViewModel.BillingViewModel) ||
        e.PropertyName == nameof(MainViewModel.ShowAboutOverlay) ||
        e.PropertyName == nameof(MainViewModel.ShowChangelogOverlay)) {
      UpdateContentIOS();
    }
  }

  private void UpdateContentIOS() {
    Content = MainViewIOS.Create(_viewModel);
  }
#else
  private void OnViewModelPropertyChangedDesktop(object? sender, PropertyChangedEventArgs e) {
    if (e.PropertyName == nameof(MainViewModel.CurrentPageIndex) ||
        e.PropertyName == nameof(MainViewModel.ErrorMessage) ||
        e.PropertyName == nameof(MainViewModel.UserName) ||
        e.PropertyName == nameof(MainViewModel.MenuViewModel) ||
        e.PropertyName == nameof(MainViewModel.BillingViewModel)) {
      UpdateContentDesktop();
    }
  }

  private void UpdateContentDesktop() {
    Content = MainViewDesktop.Create(_viewModel);
  }
#endif
}
