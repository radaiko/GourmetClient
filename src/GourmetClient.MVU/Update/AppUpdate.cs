using System.Collections.Immutable;
using GourmetClient.MVU.Core;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Messages;

namespace GourmetClient.MVU.Update
{
    public static class AppUpdate
    {
        public static (AppState, Cmd<Msg>) UpdateState(Msg message, AppState state)
        {
            return message switch
            {
                // UI Toggle Messages
                ToggleBilling => (state with { IsBillingVisible = !state.IsBillingVisible }, Cmd.None<Msg>()),
                ToggleSettings => (state with { IsSettingsVisible = !state.IsSettingsVisible }, Cmd.None<Msg>()),
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
                LoadBilling => (state with { IsLoading = true }, Cmd.OfTask(LoadBillingAsync)),
                BillingLoaded billingData => (state with 
                { 
                    IsLoading = false,
                    MenuBillingPositions = billingData.MenuBillingPositions,
                    DrinkBillingPositions = billingData.DrinkBillingPositions,
                    SumCostMenuBillingPositions = billingData.SumCostMenuBillingPositions,
                    SumCostDrinkBillingPositions = billingData.SumCostDrinkBillingPositions,
                    SumCostUnknownBillingPositions = billingData.SumCostUnknownBillingPositions
                }, Cmd.None<Msg>()),
                
                SelectMonth month => (state with { SelectedMonth = month.Month }, Cmd.OfTask(LoadBillingAsync)),
                
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
            try
            {
                // TODO: Implement actual billing loading using GourmetClient.Core
                await Task.Delay(800); // Simulate loading
                
                // Create sample billing data for now
                var menuBilling = ImmutableList.Create(
                    new GroupedBillingPositionsViewModel("Schnitzel mit Pommes", 2, 15.80m),
                    new GroupedBillingPositionsViewModel("Pasta Bolognese", 1, 8.50m)
                );
                
                var drinkBilling = ImmutableList.Create(
                    new GroupedBillingPositionsViewModel("Apfelschorle", 3, 7.50m),
                    new GroupedBillingPositionsViewModel("Kaffee", 5, 12.50m)
                );
                
                return new BillingLoaded(menuBilling, drinkBilling, 24.30m, 20.00m, 0m);
            }
            catch (Exception ex)
            {
                return new ErrorOccurred($"Failed to load billing: {ex.Message}");
            }
        }
    }
}
