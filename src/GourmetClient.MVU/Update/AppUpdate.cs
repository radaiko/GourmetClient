using System.Collections.Immutable;
using GourmetClient.MVU.Core;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Utils;
using GourmetClient.Core.Utils;
using GourmetClient.Core.Network;
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
                ToggleBilling => (state with { IsBillingVisible = !state.IsBillingVisible }, Cmd.None<Msg>()),
                ToggleSettings => state.IsSettingsVisible 
                    ? (state with { IsSettingsVisible = false }, Cmd.None<Msg>()) 
                    : (state with { IsSettingsVisible = true }, Cmd.OfTask(LoadSettingsAsync)),
                ToggleAbout => (state with { IsAboutVisible = !state.IsAboutVisible }, Cmd.None<Msg>()),
                
                // Menu Messages
                LoadMenus => (state with { IsLoading = true }, Cmd.OfTask(LoadMenusAsync)),
                MenusLoaded menuData => (state with 
                { 
                    IsLoading = false, 
                    MenuDays = menuData.MenuDays 
                }, Cmd.None<Msg>()),
                
                UpdateMenu => (state with { IsLoading = true }, Cmd.OfTask(UpdateMenuAsync)),
                
                ToggleMenuOrder toggleOrder => ToggleMenuOrderImpl(toggleOrder, state),
                
                ExecuteSelectedOrder => (state with { IsLoading = true }, Cmd.OfTask(ExecuteOrderAsync)),
                
                // Billing Messages
                LoadBilling => (state with { IsLoadingBilling = true, IsBillingVisible = true }, Cmd.OfTask(LoadBillingAsync)),
                BillingLoaded billingData => (state with 
                { 
                    IsLoadingBilling = false,
                    MenuBillingPositions = billingData.MenuBillingPositions,
                    DrinkBillingPositions = billingData.DrinkBillingPositions,
                    SumCostMenuBillingPositions = billingData.SumCostMenuBillingPositions,
                    SumCostDrinkBillingPositions = billingData.SumCostDrinkBillingPositions,
                    SumCostUnknownBillingPositions = billingData.SumCostUnknownBillingPositions
                }, Cmd.None<Msg>()),
                
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
                AppInitialized appData => (state with { Settings = appData.Settings }, Cmd.None<Msg>()),
                
                // Error handling
                ErrorOccurred error => (state with { ErrorMessage = error.Message, IsLoading = false }, Cmd.None<Msg>()),
                ClearError => (state with { ErrorMessage = null }, Cmd.None<Msg>()),
                
                _ => (state, Cmd.None<Msg>())
            };
        }

        private static (AppState, Cmd<Msg>) ToggleMenuOrderImpl(ToggleMenuOrder toggleOrder, AppState state)
        {
            var updatedMenuDays = state.MenuDays?.Select(day => day with
            {
                Menus = day.Menus.Select(menu =>
                {
                    if (menu.MenuDescription == toggleOrder.MenuTitle)
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
                }).ToImmutableList()
            }).ToImmutableList();
            
            return (state with { MenuDays = updatedMenuDays }, Cmd.None<Msg>());
        }
        
        private static async Task<Msg> LoadMenusAsync()
        {
            try
            {
                // TODO: Implement actual menu loading using GourmetClient.Core
                await Task.Delay(1000); // Simulate loading
                
                // Create sample data for now
                var sampleMenus = ImmutableList.Create(
                    new GourmetMenuDayViewModel(
                        DateTime.Today,
                        ImmutableList.Create(
                            new GourmetMenuViewModel(
                                1,
                                "Schnitzel mit Pommes",
                                Array.Empty<char>(),
                                GourmetMenuState.None,
                                false,
                                false,
                                true,
                                GourmetClient.Core.Model.GourmetMenuCategory.Menu1
                            )
                        )
                    )
                );
                
                return new MenusLoaded(sampleMenus);
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
                // TODO: Implement actual menu update using GourmetClient.Core
                await Task.Delay(500); // Simulate update
                return new LoadMenus();
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
                // TODO: Implement actual order execution using GourmetClient.Core
                await Task.Delay(1000); // Simulate order execution
                return new LoadMenus();
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to execute order: {ex.Message}");
            }
        }
        
        private static async Task<Msg> LoadBillingAsync()
        {
            var now = DateTime.Now;
            return await LoadBillingForMonthAsync(now);
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


    }
}
