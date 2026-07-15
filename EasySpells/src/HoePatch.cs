using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Wish;

namespace EasySpells;

[HarmonyPatch(typeof(Hoe))]
public static class HoePatch
{
    private static GameObject baseSelection;
    private static List<GameObject> selectionList;

    [HarmonyPatch("HandleHoeEachFrame"), HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> HandleHoeEachFrame_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var code = new List<CodeInstruction>(instructions);

        for (int i = 0; i < code.Count; i++)
        {
            // Replace ldc.r4 1.5 with Call GetHoeRange()
            if (code[i].opcode == OpCodes.Ldc_R4 && code[i].operand != null && (float)code[i].operand == 1.5f)
            {
                code[i].opcode = OpCodes.Call;
                code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(GetHoeRange));
            }
            // Replace IsFarmableDataTile with MyIsFarmableDataTile using robust name matching
            else if (code[i].opcode == OpCodes.Callvirt &&
                     code[i].operand is MethodInfo methodInfo &&
                     methodInfo.Name == "IsFarmableDataTile")
            {
                code[i].opcode = OpCodes.Call;
                code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(MyIsFarmableDataTile));
            }
            // Replace IsFarmableDataTileAndNotHoed with MyIsFarmableDataTileAndNotHoed using robust name matching
            else if (code[i].opcode == OpCodes.Callvirt &&
                     code[i].operand is MethodInfo methodInfo2 &&
                     methodInfo2.Name == "IsFarmableDataTileAndNotHoed")
            {
                code[i].opcode = OpCodes.Call;
                code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(MyIsFarmableDataTileAndNotHoed));
            }
            // Replace SelectCurrentHoeItem with MySelectCurrentHoeItemOLDGCALLOC using robust name matching
            else if ((code[i].opcode == OpCodes.Call || code[i].opcode == OpCodes.Callvirt) &&
                     code[i].operand is MethodInfo methodInfo3 &&
                     methodInfo3.Name == "SelectCurrentHoeItem")
            {
                code[i].opcode = OpCodes.Call;
                code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(MySelectCurrentHoeItemOLDGCALLOC));
            }
        }

        return code;
    }

    public static float GetHoeRange()
    {
        if (Plugin.modEnabled.Value && Plugin.remoteKey.Value.IsPressed())
        {
            return 1000.0f;
        }
        return 1.5f;
    }

    public static bool MyIsFarmableDataTile(GameManager instance, Vector2Int pos)
    {
        if (Plugin.modEnabled.Value && Plugin.remoteKey.Value.IsPressed())
        {
            return true;
        }
        return instance.IsFarmableDataTile(pos);
    }

    public static bool MyIsFarmableDataTileAndNotHoed(GameManager instance, Vector2Int pos)
    {
        if (Plugin.modEnabled.Value && Plugin.remoteKey.Value.IsPressed())
        {
            return true;
        }
        return instance.IsFarmableDataTileAndNotHoed(pos);
    }

    public static void MySelectCurrentHoeItemOLDGCALLOC(Hoe hoe)
    {
        var _selection = Traverse.Create(hoe).Field<GameObject>("_selection").Value;

        if (!Plugin.modEnabled.Value || !Plugin.remoteKey.Value.IsPressed())
        {
            // Reset custom selection objects
            selectionList?.ForEach(x => x.SetActive(false));

            // Call the original method
            Traverse.Create(hoe).Method("SelectCurrentHoeItem").GetValue();
            return;
        }

        if (_selection == null)
        {
            return;
        }

        // Calculate and update hoe.pos and hoe.currentSpot so we target the correct aimed tile!
        var traverseHoe = Traverse.Create(hoe);
        var potentialHoeingSpots = traverseHoe.Field<List<Vector2Int>>("potentialHoeingSpots").Value;
        if (potentialHoeingSpots != null && potentialHoeingSpots.Count > 0)
        {
            var currentSpot = traverseHoe.Method("GetCurrentSpot").GetValue<Vector2Int>();
            traverseHoe.Field<Vector2Int>("currentSpot").Value = currentSpot;
            traverseHoe.Field<Vector2Int>("pos").Value = currentSpot;
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
        var pos = traverseHoe.Field<Vector2Int>("pos").Value;

        for (int i = 0; i < selectionList.Count; i++)
        {
            int x = i % 5 - 2;
            int y = i / 5 - 2;
            var item = selectionList[i];
            var p = new Vector2Int(pos.x - x, pos.y - y);

            if (!SingletonBehaviour<TileManager>.Instance.HasTile(p, ScenePortalManager.ActiveSceneIndex) &&
                (SingletonBehaviour<TileManager>.Instance.IsHoeable(p) || SingletonBehaviour<TileManager>.Instance.IsFarmable(p)))
            {
                item.SetActive(true);
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
}
