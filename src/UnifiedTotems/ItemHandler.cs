using System;
using System.Reflection;
using PSS;
using UnityEngine;
using Wish;

namespace UnifiedTotems;
using BepInEx.Logging;


public static class ItemHandler
{      
    public static ManualLogSource logger;  
    public const int UNIFIED_TOTEM_ID = 99601;

    public static Scarecrow EnableTotems(ItemData item)
    {
        logger.LogInfo($"Enabling Unified Totems for item {item.name} ({item.id})");

        Scarecrow newScarecrow = new Scarecrow();

        FieldInfo[] fields = typeof(Decoration).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            try
            {
                object value = field.GetValue(item);
                field.SetValue(newScarecrow, value);
            }
            catch (Exception err)
            {
                logger.LogError($"Error occurred while enabling totem for field {field.Name}: " + err);
            }
        }

        newScarecrow.scareCrowEffect = ScareCrowEffect.Royal;
        newScarecrow.range = 6;
        newScarecrow.cropCapacity = 3;

        return newScarecrow;
    }

    public static Scarecrow CreateTotems()
    {
        Scarecrow newTotem = null;
        Database.GetData<ItemData>(UNIFIED_TOTEM_ID, data => newTotem = EnableTotems(data));
        return newTotem;
    }
}