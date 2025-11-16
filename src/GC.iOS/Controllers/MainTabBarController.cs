using GC.Models;

namespace GC.iOS.Controllers;

/// <summary>
/// The main tab bar controller that manages the primary navigation of the app.
/// Contains tabs for Order, Billing, Settings, and optionally Logs (when debug mode is enabled).
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
    private InvoiceViewController? _billingView;

    /// <summary>
    /// The view controller for the Settings tab.
    /// </summary>
    private SettingsViewController? _settingsView;

    /// <summary>
    /// The view controller for the Log tab (only shown in debug mode).
    /// </summary>
    private LogViewController? _logView;

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
    /// The navigation controller for the Log tab.
    /// </summary>
    private UINavigationController? _logNav;

    /// <summary>
    /// Called after the view has been loaded into memory.
    /// Initializes the tab view controllers and sets up the tab bar.
    /// </summary>
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Create the three main view controllers for the tabs
        _orderView = new OrderViewController();
        _billingView = new InvoiceViewController();
        _settingsView = new SettingsViewController();

        // Set the view controllers for the tab bar
        _orderNav = new UINavigationController(_orderView);
        _billingNav = new UINavigationController(_billingView);
        _settingsNav = new UINavigationController(_settingsView);

        // Subscribe to settings changes to update tabs when debug mode changes
        Settings.It.PropertyChanged += OnSettingsChanged;

        // Update tab bar with or without log tab based on debug mode
        UpdateTabBarControllers();

        // Update the tab bar images based on the current theme
        UpdateTabBarImages();
    }

    /// <summary>
    /// Called when settings change to update the tab bar controllers.
    /// </summary>
    private void OnSettingsChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Settings.DebugMode))
        {
            InvokeOnMainThread(UpdateTabBarControllers);
        }
    }

    /// <summary>
    /// Updates the tab bar controllers based on the current debug mode setting.
    /// </summary>
    private void UpdateTabBarControllers()
    {
        if (Settings.It.DebugMode)
        {
            // Create log view if it doesn't exist
            if (_logView == null)
            {
                _logView = new LogViewController();
                _logNav = new UINavigationController(_logView);
            }

            ViewControllers = [_orderNav!, _billingNav!, _settingsNav!, _logNav!];
        }
        else
        {
            ViewControllers = [_orderNav!, _billingNav!, _settingsNav!];
        }

        // Update images after changing view controllers
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

        // Set the log tab image if debug mode is enabled
        if (_logNav != null && Settings.It.DebugMode)
        {
            // Use a system icon for logs since we may not have a custom image
            _logNav.TabBarItem.Image = UIImage.GetSystemImage("list.bullet.rectangle");
        }
    }

    /// <summary>
    /// Disposes of resources used by the tab bar controller.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Settings.It.PropertyChanged -= OnSettingsChanged;
        }
        base.Dispose(disposing);
    }
}
