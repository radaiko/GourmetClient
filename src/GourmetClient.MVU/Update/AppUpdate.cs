using System.Collections.Immutable;
using GourmetClient.MVU.Core;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Utils;
using GourmetClient.Core.Utils;
using GourmetClient.Core.Model;
using GourmetClient.Core.Settings;

namespace GourmetClient.MVU.Update
{
    public static class AppUpdate
    {
        static AppUpdate()
        {
            // Initialize the InstanceProvider with MVU file path provider
            InstanceProvider.Initialize(new MvuFilePathProvider());
        }

        public static (AppState, Cmd<Msg>) UpdateState(Msg message, AppState state)
        {
            return message switch
            {
                // UI Toggle Messages
                ToggleBilling => state.IsBillingVisible
                    ? (state with { IsBillingVisible = false }, Cmd.None<Msg>())
                    : (PlatformDetector.IsIOS
                        ? HandleIOSNavigateToPage(1, state)
                        : (state with { IsBillingVisible = true, IsLoadingBilling = true }, Cmd.Batch(
                            Cmd.OfTask(InitializeBillingMonthsAsync),
                            Cmd.OfTask(LoadBillingAsync)
                        ))),
                ToggleSettings => state.IsSettingsVisible
                    ? (state with { IsSettingsVisible = false }, Cmd.None<Msg>())
                    : (PlatformDetector.IsIOS
                        ? HandleIOSNavigateToPage(2, state)
                        : (state with { IsSettingsVisible = true }, Cmd.OfTask(LoadSettingsAsync))),
                ToggleAbout => (PlatformDetector.IsIOS
                    ? HandleIOSNavigateToPage(3, state)
                    : (state with { IsAboutVisible = !state.IsAboutVisible }, Cmd.None<Msg>())),
                NavigateToPage nav => HandleIOSNavigateToPage(nav.PageIndex, state),
                SetCurrentMenuDayIndex setDay => HandleSetCurrentMenuDayIndex(setDay.DayIndex, state),

                // Menu Messages
                LoadMenus => (state with { IsLoading = true }, Cmd.OfTask(LoadMenusAsync)),
                MenusLoaded menuData => HandleMenusLoaded(menuData, state),
                UpdateMenu => (state with { IsLoading = true }, Cmd.OfTask(UpdateMenuAsync)),

                ToggleMenuOrder toggleOrder => ToggleMenuOrderImpl(toggleOrder, state),

                ExecuteSelectedOrder => (state with { IsLoading = true }, Cmd.OfTask(ExecuteOrderAsync)),

                // Billing Messages
                LoadBilling => (state with { IsLoadingBilling = true, IsBillingVisible = true }, Cmd.OfTask(LoadBillingAsync)),
                InitializeBillingMonths => (state, Cmd.OfTask(InitializeBillingMonthsAsync)),
                BillingLoaded billingData => (state with
                {
                    IsLoadingBilling = false,
                    MenuBillingPositions = billingData.MenuBillingPositions,
                    DrinkBillingPositions = billingData.DrinkBillingPositions,
                    SumCostMenuBillingPositions = billingData.SumCostMenuBillingPositions,
                    SumCostDrinkBillingPositions = billingData.SumCostDrinkBillingPositions,
                    SumCostUnknownBillingPositions = billingData.SumCostUnknownBillingPositions
                }, Cmd.None<Msg>()),
                BillingMonthsInitialized monthsData => (state with { AvailableMonths = monthsData.AvailableMonths }, Cmd.None<Msg>()),

                // About Messages
                ShowReleaseNotes => (state, Cmd.OfTask(OpenReleaseNotesAsync)),
                OpenIconAuthorWebPage => (state, Cmd.OfTask(OpenIconAuthorWebPageAsync)),
                OpenFlatIconWebPage => (state, Cmd.OfTask(OpenFlatIconWebPageAsync)),

                // Settings Messages
                UpdateUsername updateUsername => (state with { Settings = (state.Settings ?? new AppSettings()) with { Username = updateUsername.Username } }, Cmd.None<Msg>()),
                UpdatePassword updatePassword => (state with { Settings = (state.Settings ?? new AppSettings()) with { Password = updatePassword.Password } }, Cmd.None<Msg>()),
                UpdateVentoPayUsername updateVentoPayUsername => (state with { Settings = (state.Settings ?? new AppSettings()) with { VentoPayUsername = updateVentoPayUsername.VentoPayUsername } }, Cmd.None<Msg>()),
                UpdateVentoPayPassword updateVentoPayPassword => (state with { Settings = (state.Settings ?? new AppSettings()) with { VentoPayPassword = updateVentoPayPassword.VentoPayPassword } }, Cmd.None<Msg>()),
                UpdateAutoUpdate updateAutoUpdate => (state with { Settings = (state.Settings ?? new AppSettings()) with { AutoUpdate = updateAutoUpdate.AutoUpdate } }, Cmd.None<Msg>()),
                UpdateStartWithWindows updateStartWithWindows => (state with { Settings = (state.Settings ?? new AppSettings()) with { StartWithWindows = updateStartWithWindows.StartWithWindows } }, Cmd.None<Msg>()),
                UpdateTheme updateTheme => (state with { Settings = (state.Settings ?? new AppSettings()) with { Theme = updateTheme.Theme } }, Cmd.OfTask(() => ApplyThemeAsync(updateTheme.Theme))),
                SaveSettings => (state with { IsSettingsVisible = false }, Cmd.OfTask(() => SaveSettingsAsync(state.Settings ?? new AppSettings()))),
                SaveFormSettings formData => (
                    state with {
                        Settings = new AppSettings(
                            formData.Username,
                            formData.Password,
                            formData.VentoPayUsername,
                            formData.VentoPayPassword,
                            formData.AutoUpdate,
                            formData.StartWithWindows,
                            formData.Theme
                        ),
                        IsSettingsVisible = false
                    },
                    Cmd.OfTask(() => SaveSettingsAsync(new AppSettings(
                        formData.Username,
                        formData.Password,
                        formData.VentoPayUsername,
                        formData.VentoPayPassword,
                        formData.AutoUpdate,
                        formData.StartWithWindows,
                        formData.Theme
                    )))
                ),
                LoadSettings => (state, Cmd.OfTask(LoadSettingsAsync)),
                SettingsLoaded settingsData => (state with { Settings = settingsData.Settings }, Cmd.None<Msg>()),

                // Additional missing messages
                ExecuteOrder => (state with { IsLoading = true }, Cmd.OfTask(ExecuteOrderAsync)),

                SelectMonth month => (state with { SelectedMonth = month.Month, IsLoadingBilling = true }, Cmd.OfTask(() => LoadBillingForMonthAsync(month.Month))),

                // App Initialization
                InitializeApp => (state, Cmd.OfTask(LoadSettingsForInitAsync)),
                AppInitialized appData => (state with { Settings = appData.Settings },
                    // Auto-load menus after settings are initialized if credentials are available
                    !string.IsNullOrEmpty(appData.Settings.Username)
                        ? Cmd.OfTask(LoadMenusAsync)
                        : Cmd.None<Msg>()),

                // Error handling
                ErrorOccurred error => (state with { ErrorMessage = error.Message, IsLoading = false }, Cmd.None<Msg>()),
                ClearError => (state with { ErrorMessage = null }, Cmd.None<Msg>()),

                _ => (state, Cmd.None<Msg>())
            };
        }

