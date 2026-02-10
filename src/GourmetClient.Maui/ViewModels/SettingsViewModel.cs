using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GourmetClient.Maui.Core.Network;
using GourmetClient.Maui.Core.Settings;
using GourmetClient.Maui.Services;

namespace GourmetClient.Maui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly GourmetSettingsService _settingsService;
    private readonly GourmetWebClient _gourmetClient;
    private readonly VentopayWebClient _ventopayClient;
    private readonly IUpdateService _updateService;

    [ObservableProperty]
    private string _gourmetUsername = string.Empty;

    [ObservableProperty]
    private string _gourmetPassword = string.Empty;

    [ObservableProperty]
    private string _ventopayUsername = string.Empty;

    [ObservableProperty]
    private string _ventopayPassword = string.Empty;

    [ObservableProperty]
    private bool _checkForUpdates = true;

    [ObservableProperty]
    private bool _includePreRelease;

    [ObservableProperty]
    private bool _isGourmetConnected;

    [ObservableProperty]
    private bool _isVentopayConnected;

    [ObservableProperty]
    private bool _isTesting;

    public SettingsViewModel(
        GourmetSettingsService settingsService,
        GourmetWebClient gourmetClient,
        VentopayWebClient ventopayClient,
        IUpdateService updateService)
    {
        _settingsService = settingsService;
        _gourmetClient = gourmetClient;
        _ventopayClient = ventopayClient;
        _updateService = updateService;
    }

    public string GourmetStatusText => IsGourmetConnected ? "Connected" : "Not Connected";
    public Color GourmetStatusColor => IsGourmetConnected ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");
    public string VentopayStatusText => IsVentopayConnected ? "Connected" : "Not Connected";
    public Color VentopayStatusColor => IsVentopayConnected ? Color.FromArgb("#4CAF50") : Color.FromArgb("#9E9E9E");
    public bool CanTestGourmet => !string.IsNullOrWhiteSpace(GourmetUsername) && !string.IsNullOrWhiteSpace(GourmetPassword) && !IsTesting;
    public bool CanTestVentopay => !string.IsNullOrWhiteSpace(VentopayUsername) && !string.IsNullOrWhiteSpace(VentopayPassword) && !IsTesting;
    public string VersionText => $"Version {AppInfo.VersionString}";

    public void OnAppearing()
    {
        LoadSettings();
    }

    partial void OnGourmetUsernameChanged(string value) => OnPropertyChanged(nameof(CanTestGourmet));
    partial void OnGourmetPasswordChanged(string value) => OnPropertyChanged(nameof(CanTestGourmet));
    partial void OnVentopayUsernameChanged(string value) => OnPropertyChanged(nameof(CanTestVentopay));
    partial void OnVentopayPasswordChanged(string value) => OnPropertyChanged(nameof(CanTestVentopay));

    [RelayCommand]
    private async Task TestGourmetConnection()
    {
        // Prevent concurrent connection tests
        if (IsTesting)
            return;

        IsTesting = true;
        OnPropertyChanged(nameof(CanTestGourmet));
        OnPropertyChanged(nameof(CanTestVentopay));

        try
        {
            await using var handle = await _gourmetClient.Login(GourmetUsername, GourmetPassword);
            IsGourmetConnected = handle.LoginSuccessful;

            var message = handle.LoginSuccessful ? "Connection successful!" : "Connection failed. Please check your credentials.";
            await Shell.Current.DisplayAlert("Gourmet", message, "OK");
        }
        catch (Exception ex)
        {
            IsGourmetConnected = false;
            await Shell.Current.DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
        }
        finally
        {
            IsTesting = false;
            OnPropertyChanged(nameof(CanTestGourmet));
            OnPropertyChanged(nameof(CanTestVentopay));
            OnPropertyChanged(nameof(GourmetStatusText));
            OnPropertyChanged(nameof(GourmetStatusColor));
        }
    }

    [RelayCommand]
    private async Task TestVentopayConnection()
    {
        // Prevent concurrent connection tests
        if (IsTesting)
            return;

        IsTesting = true;
        OnPropertyChanged(nameof(CanTestGourmet));
        OnPropertyChanged(nameof(CanTestVentopay));

        try
        {
            await using var handle = await _ventopayClient.Login(VentopayUsername, VentopayPassword);
            IsVentopayConnected = handle.LoginSuccessful;

            var message = handle.LoginSuccessful ? "Connection successful!" : "Connection failed. Please check your credentials.";
            await Shell.Current.DisplayAlert("Ventopay", message, "OK");
        }
        catch (Exception ex)
        {
            IsVentopayConnected = false;
            await Shell.Current.DisplayAlert("Error", $"Connection failed: {ex.Message}", "OK");
        }
        finally
        {
            IsTesting = false;
            OnPropertyChanged(nameof(CanTestGourmet));
            OnPropertyChanged(nameof(CanTestVentopay));
            OnPropertyChanged(nameof(VentopayStatusText));
            OnPropertyChanged(nameof(VentopayStatusColor));
        }
    }

    [RelayCommand]
    private async Task CheckForUpdatesCommand()
    {
        if (!_updateService.IsSupported)
        {
            await Shell.Current.DisplayAlert("Updates", "Updates are not supported on this platform.", "OK");
            return;
        }

        try
        {
            var hasUpdate = await _updateService.CheckForUpdateAsync();
            if (hasUpdate)
            {
                var install = await Shell.Current.DisplayAlert("Update Available", "A new version is available. Would you like to install it?", "Yes", "No");
                if (install)
                {
                    await _updateService.DownloadAndApplyAsync();
                }
            }
            else
            {
                await Shell.Current.DisplayAlert("Up to Date", "You are running the latest version.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to check for updates: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        var settings = new UserSettings
        {
            GourmetLoginUsername = GourmetUsername,
            GourmetLoginPassword = GourmetPassword,
            VentopayUsername = VentopayUsername,
            VentopayPassword = VentopayPassword
        };

        _settingsService.SaveUserSettings(settings);

        var updateSettings = new UpdateSettings
        {
            CheckForUpdates = CheckForUpdates
        };
        _settingsService.SaveUpdateSettings(updateSettings);

        await Shell.Current.DisplayAlert("Settings", "Settings saved successfully!", "OK");
    }

    private void LoadSettings()
    {
        var settings = _settingsService.GetCurrentUserSettings();
        GourmetUsername = settings.GourmetLoginUsername ?? string.Empty;
        GourmetPassword = settings.GourmetLoginPassword ?? string.Empty;
        VentopayUsername = settings.VentopayUsername ?? string.Empty;
        VentopayPassword = settings.VentopayPassword ?? string.Empty;

        var updateSettings = _settingsService.GetCurrentUpdateSettings();
        CheckForUpdates = updateSettings.CheckForUpdates;
    }
}
