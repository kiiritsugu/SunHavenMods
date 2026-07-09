using System;
using System.IO;
using System.Collections.Generic;
using BepInEx;

namespace Shared;

public static class FileScanner
{
    /// <summary>
    /// Scans the BepInEx plugins directory and subdirectories for files matching a specific pattern.
    /// </summary>
    public static List<string> FindFiles(string searchPattern)
    {
        List<string> foundFiles = new();
        
        try
        {
            string pluginsFolder = Paths.PluginPath;

            if (!Directory.Exists(pluginsFolder))
            {
                return foundFiles;
            }

            // Use the passed pattern (e.g., "*.recipe.json")
            string[] files = Directory.GetFiles(pluginsFolder, searchPattern, SearchOption.AllDirectories);
            foundFiles.AddRange(files);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[FileScanner] Failed scanning for {searchPattern}: " + e.Message);
        }

        return foundFiles;
    }
}
