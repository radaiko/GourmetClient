using System.Security.Cryptography;
using System.Text;
using Firebase.Analytics;
using GC.Common;
using GC.Core.Cache;
using GC.iOS.Controllers;
using Plugin.Firebase.Core.Platforms.iOS;

namespace GC.iOS;

/// <summary>
///   The main application delegate for the iOS app.
///   Handles app lifecycle events and initial setup.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate {
    /// <summary>
    ///   The main window of the application.
    /// </summary>
    public override UIWindow? Window { get; set; }

    /// <summary>
    ///   Called when the application has finished launching.
    ///   Sets up the initial UI and performs any necessary initialization.
    /// </summary>
    /// <param name="application">The UIApplication instance.</param>
    /// <param name="launchOptions">Launch options dictionary.</param>
    /// <returns>True if the app launched successfully.</returns>
    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions) {
    // Setup Firebase for analytics and crash reporting
    CrossFirebase.Initialize();
    Log.AnalyticsHandler = (eventName, payload) => {
      try {
        // Sanitize payload: Firebase Analytics accepts simple values and string length max 100.
        var dict = new Dictionary<object, object>();
        if (payload != null) {
          foreach (var kv in payload) {
            var key = kv.Key?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(key))
              continue;

            object? value = kv.Value;
            if (value == null) {
              continue;
            }

            // Allow numeric and boolean types directly
            if (value is sbyte || value is byte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong || value is float || value is double || value is decimal || value is bool) {
              dict[key] = value!;
              continue;
            }

            // Otherwise stringify and truncate to 100 characters
            var s = value.ToString() ?? string.Empty;
            if (s.Length > 100) s = s.Substring(0, 100);
            dict[key] = s;
          }
        }

        // Call the SDK overload that accepts Dictionary<object,object>
        Analytics.LogEvent(eventName, dict);
      }
      catch {
        // swallow errors from analytics logging to avoid crashing the app
      }
    };

    // Generate a unique passphrase based on the device ID for encryption/security
    Base.DeviceKey = GetDevicePassphrase();

    // Initialize the memory cache for storing data
    _ = MemCache.Initialize();

    // Create the main window with the size of the device's screen
    Window = new UIWindow(UIScreen.MainScreen.Bounds);

    // Set the root view controller to the main tab bar controller
    Window.RootViewController = new MainTabBarController();

    // Make the window visible and set it as the key window
    Window.MakeKeyAndVisible();

    return true;
  }

    /// <summary>
    ///   Generates a unique passphrase for the device based on its vendor identifier.
    ///   This is used for encryption and security purposes.
    /// </summary>
    /// <returns>A base64-encoded SHA256 hash of the device ID.</returns>
    public static string GetDevicePassphrase() {
    // Get the device's unique identifier
    var uniqueId = UIDevice.CurrentDevice.IdentifierForVendor?.ToString() ?? "unknown";

    // Create a SHA256 hash of the identifier
    using var sha = SHA256.Create();
    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(uniqueId));

    // Convert to base64 string for use as a passphrase
    return Convert.ToBase64String(hash); // Results in a 44-character passphrase
  }
}