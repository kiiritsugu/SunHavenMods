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
// This patch replaces the vanilla GetNearbyScarecrowEffects method with a more accurate and efficient implementation. 11x11 = placed tile + 5 to all sides
public static class CropBoxCastColliderPatch
{
    [HarmonyPrefix]
    static bool Prefix(Crop __instance)
    {
        //Get the center of the crop's collider in world space
        Vector2 castCenter = __instance.RealCenter;

        // 11x11 grid box cast size, center tile +5 in every direction, adjusted for isometric projection
        // 1.4142135f is a magical number because the vanilla uses a isometric projection and y axis is squished
        Vector2 boxSize = new Vector2(11.0f, 11.0f * 1.4142135f); 

        // Track seen components to prevent double evaluation on multi-collider decorations
        HashSet<Scarecrow> evaluatedTotems = new HashSet<Scarecrow>();

        // 3. Execute a static box cast centered directly over the snapped crop location
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(castCenter, boxSize, 0f, Vector2.zero))
        {
            if (hit.transform == null) continue;

            Scarecrow scarecrow = hit.transform.GetComponent<Scarecrow>();
            if (scarecrow != null && !evaluatedTotems.Contains(scarecrow))
            {
                evaluatedTotems.Add(scarecrow);
                scarecrow.ApplyEffectsToCrop(__instance);
            }
        }

        return false; // Skip the original finicky vanilla calculation entirely
    }
}

[HarmonyPatch(typeof(Crop), nameof(Crop.OnPlaced))]
//Replaced the colider on crops for the new box cast method, to ensure the crop colider is centered and sized 1 tile, to ensure the box cast is centered on the crop and not offset by the colider.
public static class PhysicalColliderPatch
{
  [HarmonyPostfix]
  static void Postfix(Crop __instance)
  {
    Utilitaries.TilePerfectBoxColider2D(__instance, true);
  }
}

[HarmonyPatch(typeof(Scarecrow), nameof(Scarecrow.CalculateScarecrowEffectsForNearbyCrops))]
// This patch replaces the vanilla CalculateScarecrowEffectsForNearbyCrops method with a more accurate and efficient implementation. 11x11 = placed tile + 5 to all sides
public static class ScarecrowBoxCastColliderPatch
{
    [HarmonyPrefix]
    public static bool Prefix(Scarecrow __instance)
    {
       //Get the center of the scarecrow's collider in world space
        Vector2 castCenter = __instance.RealCenter;

        //grid box cast size, center tile +range in every direction, adjusted for isometric projection
        // 1.4142135f is a magical number because the vanilla uses a isometric projection and y axis is squished
        float width = (__instance.range * 2) + 1f;
        Vector2 boxSize = new Vector2(width, width * 1.4142135f);

        // Track seen crops to prevent double evaluation on multi-collider crop setups
        HashSet<Crop> evaluatedCrops = new HashSet<Crop>();

        // Execute the identical static box cast centered over the totem location
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(castCenter, boxSize, 0f, Vector2.zero))
        {
            if (hit.transform == null) continue;

            Crop crop = hit.transform.GetComponent<Crop>();
            if (crop != null && evaluatedCrops.Add(crop))
            {
                __instance.ApplyEffectsToCrop(crop);
            }
        }

        return false; // Skip the original finicky vanilla calculation entirely
    }
}