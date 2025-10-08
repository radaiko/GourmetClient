using System.Collections.Generic;

namespace GC.Core.Model;

public record GourmetMenuResult(GourmetUserInformation UserInformation, IReadOnlyCollection<GourmetMenu> Menus);