using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wish;

namespace UnifiedTotems;
public class UnifiedTotem : MonoBehaviour
{
  public List<ScareCrowEffect> CombinedEffects { get; set; } = new List<ScareCrowEffect>();
  public bool gloriteEnhanced = false;
  public bool initialized = false;

    private void OnEnable()
    {
      //Retrieve property values from the TotemIndex dictionary based on the totem's ID and reassign.
      if ( CombinedEffects == null || CombinedEffects.Count == 0 )
      {
        initialized = false;
        StartCoroutine(DelayedInitializeTotemRoutine());
      }
    }

    private IEnumerator DelayedInitializeTotemRoutine()
    {

      yield return null; // Wait for one frame to ensure all components are initialized

      if (initialized)
      {
        yield break;
      }

      InitializeTotem();
    }

    public void InitializeTotem()
    {
      if (initialized) { return; }
      Decoration decoration = GetComponentInParent<Decoration>();

      if (decoration != null)
      {
        int totemID = decoration.id;

        if (TotemIndex.TotemDictionary.TryGetValue(totemID, out ScareCrowEffect[] combinedEffects))
        {
          CombinedEffects = new List<ScareCrowEffect>(combinedEffects);
          
          if(
            totemID == TotemIndex.EnhancedHarvestTotemId || 
            totemID == TotemIndex.EnhancedExperienceTotemId ||
            totemID == TotemIndex.EnhancedFourSeasonsTotemId ||
            totemID == TotemIndex.EnhancedAtlasTotemId
          ) gloriteEnhanced = true;

          initialized = true;
        }
        else
        {
          if(Plugin.DebugMode) Plugin.logger.LogWarning($"UnifiedTotem OnEnable: No combined effects found for {decoration.name} (sourceId: {totemID}).");
        }
      }
      else
      {
        if(Plugin.DebugMode) Plugin.logger.LogWarning("UnifiedTotem OnEnable: Failed to find Decoration component. Cannot initialize combined effects.");
      }
    }
  }