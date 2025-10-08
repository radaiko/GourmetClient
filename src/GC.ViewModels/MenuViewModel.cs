using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GC.Core.Model;
using GC.Core.Network;
using GC.Core.Settings;
using Microsoft.Extensions.Logging;

namespace GC.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private readonly GourmetWebClient _gourmetClient;
    private readonly GourmetSettingsService _settingsService;
    private readonly ILogger<MenuViewModel> _logger;
    private GourmetUserInformation? _userInformation; // user context for ordering

    public MenuViewModel(GourmetWebClient gourmetClient, GourmetSettingsService settingsService, ILogger<MenuViewModel> logger)
    {
        _gourmetClient = gourmetClient;
        _settingsService = settingsService;
        _logger = logger;
    }

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private int _loadingProgress;

    [ObservableProperty]
    private ObservableCollection<MenuDayViewModel> _menuDays = new();

    [ObservableProperty]
    private int _currentMenuDayIndex = -1;

    [ObservableProperty]
    private bool _isApplyingChanges; // expose for UI

    public bool HasPendingChanges => MenuDays.Any(d => d.Menus.Any(m => m.IsMarkedForOrder || m.IsMarkedForCancel));
    public int PendingAdditionsCount => MenuDays.Sum(d => d.Menus.Count(m => m.IsMarkedForOrder));
    public int PendingCancellationsCount => MenuDays.Sum(d => d.Menus.Count(m => m.IsMarkedForCancel));
    private void RaisePendingChanges() {
        OnPropertyChanged(nameof(HasPendingChanges));
        OnPropertyChanged(nameof(PendingAdditionsCount));
        OnPropertyChanged(nameof(PendingCancellationsCount));
    }

    [RelayCommand]
    private async Task RefreshMenusAsync()
    {
        // Force refresh by clearing existing data
        MenuDays.Clear();
        ErrorMessage = null;
        await LoadMenusAsync();
    }

    [RelayCommand]
    private async Task LoadMenusAsync()
    {
        // Prevent concurrent loads or reloading if already loaded
        if (IsLoading || (MenuDays.Count > 0 && ErrorMessage == null))
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;
            LoadingProgress = 0;

            var settings = _settingsService.GetCurrentUserSettings();
            if (string.IsNullOrEmpty(settings.GourmetLoginUsername) || string.IsNullOrEmpty(settings.GourmetLoginPassword))
            {
                ErrorMessage = "Bitte Anmeldedaten in den Einstellungen konfigurieren";
                return;
            }

            LoadingProgress = 10; // Starting login
            var result = await _gourmetClient.Login(settings.GourmetLoginUsername, settings.GourmetLoginPassword);
            if (!result.LoginSuccessful)
            {
                ErrorMessage = "Anmeldung fehlgeschlagen. Bitte überprüfen Sie Ihre Zugangsdaten.";
                return;
            }
            
            LoadingProgress = 30; // Login successful, loading menus
            var menuResult = await _gourmetClient.GetMenus();
            _userInformation = menuResult.UserInformation;

            LoadingProgress = 60; // Menus loaded, loading orders
            var orderedMenuResult = await _gourmetClient.GetOrderedMenus();

            LoadingProgress = 80; // Processing data

            // Group menus by day
            var menuDaysDict = menuResult.Menus
                .GroupBy(m => m.Day.Date)
                .ToDictionary(g => g.Key, g => g.ToList());

            var menuDaysList = new ObservableCollection<MenuDayViewModel>();
            
            foreach (var kvp in menuDaysDict.OrderBy(kv => kv.Key))
            {
                var date = kvp.Key;
                var menusForDay = kvp.Value;
                
                var menuViewModels = menusForDay.Select(menu =>
                {
                    var orderedMenu = orderedMenuResult.OrderedMenus.FirstOrDefault(om => om.MatchesMenu(menu));
                    var isOrdered = orderedMenu != null;
                    var isOrderApproved = orderedMenu?.IsOrderApproved ?? false;
                    var isOrderCancelable = orderedMenu?.IsOrderCancelable ?? false;

                    return new MenuItemViewModel
                    {
                        MenuId = menu.MenuId,
                        MenuDescription = menu.Description,
                        Allergens = menu.Allergens,
                        IsAvailable = menu.IsAvailable,
                        IsOrdered = isOrdered,
                        IsOrderApproved = isOrderApproved,
                        IsOrderCancelable = isOrderCancelable,
                        Category = menu.Category,
                        SourceMenu = menu
                    };
                }).ToList();

                menuDaysList.Add(new MenuDayViewModel
                {
                    Date = date,
                    Menus = new ObservableCollection<MenuItemViewModel>(menuViewModels)
                });
            }

            MenuDays = menuDaysList;

            LoadingProgress = 100; // Complete

            // Set initial day index to today or next available day
            var today = DateTime.Today;
            CurrentMenuDayIndex = MenuDays.ToList().FindIndex(d => d.Date.Date == today);
            if (CurrentMenuDayIndex < 0)
                CurrentMenuDayIndex = MenuDays.ToList().FindIndex(d => d.Date.Date > today);
            if (CurrentMenuDayIndex < 0)
                CurrentMenuDayIndex = 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load menus");
            ErrorMessage = $"Fehler beim Laden der Menüs: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private Task ToggleMenuOrder(ToggleMenuOrderParameter parameter)
    {
        try
        {
            var menu = parameter.Menu;
            if (!menu.IsAvailable) return Task.CompletedTask;
            if (menu.IsOrdered && !menu.IsOrderCancelable) return Task.CompletedTask;
            if (menu.IsMarkedForOrder) menu.IsMarkedForOrder = false;
            else if (menu.IsMarkedForCancel) menu.IsMarkedForCancel = false;
            else if (menu.IsOrdered && menu.IsOrderCancelable) menu.IsMarkedForCancel = true;
            else menu.IsMarkedForOrder = true;
            OnPropertyChanged(nameof(MenuDays));
            RaisePendingChanges();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle menu order");
            ErrorMessage = $"Fehler: {ex.Message}";
            return Task.CompletedTask;
        }
    }

    [RelayCommand]
    private async Task ApplyOrderChangesAsync()
    {
        if (IsApplyingChanges || _userInformation == null)
        {
            if (_userInformation == null) _logger?.LogWarning("Cannot apply changes: missing user info");
            return;
        }
        var additions = MenuDays.SelectMany(d => d.Menus).Where(m => m.IsMarkedForOrder).ToList();
        var cancellations = MenuDays.SelectMany(d => d.Menus).Where(m => m.IsMarkedForCancel && m.IsOrdered).ToList();
        if (additions.Count == 0 && cancellations.Count == 0) return; // nothing to do
        try
        {
            IsApplyingChanges = true;
            IsLoading = true;
            LoadingProgress = 5;
            // Get current ordered menus to map cancellation
            var orderedResult = await _gourmetClient.GetOrderedMenus();
            var orderedMenus = orderedResult.OrderedMenus;
            LoadingProgress = 15;
            // Additions
            int totalAdd = additions.Count; int processed = 0;
            foreach (var add in additions)
            {
                if (add.SourceMenu != null)
                {
                    var apiResult = await _gourmetClient.AddMenuToOrderedMenu(_userInformation, add.SourceMenu);
                    _logger?.LogInformation("AddMenu {MenuId} success={Success} message={Message}", add.MenuId, apiResult.Success, apiResult.Message);
                }
                processed++;
                LoadingProgress = 15 + (int)(processed / Math.Max(1.0, totalAdd) * 35); // up to 50
            }
            // Cancellations
            LoadingProgress = 55;
            if (cancellations.Any())
            {
                var toCancel = orderedMenus
                    .Where(om => cancellations.Any(c => c.SourceMenu != null && om.MatchesMenu(c.SourceMenu)))
                    .ToList();
                if (toCancel.Any()) await _gourmetClient.CancelOrders(toCancel);
            }
            // Confirm
            LoadingProgress = 75;
            await _gourmetClient.ConfirmOrder();
            // Clear flags locally
            foreach (var day in MenuDays)
                foreach (var item in day.Menus) { item.IsMarkedForOrder = false; item.IsMarkedForCancel = false; }
            RaisePendingChanges();
            LoadingProgress = 85;
            // Reload to reflect true server state
            MenuDays.Clear();
            await LoadMenusAsync();
            LoadingProgress = 100;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ApplyOrderChanges failed");
            ErrorMessage = $"Fehler beim Bestätigen: {ex.Message}";
        }
        finally
        {
            IsApplyingChanges = false;
            IsLoading = false;
        }
    }
}

