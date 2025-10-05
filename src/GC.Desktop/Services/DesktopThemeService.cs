using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using Avalonia.Styling;
using GC.ViewModels.Services;
using Microsoft.Extensions.Logging;

namespace GC.Desktop.Services;

public class DesktopThemeService : IThemeService
{
    private Timer? _themeCheckTimer;
    private ThemeVariant _currentTheme;
    private readonly ILogger<DesktopThemeService>? _logger;
    
    public event EventHandler<ThemeVariant>? ThemeChanged;

    public DesktopThemeService(ILogger<DesktopThemeService>? logger = null)
    {
        _logger = logger;
        _currentTheme = GetSystemTheme();
        _logger?.LogInformation("DesktopThemeService initialized with theme: {Theme}", _currentTheme);
    }

    public ThemeVariant GetSystemTheme()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetWindowsTheme();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return GetMacOsTheme();
        }
        
        // Default to light theme for unsupported platforms
        return ThemeVariant.Light;
    }

    public void StartMonitoring()
    {
        _logger?.LogInformation("Starting theme monitoring");
        
        // On macOS, we could use NSDistributedNotificationCenter, but since we're in a desktop project
        // without direct Foundation access, we'll use optimized polling (5 seconds instead of 2)
        // On Windows, we could use WMI events, but polling is simpler and works well
        _themeCheckTimer = new Timer(CheckThemeChange, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    public void StopMonitoring()
    {
        _logger?.LogInformation("Stopping theme monitoring");
        _themeCheckTimer?.Dispose();
        _themeCheckTimer = null;
    }

    private void CheckThemeChange(object? state)
    {
        var newTheme = GetSystemTheme();
        if (newTheme != _currentTheme)
        {
            _logger?.LogInformation("Theme changed from {OldTheme} to {NewTheme}", _currentTheme, newTheme);
            _currentTheme = newTheme;
            ThemeChanged?.Invoke(this, newTheme);
        }
    }

    [SupportedOSPlatform("windows")]
    private static ThemeVariant GetWindowsTheme()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            
            if (value is int intValue)
            {
                return intValue == 0 ? ThemeVariant.Dark : ThemeVariant.Light;
            }
        }
        catch
        {
            // Fall back to light theme if registry read fails
        }
        
        return ThemeVariant.Light;
    }
    [SupportedOSPlatform("macos")]
    private static ThemeVariant GetMacOsTheme()
    {
        try
        {
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "defaults";
            process.StartInfo.Arguments = "read -g AppleInterfaceStyle";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            
            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();
            
            return output.Equals("Dark", StringComparison.OrdinalIgnoreCase) 
                ? ThemeVariant.Dark 
                : ThemeVariant.Light;
        }
        catch
        {
            // Fall back to light theme if command fails
        }
        
        return ThemeVariant.Light;
    }
}
