using UnityEngine;

/// <summary>A fixed, one-cell obstacle that blocks every form and pushable block.</summary>
[DisallowMultipleComponent]
public class SolidWall : MonoBehaviour
{
    public static bool IsAt(Vector2Int position)
    {
        foreach (var wall in FindObjectsByType<SolidWall>())
        {
            if (wall.isActiveAndEnabled
                && Vector2Int.RoundToInt(wall.transform.position) == position)
                return true;
        }
        return false;
    }
}
