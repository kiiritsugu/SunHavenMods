using HarmonyLib;
using UnityEngine;
using Wish;

namespace RemoteEarthquakeAndRainCloud;

[HarmonyPatch(typeof(EarthquakeSpell))]
public static class EarthQuakeSpellPatch
{
    [HarmonyPatch(nameof(EarthquakeSpell.SpawnEarthquake)), HarmonyPrefix]
    public static bool SpawnEarthquake_Prefix(EarthquakeSpell __instance, ref Vector2Int position)
    {
        if (__instance == Plugin.earthqueakeSpell)
        {
            position = Plugin.earthqueakePos;
        }
        return true;
    }
}
