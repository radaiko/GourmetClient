using System.Runtime.CompilerServices;
using GC.Common;
using GC.Models;

namespace GC.Tests;

internal static class Init {
  [ModuleInitializer]
  public static void Initialize() {
    // Ensure Hub knows we're running under tests
    Base.IsTesting = true;

    // Provide a device key for Crypto so Save/Load can encrypt/decrypt during tests
    // Use a stable test-only passphrase; it's fine for unit tests and kept out of production.
    Base.DeviceKey = "test-device-key";
    
    // Preload settings
    DotNetEnv.Env.TraversePath().Load();
    Settings.It.GourmetUsername = Environment.GetEnvironmentVariable("GourmetUsername");
    Settings.It.GourmetPassword = Environment.GetEnvironmentVariable("GourmetPassword");
    Settings.It.VentoUsername = Environment.GetEnvironmentVariable("VentoUsername");
    Settings.It.VentoPassword = Environment.GetEnvironmentVariable("VentoPassword");
    
  }
}