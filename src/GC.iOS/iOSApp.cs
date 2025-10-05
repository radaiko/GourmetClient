using GC.iOS.Services;
using GC.ViewModels.Services;
using GC.Views;

namespace GC.iOS;

public class iOSApp : App
{
    protected override IThemeService? GetPlatformThemeService()
    {
        return new iOSThemeService();
    }
}
