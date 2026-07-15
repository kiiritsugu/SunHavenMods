using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Wish;

namespace RemoteEarthquakeAndRainCloud;

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
            // Replace IsFarmableDataTile with MyIsFarmableDataTile
            else if (code[i].opcode == OpCodes.Callvirt &&
                     code[i].operand != null &&
                     code[i].OperandIs(AccessTools.Method(typeof(GameManager), nameof(GameManager.IsFarmableDataTile), new[] { typeof(Vector2Int) })))
            {
                code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(MyIsFarmableDataTile));
            }
            // Replace IsFarmableDataTileAndNotHoed with MyIsFarmableDataTileAndNotHoed
            else if (code[i].opcode == OpCodes.Callvirt &&
                     code[i].operand != null &&
                     code[i].OperandIs(AccessTools.Method(typeof(GameManager), nameof(GameManager.IsFarmableDataTileAndNotHoed), new[] { typeof(Vector2Int) })))
            {
                code[i].operand = AccessTools.Method(typeof(HoePatch), nameof(MyIsFarmableDataTileAndNotHoed));
            }
            // Replace SelectCurrentHoeItem with MySelectCurrentHoeItemOLDGCALLOC
            else if ((code[i].opcode == OpCodes.Call || code[i].opcode == OpCodes.Callvirt) &&
                     code[i].operand != null &&
                     code[i].OperandIs(AccessTools.Method(typeof(Hoe), "SelectCurrentHoeItem")))
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
        var pos = Traverse.Create(hoe).Field<Vector2Int>("pos").Value;

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
                item.transform.eulerAngles = new Vector3(0f, 0f, 0f);
                item.transform.localScale = new Vector3(1f, 1.4142135f, 1f);
                Vector3 vector = new Vector3((float)p.x + 0.5f, ((float)p.y + 0.5f) * 1.4142135f, 0f);
                float num = SingletonBehaviour<GameManager>.Instance.Depth(vector, false);
                vector = new Vector3(vector.x, vector.y + num, vector.z + num);
                item.transform.position = vector + new Vector3(0f, -0.25f, -0.25f);
                SpriteRenderer component = item.GetComponent<SpriteRenderer>();
                if (component != null)
                {
                    component.size = Vector2.one * 1.25f;
                }
                item.transform.position += new Vector3(0f, 0.001f * i, 0.001f * i);
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
