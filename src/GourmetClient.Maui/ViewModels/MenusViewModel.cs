using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GourmetClient.Maui.Core.Model;
using GourmetClient.Maui.Core.Network;
using GourmetClient.Maui.Core.Settings;

namespace GourmetClient.Maui.ViewModels;

public partial class MenusViewModel : ObservableObject
{
    private readonly GourmetCacheService _cacheService;
    private readonly GourmetSettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<MenuDayViewModel> _menuDays = [];

    [ObservableProperty]
    private MenuDayViewModel? _currentMenuDay;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _needsLogin;

    [ObservableProperty]
    private int _pendingChangesCount;

    private GourmetUserInformation? _userInformation;
    private bool _hasLoadedOnce;

    public MenusViewModel(GourmetCacheService cacheService, GourmetSettingsService settingsService)
    {
        _cacheService = cacheService;
        _settingsService = settingsService;
    }

    public string CurrentDayName => CurrentMenuDay?.DayName ?? "Today";
    public string CurrentDateFormatted => CurrentMenuDay?.DateFormatted ?? DateTime.Today.ToString("MMM d, yyyy");
    public bool HasPendingChanges => PendingChangesCount > 0;
    public string PendingChangesText => $"Submit ({PendingChangesCount})";

    public async void OnAppearing()
    {
        // Only load once on first appearance to prevent multiple login attempts
        if (!_hasLoadedOnce)
        {
            _hasLoadedOnce = true;
            await LoadMenus();
        }
    }

    public void ForceRefresh()
    {
        _hasLoadedOnce = false;
    }

    [RelayCommand]
    private void PreviousDay()
    {
        var currentIndex = MenuDays.IndexOf(CurrentMenuDay!);
        if (currentIndex > 0)
        {
            CurrentMenuDay = MenuDays[currentIndex - 1];
            OnPropertyChanged(nameof(CurrentDayName));
            OnPropertyChanged(nameof(CurrentDateFormatted));
        }
    }

    [RelayCommand]
    private void NextDay()
    {
        var currentIndex = MenuDays.IndexOf(CurrentMenuDay!);
        if (currentIndex < MenuDays.Count - 1)
        {
            CurrentMenuDay = MenuDays[currentIndex + 1];
            OnPropertyChanged(nameof(CurrentDayName));
            OnPropertyChanged(nameof(CurrentDateFormatted));
        }
    }

    [RelayCommand]
    private async Task GoToSettings()
    {
        await Shell.Current.GoToAsync("//SettingsPage");
    }

