using GourmetClient.ViewModels;

namespace GourmetClient.Views;

public partial class BillingView : InitializableView
{
    public BillingView()
    {
        InitializeComponent();

        DataContext = new BillingViewModel();
    }

    public void OnActivated()
    {
        (DataContext as BillingViewModel)?.OnActivated();
    }
}