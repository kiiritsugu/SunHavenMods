# Project Context Summary

## Overview
This file summarizes the project context, identifying key information about the Sun Haven game source, mod source, and known issues for future debugging.

## Game Source Code (Decompiled)
- **Path:** `decompiled-game-src/`
- **Purpose:** Used for cross-referencing game code with mod patches. Contains the full source code for Sun Haven, which was updated recently, causing breaking changes to mod patches.
- **Notes:** Ignore indexing by IDEs to keep performance up.

## Original Mod Source Code
- **Path:** `RemoteEarthquakeAndRainCloud/`
- **Purpose:** Contains the original, functional implementation of the mod. Used as a reference for reverting changes and restoring expected behavior.

## Known Issues/Changes after Update
1. **Method Signature Changes:** `WateringCan.Use1()` no longer takes a `Vector2Int` argument; it is parameterless. Patches targeting `Use1` were updated to handle this change using `Traverse` for field access.
2. **Method Removals/Renames:** `SelectCurrentHoeItemOLDGCALLOC` in `Hoe.cs` was removed.
3. **Harmony Transpiler Issues:** Recent Harmony versioning caused `ArgumentNullException` when calling `OperandIs()` on instructions with null operands. Added explicit null checks to fix these.
4. **Vanilla Functionality:** Reverting to original reverse-patching and transpilation structure is required to ensure vanilla game behavior (like standard tool indicators) remains intact when the mod's activation key is not held.

## Future Steps
- Review `ToolPatch` and `HoePatch` logic in a fresh session to ensure the `Use1_Prefix` and transpilation logic correctly interface with the updated game's method signatures.
- Ensure that the `HandleHoeEachFrame` transpiler remains safe from `null` operands.
