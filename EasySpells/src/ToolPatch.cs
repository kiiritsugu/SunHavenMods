using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using Wish;

namespace EasySpells;

[HarmonyPatch(typeof(Tool))]
public static class ToolPatch
{
    [HarmonyPatch("Use1"), HarmonyPrefix]
    public static bool Use1_Prefix(Tool __instance)
    {
        var posField = Traverse.Create(__instance).Field<Vector2Int>("pos");
        if (posField == null) return true; // Safety check

        Vector2Int pos = posField.Value;

        if (Plugin.modEnabled.Value && Plugin.remoteKey.Value.IsPressed())
        {
            // Verify if the player has the required spell level
            if (__instance is Hoe &&
                Plugin.earthqueakeSpell != null &&
                GameSave.Farming.GetNodeAmount("Farming5a", 3, true) > 0)
            {
                Plugin.earthqueakePos = pos;
                // Try to use the earthquake spell
                Plugin.earthqueakeSpell.UseDown1();

                // Return false to cancel vanilla behavior
                return false;
            }

            else if (__instance is Pickaxe &&
                Plugin.arcaneExplosionSpell != null &&
                GameSave.Mining.GetNodeAmount("Mining4a", 5, true) > 0)
            {
                Plugin.arcaneExplosionSpell.UseDown1();
                return false;
            }

            else if (__instance is Axe &&
                Plugin.woodcuttingSpell != null &&
                GameSave.Exploration.GetNodeAmount("Exploration5b", 3, true) > 0)
            {
                Plugin.woodcuttingSpell.UseDown1();
                return false;
            }
        }
        return true;
    }

    public class MySetSelectionOnTileBodyArg
    {
        public GameObject _selection;
        public Transform transform;
    }
    [HarmonyPatch("SetSelectionOnTile", new[] { typeof(Vector2Int) }), HarmonyReversePatch]
    public static void MySetSelectionOnTileBody(object instance, Vector2Int pos)
    {
        IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            for (int i = 0; i + 1 < code.Count; i++)
            {
                if (code[i].opcode == OpCodes.Ldarg_0)
                {
                    if (code[i + 1].opcode == OpCodes.Ldfld &&
                        code[i + 1].OperandIs(AccessTools.Field(typeof(Tool), "_selection")))
                    {
                        code[i + 1].operand = AccessTools.Field(typeof(MySetSelectionOnTileBodyArg), nameof(MySetSelectionOnTileBodyArg._selection));
                    }
                    else if (code[i + 1].opcode == OpCodes.Call &&
                               code[i + 1].OperandIs(AccessTools.PropertyGetter(typeof(Component), nameof(Component.transform))))
                    {
                        code[i + 1].opcode = OpCodes.Ldfld;
                        code[i + 1].operand = AccessTools.Field(typeof(MySetSelectionOnTileBodyArg), nameof(MySetSelectionOnTileBodyArg.transform));
                    }
                }
            }

            return code;
        }
        _ = Transpiler(null);
    }
}
