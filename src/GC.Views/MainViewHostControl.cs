using System.ComponentModel;
using Avalonia.Controls;
using GC.ViewModels;
using GC.ViewModels.Services;
using GourmetClient.Core.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GC.Views;

/// <summary>
/// Main view host for iOS single-view lifetime.
/// </summary>
public class MainViewHostControl : UserControl {
  private readonly MainViewModel _viewModel;

  public MainViewHostControl() {
    // Get services from DI container
    var settingsService = ServiceProviderHolder.Services.GetRequiredService<GourmetSettingsService>();
    var logger = ServiceProviderHolder.Services.GetRequiredService<ILogger<MainViewModel>>();
    
    _viewModel = new MainViewModel(settingsService, logger);
    DataContext = _viewModel;
    
#if IOS
    // Use iOS-specific UI on iOS platform
    // Subscribe to property changes to rebuild the view
    _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    UpdateContent();
#else
    // For desktop/other platforms, use the standard XAML view
    Content = new MainView();
#endif
  }

#if IOS
  private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e) {
    // Rebuild the entire view when properties change
    // This ensures the UI is updated when navigation or other state changes
    if (e.PropertyName == nameof(MainViewModel.CurrentPageIndex) ||
        e.PropertyName == nameof(MainViewModel.ErrorMessage) ||
        e.PropertyName == nameof(MainViewModel.UserName) ||
        e.PropertyName == nameof(MainViewModel.IsSettingsDirty)) {
      UpdateContent();
    }
  }

  private void UpdateContent() {
    Content = MainViewIOS.Create(_viewModel);
  }
#endif
}

