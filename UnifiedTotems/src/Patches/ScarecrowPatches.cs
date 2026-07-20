using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Wish;

using Shared;

namespace UnifiedTotems;

[HarmonyPatch]
//Handles aplication of effects for individual crops
public static class ApplyScarecrowEffectsPatch
{
  [HarmonyPrefix]
  [HarmonyPatch(typeof(Scarecrow), "ApplyEffectsToCrop")]
  public static bool ApplyEffectsToCropPrefix(Scarecrow __instance, Crop crop) //Runs before vanilla, apply effects to crop, prevent default behaviour if sucessfull.
  {
    try
    {
      //Replaces the vanilla effect application for a new function that acomodates for both unified and vanilla totems
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
// Handles aplication of effects that happen when placing a totem
public static class TotemPlacementPatch
{
  [HarmonyPrefix]
  public static bool CalculateScarecrowEffectsForNearbyCropsPrefix(Scarecrow __instance)
  {
    if (__instance == null) return false;

    //check if the placed totem is a enhanced one, and apply effects to all crops if so
    if (TotemHandler.CheckIfEnhanced(__instance))
    {
      TotemHandler.ApplyTotemEffectsToAll(__instance);
    }
    else
    {
      // Use the utility method to perform a box cast and collect all affected crops
      List<Crop> nearbyCrops = new List<Crop>();
      ColliderUtils.BoxCastHitsOnTypeAction<Crop>(__instance.RealCenter, UnifiedTotemState.Range, crop =>
      {
        nearbyCrops.Add(crop);
      }, false);

      // Use the frame-safe TotemHandler helper method to process crops
      TotemHandler.ApplyTotemEffectsToCrops(__instance, nearbyCrops, cropsWithAddedEffects =>
      {
        //Debug Mode
        if (Plugin.DebugMode)
        {
          Plugin.logger.LogInfo($"UnifiedTotems: Finished Applying effects to crops. Crops with added effects: {cropsWithAddedEffects}");
        }
      });
    }

    return false; // Skip the original vanilla calculation entirely
  }
}

[HarmonyPatch(typeof(Scarecrow))]
//Handles patches that happen when removing a totem
public static class TotemRemovalPatches
{
  [HarmonyPatch(nameof(Scarecrow.RemoveScarecrowEffectsForNearbyCrops))]
  [HarmonyPrefix]
  //Handles removing effects for regular totems
  public static bool RemoveScarecrowEffectsForNearbyCropsPrefix(Scarecrow __instance)
  {
    if (__instance == null) return false;

    // Use the utility method to perform a box cast and collect all affected crops
    List<Crop> affectedCrops = new List<Crop>();
    ColliderUtils.BoxCastHitsOnTypeAction<Crop>(__instance.RealCenter, __instance.range, crop =>
    {
      affectedCrops.Add(crop);
    });

    // Use the frame-safe TotemHandler helper method to process crops
    TotemHandler.RemoveTotemEffectsFromCrops(__instance, affectedCrops, cropsWithRemovedEffects =>
    {
      //Debug Mode
      if (Plugin.DebugMode)
      {
        Plugin.logger.LogInfo($"UnifiedTotems: Finished removing effects for nearby crops. Crops with removed effects: {cropsWithRemovedEffects}");
      }
    });

    return false; // Skip vanilla finicky logic entirely
  }

  [HarmonyPatch(nameof(Scarecrow.OnDestroyed))]
  [HarmonyPrefix]
  //handles removing effects for enhanced totems
  public static bool RemoveScarecrowEffectFromAll(Scarecrow __instance)
  {
    UnifiedTotem unifiedTotem = __instance.GetComponent<UnifiedTotem>();

    if (unifiedTotem == null) return true;
    if (unifiedTotem.initialized != true) unifiedTotem.InitializeTotem();
    if (!unifiedTotem.gloriteEnhanced) return true;

    CallDecorationOnDestroyed(__instance);
    TotemHandler.EvaluateEnhancedTotemsInScene(unifiedTotem);
    TotemHandler.RemoveTotemEffectsFromAll(__instance);

    return false;
  }

  [HarmonyPatch(typeof(Decoration), "OnDestroyed")]
  [HarmonyReversePatch]
  //helper for scarecrow OnDestroyed, to call the base method
  public static void CallDecorationOnDestroyed(object instance)
  {
    throw new NotImplementedException("[UnifiedTotems] Reverse OnDestroyed patch failed!");
  }
}

[HarmonyPatch]
//Handles managing state of Active Enhanced Totems on totem placement
public static class EnhancedTotemPlacedPatch
{
  [HarmonyPostfix]
  [HarmonyPatch(typeof(Scarecrow), "OnPlaced")]
  public static void ActivateEnhancedTotem(Scarecrow __instance)
  {
    if (UnifiedTotemState.ActiveEnhancedTotems.ContainsKey(__instance.id))
    {
      UnifiedTotemState.ActiveEnhancedTotems[__instance.id] = true;
    }
  }
}