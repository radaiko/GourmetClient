using System.Collections.Immutable;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Messages;

public abstract record Msg;

// UI Messages
public record ToggleBilling : Msg;
public record ToggleSettings : Msg;
public record ToggleAbout : Msg;
public record SelectMonth(DateTime Month) : Msg;

// Menu Messages
public record LoadMenus : Msg;
public record MenusLoaded(ImmutableList<GourmetMenuDayViewModel> MenuDays) : Msg;
public record UpdateMenu : Msg;
public record ToggleMenuOrder(int MenuId) : Msg;
public record ExecuteSelectedOrder : Msg;

// Billing Messages
public record LoadBilling : Msg;

public record BillingLoaded(
  ImmutableList<GroupedBillingPositionsViewModel> MenuBillingPositions,
  ImmutableList<GroupedBillingPositionsViewModel> DrinkBillingPositions,
  decimal SumCostMenuBillingPositions,
  decimal SumCostDrinkBillingPositions,
  decimal SumCostUnknownBillingPositions
) : Msg;

// Error handling
public record ErrorOccurred(string Message) : Msg;
public record ClearError : Msg;