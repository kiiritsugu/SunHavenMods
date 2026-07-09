using System;
using System.IO;

namespace Shared;

public static class JsonLoader
{
    /// <summary>
    /// Safely reads the text content of a file. Returns null if read fails.
    /// </summary>
    public static string ReadFileContent(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[JsonLoader] Failed reading file at {filePath}: {e.Message}");
        }
        
        return null;
    }
}
