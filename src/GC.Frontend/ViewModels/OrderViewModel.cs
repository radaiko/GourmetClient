using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GC.Common;
using GC.Core.Cache;
using GC.Models;

namespace GC.Frontend.ViewModels;

public partial class OrderViewModel : ObservableObject {
  [ObservableProperty] private int _selectedIndex = 0;
  [ObservableProperty] private List<Day> _availableDays = [];
  [ObservableProperty] private bool _isLoading = true;
  
  public OrderViewModel() {
    AvailableDays = MemCache.Menus ?? [];
    MemCache.IsLoadingChanged += isLoading => {
      IsLoading = isLoading; 
      if (!isLoading) {
        Log.Debug("Loading is done. Update Days in OrderViewModel.");
        AvailableDays = MemCache.Menus ?? [];
      }
    };
  }
  
  [RelayCommand] private static async Task Refresh() {
    Log.Info("Refreshing data in MemCache.");
    await MemCache.RefreshOrderDaysAsync();
  }
}