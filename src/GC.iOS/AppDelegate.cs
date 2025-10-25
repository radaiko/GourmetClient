using System.Security.Cryptography;
using System.Text;
using GC.Common;
using GC.Core.Cache;
using GC.iOS.Controllers;

namespace GC.iOS;

/// <summary>
/// The main application delegate for the iOS app.
/// Handles app lifecycle events and initial setup.
/// </summary>
[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    /// <summary>
    /// The main window of the application.
    /// </summary>
    public override UIWindow? Window { get; set; }

    /// <summary>
    /// Called when the application has finished launching.
    /// Sets up the initial UI and performs any necessary initialization.
    /// </summary>
    /// <param name="application">The UIApplication instance.</param>
    /// <param name="launchOptions">Launch options dictionary.</param>
    /// <returns>True if the app launched successfully.</returns>
    public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions)
    {
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
    /// Generates a unique passphrase for the device based on its vendor identifier.
    /// This is used for encryption and security purposes.
    /// </summary>
    /// <returns>A base64-encoded SHA256 hash of the device ID.</returns>
    public static string GetDevicePassphrase()
    {
        // Get the device's unique identifier
        var uniqueId = UIDevice.CurrentDevice.IdentifierForVendor?.ToString() ?? "unknown";

        // Create a SHA256 hash of the identifier
        using SHA256 sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(uniqueId));

        // Convert to base64 string for use as a passphrase
        return Convert.ToBase64String(hash); // Results in a 44-character passphrase
    }
}