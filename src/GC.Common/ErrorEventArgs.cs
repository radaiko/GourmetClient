// Event to notify consumers (e.g., ViewModel) about errors
namespace GC.Common;

public class ErrorEventArgs(ErrorType type, Exception? ex, string? context = null) : EventArgs {
  public ErrorType Type { get; } = type;
  public Exception? Exception { get; } = ex;
  public string? Context { get; } = context;
}

public enum ErrorType {
  GourmetApi,
  VentoApi,
  GourmetCache,
  VentoCache,
  SqLite,
  ViewModel
}