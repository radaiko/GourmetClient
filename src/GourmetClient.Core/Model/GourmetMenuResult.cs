using System.Collections.Generic;

namespace GourmetClient.Model;

public record GourmetOrderedMenuResult(
    bool IsOrderChangeForTodayPossible,
    IReadOnlyCollection<GourmetOrderedMenu> OrderedMenus);