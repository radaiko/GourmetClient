using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GC.Core.Settings;
using Microsoft.Extensions.Logging;

namespace GC.ViewModels;

public partial class MainViewModel : ObservableObject {
  private readonly GourmetSettingsService? _settingsService;
  private readonly ILogger<MainViewModel>? _logger;

  // Design-time constructor for XAML previewer
  public MainViewModel() : this(null!, null!, null!, null!) {
  }

  public MainViewModel(
    GourmetSettingsService settingsService, 
    ILogger<MainViewModel> logger,
    MenuViewModel? menuViewModel,
    BillingViewModel? billingViewModel) {
    _settingsService = settingsService;
    _logger = logger;
    MenuViewModel = menuViewModel;
    BillingViewModel = billingViewModel;
    
    // Subscribe to child ViewModel property changes to trigger UI updates on iOS
    if (menuViewModel != null) {
      menuViewModel.PropertyChanged += (_, e) => {
        // Notify that a child property changed, triggering UI refresh
        OnPropertyChanged(nameof(MenuViewModel));
      };
    }
    
    if (billingViewModel != null) {
      billingViewModel.PropertyChanged += (_, e) => {
        // Notify that a child property changed, triggering UI refresh
        OnPropertyChanged(nameof(BillingViewModel));
      };
    }
    
    if (settingsService != null) {
      _logger?.LogInformation("Initializing MainViewModel");
      LoadSettings();
    }
  }

  public MenuViewModel? MenuViewModel { get; }
  public BillingViewModel? BillingViewModel { get; }

  [ObservableProperty]
  private string _greeting = "Welcome to Gourmet Client MVVM!";

  [ObservableProperty]
  private int _currentPageIndex = 0; // iOS bottom tab navigation: 0=Menu,1=Billing,2=Settings

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

  // Overlay state (About / Changelog)
  [ObservableProperty]
  private bool _showAboutOverlay;

  [ObservableProperty]
  private bool _showChangelogOverlay;

  [RelayCommand]
  private void UpdateGreeting() {
    Greeting = $"Updated at {DateTime.Now:HH:mm:ss}";
  }

  [RelayCommand]
  private void NavigateToPage(int pageIndex) {
    // Desktop still supports About page at index 3; iOS UI only exposes 0-2
    CurrentPageIndex = Math.Clamp(pageIndex, 0, 3);
  }

  [RelayCommand]
  private void ClearError() {
    ErrorMessage = null;
  }

  [RelayCommand]
  private void SaveSettings() {
    if (_settingsService == null) return;
    
    try {
      _logger?.LogInformation("Saving user settings");
      
      // Map from ViewModel properties to UserSettings
      var userSettings = new UserSettings {
        GourmetLoginUsername = GourmetUsername ?? string.Empty,
        GourmetLoginPassword = GourmetPassword ?? string.Empty,
        VentopayUsername = VentoPayUsername ?? string.Empty,
        VentopayPassword = VentoPayPassword ?? string.Empty
      };
      
      _settingsService.SaveUserSettings(userSettings);
      IsSettingsDirty = false;
      _logger?.LogInformation("Settings saved successfully");
    }
    catch (Exception ex) {
      _logger?.LogError(ex, "Failed to save settings");
      ErrorMessage = $"Fehler beim Speichern: {ex.Message}";
    }
  }

  [RelayCommand]
  private void ShowAbout() {
    ShowChangelogOverlay = false;
    ShowAboutOverlay = true;
  }

  [RelayCommand]
  private void ShowChangelog() {
    ShowAboutOverlay = false;
    ShowChangelogOverlay = true;
  }

  [RelayCommand]
  private void CloseOverlay() {
    ShowAboutOverlay = false;
    ShowChangelogOverlay = false;
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
    if (_settingsService == null) return;
    
    try {
      _logger?.LogInformation("Loading user settings");
      var userSettings = _settingsService.GetCurrentUserSettings();
      
      // Don't trigger dirty flag when loading initial settings
      var wasDirty = IsSettingsDirty;
      
      GourmetUsername = userSettings.GourmetLoginUsername ?? string.Empty;
      GourmetPassword = userSettings.GourmetLoginPassword ?? string.Empty;
      VentoPayUsername = userSettings.VentopayUsername ?? string.Empty;
      VentoPayPassword = userSettings.VentopayPassword ?? string.Empty;
      
      // Reset dirty flag after loading
      IsSettingsDirty = wasDirty;
      _logger?.LogInformation("Settings loaded successfully");
    }
    catch (Exception ex) {
      _logger?.LogError(ex, "Failed to load settings");
      ErrorMessage = $"Fehler beim Laden der Einstellungen: {ex.Message}";
    }
  }
}
