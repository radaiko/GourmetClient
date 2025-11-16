using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using GC.Common;

namespace GC.Models;

public partial class Settings : ObservableObject {
  private static Settings? _cachedSettings;
  public static Settings It {  
    get {
      _cachedSettings ??= Load();
      return _cachedSettings;
    }
  }
  
  private static readonly string StoragePath = Base.IsTesting 
    ? Path.Combine(Path.GetTempPath(), "settings_test.json")
    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "settings.json");
  // When true, Save() will no-op so we can set properties during Load without persisting repeatedly
  private bool _loading;
  
  [ObservableProperty] private string? _gourmetUsername;
  [ObservableProperty] private string? _gourmetPassword;
  [ObservableProperty] private string? _ventoUsername;
  [ObservableProperty] private string? _ventoPassword;
  [ObservableProperty] private bool _debugMode;

  // Attach on all properties to save on change
  partial void OnGourmetUsernameChanged(string? value) => Save();
  partial void OnGourmetPasswordChanged(string? value) => Save();
  partial void OnVentoUsernameChanged(string? value) => Save();
  partial void OnVentoPasswordChanged(string? value) => Save();
  partial void OnDebugModeChanged(bool value) => Save();
  
  // Private DTO used for JSON serialization so we don't trigger property-change side effects
  private sealed class SerializedSettings {
    public string? GourmetUsername { get; init; }
    public string? GourmetPassword { get; init; }
    public string? VentoUsername { get; init; }
    public string? VentoPassword { get; init; }
    public bool DebugMode { get; init; }
  }

  public static Settings Load() {
    try {
      if (!File.Exists(StoragePath)) {
        var defaults = new Settings { _loading = false };
        Logger.Info("Settings loaded (defaults)");
        return defaults;
      }
      var encryptedBase64 = File.ReadAllText(StoragePath);
      var json = Crypto.Decrypt(encryptedBase64);
      Debug.Assert(json != null, nameof(json) + " != null");
      var data = JsonSerializer.Deserialize<SerializedSettings>(json, Base.JsonOptions) ?? new SerializedSettings();
      var settings = new Settings {
        _loading = true,
        GourmetUsername = data.GourmetUsername,
        GourmetPassword = data.GourmetPassword,
        VentoUsername = data.VentoUsername,
        VentoPassword = data.VentoPassword,
        DebugMode = data.DebugMode
      };
      settings._loading = false;
      Logger.Info("Settings loaded");
      return settings;
    }
    catch (Exception ex) {
      Logger.Error($"Failed to load settings: {ex.Message}");
      return new Settings();
    }
  }

  private void Save() {
    if (_loading) return;
    try {
      var dir = Path.GetDirectoryName(StoragePath);
      if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
      var data = new SerializedSettings {
        GourmetUsername = GourmetUsername,
        GourmetPassword = GourmetPassword,
        VentoUsername = VentoUsername,
        VentoPassword = VentoPassword,
        DebugMode = DebugMode
      };
      var json = JsonSerializer.Serialize(data, Base.JsonOptions);
      var encryptedBase64 = Crypto.Encrypt(json);
      File.WriteAllText(StoragePath, encryptedBase64);
      Logger.Debug("Settings saved");
    }
    catch (Exception ex) {
      Logger.Error($"Failed to save settings: {ex.Message}");
    }
  }
}