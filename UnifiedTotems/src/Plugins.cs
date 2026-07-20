using System;
using System.Collections;
using System.ComponentModel;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wish;

using Shared;

namespace UnifiedTotems;

[BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
[BepInDependency("CustomItems", "0.2.2")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PLUGIN_GUID);
    public static ManualLogSource logger;
    public static bool DebugMode = true;
    public const string PLUGIN_GUID = "com.kiiritsugu.sunhaven.unifiedtotems";
    public const string PLUGIN_NAME = "Unified Totems";
    public const string PLUGIN_VERSION = "1.1.0";

    public static Plugin Instance { get; private set; }

    private void Awake()
    {
        logger = Logger;
        harmony.PatchAll();
        Logger.LogInfo($"Plugin {PLUGIN_GUID} is active.");

        Instance = this;
        CoroutineRunner.SetHost(this);
    }

    [HarmonyPatch]
    public static class Patches
    {
        // Adjust vanilla totems and crop colliders as early as possible.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuController), "Start")]
        public static void InitializeVanillaAdjustments()
        {
            Plugin.logger.LogInfo("UnifiedTotems: Adjusting vanilla totems and crop colliders...");
            
            try { ItemHandler.AdjustVanillaTotems(); } catch (Exception e) { logger.LogError($"Error patching vanilla totems: {e}"); }
            try { ItemHandler.AdjustCropColliders(); } catch (Exception e) { logger.LogError($"Error patching crop colliders: {e}"); }
        }

        // CreateTotems depends on CustomItems injection, so it hooks into their event.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CustomItems.CustomItems), nameof(CustomItems.CustomItems.AddItems))]
        public static void AfterCustomItemsAdded()
        {
            Plugin.logger.LogInfo("UnifiedTotems: CustomItems initialized. Starting custom totem configuration.");
            try { ItemHandler.CreateTotems(); } catch (Exception err) { logger.LogError($"Error creating custom totems: {err}"); }
        }

        private static bool _isonFinishLoadingDecorationsSubscribed = false;
        //subscribes to event to evaluate enhanced totems in scene after decorations are finished loading
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ScenePortalManager), "Awake")]
        public static void CheckEnhancedTotems()
        {
            if(_isonFinishLoadingDecorationsSubscribed) return;

            ScenePortalManager.onFinishLoadingDecorations += () => TotemHandler.EvaluateEnhancedTotemsInScene();

            _isonFinishLoadingDecorationsSubscribed = true;
        }
    }
}
