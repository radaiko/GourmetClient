using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GC.iOS.Helpers;

/// <summary>
/// A helper class that provides safe area insets for a UIViewController.
/// It observes changes in the view's safe area and notifies when it changes.
/// This helps in laying out UI elements correctly, especially on devices with notches or home indicators.
/// </summary>
public sealed class SafeAreaHelper<T> : NSObject, INotifyPropertyChanged, IDisposable where T : UIViewController
{
    // Use a weak reference to the view controller to prevent memory leaks
    private readonly WeakReference<T> _viewControllerRef;
    private UIEdgeInsets _safeAreaInsets;

    /// <summary>
    /// Gets the current safe area insets of the view controller's view.
    /// </summary>
    public UIEdgeInsets SafeAreaInsets
    {
        get => _safeAreaInsets;
        private set
        {
            if (_safeAreaInsets.Equals(value))
            {
                return;
            }
            _safeAreaInsets = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Event raised when the safe area insets change.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Initializes a new instance of the SafeAreaHelper for the given view controller.
    /// Starts observing changes to the safe area insets.
    /// </summary>
    /// <param name="viewController">The view controller to observe.</param>
    public SafeAreaHelper(T viewController)
    {
        _viewControllerRef = new WeakReference<T>(viewController);
        
        // Observe changes to the safe area insets of the view, if the view is loaded
        if (viewController.View != null)
        {
            viewController.View.AddObserver(this, new NSString("safeAreaInsets"), 
                NSKeyValueObservingOptions.New, IntPtr.Zero);
        }
        
        // Set the initial safe area insets
        UpdateSafeAreaInsets();
    }

    /// <summary>
    /// Called when the observed property changes.
    /// Updates the safe area insets if the change is related to safeAreaInsets.
    /// </summary>
    public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
    {
        if (keyPath == "safeAreaInsets")
        {
            UpdateSafeAreaInsets();
        }
    }

    /// <summary>
    /// Updates the SafeAreaInsets property based on the current view's safe area.
    /// Handles different iOS versions appropriately.
    /// </summary>
    private void UpdateSafeAreaInsets()
    {
        if (_viewControllerRef.TryGetTarget(out T? viewController))
        {
            if (viewController.View != null)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
                {
                    // For iOS 11 and later, use the built-in SafeAreaInsets
                    SafeAreaInsets = viewController.View.SafeAreaInsets;
                }
                else
                {
                    // For older iOS versions, use layout guides as fallback
                    SafeAreaInsets = new UIEdgeInsets(
                        top: viewController.TopLayoutGuide.Length, 
                        left: 0, 
                        bottom: viewController.BottomLayoutGuide.Length, 
                        right: 0);
                }
            }
        }
    }

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
    }

    /// <summary>
    /// Disposes the helper by removing the observer.
    /// Call this when the view controller is no longer needed to avoid crashes.
    /// </summary>
    public new void Dispose()
    {
        if (_viewControllerRef.TryGetTarget(out T? viewController) && viewController.View != null)
        {
            viewController.View.RemoveObserver(this, new NSString("safeAreaInsets"));
        }
    }
}