# Sun Haven Custom Recipes - JSON Documentation Guide

This mod automatically scans for any files ending with `.recipe.json` inside the BepInEx plugins folder and recursively registers their custom crafting configurations. 

This guide describes the three supported JSON structures you can use to organize your recipes.

---

## 1. Multi-Item Grouped Layout
**Best Used For:** Grouping multiple separate items and all of their alternative recipes inside one single file. This is the cleanest way to organize massive recipe additions.

```json
[
  {
    "outputId": 65317,
    "recipes": [
      {
        "list": "RecipeList_Wizard Crafting Table",
        "amount": 1,
        "hours": 4.0,
        "inputs": [
          { "id": 10733, "amount": 1 }
        ]
      },
      {
        "list": "RecipeList_Wizard Crafting Table",
        "amount": 1,
        "hours": 4.0,
        "inputs": [
          { "id": 10694, "amount": 1 }
        ]
      }
    ]
  },
  {
    "outputId": 65301,
    "recipes": [
      {
        "list": "RecipeList_Anvil",
        "amount": 1,
        "hours": 2.0,
        "inputs": [
          { "id": 10001, "amount": 5 }
        ]
      }
    ]
  }
]
```

---

## 2. Single-Item Grouped Layout (Personal recomendation)
**Best Used For:** Creating dedicated files named after a specific target item, where you list multiple alternative recipes to craft it.

```json
{
  "outputId": 65317,
  "recipes": [
    {
      "list": "RecipeList_Wizard Crafting Table",
      "amount": 1,
      "hours": 4.0,
      "inputs": [
        { "id": 10733, "amount": 1 }
      ]
    },
    {
      "list": "RecipeList_Wizard Crafting Table",
      "amount": 1,
      "hours": 4.0,
      "inputs": [
        { "id": 10694, "amount": 1 }
      ]
    }
  ]
}
```

---

## 3. Single Recipe Layout
**Best Used For:** Quick, standalone recipes. It targets one item and one recipe configuration without wrapping anything inside nested arrays.

```json
{
  "list": "RecipeList_Wizard Crafting Table",
  "outputId": 65317,
  "amount": 1,
  "hours": 4.0,
  "inputs": [
    { "id": 10733, "amount": 1 }
  ]
}
```

---

## Schema Properties Reference

| Property Name | Data Type | Requirement | Description |
| :--- | :--- | :--- | :--- |
| `outputId` | Integer | **Mandatory** | The internal Sun Haven item ID of the item being produced. |
| `list` | String | **Mandatory** | The internal name of the crafting station (e.g., `RecipeList_Anvil`, `RecipeList_Wizard Crafting Table`). Spaces do not matter. |
| `amount` | Integer | Optional | The amount of items crafted at once. Defaults to `1` if omitted or set to `0`. |
| `hours` | Float / Number| Optional | The in-game hours needed to finish crafting. Defaults to `1.0` if omitted. |
| `inputs` | Array | **Mandatory** | A list of sub-objects representing required ingredients. |
| `inputs[].id` | Integer | **Mandatory** | The item ID of the required ingredient item. |
| `inputs[].amount` | Integer | **Mandatory** | The required quantity of that specific ingredient. |

### File Deployment Notice
* File naming conventions do not matter as long as the file ends explicitly with the `.recipe.json` extension.
* Whitespace or spacing variation inside the `list` properties will be auto-cleaned and handled dynamically.
