namespace GC.iOS.Controllers;

public class MainTabBarController : UITabBarController
{
    private OrderViewController? _orderView;
    private BillingViewController? _billingView;
    private SettingsViewController? _settingsView;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // create three view controllers for the tabs
        _orderView = new OrderViewController();

        _billingView = new BillingViewController();

        _settingsView = new SettingsViewController();

        ViewControllers = [_orderView, _billingView, _settingsView];

        UpdateTabBarImages();
    }

    public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
    {
        base.TraitCollectionDidChange(previousTraitCollection);
        UpdateTabBarImages();
    }

    private void UpdateTabBarImages()
    {
        bool isDark = TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark;
        string suffix = isDark ? "_dark_24.png" : "_light_24.png";

        var orderImage = UIImage.FromFile("order" + suffix)?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        _orderView!.TabBarItem.Image = orderImage;
        _orderView!.TabBarItem.SelectedImage = orderImage;

        var billingImage = UIImage.FromFile("billing" + suffix)?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        _billingView!.TabBarItem.Image = billingImage;
        _billingView!.TabBarItem.SelectedImage = billingImage;

        var settingsImage = UIImage.FromFile("settings" + suffix)?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        _settingsView!.TabBarItem.Image = settingsImage;
        _settingsView!.TabBarItem.SelectedImage = settingsImage;
    }
}
