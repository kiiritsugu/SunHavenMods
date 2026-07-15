# EasySpells Mod Context Summary

## Overview
This file summarizes the project context for the `EasySpells` mod, documenting the transition from `RemoteEarthquakeAndRainCloud`, known issues, architectural changes, and plans for resolving spellcasting indicators in Sun Haven 3.1.

## Game Source Code (Decompiled)
- **Path:** `3.0-game-src/` and `3.1-game-src/`
- **Purpose:** Cross-referencing game code with mod patches.
- **Key 3.1 Changes Identified:**
  - `Hoe.HandleHoeEachFrame` introduced local variables `min` / `max` for clamps and a capacity list for `potentialHoeingSpots`.
  - `SelectCurrentHoeItem()` replaced the old `SelectCurrentHoeItemOLDGCALLOC()`.
  - IL structural updates caused old offset-based transpilers (searching for offsets like `i + 2`) to fail.

## summary of Recent Architectural Changes
To restore 3.1 compatibility robustly, we refactored the mod's architecture:
1. **Standard `HarmonyTranspiler` on `Hoe.HandleHoeEachFrame`**:
   - Replaced offset-based matching with direct, robust search-and-replace:
     - All `ldc.r4 1.5` instructions are replaced with a call to `HoePatch.GetHoeRange()`, which returns `1000.0f` if the remote key is held and `1.5f` otherwise. This successfully allows aiming/casting at a distance.
     - `IsFarmableDataTile` and `IsFarmableDataTileAndNotHoed` are redirected to custom helpers returning `true` when the key is held.
     - Calls to `SelectCurrentHoeItem` are replaced with `MySelectCurrentHoeItemOLDGCALLOC(this)`.
2. **Simplified `EarthQuakeSpellPatch`**:
   - Replaced the fragile `HarmonyReversePatch` on `SpawnEarthquake` with a standard `HarmonyPrefix` that intercepts `ref Vector2Int position`.
   - Overrides position to `Plugin.earthqueakePos` when `__instance == Plugin.earthqueakeSpell` and lets original code run. This successfully restores remote earthquake spawning.
3. **Cleaned up `ToolPatch`**:
   - Kept `MySetSelectionOnTileBody` reverse-patched only for `WateringCanPatch` backwards compatibility.

## Current Status
- **Working Perfectly**:
  - Watering Can and Rain Cloud functionality (aiming, remote cloud casting, range indicators).
  - Vanilla hoeing behavior when mod key is not held.
  - Remote Earthquake spell casting (Ctrl + Hoe left click aims and spawns the 5x5 earthquake spell anywhere on the farm).
- **Broken / Remaining Issue**:
  - **The 5x5 spell range area indicators do NOT appear** when aiming. Only the single center aimed-at tile indicator displays.
  
## Diagnosing the Remaining Issue (For Next Context)
When aiming with Ctrl + Hoe, the range clamp successfully expands, and casting remote earthquake works. However, only the central target indicator is displayed. This implies one of two possibilities:
1. **Transpiler Call Replacement Failure**:
   - The transpiler might have failed to replace `SelectCurrentHoeItem` call with `MySelectCurrentHoeItemOLDGCALLOC(this)` in `HandleHoeEachFrame` (e.g. if compiler called it virtually or used a different signature), causing the vanilla `SelectCurrentHoeItem` (which only shows 1 tile) to run instead.
2. **Indicator Instantiation / Rendering Failure**:
   - `MySelectCurrentHoeItemOLDGCALLOC` successfully executes, but the other 24 indicators in `selectionList` are being deactivated, positioned incorrectly (overlapping/Z-fighting), or hidden under the ground.

## Next Steps for the Task
1. Verify if `MySelectCurrentHoeItemOLDGCALLOC` is actually being called (inject simple debug logs or file-write logging on execution).
2. If it is NOT executing, inspect the transpiler IL replacement for `SelectCurrentHoeItem`.
3. If it IS executing, debug the indicator list rendering loop:
   - Check if coordinates are correctly offset.
   - Investigate why only the center `item` is visible.
   - Inspect layer, parent, and visibility properties of cloned selection objects.
