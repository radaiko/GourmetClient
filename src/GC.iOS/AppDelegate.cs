using Avalonia;
using Avalonia.iOS;
using Foundation;

namespace GC.iOS;

[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<iOSApp> {
  protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
    return base.CustomizeAppBuilder(builder)
      .WithInterFont()
      .LogToTrace();
  }
}