# Custom Recipes
## A Sun Haven Mod that lets you add new recipes to both vanilla and modded items without writing any code

Heavily inspired by Custom Furniture and Items by Morthy
https://www.nexusmods.com/sunhaven/mods/187

While developing a separate mod, i came across a limitation on custom furniture and items, you can't add recipes to exiting items, nor add multiple recipes to the same item on a same or diferent crafting tables

This mod allows you to do both, you can create .recipe.json files for both custom modded items and vanilla items, and they will be injected to crafting tables

Refer to docs/ Recipe.MD for .recipe.json structure and CraftingTables.MD for a list of crafting tables names

You can place your .recipe.json files anywhere inside your plugins folder and subfolders
Feel free to use this mod as a dependency to add recipes to your mods. 

Credits to to Morthy for the inspiration and reference from his code on github for developing this mod
Nexus: https://www.nexusmods.com/profile/MorthyJD/mods
and to AzraelGodKing for the help on geting started with Sun Haven modding
https://www.nexusmods.com/profile/AzraelGodKing/mods

File structure
CustomRecipes/
│
├── Docs/
│   └── Reciepe.MD               <- Documentation on how to structure .recipe.json files
|
├── data/
│   └── *.recipe.json            <- You can place recipes here
|
├── Patches/
│   └── CraftingTablePatch.cs    <- Houses the crafting table Harmony hooks explicitly
│
├── Models/
│   └── RecipeDefinitions.cs     <- Holds RecipeDefinition & RecipeInputDefinition
│
├── RecipeRegistry.cs            <- Holds the static NewRecipes dictionary
├── RecipeHandler.cs             <- Handles reading/parsing AND the injection logic
└── Plugin.cs                    <- Entrypoint; handles initialization & logging
