using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace GC.Common;

public sealed class Logger {

  // Public hook for platform-specific analytics/telemetry (e.g. Firebase in GC.iOS).
  // Set this during app startup on the platform project to enable sending events.
  public static Action<string, IReadOnlyDictionary<string, string>>? AnalyticsHandler;
  
  private static readonly Lazy<Logger> Instance = new(() => new Logger());
  public static Logger It => Instance.Value;

  private readonly ObservableCollection<LogMsg> _logs = [];
  
  public static ObservableCollection<LogMsg> Logs => It._logs;

  static Logger() {
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
    var msg = new LogMsg(message, Path.GetFileNameWithoutExtension(path), method,
      level switch
      {
        "INFO" => LogLevel.Info,
        "WARN" => LogLevel.Warning,
        "ERROR" => LogLevel.Error,
        "DEBUG" => LogLevel.Debug,
        _ => LogLevel.Info
      });
    It._logs.Add(msg);
    SendAnalytics(msg);
    WriteToConsole(msg);
  }

  private static void SendAnalytics(LogMsg msg) {
    try {
      var payload = new Dictionary<string, string> {
        ["message"] = msg.Message,
        ["file"] = msg.Class,
        ["method"] = msg.Method,
      };
      
      switch (msg.Level) {
        case LogLevel.Info:
          AnalyticsHandler?.Invoke("info", payload);
          break;
        case LogLevel.Warning:
          AnalyticsHandler?.Invoke("warning", payload);
          break;
        case LogLevel.Error:
          AnalyticsHandler?.Invoke("error", payload);
          break;
        case LogLevel.Debug:
          AnalyticsHandler?.Invoke("debug", payload);
          break;
        default:
          AnalyticsHandler?.Invoke("unknown", payload);
          break;
      }
      
    } catch (Exception ex) {
      // Avoid throwing from logging; fall back to writing to stderr.
      try {
        Write("ERROR", $"Failed to send analytics: {ex}", nameof(Logger), nameof(Error));
      } catch { /* swallow */ }
    }
  }
  
  private static void WriteToConsole(LogMsg msg)
  {
    Console.WriteLine(msg.ToString());
  }
  private static string Timestamp => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
}

public sealed class LogMsg
{
  public string Timestamp { get; }
  public string Message { get; }
  public string Class { get; }
  public string Method { get; }
  public LogLevel Level { get; }
  public LogMsg(string message, string path, string method, LogLevel level)
  {
    Timestamp = GetNow;
    Message = message;
    Class = path;
    Method = method;
    Level = level;
  }

  private static string GetNow => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

  public override string ToString()
  {
    return $"[{Level}] [{Class}:{Method}] [{Timestamp}] {Message}";
  }
}

public enum LogLevel
{
  Info,
  Warning,
  Error,
  Debug
}
