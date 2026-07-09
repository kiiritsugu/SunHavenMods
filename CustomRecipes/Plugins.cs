using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CustomRecipes;

[BepInPlugin("com.kiiritsugu.customrecipes", "Custom Recipes Injector", "1.0.0")]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony("com.kiiritsugu.customrecipes");
    public static ManualLogSource logger;

    private void Awake()
    {
        logger = this.Logger;
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
