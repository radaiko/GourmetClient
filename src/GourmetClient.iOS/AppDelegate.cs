using Foundation;
using UIKit;
using Avalonia;
using Avalonia.iOS;
using GourmetClient.MVU; // Access App class
using System.Globalization;

namespace GourmetClient.iOS; // Updated namespace to match project

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<App>
{
  protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
    // Force Austrian German culture globally (currency = €)
    var culture = new CultureInfo("de-AT");
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
    // Also set current thread now (defensive)
    CultureInfo.CurrentCulture = culture;
    CultureInfo.CurrentUICulture = culture;

    return base.CustomizeAppBuilder(builder)
      .WithInterFont();
  }
}