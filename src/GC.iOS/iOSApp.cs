using GC.Core.Services;
using GC.ViewModels.Services;
using GC.Views;
using Microsoft.Extensions.DependencyInjection;

namespace GC.iOS;

public class iOSApp : App {
  public iOSApp() { }

  protected override IThemeService? GetPlatformThemeService() {
    return ServiceProviderHolder.Services.GetRequiredService<IThemeService>();
  }
}