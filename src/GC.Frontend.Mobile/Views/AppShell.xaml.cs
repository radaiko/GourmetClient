using GC.Common;
using Microsoft.Maui.Controls;
using GC.Models;

namespace GC.Frontend.Mobile.Views;

public partial class AppShell : Shell {
  public AppShell() {
    InitializeComponent();
    
    var s = Settings.It;
    
    if (s.GourmetUsername.IsBlank() || s.GourmetPassword.IsBlank() || s.VentoUsername.IsBlank() || s.VentoPassword.IsBlank()) {
      CurrentItem = SettingsTab;
    }
    CurrentItem = SettingsTab;
  }
}