    [RelayCommand]
    private async Task ExecuteOrder()
    {
        if (_userInformation == null)
            return;

        var pendingOrders = GetPendingOrders();
        var pendingCancels = GetPendingCancels();

        if (!pendingOrders.Any() && !pendingCancels.Any())
            return;

        var message = $"Submit {pendingOrders.Count} orders and {pendingCancels.Count} cancellations?";
        var confirmed = await Shell.Current.DisplayAlert("Confirm Order", message, "Submit", "Cancel");

        if (confirmed)
        {
            IsLoading = true;
            try
            {
                var result = await _cacheService.UpdateOrderedMenu(_userInformation, pendingOrders, pendingCancels);
                if (result.FailedMenusToOrder.Any())
                {
                    var failedMsg = string.Join("\n", result.FailedMenusToOrder.Select(f => $"- {f.Menu.MenuName}: {f.Message}"));
                    await Shell.Current.DisplayAlert("Order Result", $"Some orders failed:\n{failedMsg}", "OK");
                }
                await LoadMenus();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    private async Task LoadMenus()
    {
        // Prevent concurrent loading
        if (IsLoading)
            return;

        var settings = _settingsService.GetCurrentUserSettings();
        if (string.IsNullOrEmpty(settings.GourmetLoginUsername) || string.IsNullOrEmpty(settings.GourmetLoginPassword))
        {
            NeedsLogin = true;
            return;
        }

        NeedsLogin = false;
        IsLoading = true;

        try
        {
            var cache = await _cacheService.GetCache();
            _userInformation = cache.UserInformation;

            var menuDays = new List<MenuDayViewModel>();
            var menusByDate = cache.Menus.GroupBy(m => m.Day.Date).OrderBy(g => g.Key);

            foreach (var dayGroup in menusByDate)
            {
                var dayVm = new MenuDayViewModel(dayGroup.Key, this);

                var categories = dayGroup
                    .GroupBy(m => m.Category)
                    .OrderBy(g => GetCategoryOrder(g.Key));

                foreach (var categoryGroup in categories)
                {
                    var categoryVm = new MenuCategoryViewModel(GetCategoryDisplayName(categoryGroup.Key));
                    foreach (var menu in categoryGroup)
                    {
                        var orderedMenu = cache.OrderedMenus.FirstOrDefault(o => o.MatchesMenu(menu));
                        var menuItemVm = new MenuItemViewModel(menu, orderedMenu, this);
                        categoryVm.Menus.Add(menuItemVm);
                    }
                    dayVm.Categories.Add(categoryVm);
                }

                menuDays.Add(dayVm);
            }

            MenuDays = new ObservableCollection<MenuDayViewModel>(menuDays);

            // Set current day to today or first available
            var today = menuDays.FirstOrDefault(d => d.Date.Date == DateTime.Today.Date);
            CurrentMenuDay = today ?? menuDays.FirstOrDefault();

            OnPropertyChanged(nameof(CurrentDayName));
            OnPropertyChanged(nameof(CurrentDateFormatted));

            UpdatePendingCount();
        }
        finally
        {
            IsLoading = false;
        }
    }

    internal void UpdatePendingCount()
    {
        var count = MenuDays
            .SelectMany(d => d.Categories)
            .SelectMany(c => c.Menus)
            .Count(m => m.IsPendingOrder || m.IsPendingCancel);

        PendingChangesCount = count;
        OnPropertyChanged(nameof(HasPendingChanges));
        OnPropertyChanged(nameof(PendingChangesText));
    }

    private List<GourmetMenu> GetPendingOrders()
    {
        return MenuDays
            .SelectMany(d => d.Categories)
            .SelectMany(c => c.Menus)
            .Where(m => m.IsPendingOrder)
            .Select(m => m.Menu)
            .ToList();
    }

    private List<GourmetOrderedMenu> GetPendingCancels()
    {
        return MenuDays
            .SelectMany(d => d.Categories)
            .SelectMany(c => c.Menus)
            .Where(m => m.IsPendingCancel && m.OrderedMenu != null)
            .Select(m => m.OrderedMenu!)
            .ToList();
    }

    private static int GetCategoryOrder(GourmetMenuCategory category) => category switch
    {
        GourmetMenuCategory.Menu1 => 0,
        GourmetMenuCategory.Menu2 => 1,
        GourmetMenuCategory.Menu3 => 2,
        GourmetMenuCategory.SoupAndSalad => 3,
        _ => 99
    };

    private static string GetCategoryDisplayName(GourmetMenuCategory category) => category switch
    {
        GourmetMenuCategory.Menu1 => "Menü I",
        GourmetMenuCategory.Menu2 => "Menü II",
        GourmetMenuCategory.Menu3 => "Menü III",
        GourmetMenuCategory.SoupAndSalad => "Suppe & Salat",
        _ => "Other"
    };
}

public partial class MenuDayViewModel : ObservableObject
{
    private readonly MenusViewModel _parent;

    public MenuDayViewModel(DateTime date, MenusViewModel parent)
    {
        Date = date;
        _parent = parent;
    }

    public DateTime Date { get; }
    public string DayName => Date.ToString("dddd");
    public string DateFormatted => Date.ToString("MMM d, yyyy");
    public ObservableCollection<MenuCategoryViewModel> Categories { get; } = [];

    public bool HasMenus => Categories.Any(c => c.Menus.Any());
    public bool HasNoMenus => !HasMenus;
}

public partial class MenuCategoryViewModel : ObservableObject
{
    public MenuCategoryViewModel(string categoryName)
    {
        CategoryName = categoryName;
    }

    public string CategoryName { get; }
    public ObservableCollection<MenuItemViewModel> Menus { get; } = [];
}

public partial class MenuItemViewModel : ObservableObject
{
    private readonly MenusViewModel _parent;

    [ObservableProperty]
    private bool _isPendingOrder;

    [ObservableProperty]
    private bool _isPendingCancel;

    public MenuItemViewModel(GourmetMenu menu, GourmetOrderedMenu? orderedMenu, MenusViewModel parent)
    {
        Menu = menu;
        OrderedMenu = orderedMenu;
        _parent = parent;
    }

    public GourmetMenu Menu { get; }
    public GourmetOrderedMenu? OrderedMenu { get; }

    public string Title => Menu.MenuName;
    public string Subtitle => Menu.Description ?? string.Empty;
    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);
    public string AllergensText => Menu.Allergens.Length > 0 ? $"Allergens: {string.Join(", ", Menu.Allergens)}" : string.Empty;
    public bool HasAllergens => Menu.Allergens.Length > 0;
    public bool IsAvailable => Menu.IsAvailable;
    public bool IsOrdered => OrderedMenu != null;

    public string StateIcon
    {
        get
        {
            if (!IsAvailable) return "⚫";
            if (IsPendingOrder) return "🟡";
            if (IsPendingCancel) return "🟠";
            if (IsOrdered) return "🟢";
            return "⚪";
        }
    }

    public Color BackgroundColor
    {
        get
        {
            if (IsPendingOrder) return Color.FromArgb("#FFF9E6");
            if (IsPendingCancel) return Color.FromArgb("#FFEBE6");
            if (IsOrdered) return Color.FromArgb("#E6F9E6");
            return Colors.Transparent;
        }
    }

    [RelayCommand]
    private void ToggleOrder()
    {
        if (!IsAvailable) return;

        if (IsOrdered)
        {
            // Toggle cancel state
            IsPendingCancel = !IsPendingCancel;
            IsPendingOrder = false;
        }
        else
        {
            // Toggle order state
            IsPendingOrder = !IsPendingOrder;
            IsPendingCancel = false;
        }

        OnPropertyChanged(nameof(StateIcon));
        OnPropertyChanged(nameof(BackgroundColor));
        _parent.UpdatePendingCount();
    }
}
