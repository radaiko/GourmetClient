namespace GC.iOS.Controllers;

public class OrderViewController : UIViewController
{
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        View!.AddSubview(new UILabel(View.Bounds) {
            BackgroundColor = UIColor.SystemBackground,
            TextAlignment = UITextAlignment.Center,
            Text = "Bestellungen",
            AutoresizingMask = UIViewAutoresizing.All
        });
    }
}
