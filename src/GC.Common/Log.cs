using System.Runtime.CompilerServices;

namespace GC.Common;

public static class Log {

  // Public hook for platform-specific analytics/telemetry (e.g. Firebase in GC.iOS).
  // Set this during app startup on the platform project to enable sending events.
  public static Action<string, IReadOnlyDictionary<string, string>>? AnalyticsHandler;

  static Log() {
    Base.OnError += (sender, args) => {
      Error($"ErrorEvent: Type={args.Type} Context={args.Context} Exception={args.Exception}");
    };
    
    Base.OnInfo += (sender, args) => {
      Info($"InfoEvent: Type={args.Type} Context={args.Context}");
    };
  }
  
  public static void Info(string message, [CallerFilePath] string path = "Unknown", [CallerMemberName] string method = "Unknown") {
    Write("INFO", message, path, method);
  }

  public static void Warn(string message, [CallerFilePath] string path = "Unknown", [CallerMemberName] string method = "Unknown") {
    Write("WARN", message, path, method);
  }

  public static void Error(string message, [CallerFilePath] string path = "Unknown", [CallerMemberName] string method = "Unknown") {
    Write("ERROR", message, path, method);
  }

  public static void Debug(string message, [CallerFilePath] string path = "Unknown", [CallerMemberName] string method = "Unknown") {
    // if (Base.Settings is not { DebugMode: true }) return;
    Write("DEBUG", message, path, method);
  }
  
  private static void Write(string level, string message, string path, string method) {
    // Write to stderr to avoid interfering with tools that expect structured stdout (like xUnit JSON)
    Console.Error.WriteLine($"[{level}] [{Path.GetFileNameWithoutExtension(path)}:{method}] [{Timestamp}] {message}");
    
    try {
      var payload = new Dictionary<string, string> {
        ["message"] = message,
        ["file"] = Path.GetFileNameWithoutExtension(path),
        ["method"] = method,
      };
      
      switch (level) {
        case "INFO":
          AnalyticsHandler?.Invoke("info", payload);
          break;
        case "WARN":
          AnalyticsHandler?.Invoke("warning", payload);
          break;
        case "ERROR":
          AnalyticsHandler?.Invoke("error", payload);
          break;
        case "DEBUG":
          AnalyticsHandler?.Invoke("debug", payload);
          break;
        default:
          AnalyticsHandler?.Invoke("unknown", payload);
          break;
      }
      
    } catch (Exception ex) {
      // Avoid throwing from logging; fall back to writing to stderr.
      try {
        Write("ERROR", $"Failed to send analytics: {ex}", nameof(Log), nameof(Error));
      } catch { /* swallow */ }
    }
  }
  
  private static string Timestamp => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
}