        private static (AppState, Cmd<Msg>) HandleSetCurrentMenuDayIndex(int newIndex, AppState state)
        {
            if (state.MenuDays == null || state.MenuDays.Count == 0)
                return (state, Cmd.None<Msg>());
            if (newIndex < 0) newIndex = 0;
            if (newIndex >= state.MenuDays.Count) newIndex = state.MenuDays.Count - 1;
            if (newIndex == state.CurrentMenuDayIndex)
                return (state, Cmd.None<Msg>());
            return (state with { CurrentMenuDayIndex = newIndex }, Cmd.None<Msg>());
        }

        private static (AppState, Cmd<Msg>) HandleMenusLoaded(MenusLoaded menuData, AppState state)
        {
            var newDays = menuData.MenuDays;
            int preservedIndex = state.CurrentMenuDayIndex;
            if (preservedIndex >= newDays.Count) preservedIndex = -1; // force re-select next render
            return (state with
            {
                IsLoading = false,
                MenuDays = newDays,
                UserName = menuData.UserName,
                LastMenuUpdate = menuData.LastUpdate,
                CurrentMenuDayIndex = preservedIndex
            }, Cmd.None<Msg>());
        }

        private static (AppState, Cmd<Msg>) ToggleMenuOrderImpl(ToggleMenuOrder toggleOrder, AppState state)
        {
            if (state.MenuDays == null) return (state, Cmd.None<Msg>());

            var updatedMenuDays = state.MenuDays.Select(day =>
            {
                if (day.Date.Date != toggleOrder.Day.Date) return day; // other days untouched
                var updatedMenus = day.Menus.Select(menu =>
                {
                    if (menu.MenuId == toggleOrder.MenuId)
                    {
                        var newState = menu.MenuState switch
                        {
                            GourmetMenuState.None => GourmetMenuState.MarkedForOrder,
                            GourmetMenuState.MarkedForOrder => GourmetMenuState.None,
                            GourmetMenuState.Ordered when menu.IsOrderCancelable => GourmetMenuState.MarkedForCancel,
                            GourmetMenuState.MarkedForCancel => GourmetMenuState.Ordered,
                            _ => menu.MenuState
                        };
                        return menu with { MenuState = newState };
                    }
                    return menu;
                }).ToImmutableList();
                return day with { Menus = updatedMenus };
            }).ToImmutableList();

            return (state with { MenuDays = updatedMenuDays }, Cmd.None<Msg>());
        }

