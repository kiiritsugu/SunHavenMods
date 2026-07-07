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

    public const string PLUGIN_GUID = "com.kiiritsugu.sunhaven.unifiedtotems";
    public const string PLUGIN_NAME = "Unified Totems";
    public const string PLUGIN_VERSION = "0.1.1";

    private void Awake()
    {
        logger = Logger;
        harmony.PatchAll();
        Logger.LogInfo($"Plugin {PLUGIN_GUID} is active.");
    }

    [HarmonyPatch]
    public static class Patches
    {
        // CustomItems registers JSON items on MainMenuController.Start; PlayGame runs after that.
        // Phase 1 = CustomItems adds ItemData to Database. Phase 2 = ItemHandler edits behavior.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "PlayGame", new Type[] {})]
        public static void MainMenuControllerPlayGame()
        {
            try
            {
                ItemHandler.CreateTotems();
            }
            catch (Exception err)
            {
                logger.LogError($"Error occurred while creating totems: {err}");
            }

            try
            {
                ItemHandler.PatchCropColliders();
            }
            catch (Exception err)
            {
                logger.LogError($"Error occurred while patching crop colliders: {err}");
            }
        }
    }
}
