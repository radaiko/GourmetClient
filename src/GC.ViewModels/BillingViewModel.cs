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

public partial class BillingViewModel : ObservableObject
{
    private readonly VentopayWebClient _ventopayClient;
    private readonly GourmetSettingsService _settingsService;
    private readonly ILogger<BillingViewModel> _logger;

    public BillingViewModel(VentopayWebClient ventopayClient, GourmetSettingsService settingsService, ILogger<BillingViewModel> logger)
    {
        _ventopayClient = ventopayClient;
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
    private ObservableCollection<DateTime> _availableMonths = new();

    [ObservableProperty]
    private DateTime? _selectedMonth;

    [ObservableProperty]
    private ObservableCollection<GroupedBillingPosition> _menuBillingPositions = new();

    [ObservableProperty]
    private ObservableCollection<GroupedBillingPosition> _drinkBillingPositions = new();

    [ObservableProperty]
    private decimal _sumCostMenuBillingPositions;

    [ObservableProperty]
    private decimal _sumCostDrinkBillingPositions;

    [ObservableProperty]
    private decimal _sumCostUnknownBillingPositions;

    partial void OnSelectedMonthChanged(DateTime? value)
    {
        if (value.HasValue)
        {
            _ = LoadBillingForMonthAsync(value.Value);
        }
    }

    [RelayCommand]
    private async Task RefreshBillingAsync()
    {
        _logger.LogInformation("Refreshing billing data");
        // Force refresh by clearing existing data
        AvailableMonths.Clear();
        ErrorMessage = null;
        await LoadBillingAsync();
    }

    [RelayCommand]
    private async Task LoadBillingAsync()
    {
        // Prevent concurrent loads or reloading if already loaded
        if (IsLoading || (AvailableMonths.Count > 0 && ErrorMessage == null))
        {
            _logger.LogInformation("LoadBillingAsync skipped: already loading or loaded");
            return;
        }

        _logger.LogInformation("Starting to load billing data");
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var settings = _settingsService.GetCurrentUserSettings();
            if (string.IsNullOrEmpty(settings.VentopayUsername) || string.IsNullOrEmpty(settings.VentopayPassword))
            {
                _logger.LogWarning("VentoPay credentials missing in settings");
                ErrorMessage = "Bitte VentoPay-Anmeldedaten in den Einstellungen konfigurieren";
                return;
            }

            _logger.LogInformation("Attempting login to VentoPay for user {Username}", settings.VentopayUsername);
            var result = await _ventopayClient.Login(settings.VentopayUsername, settings.VentopayPassword);
            if (!result.LoginSuccessful)
            {
                _logger.LogWarning("VentoPay login failed for user {Username}", settings.VentopayUsername);
                ErrorMessage = "Anmeldung fehlgeschlagen. Bitte überprüfen Sie Ihre Zugangsdaten.";
                return;
            }
            _logger.LogInformation("VentoPay login successful for user {Username}", settings.VentopayUsername);
            
            // Generate available months (last 12 months)
            _logger.LogInformation("Generating available months");
            var months = new System.Collections.Generic.List<DateTime>();
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            for (int i = 0; i < 12; i++)
            {
                months.Add(currentMonth.AddMonths(-i));
            }

            AvailableMonths = new ObservableCollection<DateTime>(months);
            _logger.LogInformation("Generated {Count} available months", months.Count);

            // Select current month
            SelectedMonth = currentMonth;
            _logger.LogInformation("Selected current month: {Month}", currentMonth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load billing data");
            ErrorMessage = $"Fehler beim Laden der Abrechnungsdaten: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadBillingForMonthAsync(DateTime month)
    {
        _logger.LogInformation("Loading billing positions for month {Month}", month);
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            LoadingProgress = 0;

            // Get first and last day of the month
            var fromDate = new DateTime(month.Year, month.Month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);
            _logger.LogInformation("Billing period: {From} to {To}", fromDate, toDate);

            // Create progress reporter that updates the LoadingProgress property
            var progress = new Progress<int>(value => LoadingProgress = value);

            _logger.LogInformation("Fetching billing positions");
            var positions = await _ventopayClient.GetBillingPositions(fromDate, toDate, progress);
            _logger.LogInformation("Fetched {Count} billing positions", positions.Count);

            // Build grouped results off the UI thread
            _logger.LogInformation("Processing and grouping billing positions");
            var result = await Task.Run(() =>
            {
                var grouped = positions
                    .GroupBy(p => new { p.PositionName, p.PositionType })
                    .Select(g => new GroupedBillingPosition
                    {
                        Description = g.Key.PositionName,
                        PositionType = g.Key.PositionType,
                        Quantity = g.Sum(p => p.Count),
                        TotalCost = (decimal)g.Sum(p => p.SumCost)
                    })
                    .ToList();

                var menus = new System.Collections.Generic.List<GroupedBillingPosition>(
                    grouped.Where(g => g.PositionType == BillingPositionType.Menu)
                           .OrderBy(g => g.Description));

                var drinks = new System.Collections.Generic.List<GroupedBillingPosition>(
                    grouped.Where(g => g.PositionType == BillingPositionType.Drink)
                           .OrderBy(g => g.Description));

                var sumUnknown = grouped
                    .Where(g => g.PositionType == BillingPositionType.Unknown)
                    .Sum(p => p.TotalCost);

                var sumMenus = menus.Sum(p => p.TotalCost);
                var sumDrinks = drinks.Sum(p => p.TotalCost);

                return new
                {
                    Menus = menus,
                    Drinks = drinks,
                    SumMenus = sumMenus,
                    SumDrinks = sumDrinks,
                    SumUnknown = sumUnknown
                };
            });

            MenuBillingPositions = new ObservableCollection<GroupedBillingPosition>(result.Menus);
            DrinkBillingPositions = new ObservableCollection<GroupedBillingPosition>(result.Drinks);
            SumCostMenuBillingPositions = result.SumMenus;
            SumCostDrinkBillingPositions = result.SumDrinks;
            SumCostUnknownBillingPositions = result.SumUnknown;
            _logger.LogInformation("Billing positions processed: {MenuCount} menus, {DrinkCount} drinks, sums: menus {SumMenus}, drinks {SumDrinks}, unknown {SumUnknown}",
                result.Menus.Count, result.Drinks.Count, result.SumMenus, result.SumDrinks, result.SumUnknown);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load billing positions for month {Month}", month);
            ErrorMessage = $"Fehler beim Laden der Transaktionen: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

public class GroupedBillingPosition
{
    public string Description { get; set; } = "";
    public BillingPositionType PositionType { get; set; }
    public int Quantity { get; set; }
    public decimal TotalCost { get; set; }
}