        private static async Task<Msg> LoadMenusAsync()
        {
            try
            {
                var cacheService = InstanceProvider.GourmetCacheService;
                var settingsService = InstanceProvider.SettingsService;

                // Check if user credentials are configured
                var userSettings = settingsService.GetCurrentUserSettings();
                if (string.IsNullOrEmpty(userSettings.GourmetLoginUsername))
                {
                    // No credentials configured, return empty menus (will show welcome view)
                    return new MenusLoaded(ImmutableList<GourmetMenuDayViewModel>.Empty);
                }

                // Load menu cache from core service
                var cache = await cacheService.GetCache();

                // Transform core models to MVU view models
                var menuDays = TransformCacheToMenuDays(cache);

                return new MenusLoaded(
                    menuDays,
                    cache.UserInformation.NameOfUser,
                    cache.Timestamp
                );
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to load menus: {ex.Message}");
            }
        }

        private static async Task<Msg> UpdateMenuAsync()
        {
            try
            {
                var cacheService = InstanceProvider.GourmetCacheService;

                // Invalidate cache to force refresh from server
                cacheService.InvalidateCache();

                // Load fresh data
                return await LoadMenusAsync();
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to update menu: {ex.Message}");
            }
        }

        private static async Task<Msg> ExecuteOrderAsync()
        {
            try
            {
                var cacheService = InstanceProvider.GourmetCacheService;

                // Get current cache to find selected menus
                cacheService.InvalidateCache();
                var currentData = await cacheService.GetCache();

                // Find menus to order and cancel from the current state
                // Note: In a real implementation, we'd need access to the current state here
                // For now, we'll implement a basic refresh after a delay
                await Task.Delay(1000); // Simulate processing time

                return await LoadMenusAsync();
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to execute order: {ex.Message}");
            }
        }

