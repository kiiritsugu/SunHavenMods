using System.Collections.Generic;
using PSS;

namespace UnifiedTotems;

/// <summary>
/// Shared state built at runtime by ItemHandler (phase 2 after CustomItems).
/// </summary>
public static class UnifiedTotemState
{
    public static bool IsConfigured { get; set; }

    /// <summary>Effects copied from vanilla totems via Database.GetData.</summary>
    public static readonly List<ScareCrowEffect> CombinedEffects = new();

    /// <summary>Scarecrow tile radius ( taken from the largest vanilla source totem ).</summary>
    public static int Range { get; set; } = 5;

    public static int CropCapacity { get; set; } = 120;
}
