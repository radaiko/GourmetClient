using Microsoft.Maui.Controls;
using GourmetClient.ViewModels;
using System.Threading.Tasks;

namespace GourmetClient.Maui.Views;

public partial class MenuOrderView : ContentView
{
    private MenuOrderViewModel? _viewModel;
    private bool _showBillToggleButtonChecked = false;
    private BillingView? _billingView;

    public MenuOrderView()
    {
        InitializeComponent();
        InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        try
        {
            _viewModel = new MenuOrderViewModel();
            BindingContext = _viewModel;
            _viewModel.Initialize();
        }
        catch (System.Exception ex)
        {
            // Handle initialization errors gracefully
            System.Diagnostics.Debug.WriteLine($"Failed to initialize MenuOrderViewModel: {ex.Message}");

            // Show a basic error message in the UI
            BindingContext = new
            {
                ShowWelcomeMessage = true,
                MenuDays = new System.Collections.ObjectModel.ObservableCollection<object>(),
                MenuCategories = new System.Collections.ObjectModel.ObservableCollection<object>()
            };
        }
    }

    private async void OnAboutButtonClicked(object sender, System.EventArgs e)
    {
        try
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "About",
                    $"Gourmet Client - MAUI Version\nVersion: {version}\n\nA modern cross-platform menu ordering application.",
                    "OK");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show about dialog: {ex.Message}");
        }
    }

    private async void OnSettingsButtonClicked(object sender, System.EventArgs e)
    {
        try
        {
            // Toggle settings popup in ViewModel if available
            if (_viewModel != null)
            {
                _viewModel.IsSettingsPopupOpened = !_viewModel.IsSettingsPopupOpened;

                if (_viewModel.IsSettingsPopupOpened)
                {
                    await ShowSettingsView();
                    _viewModel.IsSettingsPopupOpened = false;
                }
            }
            else
            {
                await ShowSettingsView();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to handle settings: {ex.Message}");
        }
    }

    private async Task ShowSettingsView()
    {
        try
        {
            // Create a new settings view instance each time to avoid state issues
            var settingsView = new SettingsView();

            // In MAUI, we can show the settings view in a popup or navigate to it
            // For now, we'll show it in a modal page
            if (Application.Current?.MainPage != null)
            {
                var settingsPage = new ContentPage
                {
                    Title = "Settings",
                    Content = settingsView
                };

                await Application.Current.MainPage.Navigation.PushModalAsync(settingsPage);
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to show settings view: {ex.Message}");

            // Fallback to alert if navigation fails
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Settings",
                    "Settings functionality will be implemented here.\n\nFor now, please configure your login credentials through the application settings.",
                    "OK");
            }
        }
    }

    private async void OnShowBillButtonClicked(object sender, System.EventArgs e)
    {
        try
        {
            _showBillToggleButtonChecked = !_showBillToggleButtonChecked;

            if (_showBillToggleButtonChecked)
            {
                await OnShowBillToggleButtonChecked();
            }
            else
            {
                OnShowBillToggleButtonUnchecked();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to handle bill button: {ex.Message}");
        }
    }

    private async Task OnShowBillToggleButtonChecked()
    {
        try
        {
            // Initialize billing view if not already done
            if (_billingView == null)
            {
                _billingView = new BillingView();
            }

            _billingView.OnActivated();

            // For now, show a simple dialog with billing info
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Billing",
                    "Billing view activated. Transaction details will be shown here.\n\nThis feature is being implemented for the MAUI version.",
                    "OK");
            }

            // Auto-close after showing dialog
            _showBillToggleButtonChecked = false;
            OnShowBillToggleButtonUnchecked();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to activate bill view: {ex.Message}");
        }
    }

    private void OnShowBillToggleButtonUnchecked()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Bill view deactivated");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to deactivate bill view: {ex.Message}");
        }
    }

    private void OnMenuScrollViewerScrollChanged(object sender, ScrolledEventArgs e)
    {
        try
        {
            // Equivalent of MenuScrollViewerOnScrollChanged from WPF version
            // In MAUI, ScrolledEventArgs provides different properties
            if (sender is ScrollView scrollView)
            {
                var canScrollRight = scrollView.ScrollX < (scrollView.ContentSize.Width - scrollView.Width);
                System.Diagnostics.Debug.WriteLine($"Can scroll right: {canScrollRight}, ScrollX: {scrollView.ScrollX}");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to handle scroll changed: {ex.Message}");
        }
    }

    private void OnMenuBillViewOverlayTapped(object sender, System.EventArgs e)
    {
        try
        {
            // Equivalent of MenuBillViewOverlayOnMouseLeftButtonDown from WPF version
            _showBillToggleButtonChecked = false;
            OnShowBillToggleButtonUnchecked();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to handle overlay tap: {ex.Message}");
        }
    }
}