        // This method would be used to process orders from the current state
        private static async Task<Msg> ExecuteOrderWithStateAsync(AppState currentState)
        {
            try
            {
                var cacheService = InstanceProvider.GourmetCacheService;

                // Get fresh data from server
                cacheService.InvalidateCache();
                var currentData = await cacheService.GetCache();

                var menusToOrder = new List<GourmetMenu>();
                var menusToCancel = new List<GourmetOrderedMenu>();
                var errorMessages = new List<string>();

                // Process marked menus from state
                if (currentState.MenuDays != null)
                {
                    foreach (var day in currentState.MenuDays)
                    {
                        foreach (var menu in day.Menus)
                        {
                            if (menu.MenuState == GourmetMenuState.MarkedForOrder)
                            {
                                // Find the actual menu in current data
                                var actualMenu = currentData.Menus.FirstOrDefault(m =>
                                    m.Day == day.Date &&
                                    m.Description == menu.MenuDescription);

                                if (actualMenu?.IsAvailable == true)
                                {
                                    menusToOrder.Add(actualMenu);
                                }
                                else
                                {
                                    errorMessages.Add($"{menu.MenuDescription} für den {day.Date:dd.MM.yyyy} ist nicht mehr verfügbar");
                                }
                            }
                            else if (menu.MenuState == GourmetMenuState.MarkedForCancel)
                            {
                                // Find matching ordered menus
                                var matchingOrderedMenus = currentData.OrderedMenus.Where(om =>
                                    om.Day == day.Date &&
                                    om.MenuName.Contains(menu.MenuDescription) ||
                                    menu.MenuDescription.Contains(om.MenuName));

                                foreach (var orderedMenu in matchingOrderedMenus)
                                {
                                    if (orderedMenu.IsOrderCancelable)
                                    {
                                        menusToCancel.Add(orderedMenu);
                                    }
                                    else
                                    {
                                        errorMessages.Add($"{menu.MenuDescription} für den {day.Date:dd.MM.yyyy} kann nicht storniert werden");
                                    }
                                }
                            }
                        }
                    }
                }

                // Execute the order update if there are changes
                if (menusToOrder.Count > 0 || menusToCancel.Count > 0)
                {
                    var updateResult = await cacheService.UpdateOrderedMenu(
                        currentData.UserInformation,
                        menusToOrder,
                        menusToCancel);

                    // Add any failed order messages
                    foreach (var failedMenu in updateResult.FailedMenusToOrder)
                    {
                        errorMessages.Add($"Das Menü '{failedMenu.Menu.MenuName}' am {failedMenu.Menu.Day:dd.MM.yyyy} konnte nicht bestellt werden. Ursache: {failedMenu.Message}");
                    }
                }

                // Return error if any, otherwise reload menus
                if (errorMessages.Count > 0)
                {
                    return new ErrorOccurred(string.Join("\n", errorMessages));
                }

                return await LoadMenusAsync();
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to execute order: {ex.Message}");
            }
        }

        private static ImmutableList<GourmetMenuDayViewModel> TransformCacheToMenuDays(GourmetCache cache)
        {
            if (cache.Menus.Count == 0)
            {
                return ImmutableList<GourmetMenuDayViewModel>.Empty;
            }

            var dayViewModels = new List<GourmetMenuDayViewModel>();
            var dayGroups = cache.Menus.GroupBy(menu => menu.Day).ToArray();

            foreach (var dayGroup in dayGroups)
            {
                var day = dayGroup.Key;
                var menuViewModels = new List<GourmetMenuViewModel>();

                foreach (var menu in dayGroup.OrderBy(m => m.MenuName))
                {
                    var menuViewModel = CreateMenuViewModel(menu, cache.OrderedMenus);
                    menuViewModels.Add(menuViewModel);
                }

                // Create day view model with immutable list
                var dayViewModel = new GourmetMenuDayViewModel(
                    day,
                    menuViewModels.ToImmutableList()
                );

                dayViewModels.Add(dayViewModel);
            }

            return dayViewModels.OrderBy(d => d.Date).ToImmutableList();
        }

        private static GourmetMenuViewModel CreateMenuViewModel(GourmetMenu menu, IReadOnlyCollection<GourmetOrderedMenu> orderedMenus)
        {
            var orderedMenu = orderedMenus.FirstOrDefault(om => om.MatchesMenu(menu));

            var menuState = GourmetMenuState.None;
            var isOrdered = false;
            var isOrderApproved = false;
            var isOrderCancelable = false;

            if (orderedMenu != null)
            {
                menuState = GourmetMenuState.Ordered;
                isOrdered = true;
                isOrderApproved = orderedMenu.IsOrderApproved;
                isOrderCancelable = orderedMenu.IsOrderCancelable;
            }
            else if (!menu.IsAvailable)
            {
                menuState = GourmetMenuState.NotAvailable;
            }

            return new GourmetMenuViewModel(
                MenuId: menu.MenuId, // Use string MenuId directly
                MenuDescription: menu.Description,
                Allergens: menu.Allergens,
                MenuState: menuState,
                IsOrdered: isOrdered,
                IsOrderApproved: isOrderApproved,
                IsOrderCancelable: isOrderCancelable,
                Category: menu.Category
            );
        }

