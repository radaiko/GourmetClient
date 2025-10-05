using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using GC.ViewModels.Services;

namespace GC.Views;

public partial class App : Application {
  private IThemeService? _themeService;

  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted() {
    // Initialize theme service from platform-specific implementation
    _themeService = GetPlatformThemeService();
    
    if (_themeService != null) {
      // Set initial theme
      RequestedThemeVariant = _themeService.GetSystemTheme();
      
      // Listen for theme changes
      _themeService.ThemeChanged += OnSystemThemeChanged;
      _themeService.StartMonitoring();
    }

    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
      // Desktop main window
      desktop.MainWindow = new MainWindow();
      
      // Handle application shutdown to clean up theme monitoring
      desktop.ShutdownRequested += OnShutdownRequested;
    }
    else if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime) {
      // Delegate iOS SingleView setup to platform-specific partial implementation
      HookSingleViewLifetime(lifetime);
      
      // Handle iOS app lifecycle for cleanup since iOS doesn't have shutdown events
      SetupMobileLifecycleCleanup();
    }

    base.OnFrameworkInitializationCompleted();
  }

  private void OnSystemThemeChanged(object? sender, ThemeVariant newTheme) {
    // Update the application theme on the main thread
    if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess()) {
      RequestedThemeVariant = newTheme;
    } else {
      Avalonia.Threading.Dispatcher.UIThread.Post(() => RequestedThemeVariant = newTheme);
    }
  }

  private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e) {
    _themeService?.StopMonitoring();
    if (_themeService != null) {
      _themeService.ThemeChanged -= OnSystemThemeChanged;
    }
  }

  // Platform-specific implementations provide the theme service
  protected virtual IThemeService? GetPlatformThemeService() => null;

  private void SetupMobileLifecycleCleanup() {
    // This method handles cleanup for mobile platforms that don't have explicit shutdown events
    // The theme service cleanup will be handled when the app goes into background or terminates
  }

  // Desktop build provides no-op; iOS partial supplies implementation.
  partial void HookSingleViewLifetime(ISingleViewApplicationLifetime lifetime);
}

