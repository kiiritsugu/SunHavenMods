using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Wish;

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
          }
      }

      crop.SaveMeta();
      crop.SendNewMeta(crop.meta);
    }
  }
}

[HarmonyPatch(typeof(Crop), nameof(Crop.GetNearbyScarecrowEffects))]
// This patch replaces the vanilla GetNearbyScarecrowEffects method with a more accurate and efficient trough a utilitary method
public static class CropBoxCastColliderPatch
{
    [HarmonyPrefix]
    static bool Prefix(Crop __instance)
    {
      if (__instance == null) return false;
      // Use the utility method to perform a box cast and apply effects for each unique Scarecrow found
      Utilitaries.BoxCastHitsOnTypeAction<Scarecrow>(__instance.RealCenter, UnifiedTotemState.Range, scarecrow =>
      {
        scarecrow.ApplyEffectsToCrop(__instance);
      }, false);

      return false; // Skip the original finicky vanilla calculation entirely
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
      Utilitaries.BoxCastHitsOnTypeAction<Crop>(__instance.RealCenter, UnifiedTotemState.Range, crop =>
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
    Utilitaries.BoxCastHitsOnTypeAction<Crop>(__instance.RealCenter, __instance.range, crop => 
    {
      EvaluateAndRemoveEffects(__instance, crop);
    });

    return false; // Skip vanilla finicky logic entirely
  }

  private static void EvaluateAndRemoveEffects(Scarecrow removedTotem, Crop crop)
  {
    if (crop == null || crop.data == null || crop.data.scareCrowEffects == null) return;

    // Determine all individual sub-effects projected by this specific removing totem instance
    List<ScareCrowEffect> effectsToRemove = new List<ScareCrowEffect>();

    // Check if it's a unified totem trough the component and if so add the combined effects to removal list
    UnifiedTotem unifiedTotem = removedTotem.GetComponentInChildren<UnifiedTotem>();
    if (unifiedTotem != null && unifiedTotem.initialized)
    {
      effectsToRemove.AddRange(unifiedTotem.CombinedEffects);
    }
    else
    {
      // add the vanilla effect to removal list
      effectsToRemove.Add(removedTotem.scareCrowEffect);
    }

    //Perform a box-cast to aggregate all effects provided by other nearby totems
    HashSet<ScareCrowEffect> effectsProvidedByOthers = new HashSet<ScareCrowEffect>();

    Utilitaries.BoxCastHitsOnTypeAction<Scarecrow>(crop.RealCenter, 5f, nearbyTotem =>
    {
      if (nearbyTotem == removedTotem) return;

      UnifiedTotem nearbyUnifiedTotem = nearbyTotem.GetComponentInChildren<UnifiedTotem>();

      if (nearbyUnifiedTotem != null && nearbyUnifiedTotem.initialized)
      {
        // Collect all effects from this totem
        foreach (var effect in nearbyUnifiedTotem.CombinedEffects)
        {
          effectsProvidedByOthers.Add(effect);
        }
      }
      else
      {
        // Collect the vanilla effect from this totem
        effectsProvidedByOthers.Add(nearbyTotem.scareCrowEffect);
      }
    });

    bool metadataChanged = false;

    //Process every effect this totem provided against the aggregate neighbor map
    foreach (ScareCrowEffect effect in effectsToRemove)
    {
      // Skip if the crop doesn't have this effect
      if (!crop.data.scareCrowEffects.Contains(effect)) continue;

      // Check our aggregate map to see if any nearby totem is still providing this effect
      if (effectsProvidedByOthers.Contains(effect)) continue;
      
      // If not, remove the effect from the crop and mark metadata as changed
      crop.data.scareCrowEffects.Remove(effect);
      metadataChanged = true;
    }

    //Force structural save
    if (metadataChanged)
    {
      crop.SaveMeta();
      crop.SendNewMeta(crop.meta);
    }
  }

}