using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Wish;
using PSS;
using PSS.UI;

namespace RemoteEarthquakeAndRainCloud;

    [HarmonyPatch(typeof(Player))]
    public static class PlayerPatch
    {
        [HarmonyPatch(nameof(Player.InitializeAsOwner)),HarmonyPostfix]
        public static void InitializeAsOwner_Postfix(Player __instance, Transform ____spell1Transform) 
        {
            if (__instance.transform.Find("UI") != null && ____spell1Transform != null && Plugin.earthqueakeSpell == null) 
            {
                loadUseItem(306, __instance, ____spell1Transform, (useItem) => Plugin.earthqueakeSpell = useItem as EarthquakeSpell);
                loadUseItem(303, __instance, ____spell1Transform, (useItem) => Plugin.cloudSpell = useItem as CloudSpell);
            }
        }
        private static void loadUseItem(int item, Player __instance, Transform ____spell1Transform, Action<UseItem>onLoaded)
        {
            Database.GetData<ItemData>(
                item, 
                delegate (ItemData data) {
                    var transform = UnityEngine.Object.Instantiate<Transform>(____spell1Transform, ____spell1Transform.parent);
                    foreach (object obj in transform)
                    {
                        Transform t = (Transform)obj;
                        UnityEngine.Object.Destroy(t.gameObject);
                    }
                    var useItem = UnityEngine.Object.Instantiate<UseItem>(data.useItem, ____spell1Transform.position, data.useItem.transform.rotation, transform);
                    useItem.SetPlayer(__instance);
                    useItem.SetItemData(data);
                    Traverse.Create(useItem).Field("Casting").SetValue(false);
                    onLoaded(useItem);
                },
                null
            );
        }

        [HarmonyPatch(nameof(Player.UseItem), MethodType.Getter), HarmonyPostfix]
        public static void UseItem_Postfix(ref UseItem __result)
        {
            if(__result == null)
            {
                return;
            }
            if (Plugin.earthqueakeSpell && Plugin.earthqueakeSpell.Casting)
            {
                __result = Plugin.earthqueakeSpell;
                return;
            }
            if (Plugin.cloudSpell && Plugin.cloudSpell.Casting)
            {
                __result = Plugin.cloudSpell;
                return;
            }
        }
    }

