using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wish;

using Shared;
using JetBrains.Annotations;
using System.Linq;
using System.Data;
using UnityEngine.UIElements;

namespace UnifiedTotems;

public static class TotemHandler
{
  //Remove later
  public static int cropsWithRemovedEffects = 0;
  //Applies the Unified Totem or Vanilla effects to crop
  public static void ApplyTotemEffects(Scarecrow scarecrow, Crop crop)
  {
    if (scarecrow.ValidCrop(crop))
    {
      if (crop.data.scareCrowEffects == null)
      {
        crop.data.scareCrowEffects = new List<ScareCrowEffect>();
      }

      UnifiedTotem unifiedTotem = scarecrow.GetComponent<UnifiedTotem>();

      if (unifiedTotem != null)
      {
        if (unifiedTotem.initialized == false)
        {
          unifiedTotem.InitializeTotem();
        }

        foreach (ScareCrowEffect effect in unifiedTotem.CombinedEffects)
        {
          if (!crop.data.scareCrowEffects.Contains(effect))
          {
            crop.data.scareCrowEffects.Add(effect);
          }
        }
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

      //Remove later
      // Plugin.logger.LogInfo($"UnifiedTotems: Finished applying to Crop. Crop Effects: {string.Join(", ", crop.data.scareCrowEffects)}");
    }
  }

  // Removes the Unified Totem or Vanilla effects from crop if no nearby totem is providing the same effects
  public static void EvaluateAndRemoveEffects(Scarecrow removedTotem, Crop crop)
  {
    if (crop == null || crop.data == null || crop.data.scareCrowEffects == null|| removedTotem == null) return;

    // Determine all individual sub-effects projected by this specific removing totem instance
    List<ScareCrowEffect> effectsToRemove = new List<ScareCrowEffect>();

    // Check if it's a unified totem trough the component and if so add the combined effects to removal list
    UnifiedTotem unifiedTotem = removedTotem.gameObject.GetComponentInChildren<UnifiedTotem>();
    if (unifiedTotem != null)
    {
      effectsToRemove.AddRange(unifiedTotem.CombinedEffects);
    }
    else
    {
      // add the vanilla effect to removal list
      effectsToRemove.Add(removedTotem.scareCrowEffect);
    }

    // List for effects provided by other totems
    HashSet<ScareCrowEffect> effectsProvidedByOthers = new HashSet<ScareCrowEffect>();

    // Aggregate effects from active enhanced totems
    foreach (KeyValuePair<int, bool> pair in UnifiedTotemState.ActiveEnhancedTotems)
    {
      if (pair.Value == true)
      {
        if (TotemIndex.TotemDictionary.TryGetValue(pair.Key, out ScareCrowEffect[] effects))
        {
          foreach (ScareCrowEffect effect in effects)
          {
            effectsProvidedByOthers.Add(effect);
          }
        }
      }
    }

    //Perform a box-cast to aggregate all effects provided by other nearby totems
    ColliderUtils.BoxCastHitsOnTypeAction<Scarecrow>(crop.RealCenter, 5f, nearbyTotem =>
    {
      if (nearbyTotem == removedTotem) return;

      UnifiedTotem nearbyUnifiedTotem = nearbyTotem.GetComponentInChildren<UnifiedTotem>();

      if (nearbyUnifiedTotem != null)
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
      //Remove later
      cropsWithRemovedEffects++;
    }
    //Remove later
    Plugin.logger.LogInfo($"UnifiedTotems: Finished evaluating and removing effects from Crop. Crop Effects: {string.Join(", ", crop.data.scareCrowEffects)}");
  }

  // Checks if a nearby totem allows crop placement
  public static bool AllowCropPlacementCheck(Crop crop, Vector2 position)
  {
    // Safety check to ensure the instance and its seed metadata are fully loaded
    if (crop == null || crop.SeedData == null)
    {
      return false;
    }

    // control variables
    bool placementAllowed = false;
    FarmType requiredFarm = crop.SeedData.farmType;

    //Check if Enhanced Atlas totem is active
    if(UnifiedTotemState.ActiveEnhancedTotems[TotemIndex.EnhancedAtlasTotemId] == true)
    {
      placementAllowed = true;
      return placementAllowed;
    }

    // Map the seed's required FarmType to its matching vanilla ScareCrowEffect enum value
    ScareCrowEffect targetEffect = requiredFarm switch
    {
      FarmType.Normal => ScareCrowEffect.SunHaven,
      FarmType.Nelvari => ScareCrowEffect.Nelvari,
      FarmType.Withergate => ScareCrowEffect.Withergate,
      _ => ScareCrowEffect.None // Handle edge cases safely
    };

    // If the seed doesn't require a regional totem to be planted, allow it natively
    if (targetEffect == ScareCrowEffect.None)
    {
      return true;
    }

    //Telemetry logs found a pattern where the position variable is offset by by 0.92f, 0.7071f from the actual RealCenter of the crop
    //adjusting position to simulate the real center for accurate box casting.
    Vector2 centeredPosition = position + new Vector2(0.5f, 0.7071f);

    // Perform a precise box cast centered on the pending placement coordinate
    // Snapping is enabled to lock the loose placement coordinate to a perfect grid cell center
    ColliderUtils.BoxCastHitsOnTypeAction<Scarecrow>(centeredPosition, UnifiedTotemState.Range - 0.375f, scarecrow =>
    {
      // Early exit closure optimization if a previous hit in this pass already granted access
      if (placementAllowed) return;

      // 1. Check if this is one of your custom totems holding the CombinedEffects list
      var unifiedComponent = scarecrow.GetComponent<UnifiedTotem>();

      if (unifiedComponent != null && unifiedComponent.CombinedEffects != null)
      {
        if (unifiedComponent.CombinedEffects.Contains(targetEffect))
        {
          placementAllowed = true;
          return;
        }
      }

      // 2. Fallback to standard vanilla single-effect matching if it's a regular totem
      if (scarecrow.scareCrowEffect == targetEffect)
      {
        placementAllowed = true;
      }
    }, false);

    // Feed our final evaluation back into Harmony's return mechanism
    return placementAllowed;
  }

  //Checks if Totem is a enhanced totem
  public static bool CheckIfEnhanced(Scarecrow scarecrow)
  {
    UnifiedTotem unifiedTotem = scarecrow.GetComponent<UnifiedTotem>();
    if(unifiedTotem == null) return false;

    if (unifiedTotem.initialized == false) unifiedTotem.InitializeTotem();

    if (unifiedTotem.gloriteEnhanced == true) return true;

    return false;
  }

  //
  public static void ApplyTotemEffectsToAll(Scarecrow scarecrow)
  {
    UnifiedTotem unifiedTotem = scarecrow.GetComponent<UnifiedTotem>();

    if (unifiedTotem == null) return;

    if (unifiedTotem.initialized == false)
    {
      unifiedTotem.InitializeTotem();
    }

    CoroutineRunner.Instance.StartCoroutine(ApplyTotemEffectsToAllCoroutine(unifiedTotem));
  }

  private static System.Collections.IEnumerator ApplyTotemEffectsToAllCoroutine(UnifiedTotem unifiedTotem)
  {
    Crop[] crops = GameObject.FindObjectsOfType<Crop>();
    int batchSize = UnifiedTotemState.BatchSize;

    for (int i = 0; i < crops.Length; i += batchSize)
    {
      for (int j = i; j < Math.Min(i + batchSize, crops.Length); j++)
      {
        Crop crop = crops[j];
        bool metadataChanged = false;
        if (crop != null && crop.data != null)
        {
          if (crop.data.scareCrowEffects == null)
          {
            crop.data.scareCrowEffects = new List<ScareCrowEffect>();
          }

          foreach (ScareCrowEffect effect in unifiedTotem.CombinedEffects)
          {
            if (!crop.data.scareCrowEffects.Contains(effect))
            {
              crop.data.scareCrowEffects.Add(effect);
              metadataChanged = true;
            }
          }

          if (metadataChanged)
          {
            crop.SaveMeta();
            crop.SendNewMeta(crop.meta);
          }
        }
      }
      yield return null;
    }
  }

  // 
  public static void RemoveTotemEffectsFromAll(Scarecrow scarecrow)
  {
    CoroutineRunner.Instance.StartCoroutine(RemoveTotemEffectsFromAllCoroutine(scarecrow));
  }

  private static System.Collections.IEnumerator RemoveTotemEffectsFromAllCoroutine(Scarecrow scarecrow)
  {
    Crop[] crops = GameObject.FindObjectsOfType<Crop>();
    int batchSize = UnifiedTotemState.BatchSize;

    for (int i = 0; i < crops.Length; i += batchSize)
    {
      for (int j = i; j < Math.Min(i + batchSize, crops.Length); j++)
      {
        EvaluateAndRemoveEffects(scarecrow, crops[j]);
      }
      yield return null;
    }
  }

  //
  public static void EvaluateEnhancedTotemsInScene(UnifiedTotem excludedTotem = null)
  {
    string currentScene = ScenePortalManager.ActiveSceneName;

    if (!SceneIndex.farmScenes.Contains(currentScene)) return;

    List<int> enhancedTotemIDs = new List<int>(UnifiedTotemState.ActiveEnhancedTotems.Keys);
    foreach (int enhancedTotemID in enhancedTotemIDs)
    {
      UnifiedTotemState.ActiveEnhancedTotems[enhancedTotemID] = false;
    }

    UnifiedTotem[] unifiedTotems = GameObject.FindObjectsOfType<UnifiedTotem>();

    foreach (UnifiedTotem unifiedTotem in unifiedTotems)
    {
      if (unifiedTotem.gameObject.scene.name != currentScene) continue;
      if (!unifiedTotem.gameObject.activeInHierarchy) continue;
      if (excludedTotem != null && unifiedTotem == excludedTotem) continue;


      if (!unifiedTotem.initialized) unifiedTotem.InitializeTotem();

      if (unifiedTotem.gloriteEnhanced)
      {
        Scarecrow scarecrow = unifiedTotem.GetComponentInParent<Scarecrow>();

        if (scarecrow != null)
        {
          UnifiedTotemState.ActiveEnhancedTotems[scarecrow.id] = true;
          //Remove later
          Plugin.logger.LogInfo($"[Unified Totems]{scarecrow.name} is active");
        }
      }
    }
  }

  //
  public static void ApplyActiveEnhancedTotemEffects(Crop crop)
  {
    if(crop == null) return;

    bool metadataChanged = false;

    Dictionary<int,bool> enhancedTotemDictionary = UnifiedTotemState.ActiveEnhancedTotems;

    foreach(int totemID in enhancedTotemDictionary.Keys)
    {
      if(enhancedTotemDictionary[totemID] == true)
      {
        foreach (ScareCrowEffect effect in TotemIndex.TotemDictionary[totemID])
        {
          if (!crop.data.scareCrowEffects.Contains(effect))
          {
            crop.data.scareCrowEffects.Add(effect);
            metadataChanged = true;
          }
        }
      }
    }

    if(metadataChanged)
    {
      crop.SaveMeta();
      crop.SendNewMeta(crop.meta);
    }
  }
}