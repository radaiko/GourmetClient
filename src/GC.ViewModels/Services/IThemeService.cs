using System;
using Avalonia.Styling;

namespace GC.ViewModels.Services;

public interface IThemeService {
  /// <summary>
  /// Gets the current system theme variant (Light/Dark)
  /// </summary>
  ThemeVariant GetSystemTheme();

  /// <summary>
  /// Event raised when system theme changes
  /// </summary>
  event EventHandler<ThemeVariant>? ThemeChanged;

  /// <summary>
  /// Starts monitoring system theme changes
  /// </summary>
  void StartMonitoring();

  /// <summary>
  /// Stops monitoring system theme changes
  /// </summary>
  void StopMonitoring();
}