public class MenuDayViewModel : ObservableObject
{
    public DateTime Date { get; set; }
    public ObservableCollection<MenuItemViewModel> Menus { get; set; } = new();
}

public partial class MenuItemViewModel : ObservableObject
{
    public string MenuId { get; set; } = "";
    public string MenuDescription { get; set; } = "";
    public char[] Allergens { get; set; } = Array.Empty<char>();
    public bool IsAvailable { get; set; }
    public GourmetMenuCategory Category { get; set; }

    [ObservableProperty]
    private bool _isOrdered;

    [ObservableProperty]
    private bool _isOrderApproved;

    [ObservableProperty]
    private bool _isOrderCancelable;

    [ObservableProperty]
    private bool _isMarkedForOrder;

    [ObservableProperty]
    private bool _isMarkedForCancel;

    internal GourmetMenu? SourceMenu { get; set; }

    public MenuState State
    {
        get
        {
            if (!IsAvailable) return MenuState.NotAvailable;
            if (IsMarkedForOrder) return MenuState.MarkedForOrder;
            if (IsMarkedForCancel) return MenuState.MarkedForCancel;
            if (IsOrdered) return MenuState.Ordered;
            return MenuState.None;
        }
    }
}

public enum MenuState
{
    None,
    NotAvailable,
    MarkedForOrder,
    MarkedForCancel,
    Ordered
}

public record ToggleMenuOrderParameter(DateTime Date, MenuItemViewModel Menu);
