using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using Wish;

namespace SunHavenHelloWorld
{
    [BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "com.yourname.sunhaven.unifiedtotems";
        public const string PLUGIN_NAME = "Unified Totems";
        public const string PLUGIN_VERSION = "1.0.0";

        public const int TOTEM_ITEM_ID = 995500;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PLUGIN_GUID} is active. Injecting patches...");
            var harmony = new Harmony(PLUGIN_GUID);
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(Database), "InitIdentifiers")]
        class Database_InitIdentifiers_Patch
        {
            static void Postfix()
            {
                if (Database.items.ContainsKey(TOTEM_ITEM_ID)) return;

                PlaceableItem customTotem = ScriptableObject.CreateInstance<PlaceableItem>();
                
                customTotem.id = TOTEM_ITEM_ID;
                customTotem.name = "Unified_Totem_Item"; 
                customTotem.idName = "Unified_Totem_Item";
                
                customTotem._name = "Unified Totem";
                customTotem.description = "A beautiful custom totem. It doesn't seem to do anything yet.";
                customTotem.useDescription = "Place it on your farm.";
                
                customTotem.rarity = Rarity.Epic;
                customTotem.stackSize = 99;
                customTotem.canBeSold = false;
                customTotem.canBeTrashable = true;

                if (Database.items.TryGetValue(11400, out ItemData vanillaTotem))
                {
                    customTotem.icon = vanillaTotem.icon; 
                    
                    if (vanillaTotem is PlaceableItem vanillaPlaceable)
                    {
                        customTotem.prefab = vanillaPlaceable.prefab; 
                    }
                }

                Database.items.Add(TOTEM_ITEM_ID, customTotem);
                Database.ids.Add("Unified_Totem_Item", TOTEM_ITEM_ID);
                
                Debug.Log($"[Unified Totems] Successfully injected Custom Totem with ID: {TOTEM_ITEM_ID}");
            }
        }
    }
}
