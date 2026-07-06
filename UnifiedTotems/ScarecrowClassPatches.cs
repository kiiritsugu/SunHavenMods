using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace UnifiedTotems;

[HarmonyPatch]
public static class ScarecrowClassPatches
{
  [HarmonyPrefix]
  [HarmonyPatch(typeof(Scarecrow), "ApplyEffectsToCrop")]
  public static bool ApplyEffectsToCropPrefix(Scarecrow __instance, Crop crop) //Runs before vanilla, apply effects to crop, prevent default behaviour if sucessfull.
  {
    try
    {
      if (crop != null && crop.data != null && __instance != null)
      {
        Plugin.logger.LogInfo($"Testing purposes scarecrow: name:{__instance.name} range:{__instance.range} cropCapacity:{__instance.cropCapacity}");
        ApplyUnifiedTotemEffects(__instance, crop);
      }
    }
    catch (Exception err)
    {
      Plugin.logger.LogError(err);
      return true; // Allow the original method to execute if an error occurs
    }
    return false; // Prevent the original method from executing
  }

  private static void ApplyUnifiedTotemEffects(Scarecrow scarecrow, Crop crop) //Applies the combined effects from the UnifiedTotem component to the crop.
  {
    if (scarecrow.ValidCrop(crop))
    {
      if (crop.data.scareCrowEffects == null)
      {
        crop.data.scareCrowEffects = new List<ScareCrowEffect>();
      }

      UnifiedTotem unifiedTotem = scarecrow.GetComponent<UnifiedTotem>();

      if(unifiedTotem != null)
      {
        if(unifiedTotem.initialized == false)
        {
          unifiedTotem.InitializeTotem();
        }

        //Plugin.logger.LogInfo($"UnifiedTotems: Applying effects from Scarecrow {scarecrow} to Crop. Totem's scareCrowEffects: {string.Join(", ", unifiedTotem.CombinedEffects)}");
        foreach (ScareCrowEffect effect in unifiedTotem.CombinedEffects)
        {
          if (!crop.data.scareCrowEffects.Contains(effect))
          {
              crop.data.scareCrowEffects.Add(effect);
          }
        }
        Plugin.logger.LogInfo($"UnifiedTotems: Finished applying to Crop. Crop Effects: {string.Join(", ", crop.data.scareCrowEffects)}");
      } 
      else
      {
        if (!crop.data.scareCrowEffects.Contains(scarecrow.scareCrowEffect))
          {
            crop.data.scareCrowEffects.Add(scarecrow.scareCrowEffect);
            Plugin.logger.LogInfo($"UnifiedTotems: Applied default scareCrowEffect: {scarecrow.scareCrowEffect} to Crop.");
          }
      }

      crop.SaveMeta();
      crop.SendNewMeta(crop.meta);
    }
  }
}

