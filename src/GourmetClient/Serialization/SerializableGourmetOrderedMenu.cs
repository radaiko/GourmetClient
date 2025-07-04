namespace GourmetClient.Serialization;

using System;
using Model;

internal class SerializableGourmetOrderedMenu
{
    public SerializableGourmetOrderedMenu()
    {
        // Used for deserialization
    }

    public SerializableGourmetOrderedMenu(GourmetOrderedMenu orderedMenu)
    {
        orderedMenu = orderedMenu ?? throw new ArgumentNullException(nameof(orderedMenu));

        Day = orderedMenu.Day;
        PositionId = orderedMenu.PositionId;
        EatingCycleId = orderedMenu.EatingCycleId;
        MenuName = orderedMenu.MenuName;
        IsOrderApproved = orderedMenu.IsOrderApproved;
    }

    public DateTime Day { get; set; }

    public string PositionId { get; set; }

    public string EatingCycleId { get; set; }

    public string MenuName { get; set; }

    public bool IsOrderApproved { get; set; }

    public GourmetOrderedMenu ToOrderedGourmetMenu()
    {
        return new GourmetOrderedMenu(Day, PositionId, EatingCycleId, MenuName, IsOrderApproved);
    }
}