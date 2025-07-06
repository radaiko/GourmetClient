namespace GourmetClient.Serialization;

using System;
using Model;

internal class SerializableGourmetMenu
{
    public SerializableGourmetMenu()
    {
        // Used for deserialization
    }

    public SerializableGourmetMenu(GourmetMenu menu)
    {
        menu = menu ?? throw new ArgumentNullException(nameof(menu));

        Day = menu.Day;
        MenuId = menu.MenuId;
        MenuName = menu.MenuName;
        Description = menu.Description;
        Allergens = menu.Allergens;
        IsAvailable = menu.IsAvailable;
    }

    public DateTime Day { get; set; }

    public string MenuId { get; set; }

    public string MenuName { get; set; }

    public string Description { get; set; }

    public char[] Allergens { get; set; }

    public bool IsAvailable { get; set; }

    public GourmetMenu ToGourmetMenu()
    {
        return new GourmetMenu(Day, MenuId, MenuName, Description, Allergens, IsAvailable);
    }
}