using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marks one grid cell as a hole, even when a floor tile exists underneath it.
/// Movement is handled by PlayerController and Push; no collider is required.
/// </summary>
[DisallowMultipleComponent]
public class Hole : MonoBehaviour
{
    private static readonly HashSet<Hole> activeHoles = new HashSet<Hole>();

    public static bool IsHoleAt(Vector2Int position)
    {
        foreach (var hole in activeHoles)
        {
            if (hole && Vector2Int.RoundToInt(hole.transform.position) == position)
                return true;
        }
        return false;
    }

    public static void CoverAt(Vector2Int position)
    {
        foreach (var hole in activeHoles)
        {
            if (hole && Vector2Int.RoundToInt(hole.transform.position) == position
                && hole.TryGetComponent(out SpriteRenderer sprite))
                sprite.enabled = false;
        }
    }

    private void OnEnable() => activeHoles.Add(this);
    private void OnDisable() => activeHoles.Remove(this);
}
