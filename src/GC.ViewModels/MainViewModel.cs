using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GC.ViewModels;

public partial class MainViewModel : ObservableObject {
  [ObservableProperty]
  private string _greeting = "Welcome to Gourmet Client MVVM!";

  [RelayCommand]
  private void UpdateGreeting() {
    Greeting = $"Updated at {DateTime.Now:HH:mm:ss}";
  }
}

