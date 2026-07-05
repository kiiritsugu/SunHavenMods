using System;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using PSS;
using Wish;

namespace UnifiedTotems;

[BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
[BepInDependency("CustomItems", "0.2.2")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new(Plugin.PLUGIN_GUID);
    public static ManualLogSource logger;

    public const string PLUGIN_GUID = "com.yourname.sunhaven.unifiedtotems";
    public const string PLUGIN_NAME = "Unified Totems";
    public const string PLUGIN_VERSION = "1.0.0";

    private void Awake()
    {
        logger = Logger;
        harmony.PatchAll();
        Logger.LogInfo($"Plugin {PLUGIN_GUID} is active. Injecting patches...");
    }

    [HarmonyPatch]
    public static class Patches
    {
        // FIX (optional — remove unless you hit cache limits): This patch was added while chasing
        // Database.ids / CS0122 errors. Morthy's Sprinklers mod does not patch GetCacheCapacity.
        // CustomItems + Database.GetData in ItemHandler is enough for item setup; delete this
        // postfix if you do not have a proven need to raise the database cache size.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Database), nameof(Database.GetCacheCapacity))]
        public static void DatabaseGetCacheCapacity(ref int __result)
        {
            __result = 999999;
        }

        // FIX: Match Sprinklers — call item setup when a save loads, after CustomItems has
        // registered JSON items. Database.GetData (public API) runs in ItemHandler; no need for
        // Database.ids if you mutate item.useItem in place (see ItemHandler.cs).
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "PlayGame", new Type[] {})]
        public static void MainMenuControllerAwake()
        {
            try
            {
                ItemHandler.CreateTotems();
                logger.LogInfo($"Plugin {PLUGIN_NAME} is successfully loaded.");
            }
            catch (Exception err)
            {
                // FIX: Pass the exception object so the stack trace is logged (Sprinklers: logger.LogError(e)).
                logger.LogError(err);
            }
        }
    }
}
