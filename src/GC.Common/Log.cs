using System.Runtime.CompilerServices;

namespace GC.Common;

public static class Log {

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
  }
  
  private static string Timestamp => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
}