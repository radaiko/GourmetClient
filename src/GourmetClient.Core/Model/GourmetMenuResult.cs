using GourmetClient.Core.Model;
using System.Collections.Generic;

namespace GourmetClient.Core.Model;

public record GourmetMenuResult(GourmetUserInformation UserInformation, IReadOnlyCollection<GourmetMenu> Menus);