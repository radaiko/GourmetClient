using UIKit;

namespace GourmetClient.MVU.Platforms.iOS;

#pragma warning disable XCODE_26_0_PREVIEW
public class Program
{
    // This is the main entry point of the application.
    static void Main(string[] args)
    {
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        UIApplication.Main(args, null, typeof(AppDelegate));
    }
}
#pragma warning restore XCODE_26_0_PREVIEW

