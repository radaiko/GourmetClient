using GC.Core.Utils;
using GC.iOS.Services;
using GC.ViewModels.Services;
using GC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GC.iOS;

public class iOSApp : App {
  public iOSApp() { }

  protected override void ConfigureServices(ServiceCollection services) {
    base.ConfigureServices(services);
    // Platform-specific services
    services.AddSingleton<IFilePathProvider, FilePathProvider>();
    services.AddSingleton<IThemeService, iOSThemeService>();
  }

  protected override IThemeService? GetPlatformThemeService() {
    return ServiceProviderHolder.Services.GetRequiredService<IThemeService>();
  }
}