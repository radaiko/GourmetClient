using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GC.Core.Settings;
using Microsoft.Extensions.Logging;

namespace GC.ViewModels;

public partial class MainViewModel : ObservableObject {
  private readonly GourmetSettingsService? _settingsService;
  private readonly ILogger<MainViewModel>? _logger;
  private bool _isLoadingSettings = false;

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
        _logger?.LogDebug("MenuViewModel property changed: {Property}", e.PropertyName);
        // Notify that a child property changed, triggering UI refresh
        OnPropertyChanged(nameof(MenuViewModel));
      };
    }
    
    if (billingViewModel != null) {
      billingViewModel.PropertyChanged += (_, e) => {
        _logger?.LogDebug("BillingViewModel property changed: {Property}", e.PropertyName);
        // Notify that a child property changed, triggering UI refresh
        OnPropertyChanged(nameof(BillingViewModel));
      };
    }

    _logger?.LogInformation("Initializing MainViewModel");
    LoadSettings();
    PreLoadData();
  }

  public MenuViewModel? MenuViewModel { get; }
  public BillingViewModel? BillingViewModel { get; }

  [ObservableProperty]
  private string _greeting = "Welcome to Gourmet Client";

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
    _logger?.LogInformation("Updating greeting");
    Greeting = $"Updated at {DateTime.Now:HH:mm:ss}";
  }

  [RelayCommand]
  private void NavigateToPage(int pageIndex) {
    _logger?.LogInformation("Navigating to page index {PageIndex}", pageIndex);
    // Desktop still supports About page at index 3; iOS UI only exposes 0-2
    CurrentPageIndex = Math.Clamp(pageIndex, 0, 3);
  }

  [RelayCommand]
  private void ClearError() {
    _logger?.LogInformation("Clearing error message");
    ErrorMessage = null;
  }

  [RelayCommand]
  private void SaveSettings() {
    if (_settingsService == null || _isLoadingSettings) return;
    
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
      // Don't set IsSettingsDirty = false here to avoid UI refresh and focus loss
      _logger?.LogInformation("Settings saved successfully");
      PreLoadData();
    }
    catch (Exception ex) {
      _logger?.LogError(ex, "Failed to save settings");
      ErrorMessage = $"Fehler beim Speichern: {ex.Message}";
    }
  }

  [RelayCommand]
  private void ShowAbout() {
    _logger?.LogInformation("Showing about overlay");
    ShowChangelogOverlay = false;
    ShowAboutOverlay = true;
  }

  [RelayCommand]
  private void ShowChangelog() {
    _logger?.LogInformation("Showing changelog overlay");
    ShowAboutOverlay = false;
    ShowChangelogOverlay = true;
  }

  [RelayCommand]
  private void CloseOverlay() {
    _logger?.LogInformation("Closing overlay");
    ShowAboutOverlay = false;
    ShowChangelogOverlay = false;
  }

  partial void OnGourmetUsernameChanged(string value) {
    _logger?.LogInformation("Gourmet username changed to {Value}", string.IsNullOrEmpty(value) ? "empty" : "set");
    if (!_isLoadingSettings) {
      SaveSettings();
    }
  }

  partial void OnGourmetPasswordChanged(string value) {
    _logger?.LogInformation("Gourmet password changed");
    if (!_isLoadingSettings) {
      SaveSettings();
    }
  }

  partial void OnVentoPayUsernameChanged(string value) {
    _logger?.LogInformation("VentoPay username changed to {Value}", string.IsNullOrEmpty(value) ? "empty" : "set");
    if (!_isLoadingSettings) {
      SaveSettings();
    }
  }

  partial void OnVentoPayPasswordChanged(string value) {
    _logger?.LogInformation("VentoPay password changed");
    if (!_isLoadingSettings) {
      SaveSettings();
    }
  }

  private void LoadSettings() {
    if (_settingsService == null) return;
    
    try {
      _logger?.LogInformation("Loading user settings");
      var userSettings = _settingsService.GetCurrentUserSettings();
      
      // Set flag to prevent auto-save during loading
      _isLoadingSettings = true;
      
      GourmetUsername = userSettings.GourmetLoginUsername ?? string.Empty;
      GourmetPassword = userSettings.GourmetLoginPassword ?? string.Empty;
      VentoPayUsername = userSettings.VentopayUsername ?? string.Empty;
      VentoPayPassword = userSettings.VentopayPassword ?? string.Empty;
      
      // Reset flag after loading
      _isLoadingSettings = false;
      
      _logger?.LogInformation("Settings loaded successfully: Gourmet user {GourmetSet}, VentoPay user {VentoPaySet}",
        string.IsNullOrEmpty(GourmetUsername) ? "not set" : "set",
        string.IsNullOrEmpty(VentoPayUsername) ? "not set" : "set");
    }
    catch (Exception ex) {
      _isLoadingSettings = false;
      _logger?.LogError(ex, "Failed to load settings");
      ErrorMessage = $"Fehler beim Laden der Einstellungen: {ex.Message}";
    }
  }
  
  private void PreLoadData() {
    _logger?.LogDebug("PreLoadData called");
    // Pre-load data in background to improve perceived performance
    if (GourmetUsername != "" && GourmetPassword != "") {
      _logger?.LogInformation("Pre-loading menu data");
      _logger?.LogInformation("Executing LoadMenusCommand for background pre-loading");
      MenuViewModel?.LoadMenusCommand?.Execute(null);
      
    } 
    if (VentoPayUsername != "" && VentoPayPassword != "") {
      _logger?.LogInformation("Pre-loading billing data");
      _logger?.LogInformation("Executing LoadBillingCommand for background pre-loading");
      BillingViewModel?.LoadBillingCommand?.Execute(null);
    }
  }
}
