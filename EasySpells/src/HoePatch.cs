using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Wish;

namespace RemoteEarthquakeAndRainCloud;

[HarmonyPatch(typeof(Hoe))]
public static class HoePatch
{
    private static GameObject baseSelection;
    private static List<GameObject> selectionList;

    [HarmonyPatch("HandleHoeEachFrame"), HarmonyReversePatch]
    public static void MyLateUpdate(object instance)
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i + 2 < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt &&
                    code[i].operand != null &&
                    code[i].OperandIs(AccessTools.PropertyGetter(typeof(Player), nameof(Player.ExactGraphicsPosition))) &&
                    code[i + 2].operand != null &&
                    code[i + 2].OperandIs(1.5f))
                {
                    code[i + 2].operand = 1000.0f;
                }
            }

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt &&
                    code[i].operand != null &&
                    code[i].OperandIs(AccessTools.Method(typeof(GameManager), nameof(GameManager.IsFarmableDataTile), new[] { typeof(Vector2Int) })))
                {
                    code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(MyIsFarmableDataTile));
                }
                else if (code[i].opcode == OpCodes.Call &&
                         code[i].operand != null &&
                         code[i].OperandIs(AccessTools.Method(typeof(Hoe), "SelectCurrentHoeItem")))
                {
                    code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(MySelectCurrentHoeItemOLDGCALLOC));
                }
            }

            return code;
        }
        _ = Transpiler(null);
    }

    public static bool MyIsFarmableDataTile(object instance, Vector2Int pos)
    {
        if (Plugin.modEnabled.Value && Plugin.remoteKey.Value.IsPressed())
        {
            return true;
        }
        return SingletonBehaviour<GameManager>.Instance.IsFarmableDataTile(pos);
    }

    public static void MySelectCurrentHoeItemOLDGCALLOC(object instance)
    {
        Hoe hoe = (Hoe)instance;
        var pos = Traverse.Create(hoe).Field<Vector2Int>("pos").Value;
        var _selection = Traverse.Create(hoe).Field<GameObject>("_selection").Value;
        
        if (!Plugin.modEnabled.Value || !Plugin.remoteKey.Value.IsPressed())
        {
            // Fallback to calling the original method or just letting the game handle selection
            // The transpiler replaced the original call, so we must handle it if we want it to work
            // Since we're here, we need to ensure selection shows
            if (_selection != null) _selection.SetActive(true);
            return;
        }

        if (_selection == null)
        {
            return;
        }
        if (baseSelection != _selection)
        {
            baseSelection = _selection;
            selectionList?.ForEach(x => UnityEngine.Object.Destroy(x));
            selectionList?.Clear();
            selectionList = new List<GameObject>();
            for (int i = 0; i < 25; i++)
            {
                var item = UnityEngine.Object.Instantiate<GameObject>(_selection);
                item.SetActive(false);
                selectionList.Add(item);
            }
        }
        _selection.SetActive(false);
        for (int i = 0; i < selectionList.Count; i++)
        {
            int x = i % 5 - 2;
            int y = i / 5 - 2;
            var item = selectionList[i];
            var p = new Vector2Int(pos.x - x, pos.y - y);
            if (!SingletonBehaviour<TileManager>.Instance.HasTile(p, ScenePortalManager.ActiveSceneIndex) &&
                (SingletonBehaviour<TileManager>.Instance.IsHoeable(p) || SingletonBehaviour<TileManager>.Instance.IsFarmable(p)))
            {
                ToolPatch.MySetSelectionOnTileBody(new ToolPatch.MySetSelectionOnTileBodyArg { _selection = item, transform = hoe.transform }, p);
                item.transform.localScale = new Vector3(1f, 1.4142135f, 1f);
                item.gameObject.transform.position += new Vector3(0f, 0.001f * i, 0.001f * i);
            }
            else
            {
                item.SetActive(false);
            }
        }
    }

    [HarmonyPatch("OnDisable"), HarmonyPrefix]
    public static bool OnDisable_Prefix(Hoe __instance)
    {
        if (GameManager.ApplicationQuitting || GameManager.SceneTransitioning)
        {
            return true;
        }
        var _selection = Traverse.Create(__instance).Field<GameObject>("_selection").Value;
        if (baseSelection == _selection)
        {
            baseSelection = null;
            selectionList?.ForEach(x => UnityEngine.Object.Destroy(x));
            selectionList?.Clear();
        }
        return true;
    }

    [HarmonyPatch("LateUpdate"), HarmonyPrefix]
    public static bool LateUpdate_Prefix(Hoe __instance)
    {
        if (Plugin.modEnabled.Value && 
            Plugin.remoteKey.Value.IsPressed())
        {
            MyLateUpdate(__instance);
            return false;
        }
        else
        {
            // Reset selection state if mod is disabled or key not pressed
            var _selection = Traverse.Create(__instance).Field<GameObject>("_selection").Value;
            if (_selection != null) _selection.SetActive(true);
            selectionList?.ForEach(x => x.SetActive(false));
        }
        return true;
    }
}
