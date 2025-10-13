namespace GC.Core.Settings;

public record UpdateSettings {
  public bool CheckForUpdates { get; set; } = true;
}