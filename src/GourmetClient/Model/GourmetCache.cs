namespace GourmetClient.Model;

using System;
using System.Collections.Generic;

public record GourmetCache(
    DateTime Timestamp,
    GourmetUserInformation UserInformation,
    IReadOnlyCollection<GourmetMeal> Menus,
    IReadOnlyCollection<GourmetOrderedMenu> OrderedMenus);

public record InvalidatedGourmetCache()
    : GourmetCache(
        DateTime.MinValue,
        new GourmetUserInformation(
            NameOfUser: string.Empty,
            ShopModelId: string.Empty,
            EaterId: string.Empty,
            StaffGroupId: string.Empty),
        [],
        []);