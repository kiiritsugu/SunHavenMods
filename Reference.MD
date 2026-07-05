# Unified Totems:

Use sprinklers mod as reference for adding logic to placed items
https://github.com/Morthy/sunhaven-mods/tree/main/Sprinklers
Use custom furniture and items mod to add the new items and recipes
https://www.nexusmods.com/sunhaven/mods/187
https://github.com/Morthy/sunhaven-mods/blob/main/CustomItems/docs/CustomItems.md

## Relevant Classes
### Sunhaven.core.dll> Wish >
### PSS.Database.dll
#### Scarecrow
  Logic for scarecrows and totems
#### ScarecrowEffect
  List of scarecrow and totem effects
#### Crop
Get nearby scarecrow effects and others, search for carecrow in crop
#### Decoration
#### Placeable

Must both ensure that on placement it adds the scarecrow's effect to crops, and that on crop placement it checks colision with nearby scarecrows and adds the effect to the crop

must add list of scarecrow effects to the totem and add each effect to crop