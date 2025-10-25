namespace GC.iOS.Controllers;

/// <summary>
/// The main tab bar controller that manages the primary navigation of the app.
/// Contains tabs for Order, Billing, and Settings views.
/// </summary>
public class MainTabBarController : UITabBarController
{
    /// <summary>
    /// The view controller for the Order tab.
    /// </summary>
    private OrderViewController? _orderView;

    /// <summary>
    /// The view controller for the Billing tab.
    /// </summary>
    private BillingViewController? _billingView;

    /// <summary>
    /// The view controller for the Settings tab.
    /// </summary>
    private SettingsViewController? _settingsView;

    /// <summary>
    /// The navigation controller for the Order tab.
    /// </summary>
    private UINavigationController? _orderNav;

    /// <summary>
    /// The navigation controller for the Billing tab.
    /// </summary>
    private UINavigationController? _billingNav;

    /// <summary>
    /// The navigation controller for the Settings tab.
    /// </summary>
    private UINavigationController? _settingsNav;

    /// <summary>
    /// Called after the view has been loaded into memory.
    /// Initializes the tab view controllers and sets up the tab bar.
    /// </summary>
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Create the three main view controllers for the tabs
        _orderView = new OrderViewController();
        _billingView = new BillingViewController();
        _settingsView = new SettingsViewController();

        // Set the view controllers for the tab bar
        _orderNav = new UINavigationController(_orderView);
        _billingNav = new UINavigationController(_billingView);
        _settingsNav = new UINavigationController(_settingsView);
        ViewControllers = [_orderNav, _billingNav, _settingsNav];

        // Update the tab bar images based on the current theme
        UpdateTabBarImages();
    }

    /// <summary>
    /// Called when the trait collection changes (e.g., dark/light mode).
    /// Updates the tab bar images to match the new theme.
    /// </summary>
    /// <param name="previousTraitCollection">The previous trait collection.</param>
    public override void TraitCollectionDidChange(UITraitCollection? previousTraitCollection)
    {
        base.TraitCollectionDidChange(previousTraitCollection);

        // Update images when the interface style changes
        UpdateTabBarImages();
    }

    /// <summary>
    /// Updates the tab bar item images based on the current user interface style (light/dark mode).
    /// </summary>
    private void UpdateTabBarImages()
    {
        // Determine if we're in dark mode
        bool isDark = TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark;

        // Choose the appropriate image suffix
        string suffix = isDark ? "_dark_24.png" : "_light_24.png";

        // Set the order tab image
        var orderImage = UIImage.FromFile("order" + suffix)?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        _orderNav!.TabBarItem.Image = orderImage;
        _orderNav!.TabBarItem.SelectedImage = orderImage;

        // Set the billing tab image
        var billingImage = UIImage.FromFile("billing" + suffix)?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        _billingNav!.TabBarItem.Image = billingImage;
        _billingNav!.TabBarItem.SelectedImage = billingImage;

        // Set the settings tab image
        var settingsImage = UIImage.FromFile("settings" + suffix)?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        _settingsNav!.TabBarItem.Image = settingsImage;
        _settingsNav!.TabBarItem.SelectedImage = settingsImage;
    }
}
