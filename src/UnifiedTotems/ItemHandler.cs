using System;
using PSS;
using Wish;

namespace UnifiedTotems;

public static class ItemHandler
{
    // FIX: Must match data/unifiedtotem.item.json "id" exactly (currently 996001 there, 99601 here).
    // CustomItems registers the JSON id; GetData with the wrong id silently finds nothing.
    public const int UNIFIED_TOTEM_ID = 99601;

    // FIX (Sprinklers pattern — https://github.com/Morthy/sunhaven-mods/tree/main/Sprinklers):
    // Do not new Scarecrow() or copy Decoration fields via reflection. CustomItems already
    // created the item; inside GetData, cast and mutate the existing instance:
    //
    //   private static void EnableUnifiedTotem(ItemData item)
    //   {
    //       var scarecrow = (Scarecrow)item.useItem;
    //       scarecrow.scareCrowEffect = ScareCrowEffect.Royal;
    //       scarecrow.range = 6;
    //       scarecrow.cropCapacity = 3;
    //   }
    //
    // Returning a new Scarecrow never registers it with the game — the returned object is unused.
    public static Scarecrow EnableTotems(ItemData item)
    {
        Scarecrow newScarecrow = new Scarecrow();

        foreach (var field in typeof(Decoration).GetFields(
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
        {
            try
            {
                field.SetValue(newScarecrow, field.GetValue(item));
            }
            catch (Exception)
            {
                // Reflection copy is fragile; prefer direct field assignment on item.useItem instead.
            }
        }

        newScarecrow.scareCrowEffect = ScareCrowEffect.Royal;
        newScarecrow.range = 6;
        newScarecrow.cropCapacity = 3;

        return newScarecrow;
    }

    // FIX: Make void like Sprinklers.CreateSprinklerItems(). Use:
    //   Database.GetData<ItemData>(UNIFIED_TOTEM_ID, EnableUnifiedTotem);
    // No return value — GetData invokes the callback when the item exists in the database.
    public static Scarecrow CreateTotems()
    {
        Scarecrow newTotem = null;
        Database.GetData<ItemData>(UNIFIED_TOTEM_ID, data => newTotem = EnableTotems(data));
        return newTotem;
    }
}
