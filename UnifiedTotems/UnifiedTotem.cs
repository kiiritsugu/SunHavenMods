using System.Collections.Generic;
using UnityEngine;
using Wish;

namespace UnifiedTotems;
public class UnifiedTotem : MonoBehaviour
{
  public List<ScareCrowEffect> CombinedEffects { get; set; } = new List<ScareCrowEffect>();

    private void OnEnable()
    {
      //Retrieve property values from the TotemIndex dictionary based on the totem's ID and reassign.
      if (CombinedEffects == null || CombinedEffects.Count == 0)
      {
        CombinedEffects = new List<ScareCrowEffect>();

        Decoration decoration = this.gameObject.GetComponent<Decoration>();

        if (decoration != null)
        {
          int totemID = decoration.id;

          if (TotemIndex.TotemDictionary.TryGetValue(totemID, out ScareCrowEffect[] combinedEffects))
          {
            CombinedEffects = new List<ScareCrowEffect>(combinedEffects);
            Plugin.logger.LogInfo($"UnifiedTotem OnEnable: {decoration.name} received effects: {string.Join(", ", combinedEffects)}");
          }
          else
          {
            Plugin.logger.LogWarning($"UnifiedTotem OnEnable: No combined effects found for {decoration.name} (sourceId: {totemID}).");
          }
        }
      }
    }
  }