using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using PSS;
using Wish;

using Morthy.Util;
using Shared;

namespace UnifiedTotems;

public static class ItemHandler
{
    // Manage creation of unified totems from the TotemIndex dictionary
    public static void CreateTotems()
    {
        UnifiedTotemState.IsConfigured = false;

        // Convert each decoration to scarecrow and check if the conversion was successful.
        foreach (KeyValuePair<int, ScareCrowEffect[]> pair in TotemIndex.TotemDictionary)
        {
            Database.GetData<ItemData>(pair.Key, ConfigureAsScarecrow);
            DebugCheckTotemInDatabase(pair.Key);
        }

        UnifiedTotemState.IsConfigured = true;
    }

    // Configures a given Item as a Scarecrow decoration, and adding the UnifiedTotem component.
    private static void ConfigureAsScarecrow(ItemData data)
    {
        if (data == null)
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

        if (placeable._decoration is Scarecrow)
        {
            return;
        }

        //Add the scarecrow range preview mimicking vanilla totem
        CreateRangePreview(data, UnifiedTotemState.Range);

        // Convert the decoration to a Scarecrow component, copying over all relevant fields and properties.
        Decoration baseDecoration = placeable._decoration;
        Scarecrow newScarecrow = baseDecoration.gameObject.AddComponent<Scarecrow>();
        DatabaseUtils.CopyBaseToDerived(baseDecoration, newScarecrow);
        UnityEngine.Object.DestroyImmediate(baseDecoration, true);

        // Configure the new Scarecrow with the collected effects, range, and capacity, before replacing the original decoration.
        newScarecrow.scareCrowEffect = ScareCrowEffect.None;
        newScarecrow.range = UnifiedTotemState.Range;
        newScarecrow.cropCapacity = UnifiedTotemState.CropCapacity;

        //Atach the UnifiedTotem component to hold extra properties and logic
        newScarecrow.gameObject.AddComponent<UnifiedTotem>();

        ColliderUtils.TileAccurateBoxColider2D(newScarecrow, false);

        placeable._decoration = newScarecrow;

        //Sprite position adjustment fine tuned for 36px Sprites
        if (placeable != null && placeable._decoration != null)
        {
            Transform graphicsTransform = placeable._decoration.transform.Find("Graphics");

            if (graphicsTransform != null)
            {
                graphicsTransform.localPosition = new Vector3(-0.25f, 0f, 0f);
            }
        }
        placeable.previewOffset = new Vector2(-0.25f, 0f);

    }

    // Creates a range preview sprite for the item
    public static void CreateRangePreview(ItemData item, int range = 5)
    {
        if (item?.useItem is not Placeable placeable || !item.name.Contains("Totem"))
        {
            Plugin.logger.LogError($"UnifiedTotems: Item {item?.name} is not a Placeable totem, cannot create range preview.");
            return;
        }

        placeable.snapToTile = true;

        // Calculate the pivot point for the preview sprite based on the range in order to center it.
        var p = 0.5f - (1f / (range * 2 + 1) / 2);

        //Create a sprite from png image
        placeable._secondaryPreviewSprite = SpriteUtil.CreateSprite(FileLoader.LoadFileBytes(Assembly.GetExecutingAssembly(), $"preview.png"), new Vector2(p, p), $"Totem preview");

        placeable.previewOffset = new Vector2(0, 0);
        //placeable._decoration.transform.Find("Graphics").localPosition = new Vector3(-0.1f, 0, 0);
    }

    //Adjusts vanilla totem colliders in the Database cache
    public static void AdjustVanillaTotems()
    {
        foreach (int totemId in TotemIndex.VanillaTotems.Keys)
        {
            Database.GetData<ItemData>(totemId, itemData =>
            {
                if (itemData == null)
                {
                    Plugin.logger.LogError($"AdjustVanillaTotems: Item {totemId} not found in Database.");
                    return;
                }
                if (itemData.useItem is not Placeable placeable || placeable._decoration is not Scarecrow scarecrow)
                {
                    Plugin.logger.LogError($"AdjustVanillaTotems: Item {itemData.name} with ID {itemData.id} is not a totem, verify the item id or game version.");
                    return;
                }

                ColliderUtils.TileAccurateBoxColider2D(scarecrow, false);
            });
        }
    }

    public static void AdjustCropColliders()
    {
        ItemInfoDatabase cropDatabase = ItemInfoDatabase.Instance;
        if (cropDatabase == null || cropDatabase.cropInfos == null) return;

        foreach (int cropId in cropDatabase.cropInfos.Keys)
        {
            Database.GetData<ItemData>(cropId, itemData =>
            {
                if (itemData == null) return;
                if (itemData.useItem is not Seeds seed || seed._crop is not Crop crop) return;

                ColliderUtils.TileAccurateBoxColider2D(crop, true);
            });
        }
    }

    // Debugging method to check if a totem is correctly configured in the database
    public static void DebugCheckTotemInDatabase(int totemId)
    {
        Database.GetData<ItemData>(totemId, itemData =>
        {
            if (itemData == null)
            {
                Plugin.logger.LogError($"[UnifiedTotems] Item {itemData.name} not found in Database.");
                return;
            }

            if (itemData.useItem is not Placeable placeable || placeable._decoration is not Scarecrow scarecrow)
            {
                Plugin.logger.LogError($"[UnifiedTotems] Item {itemData.name} failed to be configured as a Scarecrow decoration.");
                return;
            }

            if (scarecrow.GetComponent<UnifiedTotem>() == null)
            {
                Plugin.logger.LogError($"[UnifiedTotems] Item {itemData.name} does not have a UnifiedTotem component attached.");
                return;
            }

            Plugin.logger.LogInfo($"[UnifiedTotems] Item {itemData.name} id {itemData.id} was sucessfully configured as a unified totem");
        });
    }
}
