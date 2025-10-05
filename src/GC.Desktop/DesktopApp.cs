using GC.Desktop.Services;
using GC.ViewModels.Services;
using GC.Views;

namespace GC.Desktop;

public class DesktopApp : App
{
    protected override IThemeService? GetPlatformThemeService()
    {
        return new DesktopThemeService();
    }
}
