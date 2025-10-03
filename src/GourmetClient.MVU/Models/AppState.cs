using System.Collections.Immutable;
using GourmetClient.Core.Model;

namespace GourmetClient.MVU.Models;

public record AppState(
  bool IsLoading = false
  , bool IsLoadingBilling = false
  , bool IsBillingVisible = false
  , bool IsSettingsVisible = false
  , bool IsAboutVisible = false
  , ImmutableList<GourmetMenuDayViewModel>? MenuDays = null
  , ImmutableList<GroupedBillingPositionsViewModel>? MenuBillingPositions = null
  , ImmutableList<GroupedBillingPositionsViewModel>? DrinkBillingPositions = null
  , ImmutableList<DateTime>? AvailableMonths = null
  , DateTime? SelectedMonth = null
  , decimal SumCostMenuBillingPositions = 0
  , decimal SumCostDrinkBillingPositions = 0
  , decimal SumCostUnknownBillingPositions = 0
  , string? ErrorMessage = null
  , AppSettings? Settings = null
  , string UserName = ""
  , DateTime? LastMenuUpdate = null
) {
  public static AppState Initial => new(Settings: new AppSettings());
}

public record AppSettings(
  string Username = ""
  , string Password = ""
  , string VentoPayUsername = ""
  , string VentoPayPassword = ""
  , bool AutoUpdate = true
  , bool StartWithWindows = false
  , string Theme = "System"
);

public record GourmetMenuDayViewModel(
  DateTime Date
  , ImmutableList<GourmetMenuViewModel> Menus
);

public record GourmetMenuViewModel(
  string MenuId
  , string MenuDescription
  , char[] Allergens
  , GourmetMenuState MenuState
  , bool IsOrdered
  , bool IsOrderApproved
  , bool IsOrderCancelable
  , GourmetMenuCategory Category
);

public record GroupedBillingPositionsViewModel(
  string Description
  , int Quantity
  , decimal TotalCost
);

public enum GourmetMenuState {
  None
  , NotAvailable
  , MarkedForOrder
  , MarkedForCancel
  , Ordered
}