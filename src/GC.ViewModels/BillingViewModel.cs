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
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var settings = _settingsService.GetCurrentUserSettings();
            if (string.IsNullOrEmpty(settings.VentopayUsername) || string.IsNullOrEmpty(settings.VentopayPassword))
            {
                ErrorMessage = "Bitte VentoPay-Anmeldedaten in den Einstellungen konfigurieren";
                return;
            }

            var result = await _ventopayClient.Login(settings.VentopayUsername, settings.VentopayPassword);
            if (!result.LoginSuccessful)
            {
                ErrorMessage = "Anmeldung fehlgeschlagen. Bitte überprüfen Sie Ihre Zugangsdaten.";
                return;
            }
            
            // Generate available months (last 12 months)
            var months = new System.Collections.Generic.List<DateTime>();
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            for (int i = 0; i < 12; i++)
            {
                months.Add(currentMonth.AddMonths(-i));
            }

            AvailableMonths = new ObservableCollection<DateTime>(months);

            // Select current month
            SelectedMonth = currentMonth;
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
        try
        {
            IsLoading = true;
            ErrorMessage = null;
            LoadingProgress = 0;

            // Get first and last day of the month
            var fromDate = new DateTime(month.Year, month.Month, 1);
            var toDate = fromDate.AddMonths(1).AddDays(-1);

            // Create progress reporter that updates the LoadingProgress property
            var progress = new Progress<int>(value => LoadingProgress = value);

            var positions = await _ventopayClient.GetBillingPositions(fromDate, toDate, progress);

            // Group by type
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

            MenuBillingPositions = new ObservableCollection<GroupedBillingPosition>(
                grouped.Where(g => g.PositionType == BillingPositionType.Menu).OrderBy(g => g.Description));

            DrinkBillingPositions = new ObservableCollection<GroupedBillingPosition>(
                grouped.Where(g => g.PositionType == BillingPositionType.Drink).OrderBy(g => g.Description));

            SumCostMenuBillingPositions = MenuBillingPositions.Sum(p => p.TotalCost);
            SumCostDrinkBillingPositions = DrinkBillingPositions.Sum(p => p.TotalCost);
            SumCostUnknownBillingPositions = grouped
                .Where(g => g.PositionType == BillingPositionType.Unknown)
                .Sum(p => p.TotalCost);
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

