using UnityEngine;

public class BlockTarget : MonoBehaviour
{
    public bool IsCovered(GameObject[] blocks)
    {
        Vector2Int curPos = Vector2Int.RoundToInt(transform.position);
        foreach (var b in blocks)
        {
            if (Vector2Int.RoundToInt(b.transform.position) == curPos)
                return true;
        }
        return false;
    }
}
