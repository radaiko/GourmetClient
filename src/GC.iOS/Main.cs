using System;
using System.Security.Cryptography;
using System.Text;
using GC.Common;
using UIKit;

namespace GC.iOS;

public class Program {
  // This is the main entry point of the application.
  static void Main(string[] args) {
    // if you want to use a different Application Delegate class from "AppDelegate"
    // you can specify it here.
    Base.DeviceKey = GetDevicePassphrase();
    UIApplication.Main(args, null, typeof(AppDelegate));
  }
  
  static string GetDevicePassphrase()
  {
    var uniqueId = UIDevice.CurrentDevice.IdentifierForVendor?.ToString() ?? "unknown";
    using SHA256 sha = SHA256.Create();
    var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(uniqueId));
    return Convert.ToBase64String(hash); // 44-char passphrase
  }
}