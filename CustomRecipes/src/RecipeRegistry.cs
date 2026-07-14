using System;
using System.Collections.Generic;

namespace CustomRecipes;

public static class RecipeRegistry
{
    // Key: Crafting Table Name, Value: List of recipes to inject
    public static Dictionary<string, List<RecipeDefinition>> NewRecipes = new(StringComparer.OrdinalIgnoreCase);

}