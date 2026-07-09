using System.Linq;

namespace Shared;

public static class StringUtils
{
  /// <summary>
  /// Removes all whitespace from the string.
  /// </summary>
  public static string RemoveWhiteSpace(string input)
  {
    if (string.IsNullOrEmpty(input)) return string.Empty;
    return new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
  }
}
