using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GourmetClient.Core.Model;
using GourmetClient.Core.Network;
using GourmetClient.Core.Settings;
using Microsoft.Extensions.Logging;

namespace GC.ViewModels;

public partial class MenuViewModel : ObservableObject
{
    private readonly GourmetWebClient _gourmetClient;
    private readonly GourmetSettingsService _settingsService;
    private readonly ILogger<MenuViewModel> _logger;

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
                        Category = menu.Category
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
    private Task ToggleMenuOrderAsync(ToggleMenuOrderParameter parameter)
    {
        try
        {
            var menu = parameter.Menu;

            if (!menu.IsAvailable)
                return Task.CompletedTask;

            if (menu.IsOrdered && !menu.IsOrderCancelable)
                return Task.CompletedTask;

            // Toggle logic
            if (menu.IsMarkedForOrder)
            {
                menu.IsMarkedForOrder = false;
            }
            else if (menu.IsMarkedForCancel)
            {
                menu.IsMarkedForCancel = false;
            }
            else if (menu.IsOrdered && menu.IsOrderCancelable)
            {
                menu.IsMarkedForCancel = true;
            }
            else
            {
                menu.IsMarkedForOrder = true;
            }
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to toggle menu order");
            ErrorMessage = $"Fehler: {ex.Message}";
            return Task.CompletedTask;
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

