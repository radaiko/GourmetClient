using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GourmetClient.Maui.Core.Model;
using GourmetClient.Maui.Core.Network;
using GourmetClient.Maui.Core.Settings;

namespace GourmetClient.Maui.ViewModels;

public partial class OrdersViewModel : ObservableObject
{
    private readonly GourmetCacheService _cacheService;
    private readonly GourmetSettingsService _settingsService;

    [ObservableProperty]
    private ObservableCollection<OrderGroupViewModel> _groupedOrders = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isShowingUpcoming = true;

    [ObservableProperty]
    private int _pendingCancelsCount;

    private GourmetUserInformation? _userInformation;
    private bool _hasLoadedOnce;

    public OrdersViewModel(GourmetCacheService cacheService, GourmetSettingsService settingsService)
    {
        _cacheService = cacheService;
        _settingsService = settingsService;
    }

    public bool IsShowingPast => !IsShowingUpcoming;
    public bool HasOrders => GroupedOrders.Any();
    public bool HasNoOrders => !HasOrders && !IsLoading;
    public string EmptyStateText => IsShowingUpcoming ? "No upcoming orders" : "No past orders";
    public bool HasPendingCancels => PendingCancelsCount > 0;
    public string PendingCancelsText => $"Cancel ({PendingCancelsCount})";

    public async void OnAppearing()
    {
        // Only load once on first appearance to prevent multiple login attempts
        if (!_hasLoadedOnce)
        {
            _hasLoadedOnce = true;
            await LoadOrders();
        }
    }

    public void ForceRefresh()
    {
        _hasLoadedOnce = false;
    }

    [RelayCommand]
    private async Task ShowUpcoming()
    {
        IsShowingUpcoming = true;
        OnPropertyChanged(nameof(IsShowingPast));
        await LoadOrders();
    }

    [RelayCommand]
    private async Task ShowPast()
    {
        IsShowingUpcoming = false;
        OnPropertyChanged(nameof(IsShowingPast));
        await LoadOrders();
    }

    [RelayCommand]
    private async Task ExecuteCancels()
    {
        if (_userInformation == null)
            return;

        var pendingCancels = GetPendingCancels();
        if (!pendingCancels.Any())
            return;

        var confirmed = await Shell.Current.DisplayAlert(
            "Confirm Cancellation",
            $"Cancel {pendingCancels.Count} order(s)?",
            "Yes, Cancel",
            "No");

        if (confirmed)
        {
            IsLoading = true;
            try
            {
                await _cacheService.UpdateOrderedMenu(_userInformation, [], pendingCancels);
                await LoadOrders();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    private async Task LoadOrders()
    {
        // Prevent concurrent loading
        if (IsLoading)
            return;

        IsLoading = true;

        try
        {
            var cache = await _cacheService.GetCache();
            _userInformation = cache.UserInformation;

            var today = DateTime.Today;
            var orders = cache.OrderedMenus
                .Where(o => IsShowingUpcoming ? o.Day.Date >= today : o.Day.Date < today)
                .OrderBy(o => IsShowingUpcoming ? o.Day : DateTime.MaxValue - (o.Day - DateTime.MinValue))
                .ToList();

            var groups = orders
                .GroupBy(o => o.Day.Date)
                .Select(g => new OrderGroupViewModel(g.Key, g.Select(o => new OrderItemViewModel(o, this)).ToList()))
                .ToList();

            GroupedOrders = new ObservableCollection<OrderGroupViewModel>(groups);
            OnPropertyChanged(nameof(HasOrders));
            OnPropertyChanged(nameof(HasNoOrders));
            UpdatePendingCancelsCount();
        }
        finally
        {
            IsLoading = false;
        }
    }

    internal void UpdatePendingCancelsCount()
    {
        var count = GroupedOrders
            .SelectMany(g => g)
            .Count(o => o.IsPendingCancel);

        PendingCancelsCount = count;
        OnPropertyChanged(nameof(HasPendingCancels));
        OnPropertyChanged(nameof(PendingCancelsText));
    }

    private List<GourmetOrderedMenu> GetPendingCancels()
    {
        return GroupedOrders
            .SelectMany(g => g)
            .Where(o => o.IsPendingCancel)
            .Select(o => o.Order)
            .ToList();
    }
}

public class OrderGroupViewModel : ObservableCollection<OrderItemViewModel>
{
    public OrderGroupViewModel(DateTime date, IEnumerable<OrderItemViewModel> items) : base(items)
    {
        Date = date;
    }

    public DateTime Date { get; }
    public string DateHeader => Date.ToString("dddd, MMM d");
}

public partial class OrderItemViewModel : ObservableObject
{
    private readonly OrdersViewModel _parent;

    [ObservableProperty]
    private bool _isPendingCancel;

    public OrderItemViewModel(GourmetOrderedMenu order, OrdersViewModel parent)
    {
        Order = order;
        _parent = parent;
    }

    public GourmetOrderedMenu Order { get; }

    public string Title => Order.MenuName;
    public string CategoryName => "Order"; // GourmetOrderedMenu doesn't have Category
    public bool CanCancel => Order.Day.Date >= DateTime.Today && Order.IsOrderCancelable;
    public string StatusText => Order.IsOrderApproved ? "Confirmed" : "Pending";
    public Color StatusColor => Order.IsOrderApproved ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FFC107");

    [RelayCommand]
    private void CancelOrder()
    {
        if (!CanCancel) return;

        IsPendingCancel = !IsPendingCancel;
        _parent.UpdatePendingCancelsCount();
    }
}
