using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GourmetClient.Maui.Core.Model;
using GourmetClient.Maui.Core.Network;
using GourmetClient.Maui.Core.Settings;

namespace GourmetClient.Maui.ViewModels;

public partial class BillingViewModel : ObservableObject
{
    private readonly BillingCacheService _cacheService;
    private readonly GourmetSettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<string> _availableMonths = [];

    [ObservableProperty]
    private string? _selectedMonth;

    [ObservableProperty]
    private ObservableCollection<BillingItemViewModel> _foodItems = [];

    [ObservableProperty]
    private ObservableCollection<BillingItemViewModel> _drinkItems = [];

    [ObservableProperty]
    private ObservableCollection<BillingItemViewModel> _otherItems = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _isFoodExpanded = true;

    [ObservableProperty]
    private bool _isDrinksExpanded = true;

    [ObservableProperty]
    private bool _isOtherExpanded = true;

    [ObservableProperty]
    private decimal _foodTotal;

    [ObservableProperty]
    private decimal _drinksTotal;

    [ObservableProperty]
    private decimal _otherTotal;

    private readonly List<(int month, int year)> _monthYearPairs = [];
    private bool _hasLoadedOnce;

    public BillingViewModel(BillingCacheService cacheService, GourmetSettingsService settingsService)
    {
        _cacheService = cacheService;
        _settingsService = settingsService;
        InitializeMonths();
    }

    public bool HasFoodItems => FoodItems.Any();
    public bool HasDrinkItems => DrinkItems.Any();
    public bool HasOtherItems => OtherItems.Any();
    public bool HasAnyItems => HasFoodItems || HasDrinkItems || HasOtherItems;
    public bool HasNoItems => !HasAnyItems && !IsLoading;
    public decimal GrandTotal => FoodTotal + DrinksTotal + OtherTotal;

    public async void OnAppearing()
    {
        // Only load once on first appearance to prevent multiple login attempts
        if (!_hasLoadedOnce)
        {
            _hasLoadedOnce = true;
            await LoadBilling();
        }
    }

    public void ForceRefresh()
    {
        _hasLoadedOnce = false;
    }

    private void InitializeMonths()
    {
        var months = new List<string>();
        var today = DateTime.Today;

        for (int i = 0; i < 4; i++)
        {
            var date = today.AddMonths(-i);
            months.Add(date.ToString("MMMM yyyy", CultureInfo.CurrentCulture));
            _monthYearPairs.Add((date.Month, date.Year));
        }

        AvailableMonths = new ObservableCollection<string>(months);
        SelectedMonth = months.FirstOrDefault();
    }

    partial void OnSelectedMonthChanged(string? value)
    {
        if (value != null)
        {
            _ = LoadBilling();
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadBilling();
        IsRefreshing = false;
    }

    [RelayCommand]
    private void ToggleFoodExpanded()
    {
        IsFoodExpanded = !IsFoodExpanded;
    }

    [RelayCommand]
    private void ToggleDrinksExpanded()
    {
        IsDrinksExpanded = !IsDrinksExpanded;
    }

    [RelayCommand]
    private void ToggleOtherExpanded()
    {
        IsOtherExpanded = !IsOtherExpanded;
    }

    private async Task LoadBilling()
    {
        if (SelectedMonth == null) return;

        // Prevent concurrent loading
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            // Get month/year from selected
            var monthIndex = AvailableMonths.IndexOf(SelectedMonth);
            if (monthIndex < 0 || monthIndex >= _monthYearPairs.Count)
                return;

            var (month, year) = _monthYearPairs[monthIndex];
            var progress = new Progress<int>();

            var positions = await _cacheService.GetBillingPositions(month, year, progress);

            // Group by type
            var food = positions
                .Where(p => p.PositionType == BillingPositionType.Menu)
                .Select(p => new BillingItemViewModel(p))
                .ToList();

            var drinks = positions
                .Where(p => p.PositionType == BillingPositionType.Drink)
                .Select(p => new BillingItemViewModel(p))
                .ToList();

            var other = positions
                .Where(p => p.PositionType == BillingPositionType.Unknown)
                .Select(p => new BillingItemViewModel(p))
                .ToList();

            FoodItems = new ObservableCollection<BillingItemViewModel>(food);
            DrinkItems = new ObservableCollection<BillingItemViewModel>(drinks);
            OtherItems = new ObservableCollection<BillingItemViewModel>(other);

            FoodTotal = (decimal)food.Sum(f => f.Total);
            DrinksTotal = (decimal)drinks.Sum(d => d.Total);
            OtherTotal = (decimal)other.Sum(o => o.Total);

            OnPropertyChanged(nameof(HasFoodItems));
            OnPropertyChanged(nameof(HasDrinkItems));
            OnPropertyChanged(nameof(HasOtherItems));
            OnPropertyChanged(nameof(HasAnyItems));
            OnPropertyChanged(nameof(HasNoItems));
            OnPropertyChanged(nameof(GrandTotal));
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class BillingItemViewModel
{
    public BillingItemViewModel(BillingPosition position)
    {
        Position = position;
    }

    public BillingPosition Position { get; }

    public string Name => Position.PositionName;
    public int Count => Position.Count;
    public double Total => Position.SumCost;

    public string CountText => $"{Count}×";
    public string TotalText => Total.ToString("C", CultureInfo.CurrentCulture);
}
