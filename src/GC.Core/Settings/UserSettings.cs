using System;

namespace GC.Core.Settings;

public class UserSettings
{
    public UserSettings()
    {
        GourmetLoginUsername = string.Empty;
        GourmetLoginPassword = string.Empty;
        VentopayUsername = string.Empty;
        VentopayPassword = string.Empty;
        CacheValidity = TimeSpan.FromHours(4);
        DebugMode = false;
    }

    public string GourmetLoginUsername { get; set; }

    public string GourmetLoginPassword { get; set; }

    public string VentopayUsername { get; set; }

    public string VentopayPassword { get; set; }

    public TimeSpan CacheValidity { get; set; }
    
    public bool DebugMode { get; set; }
}