using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class BlockTarget : MonoBehaviour
{
    [SerializeField] private bool movable;
    public bool Movable => movable;
    public static event System.Action TargetSettled;
    private bool moving;

    // Movable targets are floor markers: they may slide under an objective crate.
    public bool Move(Vector2 direction, Tilemap floor, GameObject[] blocks)
    {
        if (!movable || moving || !floor) return false;
        var destination = Vector2Int.RoundToInt((Vector2)transform.position + direction);
        if (SolidWall.IsAt(destination) || PermeableWall.Blocks(destination, States.Solid, true)) return false;
        bool bridge = Push.HasBridgeAt(destination, blocks);
        if ((!floor.HasTile(floor.WorldToCell((Vector3Int)destination)) || Hole.IsHoleAt(destination)) && !bridge) return false;
        foreach (var target in FindObjectsByType<BlockTarget>())
            if (target != this && Vector2Int.RoundToInt(target.transform.position) == destination) return false;
        StartCoroutine(MoveRoutine(direction));
        return true;
    }

    private IEnumerator MoveRoutine(Vector2 direction)
    {
        moving = true;
        var start = transform.position;
        var end = start + (Vector3)direction;
        float elapsed = 0;
        while (elapsed < .1f)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, elapsed / .1f);
            yield return null;
        }
        transform.position = end;
        moving = false;
        TargetSettled?.Invoke();
    }

    public bool IsCovered(GameObject[] blocks)
    {
        Vector2Int curPos = Vector2Int.RoundToInt(transform.position);
        foreach (var b in blocks)
        {
            // Bridge blocks fill holes; objective targets require regular crates.
            if (b.TryGetComponent(out Push push) && push.FillsHoles) continue;
            if (Vector2Int.RoundToInt(b.transform.position) == curPos)
                return true;
        }
        return false;
    }
}
