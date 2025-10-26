using System;
using System.Collections.Generic;
using System.Linq;

namespace GC.Models;

public static class Extensions {
  public static Day[] ToDays(this IEnumerable<Menu> menus) {
    var daysDict = new Dictionary<DateTime, Day>();

    foreach (var menu in menus) {
      if (!daysDict.TryGetValue(menu.Date, out Day? day)) {
        day = new Day(menu.Date, null!, null!, null!, null!);
        daysDict[menu.Date] = day;
      }

      switch (menu.Type) {
        case MenuType.Menu1:
          day.Menu1 = menu;
          break;
        case MenuType.Menu2:
          day.Menu2 = menu;
          break;
        case MenuType.Menu3:
          day.Menu3 = menu;
          break;
        case MenuType.SoupAndSalad:
          day.SoupAndSalad = menu;
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    return daysDict.Values.ToArray();
  }
  public static Menu[] ToMenu(this IEnumerable<Day> days) => days.SelectMany(d => d.Menus).ToArray();
}