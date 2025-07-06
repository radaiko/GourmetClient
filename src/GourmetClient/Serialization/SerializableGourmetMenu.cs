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
        Day = menu.Day;
        MenuId = menu.MenuId;
        MenuName = menu.MenuName;
        Description = menu.Description;
        Allergens = menu.Allergens;
        IsAvailable = menu.IsAvailable;
    }

    public DateTime? Day { get; set; }

    public string? MenuId { get; set; }

    public string? MenuName { get; set; }

    public string? Description { get; set; }

    public char[]? Allergens { get; set; }

    public bool? IsAvailable { get; set; }

    public GourmetMenu ToGourmetMenu()
    {
        if (Day is null || MenuId is null || MenuName is null || Description is null || IsAvailable is null)
        {
            throw new InvalidOperationException("Information for the menu is not complete");
        }

        return new GourmetMenu(Day.Value, MenuId, MenuName, Description, Allergens ?? [], IsAvailable.Value);
    }
}