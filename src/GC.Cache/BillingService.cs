using System.Collections.ObjectModel;
using System.Text.Json;
using GC.Cache.Billing;
using GC.Core.Model;
using GC.Core.Network;
using GC.Core.Settings;
using GC.Database;
using Microsoft.Extensions.Logging;

namespace GC.Cache;

public class BillingService {
    private readonly ILogger<BillingService> _logger;
    private readonly VentopayWebClient _ventopayClient;
    private readonly GourmetSettingsService _settingsService;
    private readonly SqliteService _sqliteService;
    public string? ErrorMessage { get; private set; }
    private static bool _isLoading;
    public ObservableCollection<DateTime> AvailableMonths { get; set; } = new();
    public DateTime SelectedMonth { get; set; }

    public BillingService(ILogger<BillingService> logger, VentopayWebClient ventopayClient, GourmetSettingsService settingsService, SqliteService sqliteService) {
        _logger = logger;
        _ventopayClient = ventopayClient;
        _settingsService = settingsService;
        _sqliteService = sqliteService;
    }

    /// <summary>
    /// Load billing data asynchronously
    /// </summary>
    /// <returns></returns>
    public async Task<IReadOnlyList<BillingPosition>> RefreshBillingAsync()
    {
        return await GetBillingPositionsAsync(DateTime.Today);
    }
    
    public async Task<IReadOnlyList<BillingPosition>> GetBillingPositionsAsync(DateTime month)
    {
        SelectedMonth = month;
        // Prevent concurrent loads or reloading if already loaded
        if (_isLoading || (AvailableMonths.Count > 0 && ErrorMessage == null))
        {
            _logger.LogInformation("GetBillingPositionsAsync skipped: already loading or loaded");
            return new List<BillingPosition>();
        }

        _logger.LogInformation("Starting to load billing data for month {Month}", month);
        try {
            _isLoading = true;
            ErrorMessage = null;

            var settings = _settingsService.GetCurrentUserSettings();
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            IReadOnlyList<BillingPosition> billingPositions;

            if (settings.DebugMode) {
                var cache = new BillingCacheDebug(_sqliteService);
                var cachedData = await cache.GetCachedDataAsync(month);
                var lastWrite = await cache.GetLastWriteAsync(month);
                if (lastWrite.HasValue && lastWrite.Value.Date >= DateTime.Today && cachedData != null) {
                    _logger.LogInformation("Loaded billing data from debug cache for month {Month}", month);
                    return JsonSerializer.Deserialize<List<BillingPosition>>(cachedData) ?? new List<BillingPosition>();
                } else {
                    var fromDate = lastWrite?.Date.AddDays(1) ?? month;
                    var toDate = DateTime.Today < month.AddMonths(1).AddDays(-1) ? DateTime.Today : month.AddMonths(1).AddDays(-1);
                    billingPositions = await LoadFromLiveAsync(fromDate, toDate);
                    List<BillingPosition> allPositions;
                    if (cachedData != null) {
                        var cachedPositions = JsonSerializer.Deserialize<List<BillingPosition>>(cachedData) ?? new List<BillingPosition>();
                        allPositions = cachedPositions.Concat(billingPositions).ToList();
                    } else {
                        allPositions = billingPositions.ToList();
                    }
                    await cache.SetCachedDataAsync(month, JsonSerializer.Serialize(allPositions));
                    return allPositions;
                }
            } else {
                var cache = new BillingCache(_sqliteService);
                bool shouldCache = month == currentMonth || (month == currentMonth.AddMonths(-1) && DateTime.Today.Day <= 7);
                if (shouldCache) {
                    var cachedData = await cache.GetCachedDataAsync(month);
                    var lastWrite = await cache.GetLastWriteAsync(month);
                    if (lastWrite.HasValue && lastWrite.Value.Date >= DateTime.Today && cachedData != null) {
                        _logger.LogInformation("Loaded billing data from live cache for month {Month}", month);
                        return JsonSerializer.Deserialize<List<BillingPosition>>(cachedData) ?? new List<BillingPosition>();
                    } else {
                        var fromDate = lastWrite?.Date.AddDays(1) ?? month;
                        var toDate = DateTime.Today < month.AddMonths(1).AddDays(-1) ? DateTime.Today : month.AddMonths(1).AddDays(-1);
                        billingPositions = await LoadFromLiveAsync(fromDate, toDate);
                        List<BillingPosition> allPositions;
                        if (cachedData != null) {
                            var cachedPositions = JsonSerializer.Deserialize<List<BillingPosition>>(cachedData) ?? new List<BillingPosition>();
                            allPositions = cachedPositions.Concat(billingPositions).ToList();
                        } else {
                            allPositions = billingPositions.ToList();
                        }
                        await cache.SetCachedDataAsync(month, JsonSerializer.Serialize(allPositions));
                        return allPositions;
                    }
                } else {
                    billingPositions = await LoadFromLiveAsync(month, month.AddMonths(1).AddDays(-1));
                    return billingPositions;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load billing data for month {Month}", month);
            ErrorMessage = $"Fehler beim Laden der Abrechnungsdaten: {ex.Message}";
            return new List<BillingPosition>();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task<IReadOnlyList<BillingPosition>> LoadFromLiveAsync(DateTime fromDate, DateTime toDate)
    {
        var settings = _settingsService.GetCurrentUserSettings();
        if (string.IsNullOrEmpty(settings.VentopayUsername) || string.IsNullOrEmpty(settings.VentopayPassword))
        {
            _logger.LogWarning("VentoPay credentials missing in settings");
            ErrorMessage = "Bitte VentoPay-Anmeldedaten in den Einstellungen konfigurieren";
            return new List<BillingPosition>();
        }

        _logger.LogInformation("Attempting login to VentoPay for user {Username}", settings.VentopayUsername);
        var result = await _ventopayClient.Login(settings.VentopayUsername, settings.VentopayPassword);
        if (!result.LoginSuccessful)
        {
            _logger.LogWarning("VentoPay login failed for user {Username}", settings.VentopayUsername);
            ErrorMessage = "Anmeldung fehlgeschlagen. Bitte überprüfen Sie Ihre Zugangsdaten.";
            return new List<BillingPosition>();
        }
        _logger.LogInformation("VentoPay login successful for user {Username}", settings.VentopayUsername);

        // Load billing positions for the specified date range
        var billingPositions = await _ventopayClient.GetBillingPositions(fromDate, toDate, default(IProgress<int>));
        _logger.LogInformation("Loaded {Count} billing positions from {From} to {To}", billingPositions.Count, fromDate, toDate);

        return billingPositions;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing billing service");
        try {
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
            var months = new List<DateTime>();
            var currentMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            for (int i = 0; i < 12; i++)
            {
                months.Add(currentMonth.AddMonths(-i));
            }

            // Fix: update the existing ObservableCollection instead of replacing it
            AvailableMonths.Clear();
            foreach (var month in months)
            {
                AvailableMonths.Add(month);
            }
            _logger.LogInformation("Generated {Count} available months", months.Count);

            // Select current month
            SelectedMonth = currentMonth;
            _logger.LogInformation("Selected current month: {Month}", currentMonth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize billing service");
            ErrorMessage = $"Fehler beim Initialisieren der Abrechnungsdaten: {ex.Message}";
        }
    }
}