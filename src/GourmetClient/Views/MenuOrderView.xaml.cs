using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GourmetClient.ViewModels;

namespace GourmetClient.Views;

public partial class MenuOrderView : InitializableView
{
    private static readonly DependencyPropertyKey AreAdditionalMenusOnRightSideKey = DependencyProperty.RegisterReadOnly(
        "AreAdditionalMenusOnRightSide",
        typeof(bool),
        typeof(MenuOrderView),
        new PropertyMetadata(false));

    public static readonly DependencyProperty AreAdditionalMenusOnRightSideProperty = AreAdditionalMenusOnRightSideKey.DependencyProperty;
		
    private static readonly DependencyProperty HorizontalMenuScrollOffsetProperty =DependencyProperty.Register(
        "HorizontalMenuScrollOffset", 
        typeof(double), 
        typeof(MenuOrderView), 
        new PropertyMetadata(0.0, HorizontalMenuScrollOffsetChangedCallback));

    public MenuOrderView()
    {
        InitializeComponent();

        DataContext = new MenuOrderViewModel();
    }

    public bool AreAdditionalMenusOnRightSide
    {
        get => (bool)GetValue(AreAdditionalMenusOnRightSideProperty);
        private set => SetValue(AreAdditionalMenusOnRightSideKey, value);
    }
    private double HorizontalMenuScrollOffset
    {
        get => (double)GetValue(HorizontalMenuScrollOffsetProperty);
        set => SetValue(HorizontalMenuScrollOffsetProperty, value);
    }

    private void MenuScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        var canScrollRight = (e.HorizontalOffset + e.ViewportWidth) < e.ExtentWidth;
        AreAdditionalMenusOnRightSide = canScrollRight;
    }

    private static void HorizontalMenuScrollOffsetChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var view = d as MenuOrderView;
        view?.MenuScrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
    }

    private void MenuBillViewOverlayOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        ShowBillToggleButton.IsChecked = false;
    }

    private void ShowBillToggleButtonOnChecked(object sender, RoutedEventArgs e)
    {
        BillingView.OnActivated();
    }
}