using System.Collections.Generic;

namespace GourmetClient.Maui.Core.Model;

public record GourmetMenuResult(GourmetUserInformation UserInformation, IReadOnlyCollection<GourmetMenu> Menus);