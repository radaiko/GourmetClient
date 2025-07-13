using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GourmetClient.Model;
using GourmetClient.Notifications;
using GourmetClient.Serialization;
using GourmetClient.Settings;
using GourmetClient.Utils;

namespace GourmetClient.Network;

public class GourmetCacheService
{
    private readonly GourmetWebClient _webClient;

    private readonly GourmetSettingsService _settingsService;

    private readonly NotificationService _notificationService;

    private readonly string _cacheFileName;

    private GourmetCache? _cache;

    public GourmetCacheService()
    {
        _webClient = InstanceProvider.GourmetWebClient;
        _settingsService = InstanceProvider.SettingsService;
        _notificationService = InstanceProvider.NotificationService;

        _cacheFileName = Path.Combine(App.LocalAppDataPath, "GourmetCache.json");
    }

    public void InvalidateCache()
    {
        _cache = new InvalidatedGourmetCache();
    }

    public async Task<GourmetCache> GetCache()
    {
        if (_cache == null)
        {
            _cache = await GetCacheFromFile();
        }

        var userSettings = _settingsService.GetCurrentUserSettings();

        if (_cache.Timestamp.Add(userSettings.CacheValidity) < DateTime.Now)
        {
            await UpdateCache();
        }

        return _cache;
    }

    public async Task<GourmetUpdateOrderResult> UpdateOrderedMenu(
        GourmetUserInformation userInformation,
        IReadOnlyList<GourmetMenu> menusToOrder,
        IReadOnlyList<GourmetOrderedMenu> menusToCancel)
    {
        var userSettings = _settingsService.GetCurrentUserSettings();

        await using var loginHandle = await _webClient.Login(userSettings.GourmetLoginUsername, userSettings.GourmetLoginPassword);

        if (!loginHandle.LoginSuccessful)
        {
            return FailAllMenusWithMessage("Login fehlgeschlagen");
        }

        var failedOrders = new List<FailedMenuToOrderInformation>();

        try
        {
            foreach (var orderedMenu in menusToCancel)
            {
                await _webClient.CancelOrder(orderedMenu);
            }

            foreach (var menu in menusToOrder)
            {
                GourmetApiResult apiResult = await _webClient.AddMenuToOrderedMenu(userInformation, menu);
                if (!apiResult.Success)
                {
                    failedOrders.Add(new FailedMenuToOrderInformation(menu, apiResult.Message));
                }
            }

            await _webClient.ConfirmOrder();
        }
        finally
        {
            InvalidateCache();
        }

        return new GourmetUpdateOrderResult(failedOrders);

        GourmetUpdateOrderResult FailAllMenusWithMessage(string message)
        {
            return new GourmetUpdateOrderResult(menusToOrder.Select(menu => new FailedMenuToOrderInformation(menu, message)).ToArray());
        }
    }

    private async Task UpdateCache()
    {
        _cache = await CreateCacheFromServerData();
        await SaveCache(_cache);
    }

    private async Task<GourmetCache> CreateCacheFromServerData()
    {
        var userSettings = _settingsService.GetCurrentUserSettings();

        if (string.IsNullOrEmpty(userSettings.GourmetLoginUsername))
        {
            return new InvalidatedGourmetCache();
        }

        GourmetMenuResult menuResult;
        GourmetOrderedMenuResult orderedMenuResult;

        try
        {
            await using var loginHandle = await _webClient.Login(userSettings.GourmetLoginUsername, userSettings.GourmetLoginPassword);

            if (!loginHandle.LoginSuccessful)
            {
                _notificationService.Send(new Notification(NotificationType.Error, "Daten konnten nicht aktualisiert werden. Ursache: Login fehlgeschlagen"));
                return new InvalidatedGourmetCache();
            }

            menuResult = await _webClient.GetMenus();
            orderedMenuResult = await _webClient.GetOrderedMenus();
        }
        catch (Exception exception) when (exception is GourmetRequestException || exception is GourmetParseException)
        {
            _notificationService.Send(new ExceptionNotification("Daten konnten nicht aktualisiert werden", exception));
            return new InvalidatedGourmetCache();
        }

        var menus = SetIsAvailableForTodayMenus().ToArray();
        return new GourmetCache(DateTime.Now, menuResult.UserInformation, menus, orderedMenuResult.OrderedMenus);

        IEnumerable<GourmetMenu> SetIsAvailableForTodayMenus()
        {
            foreach (GourmetMenu menu in menuResult.Menus)
            {
                if (!IsMenuForToday(menu))
                {
                    // Only look at menus for today
                    yield return menu;
                }
                else if (!menu.IsAvailable)
                {
                    // Menu is no longer available, so nothing to update
                    yield return menu;
                }
                else if (orderedMenuResult.IsOrderChangeForTodayPossible)
                {
                    // Order for today can still be changed, so nothing to update
                    yield return menu;
                }
                else
                {
                    // Order for today can no longer be changed, so set the menu as not available
                    yield return menu with { IsAvailable = false };
                }
            }
        }

        bool IsMenuForToday(GourmetMenu menu)
        {
            Debug.Assert(menu.Day.Kind == DateTimeKind.Utc);
            DateTime today = DateTime.UtcNow;
            return menu.Day.Day == today.Day && menu.Day.Month == today.Month && menu.Day.Year == today.Year;
        }
    }

    private async Task<GourmetCache> GetCacheFromFile()
    {
        if (!File.Exists(_cacheFileName))
        {
            return new InvalidatedGourmetCache();
        }

        try
        {
            await using var fileStream = new FileStream(_cacheFileName, FileMode.Open, FileAccess.Read, FileShare.None);
            var serializedCache = await JsonSerializer.DeserializeAsync<SerializableGourmetCache>(fileStream);

            if (serializedCache == null)
            {
                return new InvalidatedGourmetCache();
            }

            return serializedCache.ToGourmetMenuCache();
        }
        catch
        {
            return new InvalidatedGourmetCache();
        }
    }

    private async Task SaveCache(GourmetCache menuCache)
    {
        var serializedCache = SerializableGourmetCache.FromGourmetCache(menuCache);

        try
        {
            var cacheDirectory = Path.GetDirectoryName(_cacheFileName);
            if (cacheDirectory != null && !Directory.Exists(cacheDirectory))
            {
                Directory.CreateDirectory(cacheDirectory);
            }

            await using var fileStream = new FileStream(_cacheFileName, FileMode.Create, FileAccess.Write, FileShare.None);
            await JsonSerializer.SerializeAsync(fileStream, serializedCache, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            InvalidateCache();
        }
    }
}