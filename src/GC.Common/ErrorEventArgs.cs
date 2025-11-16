// Event to notify consumers (e.g., ViewModel) about errors
namespace GC.Common;

public class ErrorEventArgs : EventArgs {
  public ErrorEventArgs(ErrorType type, Exception? ex, string? context = null) {
    Type = type;
    Exception = ex;
    Context = context;
  }
  public ErrorEventArgs(ErrorType type, HttpResponseMessage response, string? context = null) {
    var responseString = response.Content.ReadAsStringAsync().Result;
    
    Type = type;
    Exception = new Exception($"HTTP {(int)response.StatusCode} {response.ReasonPhrase}: {responseString}");
    Context = context;
  }
  
  public ErrorEventArgs(ErrorType type, string? context = null) {
    Type = type;
    Exception = null;
    Context = context;
  }
  
  public ErrorType Type { get; }
  public Exception? Exception { get; }
  public string? Context { get; }
}

public enum ErrorType {
  GourmetApi,
  VentoApi,
  GourmetCache,
  VentoCache,
  SqLite,
  ViewModel
}