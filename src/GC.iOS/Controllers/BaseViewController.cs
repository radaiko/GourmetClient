using UIKit;
using System.ComponentModel;
using GC.iOS.Helpers;

namespace GC.iOS.Controllers;

public class BaseViewController : UIViewController
{
    protected SafeAreaHelper<BaseViewController> _safeAreaHelper;

    public override void ViewDidLoad()
    {
        base.ViewDidLoad();
        _safeAreaHelper = new SafeAreaHelper<BaseViewController>(this);
        _safeAreaHelper.PropertyChanged += OnSafeAreaChanged;
    }

    protected virtual void OnSafeAreaChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Subclasses can override this to handle safe area changes
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _safeAreaHelper.PropertyChanged -= OnSafeAreaChanged;
            _safeAreaHelper.Dispose();
        }
        base.Dispose(disposing);
    }
}
