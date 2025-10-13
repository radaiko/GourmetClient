using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GC.Cache.Menu;
using GC.Core.Model;
using GC.Core.Network;
using GC.Core.Settings;
using GC.Database;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace GC.ViewModels;

public partial class MenuViewModel(GourmetWebClient gourmetClient, GourmetSettingsService settingsService, ILogger<MenuViewModel> logger, SqliteService sqliteService)
  : ObservableObject {
  private readonly MenuCacheBase _menuCache = new MenuCache(sqliteService); // or MenuCacheDebug based on condition
  private GourmetUserInformation? _userInformation; // user context for ordering

  [ObservableProperty] private bool _isLoading;
  [ObservableProperty] private string? _errorMessage;
  [ObservableProperty] private int _loadingProgress;
  [ObservableProperty] private ObservableCollection<MenuDayViewModel> _menuDays = new();
  [ObservableProperty] private int _currentMenuDayIndex = -1;
  [ObservableProperty] private bool _isApplyingChanges;
  [ObservableProperty] private bool _LoginFailed;

  public bool HasPendingChanges => MenuDays.Any(d => d.Menus.Any(m => m.IsMarkedForOrder || m.IsMarkedForCancel));
  public int PendingAdditionsCount => MenuDays.Sum(d => d.Menus.Count(m => m.IsMarkedForOrder));
  public int PendingCancellationsCount => MenuDays.Sum(d => d.Menus.Count(m => m.IsMarkedForCancel));
  private void RaisePendingChanges() {
    OnPropertyChanged(nameof(HasPendingChanges));
    OnPropertyChanged(nameof(PendingAdditionsCount));
    OnPropertyChanged(nameof(PendingCancellationsCount));
  }

  [RelayCommand]
  private async Task RefreshMenusAsync() {
    logger.LogInformation("Refreshing menus");
    MenuDays.Clear();
    ErrorMessage = null;
    if (!LoginFailed)
      await LoadMenusAsync();
  }

  [RelayCommand]
  private async Task LoadMenusAsync() {
    if (IsLoading || (MenuDays.Count > 0 && ErrorMessage == null) || LoginFailed) return;

    logger.LogInformation("Starting to load menus");
    IsLoading = true;
    ErrorMessage = null;
    LoadingProgress = 0;

    try {
      var settings = settingsService.GetCurrentUserSettings();
      if (string.IsNullOrEmpty(settings.GourmetLoginUsername) || string.IsNullOrEmpty(settings.GourmetLoginPassword)) {
        logger.LogWarning("Credentials missing in settings");
        ErrorMessage = "Bitte Anmeldedaten in den Einstellungen konfigurieren";
        return;
      }

      var progress = new Progress<int>(p => LoadingProgress = p);

      // Offload network + processing work to background thread; awaited tasks will not block UI anyway,
      // but grouping them inside Task.Run ensures any synchronous CPU grouping happens off UI thread.
      var result = await Task.Run(async () => await InternalLoadMenusAsync(settings.GourmetLoginUsername!, settings.GourmetLoginPassword!, progress));

      if (!string.IsNullOrEmpty(result.Error)) {
        logger.LogError("Failed to load menus: {Error}", result.Error);
        ErrorMessage = result.Error;
        LoginFailed = true;
        return;
      }

      _userInformation = result.UserInformation;
      MenuDays = new ObservableCollection<MenuDayViewModel>(result.MenuDays ?? new System.Collections.Generic.List<MenuDayViewModel>());
      LoadingProgress = 100;

      // Pick initial index (today -> next future -> first)
      var today = DateTime.Today;
      CurrentMenuDayIndex = MenuDays.ToList().FindIndex(d => d.Date.Date == today);
      if (CurrentMenuDayIndex < 0)
        CurrentMenuDayIndex = MenuDays.ToList().FindIndex(d => d.Date.Date > today);
      if (CurrentMenuDayIndex < 0)
        CurrentMenuDayIndex = 0;

      logger.LogInformation("Menus loaded successfully, {Count} days", MenuDays.Count);
    }
    catch (Exception ex) {
      logger.LogError(ex, "Failed to load menus");
      ErrorMessage = $"Fehler beim Laden der Menüs: {ex.Message}";
      LoginFailed = true;
    }
    finally {
      IsLoading = false;
    }
  }

  private async Task<LoadMenusWorkResult> InternalLoadMenusAsync(string username, string password, IProgress<int> progress) {
    try {
      // Check cache first
      var cachedDataJson = await _menuCache.GetCachedDataAsync();
      if (cachedDataJson != null) {
        var lastWrite = await _menuCache.GetLastWriteAsync();
        if (lastWrite.HasValue && (DateTime.UtcNow - lastWrite.Value).TotalHours < 1) {
          try {
            var cachedData = JsonSerializer.Deserialize<CachedMenuData>(cachedDataJson);
            if (cachedData?.MenuResult != null && cachedData.OrderedResult != null) {
              logger.LogInformation("Using cached menu data");
              progress.Report(80);
              var result = ProcessMenuData(cachedData.MenuResult, cachedData.OrderedResult);
              return result;
            }
          }
          catch (Exception ex) {
            logger.LogWarning(ex, "Failed to deserialize cached menu data");
          }
        }
      }

      logger.LogInformation("Attempting login for user {Username}", username);
      progress.Report(10);
      var loginResult = await gourmetClient.Login(username, password);
      if (!loginResult.LoginSuccessful) {
        logger.LogWarning("Login failed for user {Username}", username);
        return new LoadMenusWorkResult { Error = "Anmeldung fehlgeschlagen. Bitte überprüfen Sie Ihre Zugangsdaten." };
      }
      logger.LogInformation("Login successful for user {Username}", username);

      logger.LogInformation("Fetching menus");
      progress.Report(30);
      var menuResult = await gourmetClient.GetMenus();

      logger.LogInformation("Fetching ordered menus");
      progress.Report(60);
      var orderedMenuResult = await gourmetClient.GetOrderedMenus();

      // Save to cache
      var cachedDataToSave = new CachedMenuData { MenuResult = menuResult, OrderedResult = orderedMenuResult };
      var json = JsonSerializer.Serialize(cachedDataToSave);
      await _menuCache.SetCachedDataAsync(json);

      logger.LogInformation("Processing menu data");
      progress.Report(80);
      var processedResult = ProcessMenuData(menuResult, orderedMenuResult);
      return processedResult;
    }
    catch (Exception ex) {
      logger.LogError(ex, "Error in InternalLoadMenusAsync");
      return new LoadMenusWorkResult { Error = $"Fehler beim Laden der Menüs: {ex.Message}" };
    }
  }

  private LoadMenusWorkResult ProcessMenuData(GourmetMenuResult menuResult, GourmetOrderedMenuResult orderedMenuResult) {
    var menuDaysDict = menuResult.Menus
      .GroupBy(m => m.Day.Date)
      .OrderBy(g => g.Key)
      .ToList();

    var list = new System.Collections.Generic.List<MenuDayViewModel>();
    foreach (var dayGroup in menuDaysDict) {
      var menuVMs = dayGroup
        .Select(menu => {
          var ordered = orderedMenuResult.OrderedMenus.FirstOrDefault(om => om.MatchesMenu(menu));
          return new MenuItemViewModel {
            MenuId = menu.MenuId,
            MenuDescription = menu.Description,
            Allergens = menu.Allergens,
            IsAvailable = menu.IsAvailable,
            IsOrdered = ordered != null,
            IsOrderApproved = ordered?.IsOrderApproved ?? false,
            IsOrderCancelable = ordered?.IsOrderCancelable ?? false,
            Category = menu.Category,
            SourceMenu = menu
          };
        })
        .ToList();

      list.Add(new MenuDayViewModel {
        Date = dayGroup.Key,
        Menus = new ObservableCollection<MenuItemViewModel>(menuVMs)
      });
    }

    logger.LogInformation("Menu processing completed, {DayCount} days processed", list.Count);
    return new LoadMenusWorkResult {
      UserInformation = menuResult.UserInformation,
      MenuDays = list
    };
  }

  private class LoadMenusWorkResult {
    public string? Error { get; set; }
    public GourmetUserInformation? UserInformation { get; set; }
    public System.Collections.Generic.List<MenuDayViewModel>? MenuDays { get; set; }
  }

  private class CachedMenuData {
    public GourmetMenuResult? MenuResult { get; set; }
    public GourmetOrderedMenuResult? OrderedResult { get; set; }
  }

  [RelayCommand]
  private Task ToggleMenuOrder(ToggleMenuOrderParameter parameter) {
    try {
      var menu = parameter.Menu;
      logger.LogInformation("Toggling order for menu {MenuId}", menu.MenuId);
      if (!menu.IsAvailable) {
        logger.LogWarning("Menu {MenuId} is not available, cannot toggle", menu.MenuId);
        return Task.CompletedTask;
      }
      if (menu.IsOrdered && !menu.IsOrderCancelable) {
        logger.LogWarning("Menu {MenuId} is ordered but not cancelable, cannot toggle", menu.MenuId);
        return Task.CompletedTask;
      }
      if (menu.IsMarkedForOrder) menu.IsMarkedForOrder = false;
      else if (menu.IsMarkedForCancel) menu.IsMarkedForCancel = false;
      else if (menu.IsOrdered && menu.IsOrderCancelable) menu.IsMarkedForCancel = true;
      else menu.IsMarkedForOrder = true;
      OnPropertyChanged(nameof(MenuDays));
      RaisePendingChanges();
      logger.LogInformation("Order toggled for menu {MenuId}, new state: {State}", menu.MenuId, menu.State);
      return Task.CompletedTask;
    }
    catch (Exception ex) {
      logger.LogError(ex, "Failed to toggle menu order for {MenuId}", parameter.Menu.MenuId);
      ErrorMessage = $"Fehler: {ex.Message}";
      return Task.CompletedTask;
    }
  }

  [RelayCommand]
  private async Task ApplyOrderChangesAsync() {
    if (IsApplyingChanges || _userInformation == null) {
      if (_userInformation == null) logger.LogWarning("Cannot apply changes: missing user info");
      return;
    }

    var additions = MenuDays.SelectMany(d => d.Menus).Where(m => m.IsMarkedForOrder).ToList();
    var cancellations = MenuDays.SelectMany(d => d.Menus).Where(m => m.IsMarkedForCancel && m.IsOrdered).ToList();
    if (additions.Count == 0 && cancellations.Count == 0) {
      logger.LogInformation("No changes to apply");
      return;
    }

    logger.LogInformation("Applying order changes: {Additions} additions, {Cancellations} cancellations", additions.Count, cancellations.Count);
    try {
      IsApplyingChanges = true;
      IsLoading = true;
      LoadingProgress = 5;

      var orderedResult = await gourmetClient.GetOrderedMenus();
      var orderedMenus = orderedResult.OrderedMenus;
      LoadingProgress = 15;

      int totalAdd = additions.Count;
      int processed = 0;
      foreach (var add in additions) {
        logger.LogInformation("Adding menu {MenuId} to order", add.MenuId);
        if (add.SourceMenu != null) {
          var apiResult = await gourmetClient.AddMenuToOrderedMenu(_userInformation, add.SourceMenu);
          logger.LogInformation("AddMenu {MenuId} success={Success} message={Message}", add.MenuId, apiResult.Success, apiResult.Message);
        }
        processed++;
        LoadingProgress = 15 + (int)(processed / Math.Max(1.0, totalAdd) * 35); // up to 50
      }

      LoadingProgress = 55;
      if (cancellations.Any()) {
        logger.LogInformation("Cancelling {Count} orders", cancellations.Count);
        var toCancel = orderedMenus
          .Where(om => cancellations.Any(c => c.SourceMenu != null && om.MatchesMenu(c.SourceMenu)))
          .ToList();
        if (toCancel.Any()) await gourmetClient.CancelOrders(toCancel);
      }

      LoadingProgress = 75;
      logger.LogInformation("Confirming order");
      await gourmetClient.ConfirmOrder();

      foreach (var day in MenuDays)
      foreach (var item in day.Menus) {
        item.IsMarkedForOrder = false;
        item.IsMarkedForCancel = false;
      }
      RaisePendingChanges();
      LoadingProgress = 85;

      MenuDays.Clear();
      await LoadMenusAsync();
      LoadingProgress = 100;
      logger.LogInformation("Order changes applied successfully");
    }
    catch (Exception ex) {
      logger.LogError(ex, "ApplyOrderChanges failed");
      ErrorMessage = $"Fehler beim Bestätigen: {ex.Message}";
    }
    finally {
      IsApplyingChanges = false;
      IsLoading = false;
    }
  }
}

