using System;
using System.Reflection;
using System.Linq;
using UnityEngine;

using PSS;
using Wish;

namespace UnifiedTotems;

public static class ItemHandler
{
    public const int UnifiedTotemId = 65301;

    // Example vanilla sources — verify ids in-game (/finditemid) or https://kryzik.github.io/sun-haven-items/
    private static readonly int[] VanillaTotemSourceIds =
    {
        10696, // Royal Totem
        10735, // Spring Totem
        10736, // Summer Totem
        10734, // Fall Totem
        10737, // Winter Totem
    };

    /// <summary>
    /// Phase 2 (CustomItems is phase 1): read vanilla totems from Database, then configure our custom item.
    /// </summary>
    public static void CreateTotems()
    {
        Database.GetData<ItemData>(UnifiedTotemId, ConvertIntoScarecrow);

        

        // UnifiedTotemState.CombinedEffects.Clear();
        // UnifiedTotemState.IsConfigured = false;

        // // var sourceCount = VanillaTotemSourceIds.Length;
        // // var loaded = 0;

        // // //Database.Instance.ids 
        // // foreach (var sourceId in VanillaTotemSourceIds)
        // // {
        // //     Database.GetData<ItemData>(sourceId, data =>
        // //     {
        // //         CollectFromVanillaTotem(data, sourceId);
        // //         if (++loaded >= sourceCount)
        // //         {
        // //             Database.GetData<ItemData>(UnifiedTotemId, ConfigureUnifiedTotem);
        // //         }
        // //     });
        // // }
            
    }

    private static void ConvertIntoScarecrow(ItemData data)
    {
        Plugin.logger.LogInfo($"UnifiedTotems: Converting custom item {UnifiedTotemId} into a Scarecrow decoration.");

        if (data?.useItem is not Placeable placeable)
        {
            Plugin.logger.LogError(
                $"UnifiedTotems: Custom item {UnifiedTotemId} has no Placeable useItem — is CustomItems loaded and JSON valid?");
            return;
        }

        if( placeable._decoration is not Scarecrow)
        {
            Scarecrow scarecrow = new Scarecrow();

            CopyBaseToDerived(placeable._decoration, scarecrow);

            scarecrow.range = UnifiedTotemState.Range;
            scarecrow.cropCapacity = UnifiedTotemState.CropCapacity;
            scarecrow.scareCrowEffect = ScareCrowEffect.Royal;

            placeable._decoration = scarecrow;
        }

        Database.GetData<ItemData>(UnifiedTotemId, itemData =>
        {
            if (itemData?.useItem is not Placeable placeable || placeable._decoration is not Scarecrow)
            {
                Plugin.logger.LogError(
                    $"UnifiedTotems: Custom item {UnifiedTotemId} failed to convert into a Scarecrow decoration");
                return;
            }

            Plugin.logger.LogInfo(
                $"UnifiedTotems: Custom item {UnifiedTotemId} successfully converted into a Scarecrow decoration.");
                
        });
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

    public static void CopyBaseToDerived<TBase, TDerived>(TBase source, TDerived target) where TDerived : TBase
    {
        // Copy all fields (including private ones from the game's base class)
        FieldInfo[] fields = typeof(TBase).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            field.SetValue(target, field.GetValue(source));
        }

        // Copy all properties that have both a getter and a setter
        PropertyInfo[] properties = typeof(TBase).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo prop in properties)
        {
            if (prop.CanWrite && prop.CanRead)
            {
                prop.SetValue(target, prop.GetValue(source, null), null);
            }
        }
    }

    public static void ReplaceInDatabase<ItemData>(int itemId, ItemData newData)
    {
    }
}
