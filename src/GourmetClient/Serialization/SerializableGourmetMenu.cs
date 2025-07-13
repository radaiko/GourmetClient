using GourmetClient.Model;
using System;
using System.Text.Json.Serialization;

namespace GourmetClient.Serialization;

internal class SerializableGourmetMenu
{
    public static SerializableGourmetMenu FromGourmetMenu(GourmetMenu menu)
    {
        return new SerializableGourmetMenu
        {
            Day = menu.Day,
            MenuId = menu.MenuId,
            MenuName = menu.MenuName,
            Description = menu.Description,
            Allergens = menu.Allergens,
            IsAvailable = menu.IsAvailable
        };
    }

    [JsonPropertyName("Day")]
    public required DateTime Day { get; set; }

    [JsonPropertyName("MenuId")]
    public required string MenuId { get; set; }

    [JsonPropertyName("MenuName")]
    public required string MenuName { get; set; }

    [JsonPropertyName("Description")]
    public required string Description { get; set; }

    [JsonPropertyName("Allergens")]
    public required char[] Allergens { get; set; }

    [JsonPropertyName("IsAvailable")]
    public required bool IsAvailable { get; set; }

    public GourmetMenu ToGourmetMenu()
    {
        return new GourmetMenu(Day, MenuId, MenuName, Description, Allergens, IsAvailable);
    }
}