using System.Collections.Generic;
using HarmonyLib;
using PSS;
using UnityEngine;
using Wish;

namespace UnifiedTotems;

/// <summary>
/// Example of phase 2 behavior: one placed Unified Totem applies multiple ScareCrowEffects to crops.
/// Pattern adapted from devopsdinosaur/sunhaven-mods (Crop.GetNearbyScarecrowEffects / AddScarecrowEffects).
/// </summary>
[HarmonyPatch]
public static class CropPatches
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Crop), "GetNearbyScarecrowEffects")]
    public static void GetNearbyScarecrowEffectsPostfix(Crop __instance)
    {
        ApplyUnifiedTotemEffects(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Crop), "AddScarecrowEffects")]
    public static void AddScarecrowEffectsPostfix(Crop __instance)
    {
        ApplyUnifiedTotemEffects(__instance);
    }

    private static void ApplyUnifiedTotemEffects(Crop crop)
    {
        if (!UnifiedTotemState.IsConfigured || !IsCropNearUnifiedTotem(crop))
        {
            return;
        }

        crop.data.scareCrowEffects ??= new List<ScareCrowEffect>();

        foreach (var effect in UnifiedTotemState.CombinedEffects)
        {
            if (!crop.data.scareCrowEffects.Contains(effect))
            {
                crop.data.scareCrowEffects.Add(effect);
            }
        }
    }

    private static bool IsCropNearUnifiedTotem(Crop crop)
    {
        var world = SingletonBehaviour<GameSave>.Instance.CurrentWorld;
        if (world?.Decorations == null)
        {
            return false;
        }

        var cropTile = new Vector2Int(
            Mathf.FloorToInt(crop.transform.position.x / 6f),
            Mathf.FloorToInt(crop.transform.position.y / 6f));

        var sceneId = ScenePortalManager.ActiveSceneIndex;

        foreach (var sceneDecorations in world.Decorations.Values)
        {
            foreach (var entry in sceneDecorations)
            {
                var decoration = entry.Value;
                if (decoration.id != ItemHandler.UnifiedTotemId || decoration.sceneID != sceneId)
                {
                    continue;
                }

                var totemTile = new Vector2Int(decoration.x / 6, decoration.y / 6);
                var dx = Mathf.Abs(cropTile.x - totemTile.x);
                var dy = Mathf.Abs(cropTile.y - totemTile.y);

                if (dx <= UnifiedTotemState.Range && dy <= UnifiedTotemState.Range)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
