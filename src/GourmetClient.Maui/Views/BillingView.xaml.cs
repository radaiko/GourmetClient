using Microsoft.Maui.Controls;
using GourmetClient.ViewModels;

namespace GourmetClient.Maui.Views;

public partial class BillingView : ContentView
{
    private BillingViewModel? _viewModel;

    public BillingView()
    {
        // InitializeComponent(); // Commented out until XAML generation works
        InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        try
        {
            _viewModel = new BillingViewModel();
            BindingContext = _viewModel;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize BillingViewModel: {ex.Message}");
        }
    }

    public void OnActivated()
    {
        try
        {
            // Equivalent to the WPF OnActivated method
            _viewModel?.Initialize();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to activate BillingView: {ex.Message}");
        }
    }
}