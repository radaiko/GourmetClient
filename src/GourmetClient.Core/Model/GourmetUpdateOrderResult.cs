using System.Collections.Generic;

namespace GourmetClient.Core.Model;

public record GourmetUpdateOrderResult(IReadOnlyCollection<FailedMenuToOrderInformation> FailedMenusToOrder);