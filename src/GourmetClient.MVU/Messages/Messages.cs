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
public record ToggleMenuOrder(string MenuTitle) : Msg;
public record ExecuteOrder : Msg;
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

// About Messages
public record ShowReleaseNotes : Msg;
public record OpenIconAuthorWebPage : Msg;
public record OpenFlatIconWebPage : Msg;

// Settings Messages
public record UpdateUsername(string Username) : Msg;
public record UpdatePassword(string Password) : Msg;
public record UpdateVentoPayUsername(string VentoPayUsername) : Msg;
public record UpdateVentoPayPassword(string VentoPayPassword) : Msg;
public record UpdateAutoUpdate(bool AutoUpdate) : Msg;
public record UpdateStartWithWindows(bool StartWithWindows) : Msg;
public record UpdateTheme(string Theme) : Msg;
public record SaveSettings : Msg;
public record SaveFormSettings(
  string Username,
  string Password, 
  string VentoPayUsername,
  string VentoPayPassword,
  bool AutoUpdate,
  bool StartWithWindows,
  string Theme
) : Msg;
public record LoadSettings : Msg;
public record SettingsLoaded(AppSettings Settings) : Msg;

// App Initialization
public record InitializeApp : Msg;
public record AppInitialized(AppSettings Settings) : Msg;

// Error handling
public record ErrorOccurred(string Message) : Msg;
public record ClearError : Msg;