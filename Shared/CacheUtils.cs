using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using BepInEx;

namespace Shared;

public static class CacheUtils
{
    private static string GetCachePath(string modFolderName, string fileName)
    {
        return Path.Combine(Paths.ConfigPath, modFolderName, fileName);
    }

    public static void SaveCache<T>(string modFolderName, string fileName, T data)
    {
        string path = GetCachePath(modFolderName, fileName);
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string json = SerializeToJson(data);
        File.WriteAllText(path, json);
    }

    private static string SerializeToJson<T>(T data)
    {
        if (data is IEnumerable list)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            bool first = true;
            foreach (var item in list)
            {
                if (!first) sb.Append(",");
                sb.Append(item.ToString());
                first = false;
            }
            sb.Append("]");
            return sb.ToString();
        }
        return "[]";
    }

    public static T LoadCache<T>(string modFolderName, string fileName)
    {
        string path = GetCachePath(modFolderName, fileName);
        if (!File.Exists(path)) return default(T);

        string json = File.ReadAllText(path);
        return (T)JsonParser.Parse(json);
    }
}
