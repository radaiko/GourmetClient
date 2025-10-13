using System;
using System.Diagnostics;

namespace GC.Core.Utils;

public static class Log {
  public static bool IsEnabled { get; set; } = false;
  
  public static void Write(string message) {
    if (!IsEnabled) return;
    var callerType = new StackFrame(1).GetMethod().DeclaringType;
    Console.WriteLine($"[Log {callerType.Name}] {message}");
  }
}