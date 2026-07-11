using System;
using System.Collections.Generic;
using System.Reflection;

using BepInEx.Logging;

namespace Shared;

public static class DatabaseUtils
{
  public static ManualLogSource logger;

  public static void CopyBaseToDerived<TBase, TDerived>(TBase source, TDerived target) where TDerived : TBase
  {
    //Copy fields (Copies raw data, IDs, textures, and settings)
    FieldInfo[] fields = typeof(TBase).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    foreach (FieldInfo field in fields)
    {
      field.SetValue(target, field.GetValue(source));
    }

    //Safely copy properties (Skip Unity internal properties that cause NullReferenceExceptions)
    PropertyInfo[] properties = typeof(TBase).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
    foreach (PropertyInfo prop in properties)
    {
      if (!prop.CanWrite || !prop.CanRead) continue;

      // SKIP list for Unity properties that require a live, spawned GameObject scene context
      if (prop.Name == "Position" || prop.Name == "transform" || prop.Name == "gameObject" || prop.Name == "tag")
      {
        continue;
      }

      try
      {
        prop.SetValue(target, prop.GetValue(source, null), null);
      }
      catch (Exception)
      {
        logger.LogWarning($"CopyBaseToDerived: Skipping property {prop.Name} due to exception.");
      }
    }
  }
}