using System.Collections.Generic;

namespace GC.Core.Model;

public record GourmetOrderedMenuResult(
  bool IsOrderChangeForTodayPossible,
  IReadOnlyCollection<GourmetOrderedMenu> OrderedMenus);