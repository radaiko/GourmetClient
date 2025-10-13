namespace GC.Core.Utils;

/// <summary>
/// Provides access to application-specific file paths
/// </summary>
public interface IFilePathProvider {
  /// <summary>
  /// Gets the local application data directory path
  /// </summary>
  string LocalAppDataPath { get; }
}