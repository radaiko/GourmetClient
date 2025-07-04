using System.Security;
using System.Text.Json;

namespace GourmetClient.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using Model;
    using Serialization;
    using Settings;
    using Utils;

    using Notifications;

    public class GourmetCacheService
    {
        private readonly GourmetWebClient _webClient;

        private readonly GourmetSettingsService _settingsService;

        private readonly NotificationService _notificationService;

        private readonly string _cacheFileName;

        private GourmetCache _cache;

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

        public async Task UpdateOrderedMenu(IReadOnlyList<GourmetMenuMeal> mealsToOrder, IReadOnlyList<OrderedGourmetMenuMeal> mealsToCancel)
        {
            mealsToOrder = mealsToOrder ?? throw new ArgumentNullException(nameof(mealsToOrder));
            mealsToCancel = mealsToCancel ?? throw new ArgumentNullException(nameof(mealsToCancel));

            var userSettings = _settingsService.GetCurrentUserSettings();

            if (string.IsNullOrEmpty(userSettings.GourmetLoginUsername))
            {
                return;
            }

            //await using var loginHandle = await _webClient.Login(userSettings.GourmetLoginUsername, userSettings.GourmetLoginPassword ?? new SecureString());

            //if (!loginHandle.LoginSuccessful)
            //{
            //    return;
            //}

            //try
            //{
            //    foreach (var orderedMeal in mealsToCancel)
            //    {
            //        await _webClient.CancelOrder(orderedMeal);
            //    }

            //    foreach (var meal in mealsToOrder)
            //    {
            //        await _webClient.AddMealToOrderedMenu(meal);
            //    }

            //    await _webClient.ConfirmOrder();
            //}
            //finally
            //{
            //    InvalidateCache();
            //}
        }

        private async Task UpdateCache()
        {
            _cache = await CreateCacheFromServerData();
            await SaveMenuCache(_cache);
        }

        private async Task<GourmetCache> CreateCacheFromServerData()
        {
            var userSettings = _settingsService.GetCurrentUserSettings();

            if (string.IsNullOrEmpty(userSettings.GourmetLoginUsername))
            {
                return new InvalidatedGourmetCache();
            }

            try
            {
                await using var loginHandle = await _webClient.Login(userSettings.GourmetLoginUsername, userSettings.GourmetLoginPassword ?? new SecureString());

                if (!loginHandle.LoginSuccessful)
                {
                    _notificationService.Send(new Notification(NotificationType.Error, "Daten konnten nicht aktualisiert werden. Ursache: Login fehlgeschlagen"));
                    return new InvalidatedGourmetCache();
                }

                var menuResult = await _webClient.GetMenus();
                var orderedMenus = await _webClient.GetOrderedMenus();

                return new GourmetCache(DateTime.Now, menuResult.UserInformation, menuResult.Menus, orderedMenus);
            }
            catch (Exception exception) when (exception is GourmetRequestException || exception is GourmetParseException)
            {
                _notificationService.Send(new ExceptionNotification("Daten konnten nicht aktualisiert werden", exception));
                return new InvalidatedGourmetCache();
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

                if (serializedCache.Version != 2)
                {
                    // Unsupported version
                    return new InvalidatedGourmetCache();
                }
                
                return serializedCache.ToGourmetMenuCache();
            }
            catch
            {
                return new InvalidatedGourmetCache();
            }
        }

        private async Task SaveMenuCache(GourmetCache menuCache)
        {
            menuCache = menuCache ?? throw new ArgumentNullException(nameof(menuCache));

            var serializedCache = new SerializableGourmetCache(menuCache);

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
}
