using System.Collections.Generic;

namespace CustomRecipes;

public class RecipeInputDefinition
{
    public int id; // Item ID of the input item
    public int amount; // Amount of the input item required for the recipe
}

public class RecipeDefinition
{
    public string list; // Crafting table internal name
    public int outputId; // Item ID of the crafted item
    public int amount; // Amount of the output item
    public float hours; // Crafting time in game hours
    public List<RecipeInputDefinition> inputs; // List of input items required for the recipe
}