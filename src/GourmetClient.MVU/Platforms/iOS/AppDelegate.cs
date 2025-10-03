using Avalonia;
using Avalonia.iOS;

namespace GourmetClient.MVU.Platforms.iOS;

[Register("AppDelegate")]
public class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CreateAppBuilder()
    {
        return AppBuilder.Configure<App>()
            .UseiOS()
            .LogToTrace();
    }
}
