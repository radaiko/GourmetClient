using System;
using Avalonia.Styling;
using Foundation;
using GC.ViewModels.Services;
using UIKit;

namespace GC.iOS.Services;

public class iOSThemeService : IThemeService
{
    private NSTimer? _themeCheckTimer;
    private ThemeVariant _currentTheme;
    
    public event EventHandler<ThemeVariant>? ThemeChanged;

    public iOSThemeService()
    {
        _currentTheme = GetSystemTheme();
    }

    public ThemeVariant GetSystemTheme()
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
        {
            var traitCollection = UIScreen.MainScreen.TraitCollection;
            return traitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark 
                ? ThemeVariant.Dark 
                : ThemeVariant.Light;
        }
        
        // iOS versions before 13.0 don't support dark mode
        return ThemeVariant.Light;
    }

    public void StartMonitoring()
    {
        if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
        {
            // Use a timer to poll for theme changes every 2 seconds
            _themeCheckTimer = NSTimer.CreateRepeatingScheduledTimer(2.0, CheckThemeChange);
        }
    }

    public void StopMonitoring()
    {
        _themeCheckTimer?.Invalidate();
        _themeCheckTimer = null;
    }

    private void CheckThemeChange(NSTimer timer)
    {
        var newTheme = GetSystemTheme();
        if (newTheme != _currentTheme)
        {
            _currentTheme = newTheme;
            ThemeChanged?.Invoke(this, newTheme);
        }
    }
}
