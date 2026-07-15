using System;
using HarmonyLib;
using Wish;

namespace CustomRecipes;

[HarmonyPatch(typeof(CraftingTable))]
public static class CraftingTablePatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Awake")]
    public static void AwakePostfix(CraftingTable __instance)
    {
        try
        {
            if (__instance == null) return;
            
            // Inject custom recipes into the crafting table after its Awake() method has completed
            RecipeHandler.AddRecipesToTable(__instance);
        }
        catch (Exception e)
        {
             Plugin.logger.LogError($"[CraftingTablePatch] Error trying to inject recipe: {e}");
        }
    }
}
