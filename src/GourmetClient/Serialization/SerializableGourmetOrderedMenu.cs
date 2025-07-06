using System;
using GourmetClient.Model;

namespace GourmetClient.Serialization;

internal class SerializableGourmetOrderedMenu
{
    public SerializableGourmetOrderedMenu()
    {
        // Used for deserialization
    }

    public SerializableGourmetOrderedMenu(GourmetOrderedMenu orderedMenu)
    {
        Day = orderedMenu.Day;
        PositionId = orderedMenu.PositionId;
        EatingCycleId = orderedMenu.EatingCycleId;
        MenuName = orderedMenu.MenuName;
        IsOrderApproved = orderedMenu.IsOrderApproved;
    }

    public DateTime? Day { get; set; }

    public string? PositionId { get; set; }

    public string? EatingCycleId { get; set; }

    public string? MenuName { get; set; }

    public bool? IsOrderApproved { get; set; }

    public GourmetOrderedMenu ToOrderedGourmetMenu()
    {
        if (Day is null || PositionId is null || EatingCycleId is null || MenuName is null || IsOrderApproved is null)
        {
            throw new InvalidOperationException("Information for the ordered menu is not complete");
        }

        return new GourmetOrderedMenu(Day.Value, PositionId, EatingCycleId, MenuName, IsOrderApproved.Value);
    }
}