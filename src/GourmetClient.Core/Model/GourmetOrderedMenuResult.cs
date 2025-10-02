using System;
using System.Collections.Generic;
using GourmetClient.Core.Model;

namespace GourmetClient.Core.Model;

public record GourmetOrderedMenuResult(
    bool IsOrderChangeForTodayPossible,
    IReadOnlyCollection<GourmetOrderedMenu> OrderedMenus);