public class MenuDayViewModel : ObservableObject {
  public DateTime Date { get; set; }
  public ObservableCollection<MenuItemViewModel> Menus { get; set; } = new();
}

public partial class MenuItemViewModel : ObservableObject {
  public string MenuId { get; set; } = "";
  public string MenuDescription { get; set; } = "";
  public char[] Allergens { get; set; } = Array.Empty<char>();
  public bool IsAvailable { get; set; }
  public GourmetMenuCategory Category { get; set; }

  [ObservableProperty] private bool _isOrdered;
  [ObservableProperty] private bool _isOrderApproved;
  [ObservableProperty] private bool _isOrderCancelable;
  [ObservableProperty] private bool _isMarkedForOrder;
  [ObservableProperty] private bool _isMarkedForCancel;

  internal GourmetMenu? SourceMenu { get; set; }

  public MenuState State => !IsAvailable ? MenuState.NotAvailable :
    IsMarkedForOrder ? MenuState.MarkedForOrder :
    IsMarkedForCancel ? MenuState.MarkedForCancel :
    IsOrdered ? MenuState.Ordered :
    MenuState.None;
}

public enum MenuState {
  None,
  NotAvailable,
  MarkedForOrder,
  MarkedForCancel,
  Ordered
}

public record ToggleMenuOrderParameter(DateTime Date, MenuItemViewModel Menu);