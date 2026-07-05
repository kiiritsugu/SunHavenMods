using System;
using System.Collections.Generic;
using System.Reflection;

using BepInEx;
using BepInEx.Logging;

using HarmonyLib;
using UnityEngine;

using PSS;
using Wish;
using QFSW.QC;

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
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Database), nameof(Database.GetCacheCapacity))]
        public static void DatabaseGetCacheCapacity(ref int __result)
        {
            __result = 999999;
        }

        [HarmonyPatch(typeof(MainMenuController), "PlayGame", new Type[] {})]
        public static void MainMenuControllerAwake()
        {
            try {
                ItemHandler.CreateTotems();
                logger.LogInfo($"Plugin {PLUGIN_NAME} is successfully loaded.");
                Database.Instance.ids 
            }
            catch (Exception err){
                logger.LogError($"Error occurred while adding custom totem:" + err);
            }
        }
    }
}

