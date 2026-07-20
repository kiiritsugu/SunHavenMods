using System;
using HarmonyLib;
using UnityEngine;
using Wish;

using Shared;

namespace UnifiedTotems;

[HarmonyPatch(typeof(Crop))]
// handles scarecrow effects on crop placement
public static class CropPatch
{
  [HarmonyPrefix]
  [HarmonyPatch("GetNearbyScarecrowEffects")]
  //handles applying totem effects
  static bool GetNearbyScarecrowEffectsPrefix(Crop __instance)
  {
    if (__instance == null) return false;

    try
    {
      //applies active enhanced totem effects in the farm
      TotemHandler.ApplyActiveEnhancedTotemEffects(__instance);

      // Use the utility method to perform a box cast and apply effects for each unique Scarecrow found
      ColliderUtils.BoxCastHitsOnTypeAction<Scarecrow>(__instance.RealCenter, UnifiedTotemState.Range, scarecrow =>
      {
        TotemHandler.ApplyTotemEffects(scarecrow, __instance);
      }, false);

    }
    catch (Exception err)
    {
      Plugin.logger.LogError(err);
    }

    // Skip vanilla calculation entirely
    return false;
  }

  [HarmonyPrefix]
  [HarmonyPatch("CanBePlacedBecauseScarecrowNearby")]
  // handles allowing crop placement by regional totems
  static bool CanBePlacedBecauseScarecrowNearbyPrefix(
    Crop __instance, ref bool __result, Vector2 position, Vector3Int placementPosition
  )
  {
    try
    {
      // Function that evaluates if a nearby totem allows crop placement,
      // __result feeds our final evaluation back into Harmony's return mechanism
      __result = TotemHandler.AllowCropPlacementCheck(__instance, position);
    }
    catch (Exception err)
    {
      Plugin.logger.LogError(err);
    }

    // Skip vanilla verification entirely 
    return false;
  }
}