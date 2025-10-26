using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GC.Common;
using GC.Models;

namespace GC.Core.Cache;

public static class MemCache {
  public static List<InvoiceMonth>? BillingMonths { get; private set; } = [];
  public static List<Day>? Menus { get; private set; } = [];

  private static bool _isBillingLoading;
  private static bool _isOrderLoading;

  private static bool BillingLoading {
    get => _isBillingLoading;
    set {
      if (_isBillingLoading == value) return;
      var previous = IsLoading;
      _isBillingLoading = value;
      var current = IsLoading;
      if (previous != current) {
        // Invoke handlers individually so one handler throwing won't break the caller
        if (IsLoadingChanged != null) {
          foreach (var d in IsLoadingChanged.GetInvocationList()) {
            try {
              ((Action<bool>)d)(current);
            }
            catch (Exception ex) {
              Log.Debug($"IsLoadingChanged handler threw: {ex}");
            }
          }
        }
      }
      // Log loading state changes for billing
      Log.Debug($"Billing loading changed from {previous} to {current}");
    }
  }

  private static bool OrderLoading {
    get => _isOrderLoading;
    set {
      if (_isOrderLoading == value) return;
      var previous = IsLoading;
      _isOrderLoading = value;
      var current = IsLoading;
      if (previous != current) {
        // Invoke handlers individually so one handler throwing won't break the caller
        if (IsLoadingChanged != null) {
          foreach (var d in IsLoadingChanged.GetInvocationList()) {
            try {
              ((Action<bool>)d)(current);
            }
            catch (Exception ex) {
              Log.Debug($"IsLoadingChanged handler threw: {ex}");
            }
          }
        }
      }
      // Log loading state changes for orders
      Log.Debug($"Order loading changed from {previous} to {current}");
    }
  }

  public static event Action<bool>? IsLoadingChanged;

  public static bool IsLoading => _isBillingLoading || _isOrderLoading;

  public static async Task Initialize() {
    Log.Debug("Starting initialization of caches");
    await RefreshBillingMonthsAsync();
    await RefreshOrderDaysAsync();
    Log.Debug("Finished initialization of caches");
  }

  public static async Task RefreshBillingMonthsAsync() {
    BillingLoading = true;
    Log.Debug("Billing refresh started");
    try {
      BillingMonths?.Clear();
      BillingMonths = (List<InvoiceMonth>)await InvoiceCache.GetAsync();
      Log.Debug($"Completed loading {BillingMonths?.Count ?? 0} billing months");
    }
    catch (Exception ex) {
      Log.Error($"Exception while loading billing months: {ex}");
    }
    finally {
      BillingLoading = false;
      Log.Debug("Billing refresh finished");
    }
  }

  public static async Task RefreshOrderDaysAsync() {
    OrderLoading = true;
    Log.Debug("Order refresh started");
    try {
      Menus?.Clear();
      Menus = (List<Day>)await MenuCache.GetAsync();
      Log.Debug($"Completed loading {Menus?.Count ?? 0} order days");
    }
    catch (Exception ex) {
      Log.Error($"Exception while loading order days: {ex}");
    }
    finally {
      OrderLoading = false;
      Log.Debug("Order refresh finished");
    }
  }
}