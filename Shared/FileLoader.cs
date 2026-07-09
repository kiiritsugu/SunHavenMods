// Utilitaries borrowed from https://github.com/Morthy/sunhaven-mods/tree/main/Morthy.Util All the code belongs to morthy i've requested permission but didn't receive an answer yet, if requested by the author I will remove this code from my mod and replace it with my own implementation.
// All credits and thanks to morthy 

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Morthy.Util;

public static class FileLoader {
    private static string GetResourcePath(Assembly assembly, string name)
    {
        string resourcePath = name;
        if (!name.StartsWith(assembly.GetName().Name))
        {
            resourcePath = assembly.GetManifestResourceNames()
                .Single(str => str.EndsWith(name));
        }

        return resourcePath;
    }
    
    public static string LoadFile(Assembly assembly, string name)
    {
        using (var stream = assembly.GetManifestResourceStream(GetResourcePath(assembly, name)))
        using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
        {
            return reader.ReadToEnd();
        }
    }
    
    public static byte[] LoadFileBytes(Assembly assembly, string name)
    {
        using (var stream = assembly.GetManifestResourceStream(GetResourcePath(assembly, name)))
        using (var reader = new StreamReader(stream ?? throw new InvalidOperationException()))
        {
            byte[] bytes;
            using (var memoryStream = new MemoryStream())
            {
                reader.BaseStream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            return bytes;
        }
    }
}