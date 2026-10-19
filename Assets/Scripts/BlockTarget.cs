using UnityEngine;

public class BlockTarget : MonoBehaviour
{
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
