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
        // Clear any previous state before starting the configuration process.
        UnifiedTotemState.CombinedEffects.Clear();
        UnifiedTotemState.IsConfigured = false;  

        //For starters collecting range, capacity, effects and secondary preview sprite from the first vanilla totem source id.
        //this is saved to UnifiedTotemState for acess
        Database.GetData<ItemData>(VanillaTotemSourceIds.FirstOrDefault(), data =>
        {
            CollectFromVanillaTotem(data, VanillaTotemSourceIds.FirstOrDefault());
        });

        // Convert decoration to scarecrow and check if the conversion was successful.
        Database.GetData<ItemData>(UnifiedTotemId, ConfigureAsScarecrow);


        // Health check: log the final configuration of the unified totem.
        DebugCheckTotemInDatabase();
    }

    private static void ConfigureAsScarecrow(ItemData data)
    {
        Plugin.logger.LogInfo($"UnifiedTotems: Configuring custom item {UnifiedTotemId} as a Scarecrow decoration.");

        if( data == null)
        {
            Plugin.logger.LogError(
                $"UnifiedTotems: Custom item {UnifiedTotemId} not found in Database — is CustomItems loaded and JSON valid?");
            return;
        }

        if (data?.useItem is not Placeable placeable)
        {
            Plugin.logger.LogError(
                $"UnifiedTotems: Custom item {UnifiedTotemId} has no Placeable useItem — is CustomItems loaded and JSON valid?");
            return;
        }

        if( placeable._decoration is Scarecrow)
        {   
            Plugin.logger.LogInfo(
                $"UnifiedTotems: Custom item {UnifiedTotemId} is already a Scarecrow decoration.");
            return;
        }

        //Add the scarecrow range preview from vanilla totem
        RangePreviewer.CreateRangePreview(data, UnifiedTotemState.Range);

        // Convert the decoration to a Scarecrow component, copying over all relevant fields and properties.
        Decoration baseDecoration = placeable._decoration;
        Scarecrow newScarecrow = baseDecoration.gameObject.AddComponent<Scarecrow>();

        CopyBaseToDerived(baseDecoration, newScarecrow);
        UnityEngine.Object.DestroyImmediate(baseDecoration, true);

        // Configure the new Scarecrow with the collected effects, range, and capacity, before replacing the original decoration.
        newScarecrow.scareCrowEffect = ScareCrowEffect.Royal;
        newScarecrow.range = UnifiedTotemState.Range;
        newScarecrow.cropCapacity = UnifiedTotemState.CropCapacity;
        
        placeable._decoration = newScarecrow;

        UnifiedTotemState.IsConfigured = true;
    }

    //Collects scarecrow data from vanilla totems and saves it to UnifiedTotemState
    private static void CollectFromVanillaTotem(ItemData data, int sourceId)
    {
        if (data?.useItem is not Placeable placeable || placeable._decoration is not Scarecrow scarecrow)
        {
            Plugin.logger.LogWarning(
                $"Vanilla totem {sourceId} is not a Placeable/Scarecrow — check item id or game version.");
            return;
        }

        // if (!UnifiedTotemState.CombinedEffects.Contains(scarecrow.scareCrowEffect))
        // {
        //     UnifiedTotemState.CombinedEffects.Add(scarecrow.scareCrowEffect);
        // }

        // For now, just add Royal effect for testing. Modify this to add a list of effects later;
        UnifiedTotemState.CombinedEffects.Add(ScareCrowEffect.Royal);

        UnifiedTotemState.Range = Math.Max(UnifiedTotemState.Range, scarecrow.range);
        UnifiedTotemState.CropCapacity = Math.Max(UnifiedTotemState.CropCapacity, scarecrow.cropCapacity);

        Plugin.logger.LogInfo(
            $"Collected {scarecrow.scareCrowEffect} from vanilla item {data.name} ({sourceId})");
    }

    public static void CopyBaseToDerived<TBase, TDerived>(TBase source, TDerived target) where TDerived : TBase
    {
        // 1. Copy fields (This is safe! It copies raw data, IDs, textures, and settings)
        FieldInfo[] fields = typeof(TBase).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            field.SetValue(target, field.GetValue(source));
        }

        // 2. Safely copy properties (Skip Unity internal properties that cause NullReferenceExceptions)
        PropertyInfo[] properties = typeof(TBase).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (PropertyInfo prop in properties)
        {
            if (!prop.CanWrite || !prop.CanRead) continue;

            // SKIP list for Unity properties that require a live, spawned GameObject scene context
            if (prop.Name == "Position" || prop.Name == "transform" || prop.Name == "gameObject" || prop.Name == "tag")
            {
                continue;
            }

            try
            {
                prop.SetValue(target, prop.GetValue(source, null), null);
            }
            catch (Exception)
            {
                Plugin.logger.LogWarning($"CopyBaseToDerived: Skipping property {prop.Name} due to exception.");
                // Catch-all to keep the database loading smoothly if another property complains
            }
        }
    }

    public static void DebugCheckTotemInDatabase()
    {
        Plugin.logger.LogInfo($"Checking if everything was properly setup for the totems...");
        
        Database.GetData<ItemData>(UnifiedTotemId, itemData =>
        {
            if (itemData == null)
            {
                Plugin.logger.LogError($"CheckTotemInDatabase: Item {UnifiedTotemId} not found in Database.");
                return;
            }

            if (itemData.useItem is not Placeable placeable || placeable._decoration is not Scarecrow scarecrow)
            {
                Plugin.logger.LogError($"CheckTotemInDatabase: Item {UnifiedTotemId} failed to be configured as a Scarecrow decoration.");
                return;
            }

            Plugin.logger.LogInfo($"CheckTotemInDatabase: Item {UnifiedTotemId} is a valid Scarecrow decoration.");
            Plugin.logger.LogInfo($"CheckTotemInDatabase: Scarecrow range: {scarecrow.range}, capacity: {scarecrow.cropCapacity}, effect: {scarecrow.scareCrowEffect}, secondary preview sprite: {placeable._secondaryPreviewSprite}, preview offset: {placeable.previewOffset}");
        });
    }
}
