using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx;
using HarmonyLib;
using System;
using System.Reflection;
using Wish;
using UnityEngine;

namespace EasySpells;

[BepInPlugin(PluginGuid, PluginName, PluginVer)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource logger;
    public static ConfigEntry<bool> modEnabled;
    public static ConfigEntry<KeyboardShortcut> remoteKey;
    public static EarthquakeSpell earthqueakeSpell;
    public static Vector2Int earthqueakePos;
    public static CloudSpell cloudSpell;
    public static Vector2Int cloudPos;
    public static ArcaneExplosionSpell arcaneExplosionSpell;
    public static WoodcuttingSpell woodcuttingSpell;

    private const string PluginGuid = "com.kiiritsugu.sunhaven.easyspells";
    private const string PluginName = "EasySpells";
    private const string PluginVer = "1.1.0";
    private void Awake()
    {
        logger = Logger;
        modEnabled = Config.Bind<bool>("General", "Mod Enabled", true, "Set to false to disable this mod.");
        remoteKey = Config.Bind<KeyboardShortcut>("General", "Remote Key", new KeyboardShortcut(KeyCode.LeftControl), "Earthquake: Use a hoe while holding this key. RainCloud: Use a watering can while holding this key. Ethereal Axe: Use a axe while holding this key. Vacuumulus: Use a pickaxe while holding this key.");
        var harmony = new Harmony(PluginGuid);
        harmony.PatchAll(Assembly.GetExecutingAssembly());
        Logger.LogInfo($"Plugin {PluginGuid} v{PluginVer} is loaded");
    }
}

