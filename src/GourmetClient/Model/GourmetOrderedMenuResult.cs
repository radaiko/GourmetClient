using System.Collections.Generic;

namespace GourmetClient.Model;

public record GourmetMenuResult(GourmetUserInformation UserInformation, IReadOnlyCollection<GourmetMenu> Menus);