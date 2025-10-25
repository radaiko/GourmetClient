using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GC.iOS.Core;

public class SafeAreaHelper<T> : NSObject, INotifyPropertyChanged, IDisposable where T : UIViewController
{
    // A weak reference is used to prevent a memory leak
    private readonly WeakReference<T> _viewControllerRef;
    private UIEdgeInsets _safeAreaInsets;

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

    public event PropertyChangedEventHandler? PropertyChanged;

    public SafeAreaHelper(T viewController)
    {
        _viewControllerRef = new WeakReference<T>(viewController);
        
        // This is the key part: hook into the view's lifecycle.
        // It's called whenever the view's subviews are laid out.
        viewController.View.AddObserver(this, new Foundation.NSString("safeAreaInsets"), 
            Foundation.NSKeyValueObservingOptions.New, IntPtr.Zero);
        
        // Initial set of safe area
        UpdateSafeArea();
    }

    // This method is called by the observer when the view's safe area insets change.
    public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
    {
        if (keyPath == "safeAreaInsets")
        {
            UpdateSafeArea();
        }
    }

    private void UpdateSafeArea()
    {
        if (_viewControllerRef.TryGetTarget(out T? viewController) && viewController != null)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                SafeAreaInsets = viewController.View.SafeAreaInsets;
            }
            else
            {
                // Fallback for older iOS versions
                SafeAreaInsets = new UIEdgeInsets(
                    top: viewController.TopLayoutGuide.Length, 
                    left: 0, 
                    bottom: viewController.BottomLayoutGuide.Length, 
                    right: 0);
            }
        }
    }

    // Helper method to raise the PropertyChanged event.
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
    }

    // It's important to remove the observer to avoid crashes when the view controller is deallocated.
    public void Dispose()
    {
        if (_viewControllerRef.TryGetTarget(out T? viewController) && viewController != null)
        {
            viewController.View.RemoveObserver(this, new Foundation.NSString("safeAreaInsets"));
        }
    }
}