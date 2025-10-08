using System.Collections.Generic;

namespace GC.Core.Model;

public record GourmetUpdateOrderResult(IReadOnlyCollection<FailedMenuToOrderInformation> FailedMenusToOrder);