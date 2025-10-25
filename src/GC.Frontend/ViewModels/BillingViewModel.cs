using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GC.Common;
using GC.Core.Cache;
using GC.Models;

namespace GC.Frontend.ViewModels;

public partial class BillingViewModel : ObservableObject {

  [ObservableProperty] private int _selectedIndex = 0;
  [ObservableProperty] private ObservableCollection<BillingMonth> _availableMonths = [];
  [ObservableProperty] private bool _isLoading = true;
  
  public BillingViewModel() {
    Log.Debug("Initializing BillingViewModel and setting up AvailableMonths.");
    AvailableMonths = MemCache.BillingMonths;
    Log.Debug($"AvailableMonths initialized with {AvailableMonths.Count} months.");
    Log.Debug("Subscribing to MemCache.BillingMonths.CollectionChanged event.");
    MemCache.BillingMonths.CollectionChanged += (_, _) => {
      Log.Debug("Billing months updated in MemCache, refreshing AvailableMonths in BillingViewModel.");
      AvailableMonths = MemCache.BillingMonths;
    };

    MemCache.IsLoadingChanged += isLoading => { IsLoading = isLoading; };
  }
  
  [RelayCommand] private static async Task Refresh() {
    Log.Info("Refreshing billing data in MemCache.");
    await MemCache.RefreshBillingMonthsAsync();
  }
}