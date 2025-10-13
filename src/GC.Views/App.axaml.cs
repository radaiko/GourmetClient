using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using GC.ViewModels.Services;
using Microsoft.Extensions.DependencyInjection;
using GC.Cache;
using GC.Core.Network;
using GC.Core.Settings;
using GC.Database;
using GC.ViewModels;
using Microsoft.Extensions.Logging;

namespace GC.Views;

public partial class App : Application {
  // Force reference to DataGrid assembly so it's not trimmed in AOT
  private static readonly System.Type _forceDataGridType = typeof(Avalonia.Controls.DataGrid);
  // Keep-alive instance to ensure AOT of template/generic code
  private static readonly Avalonia.Controls.DataGrid _keepAliveDataGrid = new() {
    IsVisible = false,
    Width = 0,
    Height = 0
  };

  private IThemeService? _themeService;
  private ServiceProvider? _serviceProvider;

  public override void Initialize() {
    AvaloniaXamlLoader.Load(this);
  }

  public override void OnFrameworkInitializationCompleted() {
    // Setup DI
    var services = new ServiceCollection();
    ConfigureServices(services);
    _serviceProvider = services.BuildServiceProvider();
    GC.ViewModels.Services.ServiceProviderHolder.Initialize(_serviceProvider);

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
    }
    else {
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

  // Virtual method to allow platform-specific overrides for service configuration
  protected virtual void ConfigureServices(ServiceCollection services) {
    services.AddLogging();
    services.AddSingleton<SqliteService>();
    services.AddSingleton<GourmetWebClient>();
    services.AddSingleton<GourmetSettingsService>();
    services.AddSingleton<MenuViewModel>();
    services.AddSingleton<VentopayWebClient>();
    services.AddSingleton<BillingService>();
    services.AddSingleton<BillingViewModel>();
    services.AddSingleton<MainViewModel>();
    // Register logger for MenuViewModel
    services.AddSingleton(typeof(ILogger<MenuViewModel>), sp =>
      sp.GetRequiredService<ILoggerFactory>().CreateLogger<MenuViewModel>());
    // Register logger for BillingViewModel
    services.AddSingleton(typeof(ILogger<BillingViewModel>), sp =>
      sp.GetRequiredService<ILoggerFactory>().CreateLogger<BillingViewModel>());
    // Register logger for MainViewModel
    services.AddSingleton(typeof(ILogger<MainViewModel>), sp =>
      sp.GetRequiredService<ILoggerFactory>().CreateLogger<MainViewModel>());
    // ... add other registrations as needed ...
  }
}