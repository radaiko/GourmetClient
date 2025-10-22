// Event to notify consumers (e.g., ViewModel) about info like progress
namespace GC.Common;

public class InfoEventArgs(InfoType type, string? context = null) : EventArgs {
  public InfoType Type { get; } = type;
  public string? Context { get; } = context;
}

public enum InfoType {
  GourmetApi,
  VentoApi,
  GourmetCache,
  VentoCache,
  SqLite
}