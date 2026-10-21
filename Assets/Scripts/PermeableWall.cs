using UnityEngine;
public class PermeableWall : MonoBehaviour
{
    public States allowedState = States.Gas;
    public static bool IsAt(Vector2Int position)
    {
        foreach (var wall in FindObjectsByType<PermeableWall>())
            if (Vector2Int.RoundToInt(wall.transform.position) == position) return true;
        return false;
    }
    public static bool Blocks(Vector2Int position, States state, bool pushingBlock = false)
    {
        foreach (var wall in FindObjectsByType<PermeableWall>())
            if (Vector2Int.RoundToInt(wall.transform.position) == position)
                return pushingBlock || wall.allowedState != state;
        return false;
    }
}
