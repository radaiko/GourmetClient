using System.Collections.Generic;

namespace GourmetClient.Maui.Core.Model;

public record GourmetUpdateOrderResult(IReadOnlyCollection<FailedMenuToOrderInformation> FailedMenusToOrder);