using UIKit;
using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;

namespace GC.iOS;

public class Application {
  // This is the main entry point of the application.
  static void Main(string[] args) {
    // Register global managed exception handlers as early as possible so we can capture
    // managed exceptions before they are swallowed or cause an immediate process crash.
    SetupExceptionHandlers();

    try {
      // if you want to use a different Application Delegate class from "AppDelegate"
      // you can specify it here.
      UIApplication.Main(args, null, typeof(AppDelegate));
    }
    catch (Exception ex) {
      LogException(ex, "Exception in UIApplication.Main");
      // rethrow to let the runtime handle termination after logging
      throw;
    }
  }

  static void SetupExceptionHandlers() {
    AppDomain.CurrentDomain.UnhandledException += (sender, e) => {
      Exception ex = e.ExceptionObject as Exception ?? new Exception("Non-Exception object thrown");
      LogException(ex, "UnhandledException");
    };

    TaskScheduler.UnobservedTaskException += (sender, e) => {
      LogException(e.Exception, "UnobservedTaskException");
      e.SetObserved();
    };
  }

  static void LogException(Exception? ex, string context) {
    try {
      string msg = $"[{DateTime.UtcNow:O}] {context}: {ex?.GetType().FullName}: {ex?.Message}\n{ex?.StackTrace}\n";
      // Console output (visible in macOS Console when attached to simulator/device)
      Console.WriteLine(msg);
    }
    catch {
      // Swallow all - we can't do more if logging itself fails
    }
  }
}
