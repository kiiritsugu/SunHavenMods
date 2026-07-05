using System.Collections.Generic;
using Wish;
using UnityEngine;

namespace UnifiedTotems;

/// <summary>
/// Shared state built at runtime by ItemHandler (phase 2 after CustomItems).
/// </summary>
public static class UnifiedTotemState
{
    public static bool IsConfigured { get; set; }

    /// Empty effect for vanilla behaviour, combined effect list comes from TotemIndex
    public static ScareCrowEffect ScareCrowEffect = ScareCrowEffect.None;

    /// <summary>Scarecrow tile radius ( taken from the largest vanilla source totem ).</summary>
    public static int Range { get; set; } = 5;

    public static int CropCapacity { get; set; } = 1;
}
