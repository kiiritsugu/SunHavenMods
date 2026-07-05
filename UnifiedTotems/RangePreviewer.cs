using System.Reflection;
using Morthy.Util;
using UnityEngine;
using Wish;

namespace UnifiedTotems;

public static class RangePreviewer
{
  public static void CreateRangePreview(ItemData item, int range = 5)
  {
    if (item?.useItem is not Placeable placeable || !item.name.Contains("Totem"))
    {
      Plugin.logger.LogError($"UnifiedTotems: Item {item?.name} is not a Placeable totem, cannot create range preview.");
      return;
    }

    placeable.snapToTile = true;

    // Calculate the pivot point for the preview sprite based on the range in order to center it.
    var p = 0.5f - (1f / (range * 2 + 1) / 2);

    //Create a sprite from png image
    placeable._secondaryPreviewSprite = SpriteUtil.CreateSprite(FileLoader.LoadFileBytes(Assembly.GetExecutingAssembly(), $"preview.png"), new Vector2(p, p), $"Totem preview");

    //Align the preview sprite with the decoration graphics, with a fine tune adjustment to alignment.
    placeable.previewOffset = new Vector2(-0.1f, 0);
    placeable._decoration.transform.Find("Graphics").localPosition = new Vector3(-0.1f, 0, 0);
  }
}