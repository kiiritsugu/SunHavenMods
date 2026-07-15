# EasySpells Mod Context Summary

## Overview
This file summarizes the project context for the `EasySpells` mod, documenting the transition from `RemoteEarthquakeAndRainCloud`, known issues, and planned fixes for spellcasting functionality after game updates.

## Game Source Code (Decompiled)
- **Path:** `decompiled-game-src/`
- **Purpose:** Used for cross-referencing game code with mod patches.
- **Notes:** Current game version has breaking changes compared to version 3.0.2.

## Original Mod Source Code
- **Path:** `RemoteEarthquakeAndRainCloud/`
- **Purpose:** Reference implementation used for restoring functionality.

## Summary of Changes from Original Mod
- Reverted `HoePatch` to the original `HarmonyReversePatch` structure to maintain compatibility.
- Updated transpilers with explicit null checks to fix `ArgumentNullException` from Harmony version updates.
- Refactored `MyIsFarmableDataTile` and `MySelectCurrentHoeItemOLDGCALLOC` to explicitly handle mod toggling, restoring vanilla indicator behavior when the mod is inactive.
- Updated `ToolPatch` to use `Traverse` for safe access to protected/internal game fields.
- **WateringCan/Rain Cloud Changes:** Applied reverse-patching to `HandleWateringCanEachFrame` and overridden `Use1` to handle game source code changes in tool interaction logic, ensuring compatibility with updated tile-checking and tool-usage patterns in the game source.

## Relevant Game Source Classes
- `Wish.Hoe`: Patching tool selection and hoeing logic.
- `Wish.Tool`: Base class for tool interaction and spellcasting execution.
- `Wish.Player`: Manages active spells and inputs.
- `Wish.EarthquakeSpell`: Execution logic for the earthquake spell.
- `Wish.CloudSpell`: Execution logic for the rain cloud spell.

## Current Status
- **Working:**
    - Watering Can and Rain Cloud functionality.
    - Vanilla hoeing behavior when mod key is not held.
- **Pending/In-Progress:**
    - Fixing spellcasting (ctrl + hoe) indicators and execution.
    - Comparing decompiled source with version 3.0.2 to identify underlying API/lifecycle changes.
    - Testing and verification after fixes.

## Next Steps
- Decompile classes (`Hoe`, `Tool`, `EarthquakeSpell`) from version 3.0.2.
- Compare with current 3.1.2 source to find breaking API or MonoBehaviour lifecycle changes.
- Apply identified fixes to `ToolPatch` and `EarthQuakeSpellPatch`.
