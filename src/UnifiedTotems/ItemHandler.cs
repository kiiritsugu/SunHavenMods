using System;
using System.Linq;
using PSS;
using Wish;

namespace UnifiedTotems;

public static class ItemHandler
{
    public const int UnifiedTotemId = 996001;

    // Example vanilla sources — verify ids in-game (/finditemid) or https://kryzik.github.io/sun-haven-items/
    private static readonly int[] VanillaTotemSourceIds =
    {
        1176, // Royal Totem
        1171, // Spring Totem
        1172, // Summer Totem
        1173, // Fall Totem
        1174, // Winter Totem
    };

    /// <summary>
    /// Phase 2 (CustomItems is phase 1): read vanilla totems from Database, then configure our custom item.
    /// </summary>
    public static void CreateTotems()
    {
        UnifiedTotemState.CombinedEffects.Clear();
        UnifiedTotemState.IsConfigured = false;

        var sourceCount = VanillaTotemSourceIds.Length;
        var loaded = 0;

        foreach (var sourceId in VanillaTotemSourceIds)
        {
            Database.GetData<ItemData>(sourceId, data =>
            {
                CollectFromVanillaTotem(data, sourceId);
                if (++loaded >= sourceCount)
                {
                    Database.GetData<ItemData>(UnifiedTotemId, ConfigureUnifiedTotem);
                }
            });
        }
    }

    private static void CollectFromVanillaTotem(ItemData data, int sourceId)
    {
        if (data?.useItem is not Placeable placeable || placeable._decoration is not Scarecrow scarecrow)
        {
            Plugin.logger.LogWarning(
                $"Vanilla totem {sourceId} is not a Placeable/Scarecrow — check item id or game version.");
            return;
        }

        if (!UnifiedTotemState.CombinedEffects.Contains(scarecrow.scareCrowEffect))
        {
            UnifiedTotemState.CombinedEffects.Add(scarecrow.scareCrowEffect);
        }

        UnifiedTotemState.Range = Math.Max(UnifiedTotemState.Range, scarecrow.range);
        UnifiedTotemState.CropCapacity = Math.Max(UnifiedTotemState.CropCapacity, scarecrow.cropCapacity);

        Plugin.logger.LogInfo(
            $"Collected {scarecrow.scareCrowEffect} from vanilla item {data.name} ({sourceId})");
    }

    private static void ConfigureUnifiedTotem(ItemData item)
    {
        if (item.useItem is not Placeable placeable)
        {
            Plugin.logger.LogError(
                $"Custom item {UnifiedTotemId} has no Placeable useItem — is CustomItems loaded and JSON valid?");
            return;
        }

        if (placeable._decoration is not Scarecrow scarecrow)
        {
            Plugin.logger.LogError(
                $"Custom item {UnifiedTotemId} decoration is not Scarecrow — set " +
                "\"functionality\": \"PSS.Scarecrow\" in unifiedtotem.item.json");
            return;
        }

        if (UnifiedTotemState.CombinedEffects.Count == 0)
        {
            Plugin.logger.LogWarning(
                "No vanilla totem effects collected — verify VanillaTotemSourceIds in ItemHandler.cs");
        }

        scarecrow.range = UnifiedTotemState.Range;
        scarecrow.cropCapacity = UnifiedTotemState.CropCapacity;

        // One enum on the placed Scarecrow; CropPatches adds the full CombinedEffects list to crops in range.
        scarecrow.scareCrowEffect = UnifiedTotemState.CombinedEffects.FirstOrDefault();

        UnifiedTotemState.IsConfigured = true;

        Plugin.logger.LogInfo(
            $"Configured Unified Totem with {UnifiedTotemState.CombinedEffects.Count} effect(s), " +
            $"range {UnifiedTotemState.Range}, capacity {UnifiedTotemState.CropCapacity}");
    }
}
