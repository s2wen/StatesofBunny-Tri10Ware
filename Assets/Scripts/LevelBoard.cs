using UnityEngine;
using UnityEngine.Tilemaps;
// Keep the player within the level's outer bounds without blocking empty gaps.
public class LevelBoard : MonoBehaviour
{
    public Tilemap floor;
    public bool Contains(Vector2Int position) => floor.cellBounds.Contains(floor.WorldToCell((Vector3Int)position));
}
