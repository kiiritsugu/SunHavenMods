using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Wish;

namespace RemoteEarthquakeAndRainCloud;

[HarmonyPatch(typeof(EarthquakeSpell))]
public static class EarthQuakeSpellPatch
{
    [HarmonyPatch(nameof(EarthquakeSpell.SpawnEarthquake)), HarmonyReversePatch]
    public static IEnumerator MySpawnEarthquake(object instance, Vector2Int position, int skillLevel, bool fromLocalPlayer = true) =>
        throw new NotImplementedException("It's a stub");
    [HarmonyPatch(nameof(EarthquakeSpell.SpawnEarthquake)), HarmonyPrefix]
    public static bool SpawnEarthquake_Prefix(EarthquakeSpell __instance, ref IEnumerator __result, Vector2Int position, int skillLevel, bool fromLocalPlayer)
    {
        if (__instance == Plugin.earthqueakeSpell)
        {
            var pos = Plugin.earthqueakePos;
            __result = MySpawnEarthquake(__instance, pos, skillLevel, fromLocalPlayer);
            return false;
        }
        return true;
    }
}

