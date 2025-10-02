using Microsoft.Maui.Controls;
using GourmetClient.ViewModels;

namespace GourmetClient.Maui.Views;

public partial class NotificationsView : ContentView
{
    public NotificationsView()
    {
        // InitializeComponent(); // Commented out until XAML generation works
        InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        try 
        {
            var viewModel = new NotificationsViewModel();
            BindingContext = viewModel;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize NotificationsViewModel: {ex.Message}");
        }
    }
}