using GourmetClient.Maui.Core.Model;
using System;
using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Serialization;

internal class SerializableGourmetMenu
{
    private const string CategoryValueUnknown = "Unknown";
    private const string CategoryValueMenu1 = "Menu1";
    private const string CategoryValueMenu2 = "Menu2";
    private const string CategoryValueMenu3 = "Menu3";
    private const string CategoryValueSoupAndSalad = "SoupAndSalad";

    public static SerializableGourmetMenu FromGourmetMenu(GourmetMenu menu)
    {
        return new SerializableGourmetMenu
        {
            Day = menu.Day,
            Category = CategoryToString(menu.Category),
            MenuId = menu.MenuId,
            MenuName = menu.MenuName,
            Description = menu.Description,
            Allergens = menu.Allergens,
            IsAvailable = menu.IsAvailable
        };
    }

    [JsonPropertyName("Day")]
    public required DateTime Day { get; set; }

    [JsonPropertyName("Category")]
    public required string Category { get; set; }

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
        return new GourmetMenu(Day, StringToCategory(Category), MenuId, MenuName, Description, Allergens, IsAvailable);
    }

    private static string CategoryToString(GourmetMenuCategory category)
    {
        return category switch
        {
            GourmetMenuCategory.Unknown => CategoryValueUnknown,
            GourmetMenuCategory.Menu1 => CategoryValueMenu1,
            GourmetMenuCategory.Menu2 => CategoryValueMenu2,
            GourmetMenuCategory.Menu3 => CategoryValueMenu3,
            GourmetMenuCategory.SoupAndSalad => CategoryValueSoupAndSalad,
            _ => throw new ArgumentException($"Unsupported enum value: {category}", nameof(category))
        };
    }

    private static GourmetMenuCategory StringToCategory(string value)
    {
        return value switch
        {
            CategoryValueUnknown => GourmetMenuCategory.Unknown,
            CategoryValueMenu1 => GourmetMenuCategory.Menu1,
            CategoryValueMenu2 => GourmetMenuCategory.Menu2,
            CategoryValueMenu3 => GourmetMenuCategory.Menu3,
            CategoryValueSoupAndSalad => GourmetMenuCategory.SoupAndSalad,
            _ => throw new ArgumentException($"Unsupported category value: {value}", nameof(value))
        };
    }
}