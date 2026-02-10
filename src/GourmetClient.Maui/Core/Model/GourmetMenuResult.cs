using System.Collections.Generic;

namespace GourmetClient.Maui.Core.Model;

public record GourmetOrderedMenuResult(
    bool IsOrderChangeForTodayPossible,
    IReadOnlyCollection<GourmetOrderedMenu> OrderedMenus);