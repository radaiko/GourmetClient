namespace GourmetClient.Maui.Pages;

public partial class MenusPage : ContentPage
{
    public MenusPage(ViewModels.MenusViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModels.MenusViewModel vm)
        {
            vm.OnAppearing();
        }
    }
}
