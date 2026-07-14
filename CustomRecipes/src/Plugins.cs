using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CustomRecipes;

[BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PLUGIN_GUID);
    public static ManualLogSource logger;
    public const string PLUGIN_GUID = "com.kiiritsugu.customrecipes";
    public const string PLUGIN_NAME = "Custom Recipes Injector";
    public const string PLUGIN_VERSION = "1.0.0";

    private void Awake()
    {
        logger = this.Logger;
        logger.LogInfo($"Plugin {PLUGIN_GUID} is active.");
        try
        {
            // 1. Process .recipe.json files into memory
            RecipeHandler.LoadAllCustomRecipes();

            // 2. Automatically find and load any other patch files
            harmony.PatchAll();
            
            logger.LogInfo("Custom Recipes Injector loaded successfully!");
        }
        catch (Exception e)
        {
            logger.LogError("Startup sequence failed: " + e);
        }
    }
}
