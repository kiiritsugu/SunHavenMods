using System;
using System.Collections;
using UnityEngine;
using Wish;

namespace UnifiedTotems;
public static class Utilitaries
{
  /// <summary>
  /// Reconfigures a component's gameobject colliders to have exactly one, perfectly tile-centered 1x1 collider.
  /// </summary>
  /// <param name="scarecrow">The base component you want to reconfigure.</param>
  /// <param name="trigger">Indicates whether the collider should be a trigger.</param>
  public static void TilePerfectBoxColider2D (Component  component, bool trigger)
  {
      // Delete any existing BoxCollider2D components to avoid conflicts
      BoxCollider2D[] existingColliders = component.GetComponents<BoxCollider2D>();

      foreach (BoxCollider2D collider in existingColliders)
      {
          UnityEngine.Object.DestroyImmediate(collider, true);
      }
      
      // Add a new BoxCollider2D to the component's GameObject
      BoxCollider2D logicCollider = component.gameObject.AddComponent<BoxCollider2D>();
      
      // Enforce the perfect 1-tile size and center alignment
      logicCollider.size = new Vector2(1.0f, 1.0f);
      logicCollider.offset = new Vector2(0.5f, 0.5f);

      // Mark as a trigger so characters walk through it, but BoxCastAll still detects it
      logicCollider.isTrigger = trigger;
  }
}