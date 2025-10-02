using Microsoft.Maui.Controls;
using GourmetClient.ViewModels;
using System.Threading.Tasks;
using System;

namespace GourmetClient.Maui.Views;

public partial class SettingsView : ContentView
{
    private SettingsViewModel? _viewModel;

    public SettingsView()
    {
        InitializeComponent();
        InitializeViewModel();
    }

    private void InitializeViewModel()
    {
        try
        {
            _viewModel = new SettingsViewModel();
            BindingContext = _viewModel;

            // Subscribe to settings saved event to auto-close
            _viewModel.SettingsSaved += OnSettingsSaved;

            _viewModel.Initialize();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize SettingsViewModel: {ex.Message}");
        }
    }

    private async void OnSettingsSaved(object? sender, EventArgs e)
    {
        // Auto-close the settings view when settings are saved
        await CloseSettings();
    }

    private void OnCheckForUpdatesLabelTapped(object sender, System.EventArgs e)
    {
        try
        {
            // Toggle the checkbox when the label is tapped
            CheckForUpdatesCheckBox.IsChecked = !CheckForUpdatesCheckBox.IsChecked;
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to handle checkbox label tap: {ex.Message}");
        }
    }

    private async void OnCloseButtonClicked(object sender, System.EventArgs e)
    {
        await CloseSettings();
    }

    private async void OnCancelButtonClicked(object sender, System.EventArgs e)
    {
        await CloseSettings();
    }

    private async Task CloseSettings()
    {
        try
        {
            // Unsubscribe from events to prevent memory leaks
            if (_viewModel != null)
            {
                _viewModel.SettingsSaved -= OnSettingsSaved;
            }

            // Close the modal page
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.Navigation.PopModalAsync();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to close settings view: {ex.Message}");
        }
    }
}