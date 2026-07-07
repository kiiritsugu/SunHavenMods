using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wish;

namespace UnifiedTotems;
public class UnifiedTotem : MonoBehaviour
{
  public List<ScareCrowEffect> CombinedEffects { get; set; } = new List<ScareCrowEffect>();
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
          //Plugin.logger.LogInfo($"UnifiedTotem OnEnable: {decoration.name} received effects: {string.Join(", ", CombinedEffects)}");
          initialized = true;
        }
        else
        {
          Plugin.logger.LogWarning($"UnifiedTotem OnEnable: No combined effects found for {decoration.name} (sourceId: {totemID}).");
        }
      }
      else
      {
        Plugin.logger.LogWarning("UnifiedTotem OnEnable: Failed to find Decoration component. Cannot initialize combined effects.");
      }
    }
  }