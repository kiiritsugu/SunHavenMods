using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using PSS;
using Wish;
using HarmonyLib;

namespace UnifiedTotems;

public static class ItemHandler
{

    // Example vanilla source id for collecting scarecrow data (range, capacity, effects, etc.)
    private static int VanillaTotemSourceId = 10696; // Royal Totem

    public static void CreateTotems()
    {
        // Reset the configuration state before starting the process.
        UnifiedTotemState.IsConfigured = false;  

        //Saving data from vanilla totems to UnifiedTotemState
        Database.GetData<ItemData>(VanillaTotemSourceId, data =>
        {
            CollectFromVanillaTotem(data, VanillaTotemSourceId);
        });

        // Convert each decoration to scarecrow and check if the conversion was successful.
        foreach (KeyValuePair<int, ScareCrowEffect[]> pair in TotemIndex.TotemDictionary)
        {
            Database.GetData<ItemData>(pair.Key, ConfigureAsScarecrow);

            // Health check: log the final configuration of the unified totem.
            DebugCheckTotemInDatabase(pair.Key);
        }

        UnifiedTotemState.IsConfigured = true;
    }

    private static void ConfigureAsScarecrow(ItemData data)
    {
        Plugin.logger.LogInfo($"UnifiedTotems: Configuring custom item {data?.name} as a Scarecrow decoration.");

        if( data == null)
        {
            Plugin.logger.LogError(
                $"UnifiedTotems: Custom item {data?.name} not found in Database — is CustomItems loaded and JSON valid?");
            return;
        }

        if (data?.useItem is not Placeable placeable)
        {
            Plugin.logger.LogError(
                $"UnifiedTotems: Custom item {data?.name} has no Placeable useItem — is CustomItems loaded and JSON valid?");
            return;
        }

        if( placeable._decoration is Scarecrow)
        {   
            Plugin.logger.LogInfo(
                $"UnifiedTotems: Custom item {data?.name} is already a Scarecrow decoration.");
            return;
        }

        //Add the scarecrow range preview mimicking vanilla totem
        RangePreviewer.CreateRangePreview(data, UnifiedTotemState.Range);

        // Convert the decoration to a Scarecrow component, copying over all relevant fields and properties.
        Decoration baseDecoration = placeable._decoration;
        Scarecrow newScarecrow = baseDecoration.gameObject.AddComponent<Scarecrow>();

        CopyBaseToDerived(baseDecoration, newScarecrow);
        UnityEngine.Object.DestroyImmediate(baseDecoration, true);

        // Configure the new Scarecrow with the collected effects, range, and capacity, before replacing the original decoration.
        newScarecrow.scareCrowEffect = ScareCrowEffect.None;
        newScarecrow.range = UnifiedTotemState.Range;
        newScarecrow.cropCapacity = UnifiedTotemState.CropCapacity;

        //Atach the UnifiedTotem component to hold extra properties and logic
        newScarecrow.gameObject.AddComponent<UnifiedTotem>();

        Utilitaries.TilePerfectBoxColider2D(newScarecrow, false);
        
        placeable._decoration = newScarecrow;


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

        UnifiedTotemState.Range = Math.Max(UnifiedTotemState.Range, scarecrow.range);
        UnifiedTotemState.CropCapacity = Math.Max(UnifiedTotemState.CropCapacity, scarecrow.cropCapacity);

        Plugin.logger.LogInfo(
            $"Collected {scarecrow.scareCrowEffect} from vanilla item {data.name} ({sourceId})");
    }

    //
    public static void AdjustVanillaTotems()
    {
        foreach (int totemId in TotemIndex.VanillaTotemIds)
        {
            Database.GetData<ItemData>(totemId, itemData =>
            {
                if (itemData == null)
                {
                    Plugin.logger.LogError($"AdjustVanillaTotems: Item {itemData.id} not found in Database.");
                    return;
                }
                if (itemData.useItem is not Placeable placeable || placeable._decoration is not Scarecrow scarecrow)
                {
                    Plugin.logger.LogError($"AdjustVanillaTotems: Item {itemData.name} with ID {itemData.id} is not a totem, verify the item id or game version.");
                    return;
                }               


                Utilitaries.TilePerfectBoxColider2D(scarecrow, false);
            });
        }
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

    public static void DebugCheckTotemInDatabase(int totemId)
    {
        Plugin.logger.LogInfo($"Checking if everything was properly setup for the totems...");
        
        Database.GetData<ItemData>(totemId, itemData =>
        {
            if (itemData == null)
            {
                Plugin.logger.LogError($"CheckTotemInDatabase: Item {itemData.name} not found in Database.");
                return;
            }

            if (itemData.useItem is not Placeable placeable || placeable._decoration is not Scarecrow scarecrow)
            {
                Plugin.logger.LogError($"CheckTotemInDatabase: Item {itemData.name} failed to be configured as a Scarecrow decoration.");
                return;
            }

            Plugin.logger.LogInfo($"CheckTotemInDatabase: Item {itemData.name} is a valid Scarecrow decoration.");
            Plugin.logger.LogInfo($"CheckTotemInDatabase: Scarecrow range: {scarecrow.range}, capacity: {scarecrow.cropCapacity}, effect: {scarecrow.scareCrowEffect}, combined effects: {string.Join(", ", placeable._decoration.GetComponents<UnifiedTotem>().SelectMany(unifiedTotem => unifiedTotem.CombinedEffects))}");
        });
    }

   public static void PatchCropColliders()
    {
        try
        {
            Database db = Database.Instance;
            if (db == null) return;

            // Direct, publicized access to the cache! 
            // It maps: Dictionary<Type, Dictionary<object, LinkedListNode<CacheItem>>>
            HashSet<int> validIDs = db.validIDs;

            if (validIDs == null || validIDs.Count == 0)
            {
                Plugin.logger.LogWarning("[TotemMod] Database validIDs is empty or null.");
                return;
            }


            int dynamicCropCount = 0;

            foreach (int id in validIDs)
            {
                Database.GetData<ItemData>(id, itemData =>
                {
                    if (itemData == null) return;

                    if (itemData.useItem is not Seeds seed || seed._crop is not Crop crop) return;

                    Utilitaries.TilePerfectBoxColider2D(crop, false);
                    dynamicCropCount++;
                });
            }

            Plugin.logger.LogInfo($"[TotemMod] Traversed in-memory cache and successfully injected colliders into {dynamicCropCount} Seed -> Crop prefabs.");
        }
        catch (Exception ex)
        {
            Plugin.logger.LogError($"Error occurred while patching crop colliders: {ex}");
        }
    }
}
