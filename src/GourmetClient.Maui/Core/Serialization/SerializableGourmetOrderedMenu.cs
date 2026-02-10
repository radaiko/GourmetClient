using GourmetClient.Maui.Core.Model;
using System;
using System.Text.Json.Serialization;

namespace GourmetClient.Maui.Core.Serialization;

internal class SerializableGourmetOrderedMenu
{
    public static SerializableGourmetOrderedMenu FromGourmetOrderedMenu(GourmetOrderedMenu orderedMenu)
    {
        return new SerializableGourmetOrderedMenu
        {
            Day = orderedMenu.Day,
            PositionId = orderedMenu.PositionId,
            EatingCycleId = orderedMenu.EatingCycleId,
            MenuName = orderedMenu.MenuName,
            IsOrderApproved = orderedMenu.IsOrderApproved,
            IsOrderCancelable = orderedMenu.IsOrderCancelable
        };
    }

    [JsonPropertyName("Day")]
    public required DateTime Day { get; set; }

    [JsonPropertyName("PositionId")]
    public required string PositionId { get; set; }

    [JsonPropertyName("EatingCycleId")]
    public required string EatingCycleId { get; set; }

    [JsonPropertyName("MenuName")]
    public required string MenuName { get; set; }

    [JsonPropertyName("IsOrderApproved")]
    public required bool IsOrderApproved { get; set; }

    [JsonPropertyName("IsOrderCancelable")]
    public required bool IsOrderCancelable { get; set; }

    public GourmetOrderedMenu ToOrderedGourmetMenu()
    {
        return new GourmetOrderedMenu(Day, PositionId, EatingCycleId, MenuName, IsOrderApproved, IsOrderCancelable);
    }
}