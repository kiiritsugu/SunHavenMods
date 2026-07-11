using System;
using HarmonyLib;
using UnityEngine;
using Wish;

using Shared;

namespace UnifiedTotems;

[HarmonyPatch(typeof(Crop))]
public static class CropPatch
{
  // This patch replaces the vanilla GetNearbyScarecrowEffects method with a more accurate and efficient trough a utilitary method
  [HarmonyPrefix]
  [HarmonyPatch("GetNearbyScarecrowEffects")]
  static bool GetNearbyScarecrowEffectsPrefix(Crop __instance)
  {
    if (__instance == null) return false;

    try
    {
      // Use the utility method to perform a box cast and apply effects for each unique Scarecrow found
      ColliderUtils.BoxCastHitsOnTypeAction<Scarecrow>(__instance.RealCenter, UnifiedTotemState.Range, scarecrow =>
      {
        scarecrow.ApplyEffectsToCrop(__instance);
      }, false);
    }
    catch (Exception err)
    {
      Plugin.logger.LogError(err);
    }

    // Skip vanilla calculation entirely
    return false;
  }

  //
  [HarmonyPrefix]
  [HarmonyPatch("CanBePlacedBecauseScarecrowNearby")]
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