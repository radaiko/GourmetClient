using UIKit;
using System.ComponentModel;
using GC.iOS.Helpers;

namespace GC.iOS.Controllers;

/// <summary>
/// Base view controller that provides common functionality for all view controllers in the app.
/// Includes safe area handling to help with layout on devices with notches or home indicators.
/// </summary>
public class BaseViewController : UIViewController
{
    /// <summary>
    /// Helper for observing and reacting to safe area insets changes.
    /// </summary>
    protected SafeAreaHelper<BaseViewController> _safeAreaHelper;

    /// <summary>
    /// Called after the view has been loaded into memory.
    /// Sets up the safe area helper and subscribes to its changes.
    /// </summary>
    public override void ViewDidLoad()
    {
        base.ViewDidLoad();

        // Initialize the safe area helper to monitor safe area changes
        _safeAreaHelper = new SafeAreaHelper<BaseViewController>(this);

        // Subscribe to safe area changes so subclasses can react
        _safeAreaHelper.PropertyChanged += OnSafeAreaChanged;
    }

    /// <summary>
    /// Called when the safe area insets change.
    /// Subclasses can override this to update their layout accordingly.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">Event arguments containing the property that changed.</param>
    protected virtual void OnSafeAreaChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Subclasses can override this to handle safe area changes
    }

    /// <summary>
    /// Disposes of resources used by the view controller.
    /// Unsubscribes from events and disposes the safe area helper.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Unsubscribe from the safe area helper's events
            _safeAreaHelper.PropertyChanged -= OnSafeAreaChanged;

            // Dispose the safe area helper to clean up observers
            _safeAreaHelper.Dispose();
        }
        base.Dispose(disposing);
    }
}
