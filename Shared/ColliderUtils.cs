using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wish;

namespace Shared;
public static class ColliderUtils
{
    private const float ISOMETRIC_Y_FACTOR = 1.4142135f;


    /// <summary>
    /// Reconfigures a component's gameobject colliders to have exactly one, perfectly tile-centered 1x1 collider.
    /// </summary>
    /// <param name="scarecrow">The base component you want to reconfigure.</param>
    /// <param name="trigger">Indicates whether the collider should be a trigger.</param>
    public static void TileAccurateBoxColider2D (Component  component, bool trigger)
    {
        // Delete any existing BoxCollider2D components to avoid conflicts
        BoxCollider2D[] existingColliders = component.GetComponents<BoxCollider2D>();

        foreach (BoxCollider2D collider in existingColliders)
        {
            UnityEngine.Object.DestroyImmediate(collider, true);
        }
        
        // Add a new BoxCollider2D to the component's GameObject
        BoxCollider2D newCollider = component.gameObject.AddComponent<BoxCollider2D>();
        
        // Enforce the perfect 1-tile size and center alignment
        newCollider.size = new Vector2(1.0f, 1.0f);
        newCollider.offset = new Vector2(0.5f, 0.5f);

        // If trigger is true, characters will walk through it, but BoxCastAll still detects it
        newCollider.isTrigger = trigger;
    }

    /// <summary>
    /// Casts an isometric-adjusted box area and executes a custom action on every unique component of type T found.
    /// </summary>
    /// <typeparam name="T">The Component type to look for (e.g., Crop, Scarecrow).</typeparam>
    /// <param name="center">The center point of the cast (usually RealCenter).</param>
    /// <param name="range">The radius tile range of the cast.</param>
    /// <param name="action">The function/method to run on each found component.</param>
    /// <param name="applySnapping">If true, snaps the center coordinate to the nearest perfect grid tile center (.5 boundary) before casting.</param>
    public static void BoxCastHitsOnTypeAction<T>(Vector2 center, float range, Action<T> action, bool applySnapping = false) where T : Component
    {
        if (action == null) return;

        Vector2 castCenter = center;
        // Conditionally apply grid snapping if requested
        if (applySnapping)
        {
            float snappedX = Mathf.RoundToInt(center.x - 0.5f) + 0.5f;
            float snappedY = Mathf.RoundToInt(center.y - 0.5f) + 0.5f;
            castCenter = new Vector2(snappedX, snappedY);
        }

        // Compute dynamic box cast footprint size adapted for the isometric projection
        float width = (range * 2f) + 1f;
        Vector2 boxSize = new Vector2(width, width * ISOMETRIC_Y_FACTOR);

        // Track seen components to prevent double evaluation on multi-collider objects
        HashSet<T> evaluatedComponents = new HashSet<T>();

        // Perform the static box cast
        foreach (RaycastHit2D hit in Physics2D.BoxCastAll(castCenter, boxSize, 0f, Vector2.zero))
        {
            if (hit.transform == null) continue;

            T component = hit.transform.GetComponent<T>();
            if (component != null && !evaluatedComponents.Contains(component))
            {
                evaluatedComponents.Add(component);
                action(component);
            }
        }
    }
}