using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GC.Core.Cache;

namespace GC.Frontend.ViewModels;

public partial class MainViewModel : ObservableObject {
#pragma warning disable CA1822 // Mark members as static
  public string Greeting => "Welcome to Avalonia!";
#pragma warning restore CA1822 // Mark members as static

  [ObservableProperty] private IList<string> _tabs;

  public MainViewModel() {
    _ = MemCache.Initialize();
    Tabs = new List<string> { "Menu", "Billing", "Settings", "Profile" };
  }
}