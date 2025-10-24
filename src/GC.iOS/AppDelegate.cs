using System.Security.Cryptography;
using System.Text;
using GC.Common;
using GC.Core.Cache;
using GC.iOS.Controllers;

namespace GC.iOS;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate {
  public override UIWindow? Window { get; set; }

  public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions) {
    Base.DeviceKey = GetDevicePassphrase();
    _ = MemCache.Initialize();
    // create a new window instance based on the screen size
    Window = new UIWindow(UIScreen.MainScreen.Bounds);

    Window.RootViewController = new MainTabBarController();

    // make the window visible
    Window.MakeKeyAndVisible();

    return true;
  }
  
  public static string GetDevicePassphrase()
  {
    var uniqueId = UIDevice.CurrentDevice.IdentifierForVendor?.ToString() ?? "unknown";
    using SHA256 sha = SHA256.Create();
    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(uniqueId));
    return Convert.ToBase64String(hash); // 44-char passphrase
  }
}