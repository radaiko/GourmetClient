namespace GourmetClient.Maui.Pages;

public partial class BillingPage : ContentPage
{
    public BillingPage(ViewModels.BillingViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.BillingViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}
