using System;
using Avalonia.Styling;
using Foundation;
using GC.ViewModels.Services;
using Microsoft.Extensions.Logging;
using UIKit;

namespace GC.iOS.Services;

public class iOSThemeService : IThemeService {
  private NSObject? _themeChangeObserver;
  private ThemeVariant _currentTheme;
  private readonly ILogger<iOSThemeService>? _logger;

  public event EventHandler<ThemeVariant>? ThemeChanged;

  public iOSThemeService(ILogger<iOSThemeService>? logger = null) {
    _logger = logger;
    _currentTheme = GetSystemTheme();
    _logger?.LogInformation("iOSThemeService initialized with theme: {Theme}", _currentTheme);
  }

  public ThemeVariant GetSystemTheme() {
    if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0)) {
      var traitCollection = UIScreen.MainScreen.TraitCollection;
      return traitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark
        ? ThemeVariant.Dark
        : ThemeVariant.Light;
    }

    // iOS versions before 13.0 don't support dark mode
    return ThemeVariant.Light;
  }

  public void StartMonitoring() {
    if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0)) {
      _logger?.LogInformation("Starting native iOS theme monitoring using TraitCollectionDidChange");

      // Use NSNotification to observe trait collection changes
      // This is more efficient than polling
      _themeChangeObserver = NSNotificationCenter.DefaultCenter.AddObserver(
        UIApplication.DidBecomeActiveNotification,
        notification => CheckThemeChange()
      );

      // Also observe when returning from background
      var backgroundObserver = NSNotificationCenter.DefaultCenter.AddObserver(
        UIApplication.WillEnterForegroundNotification,
        notification => CheckThemeChange()
      );
    }
    else {
      _logger?.LogInformation("iOS version < 13.0, dark mode not supported");
    }
  }

  public void StopMonitoring() {
    _logger?.LogInformation("Stopping theme monitoring");

    if (_themeChangeObserver != null) {
      NSNotificationCenter.DefaultCenter.RemoveObserver(_themeChangeObserver);
      _themeChangeObserver = null;
    }
  }

  private void CheckThemeChange() {
    var newTheme = GetSystemTheme();
    if (newTheme != _currentTheme) {
      _logger?.LogInformation("Theme changed from {OldTheme} to {NewTheme}", _currentTheme, newTheme);
      _currentTheme = newTheme;
      ThemeChanged?.Invoke(this, newTheme);
    }
  }
}