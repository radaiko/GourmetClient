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
  [ObservableProperty] private List<InvoiceMonth> _availableMonths = [];
  [ObservableProperty] private bool _isLoading = true;
  
  public BillingViewModel() {
    Logger.Debug("Initializing BillingViewModel and setting up AvailableMonths.");
    AvailableMonths = MemCache.BillingMonths ?? [];
    Logger.Debug("Subscribing to MemCache.BillingMonths.CollectionChanged event.");
    
    MemCache.IsLoadingChanged += isLoading => {
      IsLoading = isLoading;
      if (!isLoading) {
        Logger.Debug("Loading is done. Update Billing months in BillingViewModel.");
        AvailableMonths = MemCache.BillingMonths ?? [];
      }
    };
  }
  
  [RelayCommand] private static async Task Refresh() {
    Logger.Info("Refreshing billing data in MemCache.");
    await MemCache.RefreshBillingMonthsAsync();
  }
}