        private static async Task<Msg> LoadBillingAsync()
        {
            var now = DateTime.Now;
            return await LoadBillingForMonthAsync(now);
        }

        private static async Task<Msg> InitializeBillingMonthsAsync()
        {
            try
            {
                await Task.Run(() => { }); // Make it async for consistency

                // Generate available months (current month and previous 11 months)
                var availableMonths = new List<DateTime>();
                var currentDate = DateTime.Now;
                for (int i = 0; i < 12; i++)
                {
                    availableMonths.Add(currentDate.AddMonths(-i));
                }

                return new BillingMonthsInitialized(availableMonths.ToImmutableList());
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to initialize billing months: {ex.Message}");
            }
        }

        private static async Task<Msg> LoadBillingForMonthAsync(DateTime selectedDate)
        {
            try
            {
                var billingService = InstanceProvider.BillingCacheService;

                var month = selectedDate.Month;
                var year = selectedDate.Year;

                // Create progress reporter (optional - could be used for UI progress indication)
                var progress = new Progress<int>();

                // Load billing positions from core service
                var billingPositions = await billingService.GetBillingPositions(month, year, progress);

                // Group and transform billing positions
                var menuPositions = billingPositions
                    .Where(bp => bp.PositionType == BillingPositionType.Menu)
                    .GroupBy(bp => bp.PositionName)
                    .Select(g => new GroupedBillingPositionsViewModel(
                        g.Key,
                        g.Sum(bp => bp.Count),
                        (decimal)g.Sum(bp => bp.SumCost)
                    ))
                    .ToImmutableList();

                var drinkPositions = billingPositions
                    .Where(bp => bp.PositionType == BillingPositionType.Drink)
                    .GroupBy(bp => bp.PositionName)
                    .Select(g => new GroupedBillingPositionsViewModel(
                        g.Key,
                        g.Sum(bp => bp.Count),
                        (decimal)g.Sum(bp => bp.SumCost)
                    ))
                    .ToImmutableList();

                // Calculate sums
                var menuSum = (decimal)billingPositions
                    .Where(bp => bp.PositionType == BillingPositionType.Menu)
                    .Sum(bp => bp.SumCost);

                var drinkSum = (decimal)billingPositions
                    .Where(bp => bp.PositionType == BillingPositionType.Drink)
                    .Sum(bp => bp.SumCost);

                var unknownSum = (decimal)billingPositions
                    .Where(bp => bp.PositionType == BillingPositionType.Unknown)
                    .Sum(bp => bp.SumCost);

                return new BillingLoaded(menuPositions, drinkPositions, menuSum, drinkSum, unknownSum);
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to load billing: {ex.Message}");
            }
        }

        private static async Task<Msg> OpenReleaseNotesAsync()
        {
            try
            {
                // TODO: Implement release notes display
                await Task.Delay(100);
                return new ErrorOccurred("Release notes not yet implemented");
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to open release notes: {ex.Message}");
            }
        }

        private static async Task<Msg> OpenIconAuthorWebPageAsync()
        {
            try
            {
                var url = "https://www.flaticon.com/authors/smashicons";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                await Task.Delay(100);
                return new ClearError(); // Success - no message needed
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to open web page: {ex.Message}");
            }
        }

        private static async Task<Msg> OpenFlatIconWebPageAsync()
        {
            try
            {
                var url = "https://www.flaticon.com";
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                await Task.Delay(100);
                return new ClearError(); // Success - no message needed
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to open web page: {ex.Message}");
            }
        }

