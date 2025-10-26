using System;
using System.Collections.Generic;
using System.Linq;

namespace GC.Models;

public static class Extensions {
  public static IEnumerable<Day> ToDays(this IEnumerable<Menu> menus) {
    List<Day> days = [];
    foreach (var menu in menus) {
      var date = menu.Date;
      var day = days.FirstOrDefault(d => d.Date == date);
      if (day == null) {
        day = new Day(
          menus.FirstOrDefault( m=> m.Date == date && m.Type == MenuType.Menu1), 
          menus.FirstOrDefault( m=> m.Date == date && m.Type == MenuType.Menu2), 
          menus.FirstOrDefault( m=> m.Date == date && m.Type == MenuType.Menu3),
          menus.FirstOrDefault( m=> m.Date == date && m.Type == MenuType.SoupAndSalad));
        days.Add(day);
      }
    }
    return days;
  }
  public static Menu[] ToMenu(this IEnumerable<Day> days) => days.SelectMany(d => d.Menus).ToArray();
}