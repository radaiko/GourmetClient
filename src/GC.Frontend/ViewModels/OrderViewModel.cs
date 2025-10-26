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
  [ObservableProperty] private ObservableCollection<Day> _availableDays = [];
  
  public OrderViewModel() {
    AvailableDays = MemCache.Menus;
    MemCache.Menus.CollectionChanged += (_, _) => {
      AvailableDays = MemCache.Menus;
    };
  }
  
  [RelayCommand] private static async Task Refresh() {
    Log.Info("Refreshing data in MemCache.");
    await MemCache.RefreshOrderDaysAsync();
  }
}