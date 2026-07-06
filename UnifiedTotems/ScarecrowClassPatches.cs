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
  [HarmonyPatch(typeof(Scarecrow), "OnPlaced")]
  public static void ScarecrowOnPlaced(Scarecrow __instance) //Write atributes from dictionary before the scarecrow behaviour is executed
  {
    try
    {
      if (__instance != null)
      {
        UnifiedTotem unifiedTotem = __instance.gameObject.GetComponent<UnifiedTotem>();
        Decoration decoration = __instance.gameObject.GetComponent<Decoration>();
        int totemID = decoration.id;
        
        if (unifiedTotem == null)
        {
          return;
        }

        // Retrieve the combined effects for this totem from the TotemIndex
        if (TotemIndex.TotemDictionary.TryGetValue(totemID, out ScareCrowEffect[] combinedEffects))
        {
            unifiedTotem.CombinedEffects = new List<ScareCrowEffect>(combinedEffects);
            Plugin.logger.LogInfo($"ScarecrowOnPlaced: {decoration.name} received effects: {string.Join(", ", combinedEffects)}");
        }
        else
        {
            Plugin.logger.LogWarning($"ScarecrowOnPlaced: No combined effects found for {decoration.name} (sourceId: {totemID}).");
        }
      }
    }
    catch (Exception err)
    {
      Plugin.logger.LogError(err);
    }
  }

  [HarmonyPostfix]
  [HarmonyPatch(typeof(Scarecrow), "ApplyEffectsToCrop")]
  public static void ApplyEffectsToCropPostfix(Scarecrow __instance, Crop crop) //Runs after vanilla apply effects to crop behaviour.
  {
    try
    {
      if (crop != null && crop.data != null && __instance != null)
      {
        ApplyUnifiedTotemEffects(__instance, crop);
      }
    }
    catch (Exception err)
    {
      Plugin.logger.LogError(err);
    }
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
        Plugin.logger.LogInfo($"UnifiedTotems: Applying effects from Scarecrow {scarecrow} to Crop. Totem's scareCrowEffects: {string.Join(", ", unifiedTotem.CombinedEffects)}");
        foreach (ScareCrowEffect effect in unifiedTotem.CombinedEffects)
        {
          if (!crop.data.scareCrowEffects.Contains(effect))
          {
              crop.data.scareCrowEffects.Add(effect);
          }
        }
        Plugin.logger.LogInfo($"UnifiedTotems: Finished applying to Crop. Crop Effects: {string.Join(", ", crop.data.scareCrowEffects)}");
      }

      crop.SaveMeta();
      crop.SendNewMeta(crop.meta);
    }
  }
}

