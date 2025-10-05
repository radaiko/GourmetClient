using System.Runtime.InteropServices;

namespace GC.Views.Utils;

public static class PlatformDetector
{
    public static bool IsIOS => OperatingSystem.IsIOS();
    
    public static bool IsAndroid => OperatingSystem.IsAndroid();
    
    public static bool IsMobile => IsIOS || IsAndroid;
    
    public static bool IsDesktop => !IsMobile;
}
