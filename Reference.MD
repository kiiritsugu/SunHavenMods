# Unified Totems

## Mod split (two phases)

| Phase | Mod | Job |
|-------|-----|-----|
| 1 — **Add** item to game | [CustomItems](https://github.com/Morthy/sunhaven-mods/tree/main/CustomItems) | JSON + image → `ItemData`, registers in `Database.Instance` (`cache`, `validIDs`, `ids`) |
| 2 — **Edit** behavior | **UnifiedTotems** (this mod) | After CustomItems runs, change `useItem` / `Scarecrow` / crop logic so the item acts like combined vanilla totems |

Sprinklers is **not** the Database reference — it only tweaks `Placeable` for custom sprinklers. For Database access patterns, read **CustomItems/CustomItems.cs** (uses `Database.Instance.ids`, `cache`, etc.; needs Krafs.Publicizer or BepInEx Assembly Publicizer).

## Goal

One custom totem that applies **multiple** vanilla totem/scarecrow effects to nearby crops (seasonal resist, XP, cross-region planting, pest protection, etc.).

## Relevant classes (PSS.Database / Wish)

| Class | Role |
|-------|------|
| `Scarecrow` | Placed totem/scarecrow logic; has `scareCrowEffect`, `range`, `cropCapacity` |
| `ScareCrowEffect` | Enum — one value per scarecrow/totem type |
| `Crop` / crop save data | Crops store **`List<ScareCrowEffect> scareCrowEffects`** — multiple effects can exist on a crop when several totems are nearby |
| `Decoration` / `Placeable` | What CustomItems builds for `"type": "decoration"` by default |

## Implementation notes

1. **CustomItems creates a generic `Decoration`, not a `Scarecrow`.** Either set `"functionality": "PSS.Scarecrow"` in the item JSON, or replace `_decoration` on the `Placeable` after load (copy structure from a vanilla totem via `Database.GetData`).

2. **One `Scarecrow.scareCrowEffect` ≠ all effects at once.** To merge every vanilla totem into one item you will likely need Harmony patches where crops collect nearby scarecrow effects (on placement and when a crop is planted), adding each desired `ScareCrowEffect` to the crop's list — see `Crop` / `DecorationUpdater` in game assemblies.

3. **Timing:** Run setup after CustomItems registers items (`MainMenuController.Start` / `PlayGame`, or `CustomItems.OnCustomItemsAdded`).

4. **Publicizer:** Required if you touch `Database.Instance.*` or other internal members (same as CustomItems).

## Example implementation (this repo)

```
CustomItems (JSON)                    UnifiedTotems (C#)
─────────────────                    ───────────────────
unifiedtotem.item.json    →    ItemHandler.CreateTotems()
  type: decoration                  1. Database.GetData(vanilla totem ids)
  functionality: PSS.Scarecrow        → read Scarecrow.scareCrowEffect from each
  registers ItemData in Database    2. Database.GetData(996001)
                                      → set range/capacity on custom Scarecrow
                                    3. CropPatches (Harmony)
                                      → crops near placed totem get ALL CombinedEffects
```

Files: `ItemHandler.cs`, `UnifiedTotemState.cs`, `CropPatches.cs`

Reference for multi-effect crop logic: [devopsdinosaur/no_more_watering](https://github.com/devopsdinosaur/sunhaven-mods/blob/master/no_more_watering/NoMoreWateringPlugin.cs) (`Crop.GetNearbyScarecrowEffects`, `Crop.AddScarecrowEffects`).

## Links

- CustomItems docs: https://github.com/Morthy/sunhaven-mods/blob/main/CustomItems/docs/CustomItems.md
- Item IDs: https://kryzik.github.io/sun-haven-items/
- Nexus CustomItems: https://www.nexusmods.com/sunhaven/mods/187
