using System;
using System.Collections.Generic;
using System.Text;

namespace Shared;

public static class JsonParser
{
    public static object Parse(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        int index = 0;
        return ParseValue(json.Trim(), ref index);
    }

    private static object ParseValue(string json, ref int index)
    {
        SkipWhitespace(json, ref index);
        if (index >= json.Length) return null;

        char c = json[index];
        if (c == '{') return ParseObject(json, ref index);
        if (c == '[') return ParseArray(json, ref index);
        if (c == '"') return ParseString(json, ref index);
        if (char.IsDigit(c) || c == '-') return ParseNumber(json, ref index);
        if (c == 't' || c == 'f') return ParseBoolean(json, ref index);
        if (c == 'n') { index += 4; return null; } // null

        throw new FormatException($"Unexpected character '{c}' at position {index}");
    }

    private static Dictionary<string, object> ParseObject(string json, ref int index)
    {
        Dictionary<string, object> dict = new();
        index++; // Skip '{'

        while (index < json.Length)
        {
            SkipWhitespace(json, ref index);
            if (json[index] == '}') { index++; return dict; }

            if (json[index] != '"') throw new FormatException($"Expected string key at position {index}");
            string key = ParseString(json, ref index);

            SkipWhitespace(json, ref index);
            if (json[index] != ':') throw new FormatException($"Expected ':' at position {index}");
            index++; // Skip ':'

            object val = ParseValue(json, ref index);
            dict[key] = val;

            SkipWhitespace(json, ref index);
            if (json[index] == ',') index++;
            else if (json[index] != '}') throw new FormatException($"Expected ',' or '}}' at position {index}");
        }
        return dict;
    }

    private static List<object> ParseArray(string json, ref int index)
    {
        List<object> list = new();
        index++; // Skip '['

        while (index < json.Length)
        {
            SkipWhitespace(json, ref index);
            if (json[index] == ']') { index++; return list; }

            list.Add(ParseValue(json, ref index));

            SkipWhitespace(json, ref index);
            if (json[index] == ',') index++;
            else if (json[index] != ']') throw new FormatException($"Expected ',' or ']' at position {index}");
        }
        return list;
    }

    private static string ParseString(string json, ref int index)
    {
        index++; // Skip opening '"'
        StringBuilder sb = new();
        while (index < json.Length)
        {
            char c = json[index];
            if (c == '"') { index++; return sb.ToString(); }
            if (c == '\\') index++; // Skip escape characters for simplicity
            sb.Append(json[index]);
            index++;
        }
        return sb.ToString();
    }

    private static object ParseNumber(string json, ref int index)
    {
        int start = index;
        bool isFloat = false;
        while (index < json.Length)
        {
            char c = json[index];
            if (c == '.' || c == 'e' || c == 'E') isFloat = true;
            if (char.IsDigit(c) || c == '-' || c == '+' || c == '.') index++;
            else break;
        }
        string numStr = json.Substring(start, index - start);
        if (isFloat && float.TryParse(numStr, out float f)) return f;
        if (int.TryParse(numStr, out int i)) return i;
        return numStr;
    }

    private static bool ParseBoolean(string json, ref int index)
    {
        if (json[index] == 't') { index += 4; return true; }
        index += 5; return false;
    }

    private static void SkipWhitespace(string json, ref int index)
    {
        while (index < json.Length && char.IsWhiteSpace(json[index])) index++;
    }
}
