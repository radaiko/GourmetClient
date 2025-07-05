namespace GourmetClient.Model;

using System;
using System.Collections.Generic;

public record GourmetApiResult(bool Success, string Message);

public record FailedMenuToOrderInformation(GourmetMeal Menu, string Message);

public record GourmetUpdateOrderResult(IReadOnlyCollection<FailedMenuToOrderInformation> FailedMenusToOrder);

public record GourmetUserInformation(string NameOfUser, string ShopModelId, string EaterId, string StaffGroupId);

public record GourmetMenuResult(GourmetUserInformation UserInformation, IReadOnlyCollection<GourmetMeal> Menus);

public record GourmetMeal(
    DateTime Day,
    string MenuId,
    string MenuName,
    string Description,
    char[] Allergens,
    bool IsAvailable)
{
    /// <summary>
    /// Compares whether this instance is equal to another <see cref="GourmetMeal"/> instance.
    /// Two meals are considered equal if their <see cref="MenuId"/> and <see cref="Day"/> properties are equal.
    /// This is because the menu id is only unique within one day, but menus on different days can have the same menu id.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <returns>True if this instance is equal to the other instance, otherwise false.</returns>
    public virtual bool Equals(GourmetMeal other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Day.Equals(other.Day) && MenuId == other.MenuId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Day, MenuId);
    }
}

public record GourmetOrderedMenu(
    DateTime Day,
    string PositionId,
    string EatingCycleId,
    string MenuName,
    bool IsOrderApproved)
{
    /// <summary>
    /// Compares whether this instance is equal to another <see cref="GourmetOrderedMenu"/> instance.
    /// Two meals are considered equal if their <see cref="Day"/> and <see cref="MenuName"/> properties are equal.
    /// This is because if a menu is ordered multiple times, then the <see cref="PositionId"/> is different, even if
    /// they are referring to the same menu.
    /// </summary>
    /// <param name="other">The other instance.</param>
    /// <returns>True if this instance is equal to the other instance, otherwise false.</returns>
    public virtual bool Equals(GourmetOrderedMenu other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Day == other.Day && MenuName == other.MenuName;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Day, MenuName);
    }

    /// <summary>
    /// Checks whether this <see cref="GourmetOrderedMenu"/> instance matches a <see cref="GourmetMeal"/> instance.
    /// Since the ordered menu and the actual menu do not share any common identifier, the <see cref="Day"/> and
    /// <see cref="MenuName"/> are used for comparison.
    /// </summary>
    /// <param name="menu">The menu instance to match against.</param>
    /// <returns>True if the ordered menu matches the actual menu, otherwise false.</returns>
    public bool MatchesMenu(GourmetMeal menu)
    {
        return Day == menu.Day && MenuName == menu.MenuName;
    }
}

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