using System.Text.Json;

namespace GC.Common;

public static class Base {
  public static JsonSerializerOptions JsonOptions { get; } = new() {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
  };
  
  public static bool IsMobile { get; } = OperatingSystem.IsAndroid() || OperatingSystem.IsIOS();
  
  public static bool IsTesting { get; set; } = false;

  public static string? DeviceKey;

#pragma warning disable CA2211
  public static EventHandler<ErrorEventArgs>? OnError;
#pragma warning restore CA2211
  
#pragma warning disable CA2211
  public static EventHandler<InfoEventArgs>? OnInfo;
#pragma warning restore CA2211

}