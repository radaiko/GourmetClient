using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GourmetClient.Core.Settings;

namespace GC.ViewModels;

public partial class MainViewModel : ObservableObject {
  private readonly GourmetSettingsService _settingsService;

  public MainViewModel(GourmetSettingsService settingsService) {
    _settingsService = settingsService;
    LoadSettings();
  }

  [ObservableProperty]
  private string _greeting = "Welcome to Gourmet Client MVVM!";

  [ObservableProperty]
  private int _currentPageIndex = 0; // iOS navigation: 0=Menu,1=Billing,2=Settings,3=About

  [ObservableProperty]
  private bool _isLoading = false;

  [ObservableProperty]
  private string? _errorMessage;

  [ObservableProperty]
  private string _userName = "";

  [ObservableProperty]
  private DateTime? _lastMenuUpdate;

  [ObservableProperty]
  private bool _isSettingsDirty = false;

  // Settings properties
  [ObservableProperty]
  private string _gourmetUsername = "";

  [ObservableProperty]
  private string _gourmetPassword = "";

  [ObservableProperty]
  private string _ventoPayUsername = "";

  [ObservableProperty]
  private string _ventoPayPassword = "";

  [RelayCommand]
  private void UpdateGreeting() {
    Greeting = $"Updated at {DateTime.Now:HH:mm:ss}";
  }

  [RelayCommand]
  private void NavigateToPage(int pageIndex) {
    CurrentPageIndex = Math.Clamp(pageIndex, 0, 3);
  }

  [RelayCommand]
  private void ClearError() {
    ErrorMessage = null;
  }

  [RelayCommand]
  private void SaveSettings() {
    try {
      // Map from ViewModel properties to UserSettings
      var userSettings = new UserSettings {
        GourmetLoginUsername = GourmetUsername ?? string.Empty,
        GourmetLoginPassword = GourmetPassword ?? string.Empty,
        VentopayUsername = VentoPayUsername ?? string.Empty,
        VentopayPassword = VentoPayPassword ?? string.Empty
      };
      
      _settingsService.SaveUserSettings(userSettings);
      IsSettingsDirty = false;
    }
    catch (Exception ex) {
      ErrorMessage = $"Fehler beim Speichern: {ex.Message}";
    }
  }

  partial void OnGourmetUsernameChanged(string value) {
    IsSettingsDirty = true;
  }

  partial void OnGourmetPasswordChanged(string value) {
    IsSettingsDirty = true;
  }

  partial void OnVentoPayUsernameChanged(string value) {
    IsSettingsDirty = true;
  }

  partial void OnVentoPayPasswordChanged(string value) {
    IsSettingsDirty = true;
  }

  private void LoadSettings() {
    try {
      var userSettings = _settingsService.GetCurrentUserSettings();
      
      // Don't trigger dirty flag when loading initial settings
      var wasDirty = IsSettingsDirty;
      
      GourmetUsername = userSettings.GourmetLoginUsername ?? string.Empty;
      GourmetPassword = userSettings.GourmetLoginPassword ?? string.Empty;
      VentoPayUsername = userSettings.VentopayUsername ?? string.Empty;
      VentoPayPassword = userSettings.VentopayPassword ?? string.Empty;
      
      // Reset dirty flag after loading
      IsSettingsDirty = wasDirty;
    }
    catch (Exception ex) {
      ErrorMessage = $"Fehler beim Laden der Einstellungen: {ex.Message}";
    }
  }
}

