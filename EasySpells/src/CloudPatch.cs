using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using Wish;

namespace EasySpells;

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

    [HarmonyPatch("Update"), HarmonyPrefix]
    public static bool Update_Prefix(Cloud __instance, bool ___fromLocalPlayer, int ___width, HashSet<Vector2Int> ___wateredPositions)
    {
        if (!___fromLocalPlayer)
        {
            return false;
        }

        int num = -___width / 2;
        while ((float)num < (float)___width / 2f)
        {
            int num2 = -___width / 2;
            while ((float)num2 < (float)___width / 2f)
            {
                Vector3 vector = new Vector3(__instance.transform.position.x + (float)num, __instance.transform.position.y / 1.4142135f + (float)num2);
                Vector2Int vector2Int = new Vector2Int((int)vector.x, (int)vector.y);
                int arg;
                if (SingletonBehaviour<TileManager>.Instance.IsWaterable(vector2Int) && !___wateredPositions.Contains(vector2Int) && SingletonBehaviour<TileManager>.Instance.Water(vector2Int, ScenePortalManager.ActiveSceneIndex, out arg))
                {
                    Player.Instance.AddEXP(ProfessionType.Farming, 1f);
                    UnityAction<int> onWater = WateringCan.onWater;
                    if (onWater != null)
                    {
                        onWater(arg);
                    }
                    ___wateredPositions.Add(vector2Int);
                }
                num2++;
            }
            num++;
        }

        return false;
    }
}

