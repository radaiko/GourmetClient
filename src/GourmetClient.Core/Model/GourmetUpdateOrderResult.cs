using System.Collections.Generic;

namespace GourmetClient.Model;

public record GourmetUpdateOrderResult(IReadOnlyCollection<FailedMenuToOrderInformation> FailedMenusToOrder);