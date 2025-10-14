using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GC.Cache;
using GC.Core.Network;
using GC.Core.Settings;
using GC.Database;
using GC.ViewModels;
using GC.ViewModels.Services;
using GC.Views.Main;

namespace GC.Views;

public partial class App : Application {
  private IThemeService? _themeService;
  private IServiceProvider? _serviceProvider;

  public override void Initialize() => AvaloniaXamlLoader.Load(this);

  public override void OnFrameworkInitializationCompleted() {
    var services = new ServiceCollection();
    ConfigureServices(services);
    _serviceProvider = services.BuildServiceProvider();
    GC.Core.Services.ServiceProviderHolder.Initialize(_serviceProvider);

    _themeService = GetPlatformThemeService();
    if (_themeService != null) {
      RequestedThemeVariant = _themeService.GetSystemTheme();
      _themeService.ThemeChanged += OnSystemThemeChanged;
      _themeService.StartMonitoring();
    }

    switch (ApplicationLifetime) {
      case IClassicDesktopStyleApplicationLifetime desktop:
        {
          var mainWindow = new MainWindowDesktop();
          if (_serviceProvider != null) {
            var vm = _serviceProvider.GetService<MainViewModel>();
            if (vm != null) mainWindow.DataContext = vm;
          }
          desktop.MainWindow = mainWindow;
        }
        desktop.ShutdownRequested += OnShutdownRequested;
        break;
      case ISingleViewApplicationLifetime lifetime:
        HookSingleViewLifetime(lifetime);
        SetupMobileLifecycleCleanup();
        break;
    }

    base.OnFrameworkInitializationCompleted();
  }

  private void OnSystemThemeChanged(object? sender, ThemeVariant newTheme) {
    if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
      RequestedThemeVariant = newTheme;
    else
      Avalonia.Threading.Dispatcher.UIThread.Post(() => RequestedThemeVariant = newTheme);
  }

  private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e) {
    if (_themeService != null) {
      _themeService.StopMonitoring();
      _themeService.ThemeChanged -= OnSystemThemeChanged;
    }
  }

  protected virtual IThemeService? GetPlatformThemeService() => null;

  private void SetupMobileLifecycleCleanup() { }

  private void HookSingleViewLifetime(ISingleViewApplicationLifetime lifetime) {
    // For single-view (mobile) lifetimes, attempt to show a mobile-optimized main view.
    // Use reflection so the startup won't fail if the mobile view type isn't available
    // to the analyzer at edit-time. Fall back to the shared MainView.
    try {
      Avalonia.Controls.Control? mobileView = null;

      // Try direct type lookup first
      var mobileType = Type.GetType("GC.Views.Main.MainViewMobile");
      if (mobileType == null) {
        // Try scanning loaded assemblies
        mobileType = AppDomain.CurrentDomain.GetAssemblies()
          .Select(a => a.GetType("GC.Views.Main.MainViewMobile"))
          .FirstOrDefault(t => t != null);
      }

      if (mobileType != null && typeof(Avalonia.Controls.Control).IsAssignableFrom(mobileType)) {
        mobileView = Activator.CreateInstance(mobileType) as Avalonia.Controls.Control;
      }

      // Fallback to the shared view when mobile-specific type isn't available
      if (mobileView == null) mobileView = new MainView();

      if (_serviceProvider != null && mobileView != null) {
        var vm = _serviceProvider.GetService<MainViewModel>();
        if (vm != null && mobileView is Avalonia.Controls.Control c)
          c.DataContext = vm;
      }

      lifetime.MainView = mobileView;
    }
    catch {
      // Swallow any exceptions here to avoid crashing platform-specific startup.
    }
  }

  protected virtual void ConfigureServices(IServiceCollection services) {
    services.AddLogging();
    services.AddSingleton<SqliteService>();
    services.AddSingleton<GourmetWebClient>();
    services.AddSingleton<GourmetSettingsService>();
    services.AddSingleton<MenuViewModel>();
    services.AddSingleton<VentopayWebClient>();
    services.AddSingleton<BillingService>();
    services.AddSingleton<BillingViewModel>();
    services.AddSingleton<MainViewModel>();

    services.AddSingleton(typeof(ILogger<MenuViewModel>), sp =>
      sp.GetRequiredService<ILoggerFactory>().CreateLogger<MenuViewModel>());
    services.AddSingleton(typeof(ILogger<BillingViewModel>), sp =>
      sp.GetRequiredService<ILoggerFactory>().CreateLogger<BillingViewModel>());
    services.AddSingleton(typeof(ILogger<MainViewModel>), sp =>
      sp.GetRequiredService<ILoggerFactory>().CreateLogger<MainViewModel>());
  }
}