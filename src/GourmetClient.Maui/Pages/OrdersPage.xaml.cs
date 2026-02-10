namespace GourmetClient.Maui.Pages;

public partial class OrdersPage : ContentPage
{
    public OrdersPage(ViewModels.OrdersViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.OrdersViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}
