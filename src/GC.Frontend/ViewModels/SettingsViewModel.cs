using System;
using CommunityToolkit.Mvvm.ComponentModel;
using GC.Models;
using System.ComponentModel;

namespace GC.Frontend.ViewModels;

public partial class SettingsViewModel : ObservableObject{
  [ObservableProperty] private Settings _settings = Settings.It;

  // Direct passthrough properties for binding in the UI (two-way)
  public string? GourmetUsername {
    get => Settings.GourmetUsername;
    set {
      if (Settings.GourmetUsername == value) return;
      Settings.GourmetUsername = value;
      OnPropertyChanged(nameof(GourmetUsername));
    }
  }

  public string? GourmetPassword {
    get => Settings.GourmetPassword;
    set {
      if (Settings.GourmetPassword == value) return;
      Settings.GourmetPassword = value;
      OnPropertyChanged(nameof(GourmetPassword));
    }
  }

  public string? VentoUsername {
    get => Settings.VentoUsername;
    set {
      if (Settings.VentoUsername == value) return;
      Settings.VentoUsername = value;
      OnPropertyChanged(nameof(VentoUsername));
    }
  }

  public string? VentoPassword {
    get => Settings.VentoPassword;
    set {
      if (Settings.VentoPassword == value) return;
      Settings.VentoPassword = value;
      OnPropertyChanged(nameof(VentoPassword));
    }
  }

  public bool DebugMode {
    get => Settings.DebugMode;
    set {
      if (Settings.DebugMode == value) return;
      Settings.DebugMode = value;
      OnPropertyChanged(nameof(DebugMode));
    }
  }

}
