using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Wish;

namespace RemoteEarthquakeAndRainCloud;

[HarmonyPatch(typeof(Cloud))]
public static class CloudPatch
{
    //SetCloudPath
    [HarmonyPatch(nameof(Cloud.SetCloudPath)), HarmonyPrefix]
    public static bool SetCloudPath_Prefix(Cloud __instance, global::Direction direction, int skillLevel, bool fromLocalPlayer)
    {
        if (fromLocalPlayer && Plugin.cloudSpell.Casting)
        {
            var pos = new Vector3((float)(Plugin.cloudPos.x), (float)(Plugin.cloudPos.y) * 1.4142135f + 0.5f, -6f);
            __instance.transform.position = pos;
        }
        return true;
    }
}

