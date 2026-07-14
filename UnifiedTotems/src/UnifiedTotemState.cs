using System.Collections.Generic;
using Wish;

namespace UnifiedTotems;

public static class UnifiedTotemState
{
    public static Dictionary<int, bool> ActiveEnhancedTotems = new Dictionary<int, bool>
    {
        { TotemIndex.EnhancedHarvestTotemId, false },
        { TotemIndex.EnhancedExperienceTotemId, false },
        { TotemIndex.EnhancedFourSeasonsTotemId, false },
        { TotemIndex.EnhancedAtlasTotemId, false }
    };
    
    // for tracking the state of totem configuration
    public static bool IsConfigured { get; set; }

    /// Empty effect for vanilla behaviour, combined effect list comes from TotemIndex
    public static ScareCrowEffect ScareCrowEffect = ScareCrowEffect.None;

    /// <summary>Scarecrow tile radius ( taken from the largest vanilla source totem ).</summary>
    public static int Range { get; set; } = 5;

    // effect unclear, either number of a same effect that can be applied to a single tile, number of crops that the scarecrow can affect, if the first this takes no effect since i'm stoping two of the same effect from being applied
    public static int CropCapacity { get; set; } = 120;
}
