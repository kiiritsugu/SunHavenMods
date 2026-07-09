using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Wish;
using PSS;

using Shared;

namespace CustomRecipes;

public static class RecipeHandler
{
    /// <summary>
    /// Scans, parses, and caches all .recipe.json files into the registry.
    /// Called once during Plugin.Awake().
    /// </summary>
    public static void LoadAllCustomRecipes()
    {
        List<string> files = FileScanner.FindFiles("*.recipe.json");
        Plugin.logger.LogInfo($"Processing {files.Count} custom recipe files using Shared.JsonParser...");

        foreach (string file in files)
        {
            string jsonText = JsonLoader.ReadFileContent(file);
            if (string.IsNullOrEmpty(jsonText)) continue;

            try
            {
                var parsedData = JsonParser.Parse(jsonText);
                if (parsedData == null) continue;

                List<RecipeDefinition> recipesToRegister = new();

                // Case A: The root structure is a single standalone dictionary file
                if (parsedData is Dictionary<string, object> rootDict)
                {
                    // Single root item containing a sub-list of recipe paths
                    if (rootDict.ContainsKey("outputId") && rootDict.ContainsKey("recipes") && rootDict["recipes"] is List<object> groupedRecipes)
                    {
                        int fileOutputId = Convert.ToInt32(rootDict["outputId"]);
                        foreach (var recipeObj in groupedRecipes)
                        {
                            if (recipeObj is Dictionary<string, object> recipeDict)
                            {
                                RecipeDefinition recipe = ParseSingleRecipeNode(recipeDict, fileOutputId, Path.GetFileName(file));
                                if (recipe != null) recipesToRegister.Add(recipe);
                            }
                        }
                    }
                    // Legacy flat JSON format fallback
                    else if (rootDict.ContainsKey("list") && rootDict.ContainsKey("outputId"))
                    {
                        int localOutputId = Convert.ToInt32(rootDict["outputId"]);
                        RecipeDefinition recipe = ParseSingleRecipeNode(rootDict, localOutputId, Path.GetFileName(file));
                        if (recipe != null) recipesToRegister.Add(recipe);
                    }
                }
                // Case B: The root structure is an array list (Enables Option B configuration styling)
                else if (parsedData is List<object> rootList)
                {
                    foreach (var element in rootList)
                    {
                        if (element is Dictionary<string, object> nodeDict)
                        {
                            // Check if this specific node contains nested recipe groups
                            if (nodeDict.ContainsKey("outputId") && nodeDict.ContainsKey("recipes") && nodeDict["recipes"] is List<object> nestedRecipes)
                            {
                                int groupedOutputId = Convert.ToInt32(nodeDict["outputId"]);
                                foreach (var recipeObj in nestedRecipes)
                                {
                                    if (recipeObj is Dictionary<string, object> recipeDict)
                                    {
                                        RecipeDefinition recipe = ParseSingleRecipeNode(recipeDict, groupedOutputId, Path.GetFileName(file));
                                        if (recipe != null) recipesToRegister.Add(recipe);
                                    }
                                }
                            }
                            // Check if it's a flat mixed array fallback list
                            else if (nodeDict.ContainsKey("list"))
                            {
                                int localOutputId = nodeDict.ContainsKey("outputId") ? Convert.ToInt32(nodeDict["outputId"]) : 0;
                                RecipeDefinition recipe = ParseSingleRecipeNode(nodeDict, localOutputId, Path.GetFileName(file));
                                if (recipe != null) recipesToRegister.Add(recipe);
                            }
                        }
                    }
                }

                // Bind parsed configurations to the static runtime dictionary memory
                foreach (var recipe in recipesToRegister)
                {
                    string tableKey = StringUtils.RemoveWhiteSpace(recipe.list);

                    if (!RecipeRegistry.NewRecipes.ContainsKey(tableKey))
                    {
                        RecipeRegistry.NewRecipes[tableKey] = new List<RecipeDefinition>();
                    }

                    RecipeRegistry.NewRecipes[tableKey].Add(recipe);
                    Plugin.logger.LogInfo($"Successfully registered custom recipe for item {recipe.outputId} -> {tableKey}");
                }

            }
            catch (Exception e)
            {
                Plugin.logger.LogError($"Critical parsing error in file {Path.GetFileName(file)}: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Dynamically injects registered definitions into an active game crafting table instance.
    /// Called via Harmony Postfix hook on CraftingTable.Awake.
    /// </summary>
    public static void AddRecipesToTable(CraftingTable table)
    {
        if (table == null) return;

        var recipeList = Traverse.Create(table).Field("recipeList").GetValue<RecipeList>();
        if (recipeList == null) return;

        string tableKey = StringUtils.RemoveWhiteSpace(recipeList.name);

        if (!RecipeRegistry.NewRecipes.ContainsKey(tableKey))
        {
            return;
        }

        foreach (var entry in RecipeRegistry.NewRecipes[tableKey])
        {
            bool recipeExists = recipeList.craftingRecipes.Any(r =>
                r.output2.id == entry.outputId &&
                r.input2.Count == entry.inputs.Count &&
                entry.inputs.All(input => r.input2.Any(ri => ri.id == input.id && ri.amount == (input.amount == 0 ? 1 : input.amount)))
            );

            if (recipeExists) continue;

            foreach (var inputDef in entry.inputs)
            {
                Database.GetData<ItemData>(inputDef.id, null);
            }

            var recipe = ScriptableObject.CreateInstance<Recipe>();
            recipe.worldProgressTokens = new List<Progress>();
            recipe.characterProgressTokens = new List<Progress>();
            recipe.questProgressTokens = new List<QuestAsset>();
            recipe.hoursToCraft = entry.hours;

            Database.GetData<ItemData>(entry.outputId, data =>
            {
                recipe.output2 = new SerializedItemDataNamedAmount()
                {
                    id = entry.outputId,
                    name = data.name,
                    amount = entry.amount == 0 ? 1 : entry.amount
                };
            });

            recipe.input2 = new List<SerializedItemDataNamedAmount>();
            var tmpItems = new Dictionary<int, ItemData>();

            foreach (var inputDef in entry.inputs)
            {
                Database.GetData<ItemData>(inputDef.id, data =>
                {
                    tmpItems[inputDef.id] = data;

                    if (tmpItems.Count != entry.inputs.Count) return;

                    foreach (var innerInputDef in entry.inputs)
                    {
                        recipe.input2.Add(new SerializedItemDataNamedAmount()
                        {
                            id = innerInputDef.id,
                            name = tmpItems[innerInputDef.id].name,
                            amount = innerInputDef.amount == 0 ? 1 : innerInputDef.amount
                        });
                    }
                });
            }

            recipeList.craftingRecipes.Add(recipe);
            Plugin.logger.LogInfo($"Injected custom recipe variant for item {entry.outputId} into table {recipeList.name}");
        }
    }

    private static RecipeDefinition ParseSingleRecipeNode(Dictionary<string, object> node, int outputId, string fileName)
    {
        RecipeDefinition recipe = new()
        {
            list = node.ContainsKey("list") ? node["list"]?.ToString() : "",
            outputId = outputId,
            amount = node.ContainsKey("amount") ? Convert.ToInt32(node["amount"]) : 1,
            hours = node.ContainsKey("hours") ? Convert.ToSingle(node["hours"]) : 1f,
            inputs = new List<RecipeInputDefinition>()
        };

        if (string.IsNullOrEmpty(recipe.list) || recipe.outputId == 0)
        {
            Plugin.logger.LogWarning($"Skipping invalid custom recipe item node in file: {fileName}");
            return null;
        }

        if (node.ContainsKey("inputs") && node["inputs"] is List<object> rawInputs)
        {
            foreach (var inputObj in rawInputs)
            {
                if (inputObj is Dictionary<string, object> inputDict)
                {
                    recipe.inputs.Add(new RecipeInputDefinition
                    {
                        id = inputDict.ContainsKey("id") ? Convert.ToInt32(inputDict["id"]) : 0,
                        amount = inputDict.ContainsKey("amount") ? Convert.ToInt32(inputDict["amount"]) : 1
                    });
                }
            }
        }

        return recipe;
    }
}
