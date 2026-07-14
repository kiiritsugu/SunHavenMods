using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Wish;

using Shared;

namespace UnifiedTotems;

[HarmonyPatch]
public static class ApplyScarecrowEffectsPatch
{
  [HarmonyPrefix]
  [HarmonyPatch(typeof(Scarecrow), "ApplyEffectsToCrop")]
  public static bool ApplyEffectsToCropPrefix(Scarecrow __instance, Crop crop) //Runs before vanilla, apply effects to crop, prevent default behaviour if sucessfull.
  {
    try
    {
      if (crop != null && crop.data != null && __instance != null)
      {
        TotemHandler.ApplyTotemEffects(__instance, crop);
      }
    }
    catch (Exception err)
    {
      Plugin.logger.LogError(err);
    }
    return false; // Prevent the original method from executing
  }
}

[HarmonyPatch(typeof(Scarecrow), nameof(Scarecrow.CalculateScarecrowEffectsForNearbyCrops))]
// This patch replaces the vanilla CalculateScarecrowEffectsForNearbyCrops method with a more accurate and efficient trough a utilitary method
public static class ScarecrowBoxCastColliderPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Scarecrow __instance)
    {
      if (__instance == null) return false;

      // Use the utility method to perform a box cast and apply effects for each unique Scarecrow found
      ColliderUtils.BoxCastHitsOnTypeAction<Crop>(__instance.RealCenter, UnifiedTotemState.Range, crop =>
      {
        __instance.ApplyEffectsToCrop(crop);
      }, false);       

      return false; // Skip the original finicky vanilla calculation entirely
    }
}

[HarmonyPatch(typeof(Scarecrow))]
public static class TotemRemovalPatches
{
  [HarmonyPatch(nameof(Scarecrow.RemoveScarecrowEffectsForNearbyCrops))]
  [HarmonyPrefix]
  public static bool RemoveScarecrowEffectsForNearbyCropsPrefix(Scarecrow __instance)
  {
    if (__instance == null) return false; 

    // Use the utility method to perform a box cast and do remove effects evaluation for each unique crop found
    ColliderUtils.BoxCastHitsOnTypeAction<Crop>(__instance.RealCenter, __instance.range, crop => 
    {
      TotemHandler.EvaluateAndRemoveEffects(__instance, crop);
    });

    //Remove later
    // Plugin.logger.LogInfo($"UnifiedTotems: Finished removing effects for nearby crops. Crops with removed effects: {TotemHandler.cropsWithRemovedEffects}");
    TotemHandler.cropsWithRemovedEffects = 0; // Reset the counter

    return false; // Skip vanilla finicky logic entirely
  }
}