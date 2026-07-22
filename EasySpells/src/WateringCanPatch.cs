using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Wish;

namespace EasySpells;

[HarmonyPatch(typeof(WateringCan))]
public static class WateringCanPatch
{
    private static GameObject baseSelection;
    private static List<GameObject> selectionList;

    [HarmonyPatch("HandleWateringCanEachFrame"), HarmonyReversePatch]
    public static void MyLateUpdate(object instance)
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i + 2 < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt &&
                    code[i].operand != null &&
                    code[i].OperandIs(AccessTools.PropertyGetter(typeof(Player), nameof(Player.ExactPosition))) &&
                    code[i + 2].OperandIs(1.5f))
                {
                    code[i + 2].operand = 1000.0f;
                }
            }

            var setSelectionFound = false;
            for (int i = 0; i < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt &&
                    code[i].operand != null &&
                    code[i].OperandIs(AccessTools.Method(typeof(TileManager), nameof(TileManager.IsWaterable), new[] { typeof(Vector2Int) })))
                {
                    code[i].operand = AccessTools.Method(typeof(WateringCanPatch), nameof(MyIsWaterable));
                }
                else if (code[i].opcode == OpCodes.Callvirt &&
                           code[i].operand is MethodInfo &&
                           (code[i].operand as MethodInfo)?.Name == nameof(GameManager.TryGetObjectSubTile))
                {
                    code[i].operand = AccessTools.Method(typeof(WateringCanPatch), nameof(MyTryGetObjectSubTile));
                }
                else if (code[i].opcode == OpCodes.Call &&
                           code[i].operand != null &&
                           code[i].OperandIs(AccessTools.Method(typeof(Tool), "SetSelectionOnTile", new[] { typeof(Vector2Int) })))
                {
                    if (!setSelectionFound)
                    {
                        code[i].operand = AccessTools.Method(typeof(WateringCanPatch), nameof(MySetSelectionOnTile));
                        setSelectionFound = true;
                    }
                    else
                    {
                        code[i].operand = AccessTools.Method(typeof(WateringCanPatch), nameof(MySetSelectionOnTileEmpty));
                    }
                }
            }

            return code;
        }
        _ = Transpiler(null);
    }

    public static bool MyIsWaterable(object instance, Vector2Int pos)
    {
        return true;
    }

    public static bool MyTryGetObjectSubTile(object instance, Vector3Int position, out object value)
    {
        value = null;
        return false;
    }

    public static void MySetSelectionOnTileEmpty(object instance, Vector2Int pos)
    {
        return;
    }

    public static void MySetSelectionOnTile(object instance, Vector2Int pos)
    {
        WateringCan can = (WateringCan)instance;
        var traverseCan = Traverse.Create(can);
        var _selection = traverseCan.Field<GameObject>("_selection").Value;
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
            for (int i = 0; i < 65; i++)
            {
                var item = UnityEngine.Object.Instantiate<GameObject>(_selection);
                item.SetActive(false);
                selectionList.Add(item);
            }
        }
        var offset = traverseCan.Method("GetOffset", new Type[] { }).GetValue<Vector2>();
        var direction = traverseCan.Method("GetAttackDirection", new object[] { offset }).GetValue<global::Direction>();
        _selection.SetActive(false);
        for (int i = 0; i < selectionList.Count; i++)
        {
            int x;
            int y;
            switch (direction)
            {
                case Direction.North:
                    x = i % 5 - 2;
                    y = i / 5;
                    break;
                case Direction.South:
                    x = i % 5 - 2;
                    y = -(i / 5);
                    break;
                case Direction.East:
                    x = i / 5;
                    y = i % 5 - 2;
                    break;
                default:
                    x = -(i / 5);
                    y = i % 5 - 2;
                    break;
            }
            var item = selectionList[i];
            Vector2Int position = pos;
            var p = new Vector2Int(position.x + x, position.y + y);
            ToolPatch.MySetSelectionOnTileBody(new ToolPatch.MySetSelectionOnTileBodyArg { _selection = item, transform = can.transform }, p);
            item.transform.localScale = new Vector3(1f, 1.4142135f, 1f);
            item.gameObject.transform.position += new Vector3(0f, 0.0001f * i, 0.0001f * i);
        }
    }

    [HarmonyPatch("OnDisable"), HarmonyPrefix]
    public static bool OnDisable_Prefix(WateringCan __instance)
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
    public static bool LateUpdate_Prefix(WateringCan __instance)
    {
        if (Plugin.modEnabled.Value &&
            Plugin.remoteKey.Value.IsPressed())
        {
            MyLateUpdate(__instance);
            return false;
        }
        else
        {
            selectionList?.ForEach(x => x.SetActive(false));
        }
        return true;
    }

    [HarmonyPatch("Use1"), HarmonyPrefix]
    public static bool Use1_Prefix(WateringCan __instance)
    {
        if (Plugin.modEnabled.Value &&
            Plugin.remoteKey.Value.IsPressed() &&
            Plugin.cloudSpell != null)
        {
            if (GameSave.Farming.GetNodeAmount("Farming7a", 3, true) > 0)
            {
                Plugin.cloudPos = __instance.pos;
                Plugin.cloudSpell.UseDown1();
            }
            return false;
        }
        return true;
    }
}
