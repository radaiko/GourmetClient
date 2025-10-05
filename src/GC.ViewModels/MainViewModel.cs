using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace GC.ViewModels;

public partial class MainViewModel : ObservableObject {
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
}

