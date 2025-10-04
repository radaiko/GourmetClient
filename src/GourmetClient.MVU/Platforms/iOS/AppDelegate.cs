using Avalonia;
using Avalonia.iOS;
using UIKit;

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

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        var result = base.FinishedLaunching(application, launchOptions);
        
        // Configure for edge-to-edge display
        if (Window?.RootViewController != null)
        {
            Window.RootViewController.EdgesForExtendedLayout = UIRectEdge.All;
            Window.RootViewController.ExtendedLayoutIncludesOpaqueBars = true;
        }
        
        return result;
    }
}