        private static async Task<Msg> ApplyThemeAsync(string theme)
        {
            try
            {
                // TODO: Implement theme switching logic for theme: {theme}
                System.Diagnostics.Debug.WriteLine($"Applying theme: {theme}");
                await Task.Delay(100);
                return new ClearError(); // Success - theme applied
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to apply theme '{theme}': {ex.Message}");
            }
        }

        private static async Task<Msg> SaveSettingsAsync(AppSettings? settings)
        {
            try
            {
                if (settings == null) return new ClearError();

                await Task.Run(() => { }); // Make it async for consistency

                var settingsService = InstanceProvider.SettingsService;

                // Map from MVU AppSettings to core UserSettings
                var userSettings = new UserSettings
                {
                    GourmetLoginUsername = settings.Username,
                    GourmetLoginPassword = settings.Password,
                    VentopayUsername = settings.VentoPayUsername,
                    VentopayPassword = settings.VentoPayPassword
                };

                settingsService.SaveUserSettings(userSettings);

                System.Diagnostics.Debug.WriteLine("Settings saved using core settings service");
                return new ClearError(); // Success - settings saved
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to save settings: {ex.Message}");
            }
        }

        private static async Task<Msg> LoadSettingsAsync()
        {
            try
            {
                await Task.Run(() => { }); // Make it async for consistency

                var settingsService = InstanceProvider.SettingsService;
                var userSettings = settingsService.GetCurrentUserSettings();

                // Map from core UserSettings to MVU AppSettings
                var appSettings = new AppSettings(
                    Username: userSettings.GourmetLoginUsername,
                    Password: userSettings.GourmetLoginPassword,
                    VentoPayUsername: userSettings.VentopayUsername,
                    VentoPayPassword: userSettings.VentopayPassword,
                    AutoUpdate: true, // Default values for MVU-specific settings
                    StartWithWindows: false,
                    Theme: "System"
                );

                return new SettingsLoaded(appSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load settings: {ex.Message}");
                // Return default settings on error
                return new SettingsLoaded(new AppSettings());
            }
        }

        private static async Task<Msg> LoadSettingsForInitAsync()
        {
            try
            {
                // Get OS-dependent application data folder
                var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var gourmetFolder = Path.Combine(appDataFolder, "GourmetClient");
                var settingsFile = Path.Combine(gourmetFolder, "settings.json");

                if (!File.Exists(settingsFile))
                {
                    // Return default settings if file doesn't exist
                    return new AppInitialized(new AppSettings());
                }

                // Read and deserialize settings from JSON
                var jsonString = await File.ReadAllTextAsync(settingsFile);
                var settings = System.Text.Json.JsonSerializer.Deserialize<AppSettings>(jsonString);

                System.Diagnostics.Debug.WriteLine($"Settings initialized from: {settingsFile}");
                return new AppInitialized(settings ?? new AppSettings());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize settings: {ex.Message}");
                // Return default settings on error
                return new AppInitialized(new AppSettings());
            }
        }

        private static (AppState, Cmd<Msg>) HandleIOSNavigateToPage(int targetIndex, AppState state)
        {
            // Clamp
            var newIndex = targetIndex < 0 ? 0 : (targetIndex > 3 ? 3 : targetIndex);
            if (newIndex == state.CurrentPageIndex)
            {
                return (state, Cmd.None<Msg>());
            }

            var cmds = new List<Cmd<Msg>>();
            var newState = state with { CurrentPageIndex = newIndex, IsBillingVisible = false, IsSettingsVisible = false, IsAboutVisible = false };

            // Lazy load billing data
            if (newIndex == 1)
            {
                newState = newState with { IsLoadingBilling = true };
                cmds.Add(Cmd.OfTask(InitializeBillingMonthsAsync));
                cmds.Add(Cmd.OfTask(LoadBillingAsync));
            }

            // Lazy load settings
            if (newIndex == 2 && state.Settings == null)
            {
                cmds.Add(Cmd.OfTask(LoadSettingsAsync));
            }

            return (newState, cmds.Count == 0 ? Cmd.None<Msg>() : Cmd.Batch(cmds.ToArray()));
        }
    }
}
