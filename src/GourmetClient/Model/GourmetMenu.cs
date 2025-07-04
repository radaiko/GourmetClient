namespace GourmetClient.Model;

using System;
using System.Collections.Generic;

public record GourmetUserInformation(string NameOfUser, string ShopModelId, string EaterId, string StaffGroupId);

public record GourmetMenuResult(GourmetUserInformation UserInformation, IReadOnlyCollection<GourmetMeal> Menus);

public record GourmetMeal(
    DateTime Day,
    string MenuId,
    string MenuName,
    string Description,
    char[] Allergens,
    bool IsAvailable);

public record GourmetOrderedMenu(
    DateTime Day,
    string PositionId,
    string EatingCycleId,
    string MenuName,
    bool IsOrderApproved);

public class GourmetMenu
{
    public GourmetMenu()
        : this(new GourmetMenuDay[0])
    {
    }

    public GourmetMenu(IReadOnlyList<GourmetMenuDay> days)
    {
        Days = days ?? throw new ArgumentNullException(nameof(days));
    }

    public IReadOnlyList<GourmetMenuDay> Days { get; }
}